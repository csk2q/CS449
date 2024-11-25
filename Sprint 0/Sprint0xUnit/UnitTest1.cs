using System.Diagnostics;

namespace Sprint0xUnit;

public class UnitTest1
{
    /* Tests from the tutorial */
    
    [Fact]
    public void PassingTest()
    {
        Assert.Equal(4, Add(2, 2));
    }

    [Fact]
    public void FailingTest()
    {
        Assert.Equal(5, Add(2, 2));
    }

    int Add(int x, int y)
    {
        return x + y;
    }
    
    /* My Tests*/
    
    [Fact]
    public void UnsafeAbsTest()
    {
        Assert.Equal(0, SafeAbs(0));
        Assert.Equal(1, SafeAbs(-1));
        Assert.Equal(1, SafeAbs(1));
        Assert.Equal(5, SafeAbs(-5));
        Assert.Equal(5, SafeAbs(5));
        Assert.Equal(99, SafeAbs(-99));
        Assert.Equal(99, SafeAbs(99));
        Assert.Equal(100, SafeAbs(-100));
        Assert.Equal(100, SafeAbs(100));
        Assert.Equal(int.MaxValue, SafeAbs(int.MaxValue));
        
        // ReSharper disable once MathAbsMethodIsRedundant
        // ReSharper disable once IntVariableOverflow
        Assert.Throws<OverflowException>(() => { _ = Math.Abs(int.MinValue); });
    }

    [Fact]
    public void SafeAbsTest()
    {
        Assert.Equal(0, SafeAbs(0));
        Assert.Equal(1, SafeAbs(-1));
        Assert.Equal(1, SafeAbs(1));
        Assert.Equal(5, SafeAbs(-5));
        Assert.Equal(5, SafeAbs(5));
        Assert.Equal(99, SafeAbs(-99));
        Assert.Equal(99, SafeAbs(99));
        Assert.Equal(100, SafeAbs(-100));
        Assert.Equal(100, SafeAbs(100));
        Assert.Equal(int.MaxValue, SafeAbs(int.MaxValue));
        Assert.Equal(int.MaxValue, SafeAbs(int.MinValue));
    }
    
    // Same as Math.Abs() but given int.MinValue returns int.MaxValue
    int SafeAbs(int x)
    {
        return x == int.MinValue ? int.MaxValue : Math.Abs(x);
    }

    [Theory]
    [InlineData('L')]
    [InlineData('R')]
    [InlineData('W')]
    public void IsCapitalTheory(char input)
    {
        Assert.True(IsCapital(input));
    }
    
    [Theory]
    [InlineData('c')]
    [InlineData('d')]
    [InlineData('x')]
    public void IsNotCapitalTheory(char input)
    {
        Assert.False(IsCapital(input));
    }

    bool IsCapital(char input)
    {
        // Simple implementation of char.IsUpper()
        return input >= 'A' && input <= 'Z';
    }
    
}