using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.IO.Compression;
using System.Xml;
using NtsLineString = NetTopologySuite.Geometries.LineString;

namespace GeoStream.Services
{
    public class KmzConversionService : IKmzConversionService
    {
        public (string routeName, string geoJsonContent) ConvertKmzToGeoJson(byte[]? kmzFile)
        {
            if (kmzFile == null)
            {
                return (string.Empty, string.Empty);
            }

            string kmlContent = ExtractKmlFromKmz(kmzFile);
            string routeName = ExtractRouteNameFromKml(kmlContent); // Extract the route name
            string geoJsonContent = ConvertKmlToGeoJson(kmlContent);
            return (routeName, geoJsonContent);
        }

        private string ExtractKmlFromKmz(byte[] kmzFile)
        {
            using var kmzStream = new MemoryStream(kmzFile);
            using var zip = new ZipArchive(kmzStream, ZipArchiveMode.Read);
            var kmlEntry = zip.Entries.FirstOrDefault(e => e.FullName.EndsWith(".kml"));
            if (kmlEntry != null)
            {
                using var kmlStream = kmlEntry.Open();
                using var reader = new StreamReader(kmlStream);
                return reader.ReadToEnd();
            }
            throw new FileNotFoundException("KML file not found in KMZ archive.");
        }

        private string ExtractRouteNameFromKml(string kmlContent)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(kmlContent);

            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");

            // Adjust the XPath query to find the route name if it follows a specific structure
            var routeNameNode = xmlDoc.SelectSingleNode("//kml:Document/kml:name", namespaceManager);
            return routeNameNode?.InnerText ?? "Unknown Route";
        }

        private string ConvertKmlToGeoJson(string kmlContent)
        {
            var features = new List<IFeature>();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(kmlContent);

            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("kml", "http://www.opengis.net/kml/2.2");
            namespaceManager.AddNamespace("gx", "http://www.google.com/kml/ext/2.2");

            var placemarks = xmlDoc.GetElementsByTagName("Placemark");

            foreach (XmlNode placemark in placemarks)
            {
                var nameNode = placemark.SelectSingleNode("kml:name", namespaceManager);
                var descriptionNode = placemark.SelectSingleNode("kml:description", namespaceManager);
                var name = nameNode?.InnerText;
                var description = descriptionNode?.InnerText;

                var pointNode = placemark.SelectSingleNode("kml:Point/kml:coordinates", namespaceManager);
                if (pointNode != null)
                {
                    var coords = pointNode.InnerText.Split(',');
                    var coordinate = new Coordinate(double.Parse(coords[0]), double.Parse(coords[1]));
                    var ntsPoint = new NetTopologySuite.Geometries.Point(coordinate);
                    var attributes = new AttributesTable
                    {
                        { "Name", name },
                        { "Description", description }
                    };
                    features.Add(new NetTopologySuite.Features.Feature(ntsPoint, attributes));
                }

                var multiTrackNodes = placemark.SelectNodes("gx:MultiTrack/gx:Track", namespaceManager);
                if (multiTrackNodes != null)
                {
                    foreach (XmlNode trackNode in multiTrackNodes)
                    {
                        var coordinates = new List<Coordinate>();
                        var coordNodes = trackNode.SelectNodes("gx:coord", namespaceManager);
                        if (coordNodes != null)
                        {
                            foreach (XmlNode coordNode in coordNodes)
                            {
                                var coords = coordNode.InnerText.Split(' ');
                                var coordinate = new Coordinate(double.Parse(coords[0]), double.Parse(coords[1]));
                                coordinates.Add(coordinate);
                            }
                        }
                        var lineString = new NtsLineString(coordinates.ToArray());
                        var attributes = new AttributesTable
                        {
                            { "Name", name },
                            { "Description", description }
                        };
                        features.Add(new NetTopologySuite.Features.Feature(lineString, attributes));
                    }
                }

                var trackNodes = placemark.SelectNodes("gx:Track", namespaceManager);
                if (trackNodes != null)
                {
                    foreach (XmlNode trackNode in trackNodes)
                    {
                        var coordinates = new List<Coordinate>();
                        var coordNodes = trackNode.SelectNodes("gx:coord", namespaceManager);
                        if (coordNodes != null)
                        {
                            foreach (XmlNode coordNode in coordNodes)
                            {
                                var coords = coordNode.InnerText.Split(' ');
                                var coordinate = new Coordinate(double.Parse(coords[0]), double.Parse(coords[1]));
                                coordinates.Add(coordinate);
                            }
                        }

                        var lineString = new NtsLineString(coordinates.ToArray());
                        var attributes = new AttributesTable
                        {
                            { "Name", name },
                            { "Description", description }
                        };
                        features.Add(new NetTopologySuite.Features.Feature(lineString, attributes));
                    }
                }
            }

            var featureCollection = new NetTopologySuite.Features.FeatureCollection();
            foreach (var feature in features)
            {
                featureCollection.Add(feature);
            }

            var geoJsonWriter = new GeoJsonWriter();
            return geoJsonWriter.Write(featureCollection);
        }
    }
}
