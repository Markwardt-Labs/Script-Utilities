namespace Markwardt.ScriptUtilities.Tests;

public sealed class RunResultTests
{
    [Fact]
    public void IsSuccess_ZeroExitCode_ReturnsTrue()
    {
        RunResult result = new() { Output = string.Empty, ExitCode = 0 };

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void IsFailure_NonZeroExitCode_ReturnsTrue()
    {
        RunResult result = new() { Output = string.Empty, ExitCode = 1 };

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Verify_SuccessfulResult_DoesNotThrow() => new RunResult { Output = string.Empty, ExitCode = 0 }.Verify();

    [Fact]
    public void Verify_FailedResult_ThrowsInvalidOperationException()
    {
        RunResult result = new() { Output = string.Empty, ExitCode = 1 };

        Assert.Throws<InvalidOperationException>(result.Verify);
    }
}
