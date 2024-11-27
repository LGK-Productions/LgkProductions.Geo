using System.Text.Json.Serialization;

namespace LgkProductions.Geo.Projection;

[JsonDerivedType(typeof(WebMercatorProjection), "WebMercatorProjection")]
public interface IProjection : IPointProjection, ITileProjection;
