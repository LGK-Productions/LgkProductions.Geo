using System.Text.Json;
using LgkProductions.Geo;
using LgkProductions.Geo.Projection;

namespace LgkProduction_Geo.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GlobePointEdgeTest()
    {
        var proj = new WebMercatorProjection();
        
        var gb1 = new GlobePoint(90, 180);
        var gb2 = new GlobePoint(90, -180);
        var gb3 = new GlobePoint(-90, 180);
        var gb4 = new GlobePoint(-90, -180);

        var tc1 = proj.GlobePointToTileCoordinates(gb1, 1);
        var tc2 = proj.GlobePointToTileCoordinates(gb2, 1);
        var tc3 = proj.GlobePointToTileCoordinates(gb3, 1);
        var tc4 = proj.GlobePointToTileCoordinates(gb4, 1);
        Assert.Multiple(() =>
        {
            Assert.That(tc1, Is.EqualTo(new TileCoordinate(1, 0)));
            Assert.That(tc2, Is.EqualTo(new TileCoordinate(0, 0)));
            Assert.That(tc3, Is.EqualTo(new TileCoordinate(1, 1)));
            Assert.That(tc4, Is.EqualTo(new TileCoordinate(0, 1)));
        });
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
        var id = TileId.Parse(success);
        Assert.That(id, Is.EqualTo(new TileId(0, 1, 2)));
        Assert.Catch(typeof(FormatException), () =>
        {
            var res = TileId.Parse(fail);
        });
    }
    
    [Test]
    public void StringToBounds()
    {
        string successInt = "(1, 2)";
        string successfloat = "(1.2, 2.5)";
        string fail = "0,  1, #2)";
        var boundsi = Bounds<int>.Parse(successInt);
        var boundsf = Bounds<float>.Parse(successfloat);
        Assert.That(boundsi, Is.EqualTo(new Bounds<int>(1, 2)));
        Assert.That(boundsf, Is.EqualTo(new Bounds<float>(1.2f, 2.5f)));
        Assert.Catch(typeof(FormatException), () =>
        {
            var res = Bounds<int>.Parse(fail);
        });
    }
    
    [Test]
    public void StringToGlobePoint()
    {
        string success1 = "(1, 2)";
        string success2 = "(1.2, 2.5, 350)";
        string fail = "0,  1, #2)";
        Assert.That(GlobePoint.Parse(success1), Is.EqualTo(new GlobePoint(1, 2)));
        Assert.That(GlobePoint.Parse(success2), Is.EqualTo(new GlobePoint(1.2d, 2.5d, 350)));
        Assert.Catch(typeof(FormatException), () =>
        {
            var res = GlobePoint.Parse(fail);
        });
    }
    
    [Test]
    public void StringToGlobeArea()
    {
        string success1 = "((1, 2), (3, 4))";
        string success2 = "((1.2, 2.5, 350), (0.5, 1.5, 350))";
        string fail = "((), ())";
        Assert.That(GlobeArea.Parse(success1), Is.EqualTo(new GlobeArea(new GlobePoint(1, 2), new GlobePoint(3, 4))));
        Assert.That(GlobeArea.Parse(success2), Is.EqualTo(new GlobeArea(new GlobePoint(0.5, 1.5d, 350), new GlobePoint(1.2d, 2.5d))));
        Assert.Catch(typeof(FormatException), () =>
        {
            var res = GlobeArea.Parse(fail);
        });
    }
}