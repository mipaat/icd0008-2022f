// See https://aka.ms/new-console-template for more information

using ConsoleClient;
using ConsoleMenuSystem;
using ConsoleUI;
using DAL.FileSystem;
using Domain;
using GameBrain;

var selectedCheckersOptions = new CheckersOptions();
CheckersGame? selectedCheckersGame = null;
var ctx = new RepositoryContext();

EMenuFunction RunConsoleGame(Menu menu)
{
    try
    {
        var consoleGame = selectedCheckersGame != null
            ? new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(selectedCheckersGame), ctx)
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
            }), ctx);
        consoleGame.Run();
    }
    catch (ArgumentOutOfRangeException e)
    {
        menu.ConsoleWindow.MessageBox("An error occurred when attempting to run Checkers game!", e.Message);
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

    if (!int.TryParse(consoleWindow.PopupPromptTextInput("Enter board horizontal dimension:"), out var x))
    {
        consoleWindow.MessageBox("Board dimension must be an integer!");
        return EMenuFunction.Continue;
    }

    if (!int.TryParse(consoleWindow.PopupPromptTextInput("Enter board vertical dimension:"), out var y))
    {
        consoleWindow.MessageBox("Board dimension must be an integer!");
        return EMenuFunction.Continue;
    }

    selectedCheckersOptions = new CheckersOptions { Width = x, Height = y, Title = $"{x}x{y}", Saved = false };
    return RunConsoleGame(menu);
});

var loadGameOptionsMenuFactory = new MenuFactory("Pick a game options preset")
{
    MenuItemsFunc = () =>
    {
        var gameOptionsPresets = new List<MenuItem>();
        foreach (var checkersOptions in ctx.CheckersOptionsRepository.GetAll().Where(co => co.Saved))
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
        foreach (var checkersGame in ctx.CheckersGameRepository.GetAll())
        {
            gamesToLoad.Add(new MenuItem(
                $"{checkersGame.Id} | {checkersGame.CheckersOptions.Title} - Started: {checkersGame.StartedAt}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt ?? checkersGame.StartedAt}",
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
        foreach (var checkersGame in ctx.CheckersGameRepository.GetAll())
        {
            gamesToDelete.Add(new MenuItem(
                $"{checkersGame.Id} | {checkersGame.CheckersOptions.Title} - Started: {checkersGame.StartedAt}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt ?? checkersGame.StartedAt}",
                _ =>
                {
                    ctx.CheckersGameRepository.Remove(checkersGame.Id);
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
            var result = EMenuFunction.Refresh;
            while (result == EMenuFunction.Refresh)
            {
                result = new Menu("Customize options", m.ConsoleWindow, null, m,
                    new MenuItem($"Game board width: {customOptions.Width}", m2 =>
                    {
                        customOptions.Width = m2.ConsoleWindow.PopupPromptIntInput("Enter game board width");
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem($"Game board height: {customOptions.Height}", m2 =>
                    {
                        customOptions.Height = m2.ConsoleWindow.PopupPromptIntInput("Enter game board height");
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem($"Black moves first: {customOptions.BlackMovesFirst}", m2 =>
                    {
                        customOptions.BlackMovesFirst =
                            m2.ConsoleWindow.PopupPromptBoolInput("Should black pieces start the game?");
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem($"Must capture piece if able: {customOptions.MustCapture}", m2 =>
                    {
                        customOptions.MustCapture =
                            m2.ConsoleWindow.PopupPromptBoolInput(
                                "If a player can capture a piece, must they capture it?");
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem("Backwards jumps: " + (customOptions.CanJumpBackwards ? "yes" : "no"), m2 =>
                    {
                        customOptions.CanJumpBackwards =
                            m2.ConsoleWindow.PopupPromptBoolInput("Can regular pieces jump backwards?");
                        return EMenuFunction.Refresh;
                    }),
                    new MenuItem(
                        "Multi-capture backwards jumps: " +
                        (customOptions.CanJumpBackwardsDuringMultiCapture ? "yes" : "no"), m2 =>
                        {
                            customOptions.CanJumpBackwardsDuringMultiCapture =
                                m2.ConsoleWindow.PopupPromptBoolInput(
                                    "Can regular pieces jump backwards during a multi-capture jump?");
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
                        ctx.CheckersOptionsRepository.Add(customOptions);
                        return EMenuFunction.Back;
                    })
                ).Run();
            }

            return result;
        }
        catch (Exception e)
        {
            m.ConsoleWindow.MessageBox($"An error occurred: {e.ToString().Split("\n")[0]}");
        }
    }
});

var viewOptionsMenuFactory = new MenuFactory("View options")
{
    MenuItemsFunc = () =>
    {
        var checkersOptionsToView = new List<MenuItem>();
        foreach (var checkersOptions in ctx.CheckersOptionsRepository.GetAll().Where(co => co.Saved))
        {
            checkersOptionsToView.Add(new MenuItem($"{checkersOptions.Title}",
                m =>
                {
                    m.ConsoleWindow.MessageBox("Title: " + checkersOptions.Title,
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
        foreach (var checkersOption in ctx.CheckersOptionsRepository.GetAll().Where(co => !co.BuiltIn && co.Saved))
        {
            checkersOptionsToDelete.Add(new MenuItem($"{checkersOption.Title}",
                _ =>
                {
                    ctx.CheckersOptionsRepository.Remove(checkersOption.Id);
                    return EMenuFunction.Back;
                }));
        }

        return checkersOptionsToDelete;
    }
};

var optionsManagerMenuCreator = new MenuFactory("Options",
    createOptions,
    new MenuItem("View", viewOptionsMenuFactory),
    new MenuItem("Delete", deleteOptionsMenuFactory));

var mainMenuCreator = new MenuFactory("Main menu",
    new MenuItem("New game", loadGameOptionsMenuFactory),
    new MenuItem("Load game", loadGameMenuFactory),
    new MenuItem("Delete game", deleteGameMenuFactory),
    new MenuItem("Options", optionsManagerMenuCreator));

var window = new ConsoleWindow("Checkers", 50, 20);
var mainMenu = mainMenuCreator.Create(window);

mainMenu.Run();
window.Close();