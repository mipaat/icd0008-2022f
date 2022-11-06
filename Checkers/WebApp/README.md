~~~bash
dotnet aspnet-codegenerator razorpage -m CheckersGame -dc AppDbContext -udl -outDir Pages/CheckersGames --referenceScriptLibraries
dotnet aspnet-codegenerator razorpage -m CheckersRuleset -dc AppDbContext -udl -outDir Pages/CheckersRulesets --referenceScriptLibraries
dotnet aspnet-codegenerator razorpage -m CheckersState -dc AppDbContext -udl -outDir Pages/CheckersStates --referenceScriptLibraries
~~~