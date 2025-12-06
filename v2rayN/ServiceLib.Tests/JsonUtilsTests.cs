using ServiceLib.Common;

namespace ServiceLib.Tests;

public class JsonUtilsTests
{
    public class TestModel
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    [Fact]
    public void Serialize_ShouldReturnValidJson()
    {
        var model = new TestModel { Name = "Test", Age = 25 };

        var json = JsonUtils.Serialize(model);

        Assert.NotNull(json);
        Assert.Contains("Test", json);
        Assert.Contains("25", json);
    }

    [Fact]
    public void Deserialize_ShouldReturnValidObject()
    {
        var json = "{\"Name\":\"Test\",\"Age\":25}";

        var model = JsonUtils.Deserialize<TestModel>(json);

        Assert.NotNull(model);
        Assert.Equal("Test", model.Name);
        Assert.Equal(25, model.Age);
    }

    [Fact]
    public void Deserialize_WithInvalidJson_ShouldReturnNull()
    {
        var json = "invalid json";

        var model = JsonUtils.Deserialize<TestModel>(json);

        Assert.Null(model);
    }

    [Fact]
    public void DeepCopy_ShouldCreateIndependentCopy()
    {
        var original = new TestModel { Name = "Original", Age = 30 };

        var copy = JsonUtils.DeepCopy(original);
        copy.Name = "Copy";

        Assert.Equal("Original", original.Name);
        Assert.Equal("Copy", copy.Name);
    }
}
