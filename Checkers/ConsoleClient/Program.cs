// See https://aka.ms/new-console-template for more information

using ConsoleClient;
using ConsoleMenuSystem;
using ConsoleUI;
using DAL;
using DAL.Db;
using Domain;
using GameBrain;
using Microsoft.EntityFrameworkCore;

var window = new ConsoleWindow("Checkers");

var selectedCheckersRuleset = new CheckersRuleset();
CheckersGame? selectedCheckersGame = null;

using var ctx = AppDbContextFactory.CreateDbContext();
ctx.Database.Migrate();

RepositoryContext repoDb = new RepositoryContext(ctx);
DAL.FileSystem.RepositoryContext repoFs = new DAL.FileSystem.RepositoryContext();
IRepositoryContext repoCtx = repoDb;

EMenuFunction RunConsoleGame(Menu menu)
{
    try
    {
        ConsoleGame consoleGame;
        if (selectedCheckersGame != null)
        {
            consoleGame = new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(selectedCheckersGame), repoCtx);
        }
        else
        {
            var firstPlayerName = menu.ConsoleWindow.PopupPromptTextInput("Please enter a name for the first player:");
            var secondPlayerName = menu.ConsoleWindow.PopupPromptTextInput("Please enter a name for the second player:");
            // TODO: Allow users to randomize starting order?
            consoleGame = new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(new CheckersRuleset
            {
                Id = default,
                BuiltIn = false,
                Saved = false,
                Width = selectedCheckersRuleset.Width,
                Height = selectedCheckersRuleset.Height,
                Title = selectedCheckersRuleset.Title,
                Description = selectedCheckersRuleset.Description,
                BlackMovesFirst = selectedCheckersRuleset.BlackMovesFirst,
                MustCapture = selectedCheckersRuleset.MustCapture,
                CanCaptureBackwards = selectedCheckersRuleset.CanCaptureBackwards,
                CanCaptureBackwardsDuringMultiCapture = selectedCheckersRuleset.CanCaptureBackwardsDuringMultiCapture,
                FlyingKings = selectedCheckersRuleset.FlyingKings
            }, firstPlayerName, secondPlayerName), repoCtx);
        }
        consoleGame.Run();
    }
    catch (ArgumentOutOfRangeException e)
    {
        menu.ConsoleWindow.PopUpMessageBox("An error occurred when attempting to run Checkers game!", e.Message);
    }
    finally
    {
        selectedCheckersRuleset = null;
        selectedCheckersGame = null;
    }

    return EMenuFunction.MainMenu;
}

var customBoardSize = new MenuItem("Custom board size", menu =>
{
    var consoleWindow = menu.ConsoleWindow;

    var x = consoleWindow.PopupPromptIntInput("Enter board horizontal dimension:", CheckersRuleset.IsDimensionValid);
    var y = consoleWindow.PopupPromptIntInput("Enter board vertical dimension:", CheckersRuleset.IsDimensionValid);

    selectedCheckersRuleset = new CheckersRuleset { Width = x, Height = y, Title = $"{x}x{y}", Saved = false };
    return RunConsoleGame(menu);
});

var loadCheckersRulesetsMenuFactory = new MenuFactory("Pick a Checkers ruleset", _ =>
{
    var checkersRulesets = new List<MenuItem>();
    foreach (var checkersRuleset in repoCtx.CheckersRulesetRepository.GetAll().Where(cr => cr.Saved))
    {
        checkersRulesets.Add(new MenuItem(checkersRuleset.TitleText, m =>
        {
            selectedCheckersRuleset = checkersRuleset;
            return RunConsoleGame(m);
        }));
    }

    checkersRulesets.Add(customBoardSize);

    return checkersRulesets;
});

var loadGameMenuFactory = new MenuFactory("Load game", _ =>
{
    var gamesToLoad = new List<MenuItem>();
    foreach (var checkersGame in repoCtx.CheckersGameRepository.GetAll())
    {
        gamesToLoad.Add(new MenuItem(
            $"{checkersGame.Id} | {checkersGame.CheckersRuleset!.Title} - Started: {checkersGame.StartedAt.ToLocalTime()}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt.ToLocalTime() ?? checkersGame.StartedAt.ToLocalTime()}",
            m =>
            {
                selectedCheckersGame = checkersGame;
                return RunConsoleGame(m);
            }));
    }

    return gamesToLoad;
});

var deleteGameMenuFactory = new MenuFactory("Delete game", _ =>
{
    var gamesToDelete = new List<MenuItem>();
    foreach (var checkersGame in repoCtx.CheckersGameRepository.GetAll())
    {
        gamesToDelete.Add(new MenuItem(
            $"{checkersGame.Id} | {checkersGame.CheckersRuleset!.Title} - Started: {checkersGame.StartedAt.ToLocalTime()}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt.ToLocalTime() ?? checkersGame.StartedAt.ToLocalTime()}",
            _ =>
            {
                repoCtx.CheckersGameRepository.Remove(checkersGame.Id);
                return EMenuFunction.MainMenu;
            }));
    }

    return gamesToDelete;
});

