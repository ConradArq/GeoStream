using AutoMapper;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Route;
using GeoStream.Api.Application.Exceptions;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoJSON.Net.Geometry;
using System.IO.Compression;
using System.Xml;
using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using Route = GeoStream.Api.Domain.Models.Entities.Route;
using NtsLineString = NetTopologySuite.Geometries.LineString;
using LineString = NetTopologySuite.Geometries.LineString;
using Polygon = NetTopologySuite.Geometries.Polygon;
using FeatureCollection = GeoJSON.Net.Feature.FeatureCollection;
using Point = NetTopologySuite.Geometries.Point;
using NetTopologySuite.IO;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Microsoft.AspNetCore.Mvc;
using GeoStream.Api.Application.Resources;
using GeoStream.Api.Helpers;
using GeoStream.Api.Application.Dtos.Hub;
using GeoStream.Api.Application.Dtos.RouteHub;
using GeoStream.Api.Domain.Models.Entities;

namespace GeoStream.Api.Application.Services
{
    internal class RouteService : IRouteService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public RouteService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDto<ResponseRouteDto>> CreateAsync(CreateRouteDto requestDto)
        {
            var existingRoutes = await _unitOfWork.RouteRepository.GetAsync(x => x.Name == requestDto.Name);
            if (existingRoutes.Any())
            {
                throw new ValidationException(ValidationMessages.ExistingRouteNameError);
            }

            var entity = _mapper.Map<Route>(requestDto);
            entity.GeoJson = ConvertKmzToGeoJson(entity.KmzFile).geoJsonContent;

            _unitOfWork.RouteRepository.Create(entity);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<ResponseRouteDto>(_mapper.Map<ResponseRouteDto>(entity));
            return response;
        }

        public async Task<ResponseDto<ResponseRouteDto>> UpdateAsync(int id, UpdateRouteDto requestDto)
        {
            if (requestDto.Name != null)
            {
                var existingRutas = await _unitOfWork.RouteRepository.GetAsync(x => x.Id != id && x.Name == requestDto.Name);
                if (existingRutas.Any())
                {
                    throw new ValidationException(ValidationMessages.ExistingRouteNameError);
                }
            }

            var entity = await _unitOfWork.RouteRepository.GetSingleAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            _mapper.Map(requestDto, entity);
            entity.GeoJson = ConvertKmzToGeoJson(entity.KmzFile).geoJsonContent;

            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<ResponseRouteDto>(_mapper.Map<ResponseRouteDto>(entity));
            return response;
        }

        public async Task<ResponseDto<List<ResponseRouteDto>>> UpdateRangeAsync([FromBody] List<UpdateRouteDto> requestDtos)
        {
            List<UpdateRouteDto> addedRouteDtos = new List<UpdateRouteDto>();
            List<UpdateRouteDto> existingRouteDtos = new List<UpdateRouteDto>();
            HashSet<string> duplicateNames = new HashSet<string>();
            string errorMessage = string.Empty;

            // Create a list to hold the geoJsonRoutes and their names
            var geoJsonRoutes = requestDtos
                .Select(requestDto =>
                {
                    var geoJsonRoute = ConvertKmzToGeoJson(requestDto.KmzFile);
                    requestDto.Name = geoJsonRoute.routeName;
                    return new { RequestDto = requestDto, GeoJsonRoute = geoJsonRoute };
                })
                .ToList();

            // Check for duplicates within the provided list
            var duplicateEntities = geoJsonRoutes
                .GroupBy(x => x.RequestDto.Name)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Select(x => x.RequestDto))
                .ToList();

            existingRouteDtos.AddRange(duplicateEntities);
            if (duplicateEntities != null)
            {
                duplicateNames.UnionWith(duplicateEntities
                    .Select(entity => entity.Name)
                    .Where(name => name != null)
                    .Cast<string>());
            }

