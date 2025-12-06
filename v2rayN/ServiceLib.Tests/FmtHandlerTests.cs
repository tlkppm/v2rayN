using ServiceLib.Handler.Fmt;
using ServiceLib.Enums;

namespace ServiceLib.Tests;

public class FmtHandlerTests
{
    [Theory]
    [InlineData("ss://YWVzLTI1Ni1nY206dGVzdEAxMjcuMC4wLjE6ODA4MA==#TestSS")]
    public void ResolveConfig_Shadowsocks_ShouldParse(string uri)
    {
        var result = FmtHandler.ResolveConfig(uri, out var msg);

        Assert.NotNull(result);
        Assert.Equal(EConfigType.Shadowsocks, result.ConfigType);
    }

    [Theory]
    [InlineData("vmess://eyJhZGQiOiIxMjcuMC4wLjEiLCJwb3J0Ijo4MDgwLCJpZCI6IjEyMzQ1Njc4LTEyMzQtMTIzNC0xMjM0LTEyMzQ1Njc4OTAxMiIsImFpZCI6MCwibmV0IjoidGNwIiwidHlwZSI6Im5vbmUiLCJ0bHMiOiIiLCJwcyI6IlRlc3RWTWVzcyJ9")]
    public void ResolveConfig_VMess_ShouldParse(string uri)
    {
        var result = FmtHandler.ResolveConfig(uri, out var msg);

        Assert.NotNull(result);
        Assert.Equal(EConfigType.VMess, result.ConfigType);
    }

    [Theory]
    [InlineData("trojan://password@127.0.0.1:443?sni=example.com#TestTrojan")]
    public void ResolveConfig_Trojan_ShouldParse(string uri)
    {
        var result = FmtHandler.ResolveConfig(uri, out var msg);

        Assert.NotNull(result);
        Assert.Equal(EConfigType.Trojan, result.ConfigType);
    }

    [Theory]
    [InlineData("vless://12345678-1234-1234-1234-123456789012@127.0.0.1:443?encryption=none&type=tcp#TestVLESS")]
    public void ResolveConfig_VLESS_ShouldParse(string uri)
    {
        var result = FmtHandler.ResolveConfig(uri, out var msg);

        Assert.NotNull(result);
        Assert.Equal(EConfigType.VLESS, result.ConfigType);
    }

    [Theory]
    [InlineData("invalid://uri")]
    [InlineData("")]
    [InlineData("random text")]
    public void ResolveConfig_InvalidUri_ShouldReturnNull(string uri)
    {
        var result = FmtHandler.ResolveConfig(uri, out var msg);

        Assert.Null(result);
    }

    [Fact]
    public void GetShareUri_Shadowsocks_ShouldGenerateValidUri()
    {
        var profileItem = new Models.ProfileItem
        {
            ConfigType = EConfigType.Shadowsocks,
            Address = "127.0.0.1",
            Port = 8080,
            Id = "password",
            Security = "aes-256-gcm",
            Remarks = "TestSS"
        };

        var uri = FmtHandler.GetShareUri(profileItem);

        Assert.NotNull(uri);
        Assert.StartsWith("ss://", uri);
    }
}
