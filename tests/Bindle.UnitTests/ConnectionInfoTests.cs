using System;
using Xunit;

namespace Deislabs.Bindle.UnitTests;

public class ConnectionInfoTests
{
    [Fact]
    public void ShouldAcceptServerAliases()
    {
        Assert.Equal("", (new ConnectionInfo("").BaseUri));
        Assert.Equal("", (new ConnectionInfo("server=").BaseUri));
        Assert.Equal("localhost", (new ConnectionInfo("server=localhost").BaseUri));
        Assert.Equal("localhost", (new ConnectionInfo("host=localhost").BaseUri));
        Assert.Equal("localhost", (new ConnectionInfo("data source=localhost").BaseUri));
        Assert.Equal("localhost", (new ConnectionInfo("datasource=localhost").BaseUri));
        Assert.Equal("localhost", (new ConnectionInfo("address=localhost").BaseUri));
        Assert.Equal("localhost", (new ConnectionInfo("addr=localhost").BaseUri));
        Assert.Equal("localhost", (new ConnectionInfo("network address=localhost").BaseUri));
    }

    [Fact]
    public void ShouldAcceptSslModeAliases()
    {
        Assert.Null(new ConnectionInfo("").SslMode);
        Assert.Null(new ConnectionInfo("sslmode=").SslMode);
        Assert.Null(new ConnectionInfo("sslmode=doesnotexist").SslMode);
        Assert.Equal(SslMode.Disable, (new ConnectionInfo("sslmode=disable").SslMode));
        Assert.Equal(SslMode.Disable, (new ConnectionInfo("sslmode=Disable").SslMode));
        Assert.Equal(SslMode.Disable, (new ConnectionInfo("ssl mode=disable").SslMode));
        Assert.Equal(SslMode.Allow, (new ConnectionInfo("sslmode=allow").SslMode));
        Assert.Equal(SslMode.Prefer, (new ConnectionInfo("sslmode=prefer").SslMode));
        Assert.Equal(SslMode.Require, (new ConnectionInfo("sslmode=require").SslMode));
        Assert.Equal(SslMode.VerifyCA, (new ConnectionInfo("sslmode=verifyca").SslMode));
        Assert.Equal(SslMode.VerifyFull, (new ConnectionInfo("sslmode=verifyfull").SslMode));
    }

    [Fact]
    public void ShouldAcceptUserNameAliases()
    {
        Assert.Equal("", (new ConnectionInfo("").UserName));
        Assert.Equal("", (new ConnectionInfo("username=").UserName));
        Assert.Equal("spongebob", (new ConnectionInfo("username=spongebob").UserName));
        Assert.Equal("patrick", (new ConnectionInfo("user=patrick").UserName));
    }

    [Fact]
    public void ShouldAcceptPasswordAliases()
    {
        Assert.Equal("", (new ConnectionInfo("").Password));
        Assert.Equal("", (new ConnectionInfo("password=").Password));
        Assert.Equal("imagoofygooberyeah", (new ConnectionInfo("password=imagoofygooberyeah").Password));
        Assert.Equal("uragoofygooberyeah", (new ConnectionInfo("pass=uragoofygooberyeah").Password));
        Assert.Equal("wereallgoofygoobersyeah", (new ConnectionInfo("passwd=wereallgoofygoobersyeah").Password));
    }

    [Fact]
    public void ShouldAcceptMultipleOptions()
    {
        var connectionInfos = new ConnectionInfo[]
        {
            new ConnectionInfo("server=localhost;sslmode=verifyfull"),
            new ConnectionInfo("server=localhost; sslmode=verifyfull"),
            new ConnectionInfo(" server=localhost;sslmode=verifyfull"),
            new ConnectionInfo(" server=localhost; sslmode=verifyfull"),
            new ConnectionInfo("server=localhost;sslmode=verifyfull "),
            new ConnectionInfo(" server=localhost;sslmode=verifyfull "),
            new ConnectionInfo("server=localhost;;sslmode=verifyfull"),
            new ConnectionInfo("server=localhost;    sslmode=verifyfull"),
        };
        foreach (var connectionInfo in connectionInfos)
        {
            Assert.Equal("localhost", connectionInfo.BaseUri);
            Assert.Equal(SslMode.VerifyFull, connectionInfo.SslMode);
        }
    }

    [Fact]
    public void ShouldNotAcceptDuplicates()
    {
        Assert.Throws<ArgumentException>(() => new ConnectionInfo("sslmode=disable;sslmode=verifyfull"));
    }
}
