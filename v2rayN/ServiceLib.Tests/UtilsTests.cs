using ServiceLib.Common;

namespace ServiceLib.Tests;

public class UtilsTests
{
    [Theory]
    [InlineData("SGVsbG8gV29ybGQ=", true)]
    [InlineData("SGVsbG8gV29ybGQh", true)]
    [InlineData("not-base64!", false)]
    [InlineData("", false)]
    public void IsBase64String_ShouldReturnCorrectResult(string input, bool expected)
    {
        var result = Utils.IsBase64String(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("SGVsbG8gV29ybGQ=", "Hello World")]
    [InlineData("", "")]
    public void Base64Decode_ShouldDecodeCorrectly(string input, string expected)
    {
        var result = Utils.Base64Decode(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello World", "SGVsbG8gV29ybGQ=")]
    [InlineData("", "")]
    public void Base64Encode_ShouldEncodeCorrectly(string input, string expected)
    {
        var result = Utils.Base64Encode(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("172.16.0.1", true)]
    [InlineData("8.8.8.8", false)]
    [InlineData("1.1.1.1", false)]
    public void IsPrivateNetwork_ShouldIdentifyCorrectly(string ip, bool expected)
    {
        var result = Utils.IsPrivateNetwork(ip);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetGuid_ShouldReturnValidGuid()
    {
        var guid1 = Utils.GetGuid();
        var guid2 = Utils.GetGuid();

        Assert.NotEqual(guid1, guid2);
        Assert.True(Guid.TryParse(guid1, out _));
    }

    [Fact]
    public void GetGuid_ShortFormat_ShouldReturnNumericString()
    {
        var guid = Utils.GetGuid(false);

        Assert.DoesNotContain("-", guid);
        Assert.True(long.TryParse(guid, out _));
    }

    [Theory]
    [InlineData("test@example.com", "test%40example.com")]
    [InlineData("hello world", "hello%20world")]
    public void UrlEncode_ShouldEncodeCorrectly(string input, string expected)
    {
        var result = Utils.UrlEncode(input);
        Assert.Equal(expected, result);
    }
}
