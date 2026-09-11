namespace Markwardt.ScriptUtilities.Tests;

/// <summary>
/// Creates a temporary directory for a test's file system operations and deletes it on disposal.
/// </summary>
public sealed class TemporaryDirectory : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemporaryDirectory"/> class, creating a fresh
    /// uniquely-named directory under the system temporary folder.
    /// </summary>
    public TemporaryDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "script-utilities-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    /// <summary>
    /// Gets the temporary directory's full path.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Deletes the temporary directory and everything inside it.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
