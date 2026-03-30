using System.Globalization;
using System.Xml;
using GeoImageService.Application.Models.Images;
using GeoImageService.Application.Models.Options;
using GeoImageService.Application.Parsers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OSGeo.GDAL;

namespace GeoImageService.Application.Helpers;

// имеет смысл спрятать за интерфейс, так как может быть реализация обработки картинок не только с GDAL. Еще н еочень нравится Helper в названии
public class GdalHelper
{
    private readonly StorageOptions _storageOptions;

    public GdalHelper(IOptions<StorageOptions> storageOptions)
    {
        _storageOptions = storageOptions.Value;
    }

    public async Task CreateGeoTiffFile(IFormFile kmlFile, string fileName,
        CancellationToken cancellationToken)
    {
        await using var kmlStream = kmlFile.OpenReadStream();
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(kmlStream);
        var cornersCoordinates = KmlParser.TryParseCoordinates(xmlDocument);

        if (cornersCoordinates != null)
        {
            var minLon = new[]
            {
                cornersCoordinates.TopLeft.Longitude,
                cornersCoordinates.TopRight.Longitude,
                cornersCoordinates.BottomLeft.Longitude,
                cornersCoordinates.BottomRight.Longitude
            }.Min();

            var maxLon = new[]
            {
                cornersCoordinates.TopLeft.Longitude,
                cornersCoordinates.TopRight.Longitude,
                cornersCoordinates.BottomLeft.Longitude,
                cornersCoordinates.BottomRight.Longitude
            }.Max();

            var minLat = new[]
            {
                cornersCoordinates.TopLeft.Latitude,
                cornersCoordinates.TopRight.Latitude,
                cornersCoordinates.BottomLeft.Latitude,
                cornersCoordinates.BottomRight.Latitude
            }.Min();

            var maxLat = new[]
            {
                cornersCoordinates.TopLeft.Latitude,
                cornersCoordinates.TopRight.Latitude,
                cornersCoordinates.BottomLeft.Latitude,
                cornersCoordinates.BottomRight.Latitude
            }.Max();

            if (_storageOptions.GeoTiffPath != null)
            {
                var tiffPath = Path.Combine(_storageOptions.GeoTiffPath,
                    fileName + ".tif");
                if (_storageOptions.StoragePath == null)
                {
                    throw new Exception("Storage path not set");
                }

                var photoPath = Path.Combine(_storageOptions.StoragePath, fileName + ".jpg");

                var options = new GDALTranslateOptions([
                    "-of", "GTiff",
                    "-a_ullr",
                    minLon.ToString(CultureInfo.InvariantCulture),
                    maxLat.ToString(CultureInfo.InvariantCulture),
                    maxLon.ToString(CultureInfo.InvariantCulture),
                    minLat.ToString(CultureInfo.InvariantCulture),
                    "-a_srs", "EPSG:4326", "-co", "TFW=NO"
                ]);
                using var src = Gdal.Open(photoPath, Access.GA_ReadOnly);
                using var dst = Gdal.wrapper_GDALTranslate(tiffPath, src, options, null, null);
            }
        }
    }

    public static bool DoesRectangleCrossesImage(CornersCoordinates rect, CornersCoordinates img)
    {
        var rectMinLat = new[]
                { rect.TopLeft.Latitude, rect.TopRight.Latitude, rect.BottomLeft.Latitude, rect.BottomRight.Latitude }
            .Min();
        var rectMaxLat = new[]
                { rect.TopLeft.Latitude, rect.TopRight.Latitude, rect.BottomLeft.Latitude, rect.BottomRight.Latitude }
            .Max();
        var rectMinLon = new[]
            {
                rect.TopLeft.Longitude, rect.TopRight.Longitude, rect.BottomLeft.Longitude, rect.BottomRight.Longitude
            }
            .Min();
        var rectMaxLon = new[]
            {
                rect.TopLeft.Longitude, rect.TopRight.Longitude, rect.BottomLeft.Longitude, rect.BottomRight.Longitude
            }
            .Max();

        var imgMinLat = new[]
            { img.TopLeft.Latitude, img.TopRight.Latitude, img.BottomLeft.Latitude, img.BottomRight.Latitude }.Min();
        var imgMaxLat = new[]
            { img.TopLeft.Latitude, img.TopRight.Latitude, img.BottomLeft.Latitude, img.BottomRight.Latitude }.Max();
        var imgMinLon = new[]
                { img.TopLeft.Longitude, img.TopRight.Longitude, img.BottomLeft.Longitude, img.BottomRight.Longitude }
            .Min();
        var imgMaxLon = new[]
                { img.TopLeft.Longitude, img.TopRight.Longitude, img.BottomLeft.Longitude, img.BottomRight.Longitude }
            .Max();

        return rectMinLon <= imgMaxLon &&
               rectMaxLon >= imgMinLon &&
               rectMinLat <= imgMaxLat &&
               rectMaxLat >= imgMinLat;
    }
}
