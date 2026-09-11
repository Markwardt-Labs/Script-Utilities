#:project ../Core/Core.csproj
#:property TreatWarningsAsErrors=true

// Creates the GitHub Release/tag for a version (locally, since a repo ruleset blocks GITHUB_TOKEN from
// creating tags), dispatches the publish workflow, waits for it to finish, and rolls the release/tag back
// if it fails - so a failed publish never leaves one behind. File-based app (dotnet run) instead of a task
// script since this needs real control flow (polling, conditional rollback) that stays identical on any OS
// with dotnet installed, unlike relying on the local shell (sh and cmd.exe don't share syntax for this).
// Run from the repo root, e.g. `dotnet run Tasks/Publish.cs`.

using Markwardt.ScriptUtilities;
using System.Text.Json;

string version = await Script.Query("Version to publish (e.g. 1.2.3)");
if (string.IsNullOrEmpty(version))
{
    Script.Error("A version is required.");
    return 1;
}

if ((await Script.Run("gh", true, "release", "create", version, "--title", version, "--generate-notes")).IsFailure)
{
    return 1;
}

if ((await Script.Run("gh", true, "workflow", "run", "csharp.yml", "-f", $"version={version}")).IsFailure)
{
    return await Rollback("Failed to dispatch the publish workflow");
}

DateTimeOffset dispatchedAt = DateTimeOffset.UtcNow;
long? runId = null;

for (int attempt = 0; attempt < 15 && runId is null; attempt++)
{
    await Script.Wait(2);

    RunResult list = await Script.Run("gh", false, "run", "list", "--workflow=csharp.yml", "--event=workflow_dispatch", "--limit=5", "--json", "databaseId,createdAt");
    if (list.IsFailure)
    {
        continue;
    }

    foreach (JsonElement run in JsonDocument.Parse(list.Output).RootElement.EnumerateArray())
    {
        if (run.GetProperty("createdAt").GetDateTimeOffset() >= dispatchedAt)
        {
            runId = run.GetProperty("databaseId").GetInt64();
            break;
        }
    }
}

if (runId is null)
{
    return await Rollback("Could not find the dispatched run");
}

if ((await Script.Run("gh", true, "run", "watch", runId.Value.ToString(), "--exit-status")).IsFailure)
{
    return await Rollback("Publish workflow failed");
}

Script.Log($"Published {version}.");
return 0;

async Task<int> Rollback(string reason)
{
    Script.Error($"{reason} - rolling back release {version}.");
    await Script.Run("gh", true, "release", "delete", version, "--yes", "--cleanup-tag");
    return 1;
}
