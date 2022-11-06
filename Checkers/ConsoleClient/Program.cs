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

var selectedCheckersOptions = new CheckersOptions();
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
        var consoleGame = selectedCheckersGame != null
            ? new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(selectedCheckersGame), repoCtx)
            : new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(new CheckersOptions()
            {
                Id = default,
                BuiltIn = false,
                Saved = false,
                Width = selectedCheckersOptions.Width,
                Height = selectedCheckersOptions.Height,
                Title = selectedCheckersOptions.Title,
                Description = selectedCheckersOptions.Description,
                BlackMovesFirst = selectedCheckersOptions.BlackMovesFirst,
                MustCapture = selectedCheckersOptions.MustCapture,
                CanJumpBackwards = selectedCheckersOptions.CanJumpBackwards,
                CanJumpBackwardsDuringMultiCapture = selectedCheckersOptions.CanJumpBackwardsDuringMultiCapture
            }), repoCtx);
        consoleGame.Run();
    }
    catch (ArgumentOutOfRangeException e)
    {
        menu.ConsoleWindow.PopUpMessageBox("An error occurred when attempting to run Checkers game!", e.Message);
    }
    finally
    {
        selectedCheckersOptions = null;
        selectedCheckersGame = null;
    }

    return EMenuFunction.MainMenu;
}

var customBoardSize = new MenuItem("Custom board size", menu =>
{
    var consoleWindow = menu.ConsoleWindow;

    var x = consoleWindow.PopupPromptIntInput("Enter board horizontal dimension:", CheckersOptions.IsDimensionValid);
    var y = consoleWindow.PopupPromptIntInput("Enter board vertical dimension:", CheckersOptions.IsDimensionValid);

    selectedCheckersOptions = new CheckersOptions { Width = x, Height = y, Title = $"{x}x{y}", Saved = false };
    return RunConsoleGame(menu);
});

var loadGameOptionsMenuFactory = new MenuFactory("Pick a game options preset", _ =>
{
    var gameOptionsPresets = new List<MenuItem>();
    foreach (var checkersOptions in repoCtx.CheckersOptionsRepository.GetAll().Where(co => co.Saved))
    {
        gameOptionsPresets.Add(new MenuItem(checkersOptions.TitleText, m =>
        {
            selectedCheckersOptions = checkersOptions;
            return RunConsoleGame(m);
        }));
    }

    gameOptionsPresets.Add(customBoardSize);

    return gameOptionsPresets;
});

var loadGameMenuFactory = new MenuFactory("Load game", _ =>
{
    var gamesToLoad = new List<MenuItem>();
    foreach (var checkersGame in repoCtx.CheckersGameRepository.GetAll())
    {
        gamesToLoad.Add(new MenuItem(
            $"{checkersGame.Id} | {checkersGame.CheckersOptions.Title} - Started: {checkersGame.StartedAt.ToLocalTime()}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt.ToLocalTime() ?? checkersGame.StartedAt.ToLocalTime()}",
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
            $"{checkersGame.Id} | {checkersGame.CheckersOptions.Title} - Started: {checkersGame.StartedAt.ToLocalTime()}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt.ToLocalTime() ?? checkersGame.StartedAt.ToLocalTime()}",
            _ =>
            {
                repoCtx.CheckersGameRepository.Remove(checkersGame.Id);
                return EMenuFunction.MainMenu;
            }));
    }

    return gamesToDelete;
});

