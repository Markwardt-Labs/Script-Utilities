#:project ../Core/Core.csproj
#:property TreatWarningsAsErrors=true

// Applies formatting fixes and regenerates the coverage badge, leaving any changes staged in the working
// tree for review - run this and commit/push manually if there are changes, before running Publish. File-based
// app (dotnet run) - run from the repo root, e.g. `dotnet run Tasks/PrePublish.cs`.

using Markwardt.ScriptUtilities;

(await Script.Run("dotnet", true, "format")).Verify();
(await Script.Run("dotnet", false, "tool", "restore")).Verify();
Script.Delete("Tests/TestResults");

(await Script.Run("dotnet", true, "test", "Tests/Tests.csproj", "--configuration", "Debug", "--settings", "Tests/coverage.runsettings", "--collect:XPlat Code Coverage")).Verify();

(await Script.Run("dotnet", false, "reportgenerator", "-reports:Tests/TestResults/**/coverage.cobertura.xml", "-targetdir:.github/badges", "-reporttypes:Badges")).Verify();
Script.Delete("Tests/TestResults");

Script.Log("Pre-publish complete - review any formatting/badge changes and commit and push them yourself if there are any, then run Publish.");
