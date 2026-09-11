namespace Markwardt.ScriptUtilities;

/// <summary>
/// Static, console-based utility methods for single-file C# script programs: the file, process, and
/// console helpers ad hoc automation scripts need.
/// </summary>
public static class Script
{
    /// <summary>
    /// Pauses execution for the given number of seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds to pause for.</param>
    /// <returns>A task that completes once the pause has elapsed.</returns>
    public static async Task Wait(double seconds) => await Task.Delay(TimeSpan.FromSeconds(seconds));

    /// <summary>
    /// Runs a program directly, without an intermediate shell, with the given arguments, in the current
    /// working directory. Arguments are passed through exactly as given, with no shell quoting, wildcard
    /// expansion, or piping, keeping behavior consistent across operating systems.
    /// </summary>
    /// <param name="fileName">The program to run.</param>
    /// <param name="stream">If true, also streams the program's standard output to the console as it runs, in addition to capturing it.</param>
    /// <param name="arguments">The program's arguments.</param>
    /// <returns>
    /// The program's result, including its captured standard output and exit code. A non-zero exit code is not
    /// treated as a failure and does not throw - check <see cref="RunResult.IsFailure"/> to detect that.
    /// </returns>
    /// <exception cref="InvalidOperationException">The program could not be started.</exception>
    public static async Task<RunResult> Run(string fileName, bool stream = true, params string[] arguments)
    {
        ProcessStartInfo startInfo = new(fileName) { UseShellExecute = false, RedirectStandardOutput = true };
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start '{fileName}'.");

        StringBuilder output = new();
        Task readOutput = ReadOutput(process, stream, output);
        await Task.WhenAll(process.WaitForExitAsync(), readOutput);

        return new RunResult { Output = output.ToString(), ExitCode = process.ExitCode };
    }

    /// <summary>
    /// Writes a message to the console.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public static void Log(string message) => Console.WriteLine(message);

    /// <summary>
    /// Writes a message to the console's error output.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public static void Error(string message) => Console.Error.WriteLine(message);

    /// <summary>
    /// Deletes the file or folder at the given path, including all of its contents if it is a folder. Does
    /// nothing if nothing exists at the path.
    /// </summary>
    /// <param name="path">The path to delete.</param>
    public static void Delete(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Deletes every file and folder inside the given folder, leaving the folder itself in place. Does
    /// nothing if the folder doesn't exist.
    /// </summary>
    /// <param name="path">The folder to empty.</param>
    public static void Clean(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (string filePath in Directory.GetFiles(path))
        {
            File.Delete(filePath);
        }

        foreach (string directoryPath in Directory.GetDirectories(path))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    /// <summary>
    /// Changes the current process's working directory.
    /// </summary>
    /// <param name="path">The folder to switch to.</param>
    public static void Switch(string path) => Environment.CurrentDirectory = Path.GetFullPath(path);

    /// <summary>
    /// Moves a file or folder to a new path, creating any missing intermediate directories. If something
    /// already exists at the destination, it is deleted first.
    /// </summary>
    /// <param name="source">The existing file or folder to move.</param>
    /// <param name="destination">The path to move it to.</param>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> does not exist.</exception>
    public static void Move(string source, string destination)
    {
        if (!Directory.Exists(source) && !File.Exists(source))
        {
            throw new InvalidOperationException($"Cannot move '{source}' because it does not exist.");
        }

        CreateParentFolder(destination);
        Delete(destination);

        if (Directory.Exists(source))
        {
            Directory.Move(source, destination);
        }
        else
        {
            File.Move(source, destination);
        }
    }

    /// <summary>
    /// Copies a file or folder (recursively) to a new path, creating any missing intermediate directories.
    /// If something already exists at the destination, it is deleted first. The source is left in place.
    /// </summary>
    /// <param name="source">The existing file or folder to copy.</param>
    /// <param name="destination">The path to copy it to.</param>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> does not exist.</exception>
    public static void Copy(string source, string destination)
    {
        if (!Directory.Exists(source) && !File.Exists(source))
        {
            throw new InvalidOperationException($"Cannot copy '{source}' because it does not exist.");
        }

        CreateParentFolder(destination);
        Delete(destination);

        if (Directory.Exists(source))
        {
            CopyFolder(source, destination);
        }
        else
        {
            File.Copy(source, destination);
        }
    }

    /// <summary>
    /// Renames a file or folder in place, within its existing parent directory. If something already
    /// exists at the resulting path, it is deleted first.
    /// </summary>
    /// <param name="path">The existing file or folder to rename.</param>
    /// <param name="newName">The new name to give it.</param>
    public static void Rename(string path, string newName)
    {
        string parent = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
        Move(path, Path.Combine(parent, newName));
    }

    /// <summary>
    /// Creates a file with the given content, creating any missing intermediate directories. If a file or
    /// folder already exists at the path, it is deleted and replaced.
    /// </summary>
    /// <param name="path">The path of the file to create.</param>
    /// <param name="content">The file's content.</param>
    /// <returns>A task that completes once the file has been written.</returns>
    public static async Task Write(string path, string content = "")
    {
        CreateParentFolder(path);
        Delete(path);
        await File.WriteAllTextAsync(path, content);
    }

    /// <summary>
    /// Appends content to the end of the file at the path, creating any missing intermediate directories.
    /// If the file doesn't exist, it is created with that content.
    /// </summary>
    /// <param name="path">The path of the file to append to.</param>
    /// <param name="content">The content to append.</param>
    /// <returns>A task that completes once the content has been appended.</returns>
    public static async Task Append(string path, string content)
    {
        CreateParentFolder(path);
        await File.AppendAllTextAsync(path, content);
    }

    /// <summary>
    /// Creates a folder at the path, including any missing intermediate directories. Does nothing if a
    /// folder already exists there.
    /// </summary>
    /// <param name="path">The path of the folder to create.</param>
    public static void Folder(string path) => Directory.CreateDirectory(path);

    /// <summary>
    /// Reads a file's content.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <returns>The file's content, or <see langword="null"/> if it doesn't exist.</returns>
    public static async Task<string?> Read(string path) => File.Exists(path) ? await File.ReadAllTextAsync(path) : null;

    /// <summary>
    /// Writes a message to the console and returns a line of text read back from the console.
    /// </summary>
    /// <param name="message">The message to prompt with.</param>
    /// <returns>The line of text read from the console, or an empty string if none was available.</returns>
    public static async Task<string> Query(string message)
    {
        Console.Write($"{message}: ");
        return await Console.In.ReadLineAsync() ?? string.Empty;
    }

    private static async Task ReadOutput(Process process, bool stream, StringBuilder output)
    {
        string? line;
        while ((line = await process.StandardOutput.ReadLineAsync()) is not null)
        {
            output.AppendLine(line);
            if (stream)
            {
                Console.WriteLine(line);
            }
        }
    }

    private static void CreateParentFolder(string path)
    {
        string? parent = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(parent))
        {
            Directory.CreateDirectory(parent);
        }
    }

    private static void CopyFolder(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (string filePath in Directory.GetFiles(source))
        {
            File.Copy(filePath, Path.Combine(destination, Path.GetFileName(filePath)));
        }

        foreach (string directoryPath in Directory.GetDirectories(source))
        {
            CopyFolder(directoryPath, Path.Combine(destination, Path.GetFileName(directoryPath)));
        }
    }
}
