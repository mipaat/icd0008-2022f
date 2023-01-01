using ConsoleMenuSystem;
using ConsoleUI;
using DAL;
using Domain;
using GameBrain;

namespace ConsoleClient;

public class MainMenuCreator
{
    private MenuFactory MainMenuFactory { get; }
    private List<IRepositoryContext> RepositoryContexts { get; }
    private int _repoCtxIndex;
    private IRepositoryContext RepoCtx => RepositoryContexts[_repoCtxIndex];
    private int NextRepoCtxIndex => (_repoCtxIndex + 1) % RepositoryContexts.Count;
    private IRepositoryContext NextRepoCtx => RepositoryContexts[NextRepoCtxIndex];

    private CheckersRuleset? SelectedCheckersRuleset { get; set; }
    private CheckersGame? SelectedCheckersGame { get; set; }

    public MainMenuCreator(params IRepositoryContext[] repositoryContexts)
    {
        if (repositoryContexts.Length == 0) throw new ArgumentException("At least one repository context is required!");
        RepositoryContexts = repositoryContexts.ToList();

        MainMenuFactory = new MenuFactory("Main menu",
            new MenuItem("New game", CreateNewGameMenuFactory()),
            new MenuItem("Load game", CreateLoadGameMenuFactory()),
            new MenuItem("Options", CreateOptionsMenuFactory()));
    }

    private MenuFactory CreateNewGameMenuFactory()
    {
        var customBoardSize = new MenuItem("Custom board size", menu =>
        {
            var consoleWindow = menu.ConsoleWindow;

            var x = consoleWindow.PopupPromptIntInput("Enter board horizontal dimension:",
                CheckersRuleset.IsDimensionValid);
            var y = consoleWindow.PopupPromptIntInput("Enter board vertical dimension:",
                CheckersRuleset.IsDimensionValid);

            SelectedCheckersRuleset = new CheckersRuleset { Width = x, Height = y, Title = $"{x}x{y}", Saved = false };
            return RunConsoleGame(menu);
        });

        return new MenuFactory("Pick a Checkers ruleset", _ =>
        {
            // When selected, a MenuItem from this list will create and run a new ConsoleGame using its CheckersRuleset
            var checkersRulesetItems = new List<MenuItem>();
            foreach (var checkersRuleset in RepoCtx.CheckersRulesetRepository.GetAllSaved())
            {
                checkersRulesetItems.Add(new MenuItem(checkersRuleset.TitleText, m =>
                {
                    SelectedCheckersRuleset = checkersRuleset;
                    return RunConsoleGame(m);
                }));
            }

            checkersRulesetItems.Add(customBoardSize);

            return checkersRulesetItems;
        });
    }

    private MenuFactory CreateLoadGameMenuFactory()
    {
        return new MenuFactory("Load game", _ =>
        {
            var gamesToLoad = new List<MenuItem>();
            foreach (var checkersGame in RepoCtx.CheckersGameRepository.GetAll())
            {
                gamesToLoad.Add(new MenuItem(
                        $"{checkersGame.Id} | {checkersGame.CheckersRuleset!.Title} - Started: {checkersGame.StartedAt.ToLocalTime()}, Last played: {checkersGame.CurrentCheckersState?.CreatedAt.ToLocalTime() ?? checkersGame.StartedAt.ToLocalTime()}",
                        m =>
                        {
                            SelectedCheckersGame = checkersGame;
                            return RunConsoleGame(m);
                        })
                    {
                        CustomCallBack = (input, menu, _) =>
                        {
                            if (input is not { KeyInfo.Key: ConsoleKey.Delete }) return;
                            var confirmDelete = ConfirmDelete(menu.ConsoleWindow, menu, $"CheckersGame with ID {checkersGame.Id}");
                            if (!confirmDelete) return;
                            RepoCtx.CheckersGameRepository.Remove(checkersGame);
                            menu.ClearMenuItemsCache();
                        }
                    }
                );
            }

            return gamesToLoad;
        })
        {
            CustomHeader = "Press DELETE to delete"
        };
    }

    private static bool ConfirmDelete(ConsoleWindow consoleWindow, Menu? parentMenu,
        string? deleteItemDisplayString = null)
    {
        var result = false;
        var menuFactory = new MenuFactory(
                "Are you sure you want to delete " + (deleteItemDisplayString ?? "this") + "?",
                new MenuItem("Yes", () =>
                {
                    result = true;
                    return EMenuFunction.Back;
                }),
                new MenuItem("No", () =>
                {
                    result = false;
                    return EMenuFunction.Back;
                }))
            { AppendDefaultMenuItems = false };
        var menu = menuFactory.Create(consoleWindow, parentMenu);
        menu.Run();
        return result;
    }

