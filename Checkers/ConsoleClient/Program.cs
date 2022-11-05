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

var loadGameOptionsMenuFactory = new MenuFactory("Pick a game options preset")
{
    MenuItemsFunc = () =>
    {
        var gameOptionsPresets = new List<MenuItem>();
        foreach (var checkersOptions in repoCtx.CheckersOptionsRepository.GetAll().Where(co => co.Saved))
        {
            gameOptionsPresets.Add(new MenuItem(checkersOptions.Title, m =>
            {
                selectedCheckersOptions = checkersOptions;
                return RunConsoleGame(m);
            }));
        }

        gameOptionsPresets.Add(customBoardSize);

        return gameOptionsPresets;
    }
};

var loadGameMenuFactory = new MenuFactory("Load game")
{
    MenuItemsFunc = () =>
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
    }
};

var deleteGameMenuFactory = new MenuFactory("Delete game")
{
    MenuItemsFunc = () =>
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
    }
};

var createOptions = new MenuItem("Create new custom options", m =>
{
    var customOptions = new CheckersOptions();

    while (true)
    {
        try
        {
            var previousCursorPosition = 0;
            var result = EMenuFunction.Refresh;
            while (result == EMenuFunction.Refresh)
            {
                var optionsEditMenu = new Menu("Customize options", m.ConsoleWindow, null, m,
                    new MenuItem($"Game board width: {customOptions.Width}", m2 =>
                    {
                        customOptions.Width = m2.ConsoleWindow.PopupPromptIntInput("Enter game board width", CheckersOptions.IsDimensionValid);
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem($"Game board height: {customOptions.Height}", m2 =>
                    {
                        customOptions.Height = m2.ConsoleWindow.PopupPromptIntInput("Enter game board height", CheckersOptions.IsDimensionValid);
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem($"Black moves first: {customOptions.BlackMovesFirst}", _ =>
                    {
                        customOptions.BlackMovesFirst = !customOptions.BlackMovesFirst;
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem($"Must capture piece if able: {customOptions.MustCapture}", _ =>
                    {
                        customOptions.MustCapture = !customOptions.MustCapture;
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem($"Backwards jumps: {customOptions.CanJumpBackwards}", _ =>
                    {
                        customOptions.CanJumpBackwards = !customOptions.CanJumpBackwards;
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem(
                        $"Multi-capture backwards jumps: {customOptions.CanJumpBackwardsDuringMultiCapture}", _ =>
                        {
                            customOptions.CanJumpBackwardsDuringMultiCapture =
                                !customOptions.CanJumpBackwardsDuringMultiCapture;
                            return EMenuFunction.Refresh;
                        }),
                    new MenuItem("Options title: " + customOptions.Title, m2 =>
                    {
                        customOptions.Title = m2.ConsoleWindow.PopupPromptTextInput("Enter a title for your Menu!");
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem("Options description: " + customOptions.Description, m2 =>
                    {
                        customOptions.Description =
                            m2.ConsoleWindow.PopupPromptTextInput(
                                "You may enter a description for your options preset");
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem("Save", _ =>
                    {
                        repoCtx.CheckersOptionsRepository.Add(customOptions);
                        return EMenuFunction.Back;
                    })
                );
                optionsEditMenu.CursorPosition = previousCursorPosition;
                result = optionsEditMenu.Run();
                previousCursorPosition = optionsEditMenu.CursorPosition;
            }

            return result;
        }
        catch (Exception e)
        {
            m.ConsoleWindow.PopUpMessageBox($"An error occurred: {e.ToString().Split("\n")[0]}");
        }
    }
});

var viewOptionsMenuFactory = new MenuFactory("View options")
{
    MenuItemsFunc = () =>
    {
        var checkersOptionsToView = new List<MenuItem>();
        foreach (var checkersOptions in repoCtx.CheckersOptionsRepository.GetAll().Where(co => co.Saved))
        {
            checkersOptionsToView.Add(new MenuItem($"{checkersOptions.Title}",
                m =>
                {
                    m.ConsoleWindow.PopUpMessageBox("Title: " + checkersOptions.Title,
                        "Description: " + checkersOptions.Description,
                        "Width: " + checkersOptions.Width,
                        "Height: " + checkersOptions.Height,
                        "BlackMovesFirst: " + checkersOptions.BlackMovesFirst,
                        "MustCapture: " + checkersOptions.MustCapture,
                        "CanJumpBackwards: " + checkersOptions.CanJumpBackwards,
                        "CanJumpBackwardsDuringMultiCapture: " + checkersOptions.CanJumpBackwardsDuringMultiCapture
                    );
                    return EMenuFunction.Continue;
                }));
        }

        return checkersOptionsToView;
    }
};

var deleteOptionsMenuFactory = new MenuFactory("Delete options")
{
    MenuItemsFunc = () =>
    {
        var checkersOptionsToDelete = new List<MenuItem>();
        foreach (var checkersOption in repoCtx.CheckersOptionsRepository.GetAll().Where(co => !co.BuiltIn && co.Saved))
        {
            checkersOptionsToDelete.Add(new MenuItem($"{checkersOption.Title}",
                _ =>
                {
                    repoCtx.CheckersOptionsRepository.Remove(checkersOption.Id);
                    return EMenuFunction.Back;
                }));
        }

        return checkersOptionsToDelete;
    }
};

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
    createOptions,
    new MenuItem("View", viewOptionsMenuFactory),
    new MenuItem("Delete", deleteOptionsMenuFactory),
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