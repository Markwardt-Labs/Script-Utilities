namespace Markwardt.ScriptUtilities.Tests;

public sealed class ScriptTests
{
    [Fact]
    public async Task Run_SuccessfulCommand_ReturnsSuccessResultWithCapturedOutput()
    {
        RunResult result = await Script.Run("dotnet", false, "--version");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(0, result.ExitCode);
        Assert.Matches(@"\d+\.\d+", result.Output);
    }

    [Fact]
    public async Task Run_FailingCommand_ReturnsFailureResult()
    {
        RunResult result = await Script.Run("dotnet", false, "nosuchcommand12345");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public async Task Run_Stream_WritesOutputToConsole()
    {
        TextWriter original = Console.Out;
        StringWriter writer = new();
        try
        {
            Console.SetOut(writer);
            await Script.Run("dotnet", true, "--version");
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Matches(@"\d+\.\d+", writer.ToString());
    }

    [Fact]
    public async Task Run_NoStream_DoesNotWriteOutputToConsole()
    {
        TextWriter original = Console.Out;
        StringWriter writer = new();
        try
        {
            Console.SetOut(writer);
            await Script.Run("dotnet", false, "--version");
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void Log_WritesMessageToConsole()
    {
        TextWriter original = Console.Out;
        StringWriter writer = new();
        try
        {
            Console.SetOut(writer);
            Script.Log("hello there");
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Equal("hello there" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public void Error_WritesMessageToConsoleError()
    {
        TextWriter original = Console.Error;
        StringWriter writer = new();
        try
        {
            Console.SetError(writer);
            Script.Error("hello there");
        }
        finally
        {
            Console.SetError(original);
        }

        Assert.Equal("hello there" + Environment.NewLine, writer.ToString());
    }

    [Fact]
    public async Task Query_PromptsAndReturnsInput()
    {
        TextWriter originalOut = Console.Out;
        TextReader originalIn = Console.In;
        StringWriter writer = new();
        try
        {
            Console.SetOut(writer);
            Console.SetIn(new StringReader("an answer"));

            string result = await Script.Query("What?");

            Assert.Equal("an answer", result);
            Assert.Equal("What?: ", writer.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
        }
    }

    [Fact]
    public async Task Query_NoInput_ReturnsEmptyString()
    {
        TextWriter originalOut = Console.Out;
        TextReader originalIn = Console.In;
        try
        {
            Console.SetOut(new StringWriter());
            Console.SetIn(new StringReader(string.Empty));

            Assert.Equal(string.Empty, await Script.Query("What?"));
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetIn(originalIn);
        }
    }

    [Fact]
    public async Task Write_CreatesFileWithContent()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "out.txt");

        await Script.Write(path, "hello there");

        Assert.Equal("hello there", File.ReadAllText(path));
    }

    [Fact]
    public async Task Write_ExistingFile_IsReplaced()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "out.txt");
        File.WriteAllText(path, "old content that is longer");

        await Script.Write(path, "new");

        Assert.Equal("new", File.ReadAllText(path));
    }

    [Fact]
    public async Task Write_NoContent_CreatesEmptyFile()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "out.txt");

        await Script.Write(path);

        Assert.Equal(string.Empty, File.ReadAllText(path));
    }

    [Fact]
    public async Task Write_MissingIntermediateDirectories_AreCreated()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "nested", "deep", "out.txt");

        await Script.Write(path, "content");

        Assert.Equal("content", File.ReadAllText(path));
    }

