namespace Markwardt.ScriptUtilities;

/// <summary>
/// The result of running a program via <see cref="Script.Run(bool, string, string[])"/>.
/// </summary>
public sealed record RunResult
{
    /// <summary>
    /// The program's captured standard output.
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// The program's exit code.
    /// </summary>
    public required int ExitCode { get; init; }

    /// <summary>
    /// Whether the program exited successfully, i.e. <see cref="ExitCode"/> is zero.
    /// </summary>
    public bool IsSuccess => ExitCode == 0;

    /// <summary>
    /// Whether the program exited with a failure, i.e. <see cref="ExitCode"/> is non-zero.
    /// </summary>
    public bool IsFailure => ExitCode != 0;

    /// <summary>
    /// Throws an exception if the program exited with a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="IsFailure"/> is true.</exception>
    public void Verify()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException($"Command exited with code {ExitCode}.");
        }
    }
}