    private MenuItem GetEditableCheckersRulesetMenuItem(CheckersRuleset checkersRuleset, string? menuItemText = null)
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
                                    checkersRuleset.Width = m2.ConsoleWindow.PopupPromptIntInput(
                                        "Enter game board width",
                                        CheckersRuleset.IsDimensionValid);
                                    return EMenuFunction.Continue;
                                }),
                                new($"Game board height: {checkersRuleset.Height}", m2 =>
                                {
                                    checkersRuleset.Height = m2.ConsoleWindow.PopupPromptIntInput(
                                        "Enter game board height",
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
                                new(
                                    $"Can capture backwards during multi-capture: {checkersRuleset.CanCaptureBackwardsDuringMultiCapture}",
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
                                    RepoCtx.CheckersRulesetRepository.Upsert(checkersRuleset);
                                    return EMenuFunction.Back;
                                }));
                                rulesetMenuItems.Add(new MenuItem("Delete", _ =>
                                {
                                    RepoCtx.CheckersRulesetRepository.Remove(checkersRuleset);
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

    private MenuFactory CreateOptionsMenuFactory()
    {
        var manageCheckersRulesetsMenuFactory = new MenuFactory("Manage Checkers rulesets", _ =>
        {
            var checkersRulesets = new List<MenuItem>
                { GetEditableCheckersRulesetMenuItem(new CheckersRuleset(), "CREATE NEW RULESET") };

            foreach (var checkersRuleset in RepoCtx.CheckersRulesetRepository.GetAllSaved())
            {
                RepoCtx.CheckersRulesetRepository.Refresh(checkersRuleset); // TODO: figure out a better way to do this
                checkersRulesets.Add(GetEditableCheckersRulesetMenuItem(checkersRuleset));
            }

            return checkersRulesets;
        });

        var swapPersistenceEngine = new MenuItem(RepoChangeText, (_, mi) =>
        {
            ChangeToNextRepoCtx();
            mi.Text = RepoChangeText;
            return EMenuFunction.Continue;
        });

        return new MenuFactory("Options", _ =>
        {
            var result = new List<MenuItem> { new("Manage Checkers rulesets", manageCheckersRulesetsMenuFactory) };
            if (RepositoryContexts.Count > 1)
            {
                result.Add(swapPersistenceEngine);
            }

            return result;
        });
    }

    private static EPlayerColor? GetPlayMode(Menu parentMenu, CheckersGame checkersGame)
    {
        EPlayerColor? result = null;
        if (checkersGame.BlackAiType != null || checkersGame.WhiteAiType != null || checkersGame.Ended)
            return result;
        var menuItems = new List<MenuItem>
        {
            new("Play as both in shared window", _ =>
            {
                result = null;
                return EMenuFunction.Exit;
            }),
            new("Play as black", _ =>
            {
                result = EPlayerColor.Black;
                return EMenuFunction.Exit;
            }),
            new("Play as white", _ =>
            {
                result = EPlayerColor.White;
                return EMenuFunction.Exit;
            })
        };

        var selectPlayModeMenuFactory = new MenuFactory($"Select playing mode", _ => menuItems)
        {
            IsExitable = false,
            AppendDefaultMenuItems = false
        };
        var selectPlayModeMenu = selectPlayModeMenuFactory.Create(parentMenu.ConsoleWindow, parentMenu);
        selectPlayModeMenu.Run();
        return result;
    }

    private static EAiType? GetPlayerType(string playerIdentifier, Menu parentMenu)
    {
        EAiType? result = null;
        var menuItems = new List<MenuItem> { new("Player", EMenuFunction.Exit) };
        foreach (var enumValue in Enum.GetValues(typeof(EAiType)))
        {
            menuItems.Add(new MenuItem(enumValue.ToString() ?? "???", _ =>
            {
                result = (EAiType)enumValue;
                return EMenuFunction.Exit;
            }));
        }

        var selectAiMenuFactory = new MenuFactory(
            $"Select {playerIdentifier} type", _ => menuItems)
        {
            IsExitable = false,
            AppendDefaultMenuItems = false
        };
        var selectAiMenu = selectAiMenuFactory.Create(parentMenu.ConsoleWindow, parentMenu);
        selectAiMenu.Run();
        return result;
    }

    EMenuFunction RunConsoleGame(Menu menu)
    {
        try
        {
            ConsoleGame consoleGame;
            if (SelectedCheckersGame != null)
            {
                consoleGame = new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(SelectedCheckersGame), RepoCtx,
                    GetPlayMode(menu, SelectedCheckersGame));
            }
            else
            {
                var whitePlayerAiType = GetPlayerType("white player", menu);
                var whitePlayerName =
                    menu.ConsoleWindow.PopupPromptTextInput("Please enter a name for the white player:");
                var blackPlayerAiType = GetPlayerType("black player", menu);
                var blackPlayerName =
                    menu.ConsoleWindow.PopupPromptTextInput("Please enter a name for the black player:");
                var checkersBrain = new CheckersBrain(
                    SelectedCheckersRuleset?.GetClone(false) ?? new CheckersRuleset(),
                    whitePlayerName,
                    blackPlayerName,
                    whitePlayerAiType,
                    blackPlayerAiType);
                RepoCtx.CheckersGameRepository.Add(checkersBrain.CheckersGame);
                consoleGame = new ConsoleGame(menu.ConsoleWindow,
                    checkersBrain,
                    RepoCtx,
                    GetPlayMode(menu, checkersBrain.CheckersGame));
            }

            consoleGame.Run();
        }
        catch (Exception e)
        {
            menu.ConsoleWindow.PopUpMessageBox("An error occurred when attempting to run Checkers game!",
                e.Message);
        }
        finally
        {
            SelectedCheckersRuleset = null;
            SelectedCheckersGame = null;
        }

        return EMenuFunction.MainMenu;
    }

    private void ChangeToNextRepoCtx()
    {
        _repoCtxIndex = NextRepoCtxIndex;
    }

    private string RepoChangeText => $"Change persistence engine? ({RepoCtx.Name} => {NextRepoCtx.Name})";

    public Menu CreateMainMenu(ConsoleWindow? window)
    {
        return MainMenuFactory.Create(window ?? new ConsoleWindow("Checkers"));
    }
}