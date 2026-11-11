using CapturaTool.Output;

namespace CapturaTool.Tests;

public class RetryTests
{
    private int _attempts;
    private int _waits;

    private void CountWait() => _waits++;

    [Fact]
    public void FirstAttemptWorks_DoesNotRepeat()
    {
        bool ok = Retry.Run(() => { _attempts++; return true; }, 10, CountWait, out int used);

        Assert.True(ok);
        Assert.Equal(1, _attempts);
        Assert.Equal(1, used);
        Assert.Equal(0, _waits);
    }

    [Fact]
    public void FailsTwiceThenWorks_StopsAtThirdAttempt()
    {
        bool ok = Retry.Run(() => { _attempts++; return _attempts == 3; }, 10, CountWait, out int used);

        Assert.True(ok);
        Assert.Equal(3, _attempts);
        Assert.Equal(3, used);
        Assert.Equal(2, _waits);
    }

    [Fact]
    public void AlwaysFails_TriesExactlyMaxAttempts()
    {
        bool ok = Retry.Run(() => { _attempts++; return false; }, 10, CountWait, out int used);

        Assert.False(ok);
        Assert.Equal(10, _attempts);
        Assert.Equal(10, used);
    }

    [Fact]
    public void AlwaysFails_DoesNotWaitAfterLastAttempt()
    {
        Retry.Run(() => false, 10, CountWait, out _);

        Assert.Equal(9, _waits);
    }
}
