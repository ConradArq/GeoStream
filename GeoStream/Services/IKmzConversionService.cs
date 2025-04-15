namespace GeoStream.Services
{
    public interface IKmzConversionService
    {
        (string routeName, string geoJsonContent) ConvertKmzToGeoJson(byte[]? kmzFile);
    }
}
