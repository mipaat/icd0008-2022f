using Common;
using ConsoleMenuSystem;
using ConsoleUI;
using DAL;
using DAL.Filters;
using Domain;
using Domain.Model;
using Domain.Model.Helpers;
using GameBrain;

namespace ConsoleClient;

public class MainMenuCreator
{
    private int _repoCtxIndex;

    public MainMenuCreator(params IRepositoryContextFactory[] repositoryContexts)
    {
        if (repositoryContexts.Length == 0) throw new ArgumentException("At least one repository context is required!");
        RepositoryContextFactories = repositoryContexts.ToList();

        MainMenuFactory = new MenuFactory("Main menu",
            new MenuItem("New game", CreateNewGameMenuFactory()),
            new MenuItem("Load game", CreateLoadGameMenuFactory()),
            new MenuItem("Options", CreateOptionsMenuFactory()));
    }

    private MenuFactory MainMenuFactory { get; }
    private List<IRepositoryContextFactory> RepositoryContextFactories { get; }
    private IRepositoryContextFactory RepoCtxFactory => RepositoryContextFactories[_repoCtxIndex];
    private IRepositoryContext GetRepoCtx() => RepoCtxFactory.CreateRepositoryContext();
    private int NextRepoCtxIndex => (_repoCtxIndex + 1) % RepositoryContextFactories.Count;
    private IRepositoryContextFactory NextRepoCtxFactory => RepositoryContextFactories[NextRepoCtxIndex];

    private CheckersRuleset? SelectedCheckersRuleset { get; set; }
    private CheckersGame? SelectedCheckersGame { get; set; }

    private string RepoChangeText => $"Change persistence engine? ({RepoCtxFactory.Name} => {NextRepoCtxFactory.Name})";

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
            using var repoCtx = GetRepoCtx();

            // When selected, a MenuItem from this list will create and run a new ConsoleGame using its CheckersRuleset
            var checkersRulesetItems = new List<MenuItem>();
            foreach (var checkersRuleset in repoCtx.CheckersRulesetRepository.GetAllSaved())
                checkersRulesetItems.Add(new MenuItem(checkersRuleset.ToString(), m =>
                {
                    SelectedCheckersRuleset = checkersRuleset;
                    return RunConsoleGame(m);
                }));

            checkersRulesetItems.Add(customBoardSize);