MenuItem GetEditableCheckersOptionsMenuItem(CheckersOptions checkersOptions, string? menuItemText = null)
{
    return new MenuItem(
        menuItemText ??
        (checkersOptions.BuiltIn ? $"{checkersOptions.TitleText} (Built-in)" : checkersOptions.TitleText), m =>
        {
            var previousCursorPosition = 0;
            var result = EMenuFunction.Refresh;
            while (result == EMenuFunction.Refresh)
            {
                var optionsEditMenuFactory = new MenuFactory(
                    _ => checkersOptions.BuiltIn ? "Viewing built-in game options" : "Editing custom game options",
                    _ =>
                    {
                        var optionsMenuItems = new List<MenuItem>
                        {
                            new($"Game board width: {checkersOptions.Width}", m2 =>
                            {
                                checkersOptions.Width = m2.ConsoleWindow.PopupPromptIntInput("Enter game board width",
                                    CheckersOptions.IsDimensionValid);
                                return EMenuFunction.Continue;
                            }),
                            new($"Game board height: {checkersOptions.Height}", m2 =>
                            {
                                checkersOptions.Height = m2.ConsoleWindow.PopupPromptIntInput("Enter game board height",
                                    CheckersOptions.IsDimensionValid);
                                return EMenuFunction.Continue;
                            }),
                            new($"Black moves first: {checkersOptions.BlackMovesFirst}", _ =>
                            {
                                checkersOptions.BlackMovesFirst = !checkersOptions.BlackMovesFirst;
                                return EMenuFunction.Continue;
                            }),
                            new($"Must capture piece if able: {checkersOptions.MustCapture}", _ =>
                            {
                                checkersOptions.MustCapture = !checkersOptions.MustCapture;
                                return EMenuFunction.Continue;
                            }),
                            new($"Backwards jumps: {checkersOptions.CanJumpBackwards}", _ =>
                            {
                                checkersOptions.CanJumpBackwards = !checkersOptions.CanJumpBackwards;
                                return EMenuFunction.Continue;
                            }),
                            new($"Multi-capture backwards jumps: {checkersOptions.CanJumpBackwardsDuringMultiCapture}",
                                _ =>
                                {
                                    checkersOptions.CanJumpBackwardsDuringMultiCapture =
                                        !checkersOptions.CanJumpBackwardsDuringMultiCapture;
                                    return EMenuFunction.Continue;
                                }),
                            new("Options title: " + checkersOptions.Title, m2 =>
                            {
                                checkersOptions.Title =
                                    m2.ConsoleWindow.PopupPromptTextInput("Enter a title for your Menu!");
                                return EMenuFunction.Continue;
                            }),
                            new("Options description: " + checkersOptions.Description, m2 =>
                            {
                                checkersOptions.Description =
                                    m2.ConsoleWindow.PopupPromptTextInput(
                                        "You may enter a description for your options preset");
                                return EMenuFunction.Continue;
                            })
                        };

                        if (!checkersOptions.BuiltIn)
                        {
                            optionsMenuItems.Add(new MenuItem("Save", _ =>
                            {
                                repoCtx.CheckersOptionsRepository.Upsert(checkersOptions);
                                return EMenuFunction.Back;
                            }));
                            optionsMenuItems.Add(new MenuItem("Delete", _ =>
                            {
                                repoCtx.CheckersOptionsRepository.Remove(checkersOptions);
                                return EMenuFunction.Back;
                            }));
                        }

                        optionsMenuItems.Add(
                            GetEditableCheckersOptionsMenuItem(checkersOptions.GetClone(), "Create a copy"));

                        return optionsMenuItems;
                    }
                );

                var optionsEditMenu = optionsEditMenuFactory.Create(m.ConsoleWindow, m);

                optionsEditMenu.CursorPosition = previousCursorPosition;
                result = optionsEditMenu.Run();
                previousCursorPosition = optionsEditMenu.CursorPosition;
            }

            return result;
        });
}

var manageCheckersOptionsMenuFactory = new MenuFactory("Manage game options", _ =>
{
    var checkersOptionsList = new List<MenuItem>();

    checkersOptionsList.Add(GetEditableCheckersOptionsMenuItem(new CheckersOptions(), "CREATE NEW OPTIONS"));
    foreach (var checkersOptions in repoCtx.CheckersOptionsRepository.GetAll().Where(co => co.Saved))
    {
        repoCtx.CheckersOptionsRepository.Refresh(checkersOptions); // TODO: figure out a better way to do this
        checkersOptionsList.Add(GetEditableCheckersOptionsMenuItem(checkersOptions));
    }

    return checkersOptionsList;
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
    new MenuItem("Manage game options", manageCheckersOptionsMenuFactory),
    swapPersistenceEngine
);

var mainMenuCreator = new MenuFactory("Main menu",
    new MenuItem("New game", loadGameOptionsMenuFactory),
    new MenuItem("Load game", loadGameMenuFactory),
    new MenuItem("Delete game", deleteGameMenuFactory),
    new MenuItem("Options", optionsManagerMenuCreator));

var mainMenu = mainMenuCreator.Create(window);

mainMenu.Run();
window.Close();