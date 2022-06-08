using Xunit;

namespace Deislabs.Bindle.UnitTests;

public class TomlNamingHelperTests
{
    [Fact]
    public void ShouldConvertToCamelCase()
    {
        Assert.Equal("helloWorld", TomlNamingHelper.PascalToCamelCase("HelloWorld"));
        Assert.Equal("helloWorld", TomlNamingHelper.PascalToCamelCase("helloWorld"));
        Assert.Equal("t", TomlNamingHelper.PascalToCamelCase("T"));
    }
}