            return checkersRulesetItems;
        });
    }

    private MenuFactory CreateLoadGameMenuFactory()
    {
        var completionFilter = CompletionFilter.Default;
        var aiTypeFilter = AiTypeFilter.Default;
        var playerNameSearch = "";
        var rulesetSearch = "";
        return new MenuFactory("Load game",
            _ => GetCheckersGameMenuItems(
                completionFilter.FilterFunc,
                aiTypeFilter.FilterFunc,
                new FilterFunc<CheckersGame>(
                    iq => iq.Where(cg =>
                        Utils.StringAnyContain(playerNameSearch, cg.WhitePlayerName, cg.BlackPlayerName)),
                    false),
                new FilterFunc<CheckersGame>(
                    iq => iq.Where(cg =>
                        Utils.StringAnyContain(rulesetSearch, cg.CheckersRuleset!.ToString())),
                    false)
            ))
        {
            CustomHeaderFunc = () =>
            {
                var filterTexts = new List<string>
                {
                    $"[C] Completion: {completionFilter}",
                    $"[A] AI: {aiTypeFilter}",
                    "[P] " +
                    (playerNameSearch.Length == 0 ? "Search player name" : $"Player name: '{playerNameSearch}'"),
                    "[R] " + (rulesetSearch.Length == 0 ? "Search rulesets" : $"Ruleset search: '{rulesetSearch}'")
                };
                return "Press DELETE to delete. Filtering: " + string.Join(", ", filterTexts);
            },
            CustomBehaviour = (ConsoleInput input, Menu menu, ref List<MenuItem>? _) =>
            {
                if (input is { KeyInfo.Key: ConsoleKey.C })
                {
                    completionFilter = menu.GetSelectedItem(CompletionFilter.Values, "Select filtering mode:",
                        completionFilter);
                    menu.ClearMenuItemsCache();
                }
                else if (input is { KeyInfo.Key: ConsoleKey.A })
                {
                    aiTypeFilter = menu.GetSelectedItem(AiTypeFilter.Values, "Select AI filtering mode:", aiTypeFilter);
                    menu.ClearMenuItemsCache();
                } else if (input is { KeyInfo.Key: ConsoleKey.P })
                {
                    playerNameSearch = menu.ConsoleWindow.PopupPromptTextInput("Search by player name");
                    menu.ClearMenuItemsCache();
                } else if (input is { KeyInfo.Key: ConsoleKey.R })
                {
                    rulesetSearch = menu.ConsoleWindow.PopupPromptTextInput("Search by ruleset summary text");
                    menu.ClearMenuItemsCache();
                }
            }
        };
    }

    private List<MenuItem> GetCheckersGameMenuItems(params FilterFunc<CheckersGame>[] filters)
    {
        using var repoCtx = GetRepoCtx();
        var checkersGames = repoCtx.CheckersGameRepository.GetAll(filters);
        return GetCheckersGameMenuItems(checkersGames);
    }

    private List<MenuItem> GetCheckersGameMenuItems(IEnumerable<CheckersGame> checkersGames)
    {
        var result = new List<MenuItem>();
        foreach (var checkersGame in checkersGames)
            result.Add(new MenuItem(
                    GetCheckersGameText(checkersGame),
                    m =>
                    {
                        SelectedCheckersGame = checkersGame;
                        return RunConsoleGame(m);
                    }
                )
                { CustomCallBack = GetDeleteDbEntityMenuItemCallback(checkersGame) });

        return result;
    }

    private static string GetPlayerText(Player player)
    {
        var result = player.ToString();
        if (result.Any(char.IsWhiteSpace)) result = $"'{result.Replace('\'', '"')}'";
        return result;
    }

    private static string GetCheckersGameText(CheckersGame checkersGame)
    {
        var rulesetText = "(" + checkersGame.CheckersRuleset! + ")";
        var contents = new List<string>
        {
            checkersGame.Id.ToString(),
            GetPlayerText(checkersGame.BlackPlayer) + " VS " + GetPlayerText(checkersGame.WhitePlayer)
        };

        if (checkersGame.Tied)
        {
            contents.Add($"Tied at {checkersGame.EndedAt!.Value.ToLocalTime()}");
            contents.Add(rulesetText);
        }
        else if (checkersGame.Ended)
        {
            contents.Add($"{checkersGame.Winner} won at {checkersGame.EndedAt!.Value.ToLocalTime()}");
            contents.Add(rulesetText);
        }
        else
        {
            contents.Add(rulesetText);
            contents.Add($"Last played at {checkersGame.LastPlayed.ToLocalTime()}");
        }

        return string.Join(" | ", contents);
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

    private MenuItem GetEditableCheckersRulesetMenuItem(CheckersRuleset? checkersRuleset, string? menuItemText = null)
    {
        var checkersRulesetReal = checkersRuleset ?? new CheckersRuleset();
        return new MenuItem(
            menuItemText ??
            checkersRulesetReal + (checkersRulesetReal.BuiltIn ? " (Built-in)" : ""), m =>
            {
                var previousCursorPosition = 0;
                var result = EMenuFunction.Refresh;
                while (result == EMenuFunction.Refresh)
                {
                    var editCheckersRulesetsMenuFactory = new MenuFactory(
                        _ => checkersRulesetReal.BuiltIn
                            ? "Viewing built-in Checkers ruleset"
                            : "Editing custom Checkers ruleset",
                        _ =>
                        {
                            var rulesetMenuItems = new List<MenuItem>
                            {
                                new($"Game board width: {checkersRulesetReal.Width}", m2 =>
                                {
                                    checkersRulesetReal.Width = m2.ConsoleWindow.PopupPromptIntInput(
                                        "Enter game board width",
                                        CheckersRuleset.IsDimensionValid);
                                    return EMenuFunction.Continue;
                                }),
                                new($"Game board height: {checkersRulesetReal.Height}", m2 =>
                                {
                                    checkersRulesetReal.Height = m2.ConsoleWindow.PopupPromptIntInput(
                                        "Enter game board height",
                                        CheckersRuleset.IsDimensionValid);
                                    return EMenuFunction.Continue;
                                }),
                                new($"Black moves first: {checkersRulesetReal.BlackMovesFirst}", _ =>
                                {
                                    checkersRulesetReal.BlackMovesFirst = !checkersRulesetReal.BlackMovesFirst;
                                    return EMenuFunction.Continue;
                                }),
                                new($"Must capture piece if able: {checkersRulesetReal.MustCapture}", _ =>
                                {
                                    checkersRulesetReal.MustCapture = !checkersRulesetReal.MustCapture;
                                    return EMenuFunction.Continue;
                                }),
                                new($"Can capture backwards: {checkersRulesetReal.CanCaptureBackwards}", _ =>
                                {
                                    checkersRulesetReal.CanCaptureBackwards = !checkersRulesetReal.CanCaptureBackwards;
                                    return EMenuFunction.Continue;
                                }),
                                new(
                                    $"Can capture backwards during multi-capture: {checkersRulesetReal.CanCaptureBackwardsDuringMultiCapture}",
                                    _ =>
                                    {
                                        checkersRulesetReal.CanCaptureBackwardsDuringMultiCapture =
                                            !checkersRulesetReal.CanCaptureBackwardsDuringMultiCapture;
                                        return EMenuFunction.Continue;
                                    }),
                                new($"Kings can move multiple cells: {checkersRulesetReal.FlyingKings}",
                                    _ =>
                                    {
                                        checkersRulesetReal.FlyingKings =
                                            !checkersRulesetReal.FlyingKings;
                                        return EMenuFunction.Continue;
                                    }),
                                new("Ruleset title: " + checkersRulesetReal.Title, m2 =>
                                {
                                    checkersRulesetReal.Title =
                                        m2.ConsoleWindow.PopupPromptTextInput("Enter a title for your ruleset!");
                                    return EMenuFunction.Continue;
                                }),
                                new("Ruleset description: " + checkersRulesetReal.Description, m2 =>
                                {
                                    checkersRulesetReal.Description =
                                        m2.ConsoleWindow.PopupPromptTextInput(
                                            "You may enter a description for your ruleset");
                                    return EMenuFunction.Continue;
                                })
                            };

                            if (checkersRulesetReal.IsEditable)
                            {
                                rulesetMenuItems.Add(new MenuItem("Save", _ =>
                                {
                                    using var repoCtx = GetRepoCtx();
                                    repoCtx.CheckersRulesetRepository.Upsert(checkersRulesetReal);
                                    return EMenuFunction.Back;
                                }));
                                rulesetMenuItems.Add(new MenuItem("Delete", _ =>
                                {
                                    using var repoCtx = GetRepoCtx();
                                    repoCtx.CheckersRulesetRepository.Remove(checkersRulesetReal);
                                    return EMenuFunction.Back;
                                }));
                            }

                            rulesetMenuItems.Add(
                                GetEditableCheckersRulesetMenuItem(checkersRulesetReal.GetClone(), "Create a copy"));

                            return rulesetMenuItems;
                        }
                    );

                    var rulesetEditMenu = editCheckersRulesetsMenuFactory.Create(m.ConsoleWindow, m);

                    rulesetEditMenu.CursorPosition = previousCursorPosition;
                    result = rulesetEditMenu.Run();
                    previousCursorPosition = rulesetEditMenu.CursorPosition;
                }

                return result;
            })
        {
            CustomCallBack = !checkersRulesetReal.BuiltIn ? GetDeleteDbEntityMenuItemCallback(checkersRulesetReal) : null
        };
    }

    private MenuFactory CreateOptionsMenuFactory()
    {
        var manageCheckersRulesetsMenuFactory = new MenuFactory("Manage Checkers rulesets", _ =>
        {
            var checkersRulesets = new List<MenuItem>
                { GetEditableCheckersRulesetMenuItem(null, "CREATE NEW RULESET") };

            using var repoCtx = GetRepoCtx();
            foreach (var checkersRuleset in repoCtx.CheckersRulesetRepository.GetAllSaved())
            {
                checkersRulesets.Add(GetEditableCheckersRulesetMenuItem(checkersRuleset));
            }

            return checkersRulesets;
        })
        {
            CustomHeader = "Press DELETE to delete"
        };

        var swapPersistenceEngine = new MenuItem(RepoChangeText, (_, mi) =>
        {
            ChangeToNextRepoCtx();
            mi.Text = RepoChangeText;
            return EMenuFunction.Continue;
        });

        return new MenuFactory("Options", _ =>
        {
            var result = new List<MenuItem> { new("Manage Checkers rulesets", manageCheckersRulesetsMenuFactory) };
            if (RepositoryContextFactories.Count > 1) result.Add(swapPersistenceEngine);

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

        var selectPlayModeMenuFactory = new MenuFactory("Select playing mode", _ => menuItems)
        {
            IsExitable = false,
            AppendDefaultMenuItems = false
        };
        var selectPlayModeMenu = selectPlayModeMenuFactory.Create(parentMenu.ConsoleWindow, parentMenu);
        selectPlayModeMenu.Run();
        return result;
    }

    private static EAiType? GetPlayerType(string playerIdentifier, Menu menu)
    {
        return menu.GetSelectedEnumNullable<EAiType>($"Select {playerIdentifier} type", null, "Player");
    }

    private CustomMenuItemCallback GetDeleteDbEntityMenuItemCallback<T>(T entity)
        where T : class, IDatabaseEntity, new()
    {
        return (input, menu, _) =>
        {
            if (input is not { KeyInfo.Key: ConsoleKey.Delete }) return;
            var confirmDelete = ConfirmDelete(menu.ConsoleWindow, menu, $"{typeof(T).Name} with ID {entity.Id}");
            if (!confirmDelete) return;

            using (var repoCtx = GetRepoCtx())
            {
                var repo = repoCtx.GetRepo(entity);
                if (repo == null)
                {
                    menu.ConsoleWindow.PopUpMessageBox("Failed to delete item! (Failed to find repository)");
                    return;
                }

                try
                {
                    repo.Remove(entity);
                }
                catch (Exception e)
                {
                    menu.ConsoleWindow.PopUpMessageBox($"Failed to delete item! ({e.Message})");
                }
            }

            menu.ClearMenuItemsCache();
        };
    }

    private EMenuFunction RunConsoleGame(Menu menu)
    {
        try
        {
            ConsoleGame consoleGame;
            if (SelectedCheckersGame != null)
            {
                consoleGame = new ConsoleGame(menu.ConsoleWindow, new CheckersBrain(SelectedCheckersGame), RepoCtxFactory,
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
                using (var repoCtx = GetRepoCtx())
                {
                    repoCtx.CheckersGameRepository.Add(checkersBrain.CheckersGame);
                }
                consoleGame = new ConsoleGame(menu.ConsoleWindow,
                    checkersBrain,
                    RepoCtxFactory,
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

    public Menu CreateMainMenu(ConsoleWindow? window)
    {
        return MainMenuFactory.Create(window ?? new ConsoleWindow("Checkers"));
    }
}