MenuItem GetEditableCheckersRulesetMenuItem(CheckersRuleset checkersRuleset, string? menuItemText = null)
{
    return new MenuItem(
        menuItemText ??
        (checkersRuleset.BuiltIn ? $"{checkersRuleset.TitleText} (Built-in)" : checkersRuleset.TitleText), m =>
        {
            var previousCursorPosition = 0;
            var result = EMenuFunction.Refresh;
            while (result == EMenuFunction.Refresh)
            {
                var editCheckersRulesetsMenuFactory = new MenuFactory(
                    _ => checkersRuleset.BuiltIn
                        ? "Viewing built-in Checkers ruleset"
                        : "Editing custom Checkers ruleset",
                    _ =>
                    {
                        var rulesetMenuItems = new List<MenuItem>
                        {
                            new($"Game board width: {checkersRuleset.Width}", m2 =>
                            {
                                checkersRuleset.Width = m2.ConsoleWindow.PopupPromptIntInput("Enter game board width",
                                    CheckersRuleset.IsDimensionValid);
                                return EMenuFunction.Continue;
                            }),
                            new($"Game board height: {checkersRuleset.Height}", m2 =>
                            {
                                checkersRuleset.Height = m2.ConsoleWindow.PopupPromptIntInput("Enter game board height",
                                    CheckersRuleset.IsDimensionValid);
                                return EMenuFunction.Continue;
                            }),
                            new($"Black moves first: {checkersRuleset.BlackMovesFirst}", _ =>
                            {
                                checkersRuleset.BlackMovesFirst = !checkersRuleset.BlackMovesFirst;
                                return EMenuFunction.Continue;
                            }),
                            new($"Must capture piece if able: {checkersRuleset.MustCapture}", _ =>
                            {
                                checkersRuleset.MustCapture = !checkersRuleset.MustCapture;
                                return EMenuFunction.Continue;
                            }),
                            new($"Can capture backwards: {checkersRuleset.CanCaptureBackwards}", _ =>
                            {
                                checkersRuleset.CanCaptureBackwards = !checkersRuleset.CanCaptureBackwards;
                                return EMenuFunction.Continue;
                            }),
                            new($"Can capture backwards during multi-capture: {checkersRuleset.CanCaptureBackwardsDuringMultiCapture}",
                                _ =>
                                {
                                    checkersRuleset.CanCaptureBackwardsDuringMultiCapture =
                                        !checkersRuleset.CanCaptureBackwardsDuringMultiCapture;
                                    return EMenuFunction.Continue;
                                }),
                            new($"Kings can move multiple cells: {checkersRuleset.FlyingKings}",
                                _ =>
                                {
                                    checkersRuleset.FlyingKings =
                                        !checkersRuleset.FlyingKings;
                                    return EMenuFunction.Continue;
                                }),
                            new("Ruleset title: " + checkersRuleset.Title, m2 =>
                            {
                                checkersRuleset.Title =
                                    m2.ConsoleWindow.PopupPromptTextInput("Enter a title for your ruleset!");
                                return EMenuFunction.Continue;
                            }),
                            new("Ruleset description: " + checkersRuleset.Description, m2 =>
                            {
                                checkersRuleset.Description =
                                    m2.ConsoleWindow.PopupPromptTextInput(
                                        "You may enter a description for your ruleset");
                                return EMenuFunction.Continue;
                            })
                        };

                        if (!checkersRuleset.BuiltIn)
                        {
                            rulesetMenuItems.Add(new MenuItem("Save", _ =>
                            {
                                repoCtx.CheckersRulesetRepository.Upsert(checkersRuleset);
                                return EMenuFunction.Back;
                            }));
                            rulesetMenuItems.Add(new MenuItem("Delete", _ =>
                            {
                                repoCtx.CheckersRulesetRepository.Remove(checkersRuleset);
                                return EMenuFunction.Back;
                            }));
                        }

                        rulesetMenuItems.Add(
                            GetEditableCheckersRulesetMenuItem(checkersRuleset.GetClone(), "Create a copy"));

                        return rulesetMenuItems;
                    }
                );

                var rulesetEditMenu = editCheckersRulesetsMenuFactory.Create(m.ConsoleWindow, m);

                rulesetEditMenu.CursorPosition = previousCursorPosition;
                result = rulesetEditMenu.Run();
                previousCursorPosition = rulesetEditMenu.CursorPosition;
            }

            return result;
        });
}

var manageCheckersRulesetsMenuFactory = new MenuFactory("Manage Checkers rulesets", _ =>
{
    var checkersRulesets = new List<MenuItem>();

    checkersRulesets.Add(GetEditableCheckersRulesetMenuItem(new CheckersRuleset(), "CREATE NEW RULESET"));
    foreach (var checkersRuleset in repoCtx.CheckersRulesetRepository.GetAll().Where(cr => cr.Saved))
    {
        repoCtx.CheckersRulesetRepository.Refresh(checkersRuleset); // TODO: figure out a better way to do this
        checkersRulesets.Add(GetEditableCheckersRulesetMenuItem(checkersRuleset));
    }

    return checkersRulesets;
});

IRepositoryContext OtherRepoCtx()
{
    return repoCtx == repoDb ? repoFs : repoDb;
}

string GetRepoSwapText()
{
    return $"Swap persistence engine? ({repoCtx.Name} => {OtherRepoCtx().Name})";
}

var swapPersistenceEngine = new MenuItem(GetRepoSwapText(), (_, mi) =>
{
    repoCtx = OtherRepoCtx();
    mi.Text = GetRepoSwapText();
    return EMenuFunction.Continue;
});

var optionsManagerMenuCreator = new MenuFactory("Options",
    new MenuItem("Manage Checkers rulesets", manageCheckersRulesetsMenuFactory),
    swapPersistenceEngine
);

var mainMenuCreator = new MenuFactory("Main menu",
    new MenuItem("New game", loadCheckersRulesetsMenuFactory),
    new MenuItem("Load game", loadGameMenuFactory),
    new MenuItem("Delete game", deleteGameMenuFactory),
    new MenuItem("Options", optionsManagerMenuCreator));

var mainMenu = mainMenuCreator.Create(window);

mainMenu.Run();
window.Close();