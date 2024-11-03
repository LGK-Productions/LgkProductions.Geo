using System.Text.Json;
using LgkProductions.Geo;

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
    
    [Test]
    public void StringToTileId()
    {
        string success = "( 0,  1, 2)";
        string fail = "0,  1, #2)";
        var TileId = (TileId)success;
        Assert.That(TileId, Is.EqualTo(new TileId(0, 1, 2)));
        Assert.Catch(typeof(FormatException), () =>
        {
            var res = (TileId)fail;
        });
    }
    
    [Test]
    public void StringToBounds()
    {
        string successInt = "(1, 2)";
        string successfloat = "(1.2, 2.5)";
        string fail = "0,  1, #2)";
        var boundsi = (Bounds<int>)successInt;
        var boundsf = (Bounds<float>)successfloat;
        Assert.That(boundsi, Is.EqualTo(new Bounds<int>(1, 2)));
        Assert.That(boundsf, Is.EqualTo(new Bounds<float>(1.2f, 2.5f)));
        Assert.Catch(typeof(FormatException), () =>
        {
            var res = (Bounds<int>)fail;
        });
    }
    
    [Test]
    public void StringToGlobePoint()
    {
        string success1 = "(1, 2)";
        string success2 = "(1.2, 2.5, 350)";
        string fail = "0,  1, #2)";
        Assert.That((GlobePoint)success1, Is.EqualTo(new GlobePoint(1, 2)));
        Assert.That((GlobePoint)success2, Is.EqualTo(new GlobePoint(1.2d, 2.5d, 350)));
        Assert.Catch(typeof(FormatException), () =>
        {
            var res = (GlobePoint)fail;
        });
    }
    
    [Test]
    public void StringToGlobeArea()
    {
        string success1 = "((1, 2), (3, 4))";
        string success2 = "((1.2, 2.5, 350), (0.5, 1.5, 350))";
        string fail = "((), ())";
        Assert.That((GlobeArea)success1, Is.EqualTo(new GlobeArea(new GlobePoint(1, 2), new GlobePoint(3, 4))));
        Assert.That((GlobeArea)success2, Is.EqualTo(new GlobeArea(new GlobePoint(0.5, 1.5d, 350), new GlobePoint(1.2d, 2.5d))));
        Assert.Catch(typeof(FormatException), () =>
        {
            var res = (GlobePoint)fail;
        });
    }
}