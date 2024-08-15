using System.Text.Json;

namespace LgkProduction_Geo.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        string json = "{\"ZoomBounds\": {\"Min\": 0,\"Max\": 19}}";
        var settings = JsonSerializer.Deserialize<TestSettings>(json);
        Assert.That(settings.ZoomBounds.Max, Is.EqualTo(19));
    }
}