    [Fact]
    public async Task Append_MissingFile_CreatesItWithContent()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "out.txt");

        await Script.Append(path, "hello");

        Assert.Equal("hello", File.ReadAllText(path));
    }

    [Fact]
    public async Task Append_ExistingFile_AddsToEnd()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "out.txt");
        File.WriteAllText(path, "hello ");

        await Script.Append(path, "world");

        Assert.Equal("hello world", File.ReadAllText(path));
    }

    [Fact]
    public async Task Read_ExistingFile_ReturnsContent()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "in.txt");
        File.WriteAllText(path, "file contents");

        Assert.Equal("file contents", await Script.Read(path));
    }

    [Fact]
    public async Task Read_MissingFile_ReturnsNull()
    {
        using TemporaryDirectory temp = new();

        Assert.Null(await Script.Read(Path.Combine(temp.Path, "missing.txt")));
    }

    [Fact]
    public void Folder_MissingFolder_CreatesIt()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "nested", "child");

        Script.Folder(path);

        Assert.True(Directory.Exists(path));
    }

    [Fact]
    public void Folder_ExistingFolder_LeavesContentsInPlace()
    {
        using TemporaryDirectory temp = new();
        string path = Path.Combine(temp.Path, "existing");
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "keep.txt"), "keep me");

        Script.Folder(path);

        Assert.True(File.Exists(Path.Combine(path, "keep.txt")));
    }

    [Fact]
    public void Delete_MissingPath_DoesNotThrow()
    {
        using TemporaryDirectory temp = new();
        Script.Delete(Path.Combine(temp.Path, "missing.txt"));
    }

    [Fact]
    public void Delete_ExistingFolderWithContents_RemovesEverything()
    {
        using TemporaryDirectory temp = new();
        string folder = Path.Combine(temp.Path, "folder");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "file.txt"), "x");

        Script.Delete(folder);

        Assert.False(Directory.Exists(folder));
    }

    [Fact]
    public void Clean_RemovesContentsButKeepsFolder()
    {
        using TemporaryDirectory temp = new();
        string folder = Path.Combine(temp.Path, "folder");
        Directory.CreateDirectory(Path.Combine(folder, "sub"));
        File.WriteAllText(Path.Combine(folder, "file.txt"), "x");

        Script.Clean(folder);

        Assert.True(Directory.Exists(folder));
        Assert.Empty(Directory.GetFileSystemEntries(folder));
    }

    [Fact]
    public void Clean_MissingFolder_DoesNotThrow()
    {
        using TemporaryDirectory temp = new();
        Script.Clean(Path.Combine(temp.Path, "missing"));
    }

    [Fact]
    public void Move_CreatesIntermediateDirectories()
    {
        using TemporaryDirectory temp = new();
        string source = Path.Combine(temp.Path, "source.txt");
        string destination = Path.Combine(temp.Path, "nested", "deep", "target.txt");
        File.WriteAllText(source, "source content");

        Script.Move(source, destination);

        Assert.False(File.Exists(source));
        Assert.Equal("source content", File.ReadAllText(destination));
    }

    [Fact]
    public void Move_ExistingDestination_IsReplaced()
    {
        using TemporaryDirectory temp = new();
        string source = Path.Combine(temp.Path, "source.txt");
        string destination = Path.Combine(temp.Path, "target.txt");
        File.WriteAllText(source, "source content");
        File.WriteAllText(destination, "old content");

        Script.Move(source, destination);

        Assert.Equal("source content", File.ReadAllText(destination));
    }

    [Fact]
    public void Move_MissingSource_ThrowsInvalidOperationException()
    {
        using TemporaryDirectory temp = new();
        Assert.Throws<InvalidOperationException>(() =>
            Script.Move(Path.Combine(temp.Path, "missing.txt"), Path.Combine(temp.Path, "target.txt")));
    }

    [Fact]
    public void Copy_LeavesSourceInPlace()
    {
        using TemporaryDirectory temp = new();
        string source = Path.Combine(temp.Path, "source.txt");
        string destination = Path.Combine(temp.Path, "copies", "target.txt");
        File.WriteAllText(source, "source content");

        Script.Copy(source, destination);

        Assert.Equal("source content", File.ReadAllText(source));
        Assert.Equal("source content", File.ReadAllText(destination));
    }

    [Fact]
    public void Copy_Folder_CopiesRecursively()
    {
        using TemporaryDirectory temp = new();
        string source = Path.Combine(temp.Path, "source");
        string destination = Path.Combine(temp.Path, "target");
        Directory.CreateDirectory(Path.Combine(source, "sub"));
        File.WriteAllText(Path.Combine(source, "a.txt"), "a");
        File.WriteAllText(Path.Combine(source, "sub", "b.txt"), "b");

        Script.Copy(source, destination);

        Assert.Equal("a", File.ReadAllText(Path.Combine(destination, "a.txt")));
        Assert.Equal("b", File.ReadAllText(Path.Combine(destination, "sub", "b.txt")));
    }

    [Fact]
    public void Rename_ChangesNameWithinSameParentDirectory()
    {
        using TemporaryDirectory temp = new();
        string parent = Path.Combine(temp.Path, "parent");
        Directory.CreateDirectory(parent);
        File.WriteAllText(Path.Combine(parent, "old.txt"), "content");

        Script.Rename(Path.Combine(parent, "old.txt"), "new.txt");

        Assert.False(File.Exists(Path.Combine(parent, "old.txt")));
        Assert.Equal("content", File.ReadAllText(Path.Combine(parent, "new.txt")));
    }

    [Fact]
    public void Switch_ChangesCurrentDirectory()
    {
        string original = Environment.CurrentDirectory;
        using TemporaryDirectory temp = new();
        try
        {
            Script.Switch(temp.Path);

            Assert.Equal(Path.GetFullPath(temp.Path), Environment.CurrentDirectory);
        }
        finally
        {
            Environment.CurrentDirectory = original;
        }
    }
}