            foreach (var geoJsonRoutePair in geoJsonRoutes)
            {
                var requestDto = geoJsonRoutePair.RequestDto;
                var routeName = requestDto.Name;

                // Check for existing routes in the database with the same name
                var existingRoutesInDb = await _unitOfWork.RouteRepository.GetAsync(x => x.Id != requestDto.Id && x.Name == routeName);
                if (existingRoutesInDb.Any() || routeName != null && duplicateNames.Contains(routeName))
                {
                    existingRouteDtos.Add(requestDto);
                    if (routeName != null)
                    {
                        duplicateNames.Add(routeName);
                    }
                    continue;
                }

                //Generate GeoJson and unique color for route
                requestDto.GeoJson = geoJsonRoutePair.GeoJsonRoute.geoJsonContent;

                string generatedUniqueColor = await GenerateUniqueColorAsync();

                if (generatedUniqueColor == string.Empty)
                {
                    errorMessage += $"Could not assign a color to the route {requestDto.Name}, and therefore it could not be added.";
                    continue;
                }

                requestDto.Color = generatedUniqueColor;

                addedRouteDtos.Add(requestDto);
            }

            var entities = _mapper.Map<ICollection<Route>>(addedRouteDtos);

            _unitOfWork.RouteRepository.UpdateRange(entities);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<List<ResponseRouteDto>>(_mapper.Map<List<ResponseRouteDto>>(entities));

            if (existingRouteDtos.Any())
            {
                response.Message = string.Concat($"The routes {string.Join(", ", existingRouteDtos.Select(entity => entity.Name).Distinct())} could not be added because routes with the same name already exist.");
            }
            return response;
        }

        public async Task<ResponseDto<object>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.RouteRepository.GetSingleAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            _unitOfWork.RouteRepository.Delete(entity);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<object>();
            return response;
        }

        public async Task<ResponseDto<ResponseRouteDto>> GetAsync(int id)
        {
            var entity = await _unitOfWork.RouteRepository.GetSingleAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            var response = new ResponseDto<ResponseRouteDto>(_mapper.Map<ResponseRouteDto>(entity));
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseRouteDto>>> GetAllAsync(QueryRequestDto? requestDto)
        {
            var entities = await _unitOfWork.RouteRepository.GetAsync(orderBy: QueryHelper.BuildOrderByFunction<Route>(requestDto));

            var response = new ResponseDto<IEnumerable<ResponseRouteDto>>(_mapper.Map<IEnumerable<ResponseRouteDto>>(entities));
            return response;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseRouteDto>>> GetAllPaginatedAsync(PaginationRequestDto requestDto)
        {
            var entities = await _unitOfWork.RouteRepository.GetPaginatedAsync(requestDto.PageNumber, requestDto.PageSize, orderBy: QueryHelper.BuildOrderByFunction<Route>(requestDto));

            var response = new PaginatedResponseDto<IEnumerable<ResponseRouteDto>>(_mapper.Map<IEnumerable<ResponseRouteDto>>(entities.Data), requestDto.PageNumber, requestDto.PageSize, entities.TotalItems);
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseRouteDto>>> SearchAsync(SearchRouteDto requestDto)
        {
            var searchExpression = QueryHelper.BuildPredicate<Route>(requestDto);

            var entities = await _unitOfWork.RouteRepository.GetAsync(searchExpression, orderBy: QueryHelper.BuildOrderByFunction<Route>(requestDto));

            if (requestDto.HubId != null && requestDto.HubLatitude != null && requestDto.HubLongitude != null)
            {
                entities = AssociateHubsWithRoutes(entities, requestDto.HubLatitude.Value, requestDto.HubLongitude.Value, 25);
            }

            var response = new ResponseDto<IEnumerable<ResponseRouteDto>>(_mapper.Map<IEnumerable<ResponseRouteDto>>(entities));
            return response;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseRouteDto>>> SearchPaginatedAsync(SearchPaginatedRouteDto requestDto)
        {
            var searchExpression = QueryHelper.BuildPredicate<Route>(requestDto);
            var entities = await _unitOfWork.RouteRepository.GetPaginatedAsync(requestDto.PageNumber, requestDto.PageSize, searchExpression, orderBy: QueryHelper.BuildOrderByFunction<Route>(requestDto));

            var response = new PaginatedResponseDto<IEnumerable<ResponseRouteDto>>(_mapper.Map<IEnumerable<ResponseRouteDto>>(entities.Data), requestDto.PageNumber, requestDto.PageSize, entities.TotalItems);
            return response;
        }

        public async Task<ResponseDto<FileStreamResult>> GetKmzFileStreamResult(int routeId)
        {
            var entity = await _unitOfWork.RouteRepository.GetSingleAsync(routeId);

            if (entity == null)
            {
                throw new NotFoundException(routeId);
            }

            var memoryStream = new MemoryStream(entity.KmzFile);
            var kmzFileStreamResult = new FileStreamResult(memoryStream, "application/vnd.google-earth.kmz+xml")
            {
                FileDownloadName = string.Concat(entity.Name, ".kmz")
            };

            var response = new ResponseDto<FileStreamResult>(kmzFileStreamResult);
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseRouteHubDto>>> GetAllRouteHubsAsync(QueryRequestDto? requestDto)
        {
            var entities = await _unitOfWork.RouteHubRepository.GetAsync(orderBy: QueryHelper.BuildOrderByFunction<RouteHub>(requestDto));

            var response = new ResponseDto<IEnumerable<ResponseRouteHubDto>>(_mapper.Map<IEnumerable<ResponseRouteHubDto>>(entities));
            return response;
        }

        private (string routeName, string geoJsonContent) ConvertKmzToGeoJson(byte[]? kmzFile)
        {
            if (kmzFile == null || kmzFile.Length == 0)
            {
                return (string.Empty, string.Empty);
            }

            string kmlContent = ExtractKmlFromKmz(kmzFile);
            string routeName = ExtractRouteNameFromKml(kmlContent);
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

        private List<Route> AssociateHubsWithRoutes(IReadOnlyList<Route> routes, decimal hubLatitude, decimal hubLongitude, double bufferDistance)
        {
            List<Route> filteredRoutes = new List<Route>();

            // Transform the hub point to UTM coordinates
            var (utmX, utmY, utmZone) = TransformToUtm((double)hubLatitude, (double)hubLongitude);
            var hubPoint = new Point(new Coordinate(utmX, utmY));
            var buffer = hubPoint.Buffer(bufferDistance);

            foreach (var route in routes)
            {
                var geoJson = route.GeoJson;
                if (geoJson != null)
                {
                    var featureCollection = Newtonsoft.Json.JsonConvert.DeserializeObject<FeatureCollection>(geoJson);
                    if (featureCollection != null)
                    {
                        foreach (var feature in featureCollection.Features)
                        {
                            var geometry = feature.Geometry;
                            if (geometry != null)
                            {
                                var geom = ConvertToNetTopologySuiteGeometry(geometry);
                                geom = TransformToUtm(geom, utmZone); // Transform route geometry to UTM

                                if (geom.Intersects(buffer))
                                {
                                    filteredRoutes.Add(route);
                                }
                            }
                        }
                    }
                }
            }

            return filteredRoutes;
        }

        private (double utmX, double utmY, int utmZone) TransformToUtm(double latitude, double longitude)
        {
            var csFactory = new CoordinateSystemFactory();
            var wgs84 = csFactory.CreateGeographicCoordinateSystem("WGS84",
                AngularUnit.Degrees, HorizontalDatum.WGS84, PrimeMeridian.Greenwich,
                new AxisInfo("Lon", AxisOrientationEnum.East),
                new AxisInfo("Lat", AxisOrientationEnum.North));

            int utmZone = (int)Math.Floor((longitude + 180) / 6) + 1;
            var isNorthernHemisphere = latitude >= 0;
            var utm = ProjectedCoordinateSystem.WGS84_UTM(utmZone, isNorthernHemisphere);

            var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(wgs84, utm);
            var transformed = transform.MathTransform.Transform(new[] { longitude, latitude });
            return (transformed[0], transformed[1], utmZone);
        }

        private Geometry TransformToUtm(Geometry geometry, int utmZone)
        {
            var csFactory = new CoordinateSystemFactory();
            var wgs84 = csFactory.CreateGeographicCoordinateSystem("WGS84",
                AngularUnit.Degrees, HorizontalDatum.WGS84, PrimeMeridian.Greenwich,
                new AxisInfo("Lon", AxisOrientationEnum.East),
                new AxisInfo("Lat", AxisOrientationEnum.North));

            var isNorthernHemisphere = geometry.Centroid.Y >= 0;
            var utm = ProjectedCoordinateSystem.WGS84_UTM(utmZone, isNorthernHemisphere);

            var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(wgs84, utm);
            var geometryFactory = new GeometryFactory();

            if (geometry is LineString lineString)
            {
                var transformedCoordinates = lineString.Coordinates.Select(c =>
                {
                    var transformed = transform.MathTransform.Transform(new[] { c.X, c.Y });
                    return new Coordinate(transformed[0], transformed[1]);
                }).ToArray();

                return new LineString(transformedCoordinates);
            }

            if (geometry is Polygon polygon)
            {
                var transformedShell = TransformLinearRing(polygon.Shell, transform);
                var transformedHoles = polygon.Holes.Select(hole => TransformLinearRing(hole, transform)).ToArray();

                return new Polygon(transformedShell, transformedHoles);
            }

            // Handle other geometry types as needed
            return geometry;
        }

        private LinearRing TransformLinearRing(LinearRing ring, ICoordinateTransformation transform)
        {
            var transformedCoordinates = ring.Coordinates.Select(c =>
            {
                var transformed = transform.MathTransform.Transform(new[] { c.X, c.Y });
                return new Coordinate(transformed[0], transformed[1]);
            }).ToArray();

            return new LinearRing(transformedCoordinates);
        }

        private Geometry ConvertToNetTopologySuiteGeometry(IGeometryObject geoJsonGeometry)
        {
            var writer = new GeoJsonWriter();
            var reader = new GeoJsonReader();
            var json = writer.Write(geoJsonGeometry);
            return reader.Read<Geometry>(json);
        }

        private async Task<string> GenerateUniqueColorAsync()
        {
            Random random = new Random();
            string generatedColor = string.Empty;

            var rutas = await _unitOfWork.RouteRepository.GetAsync();
            List<string> existingColors = rutas.Select(x => x.Color).ToList();

            do
            {
                generatedColor = GetRandomVibrantColor(random);
            } while (existingColors.Contains(generatedColor));

            return generatedColor;
        }

        private string GetRandomVibrantColor(Random random)
        {
            double hue = random.NextDouble() * 360; // Hue between 0 and 360
            double saturation = 0.9; // Saturation fixed at 90% for vibrancy
            double lightness = 0.5; // Lightness fixed at 50% for visibility

            return HslToHex(hue, saturation, lightness);
        }

        private string HslToHex(double hue, double saturation, double lightness)
        {
            double c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
            double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            double m = lightness - c / 2;

            double r = 0, g = 0, b = 0;

            if (0 <= hue && hue < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (60 <= hue && hue < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (120 <= hue && hue < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (180 <= hue && hue < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (240 <= hue && hue < 300)
            {
                r = x; g = 0; b = c;
            }
            else if (300 <= hue && hue < 360)
            {
                r = c; g = 0; b = x;
            }

            int rInt = (int)((r + m) * 255);
            int gInt = (int)((g + m) * 255);
            int bInt = (int)((b + m) * 255);

            return $"#{rInt:X2}{gInt:X2}{bInt:X2}";
        }
    }
}
