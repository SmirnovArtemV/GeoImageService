using System.Globalization;
using System.Xml;
using GeoImageSerrvice.Abstractions.Repositories;
using GeoImageService.Application.Models.Images;
using GeoImageService.Application.Models.Options;
using GeoImageService.Application.Parsers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OSGeo.GDAL;

namespace GeoImageService.Application.Images;

public class ImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly StorageOptions _storageOptions;

    public ImageService(IImageRepository imageRepository, IOptions<StorageOptions> storageOptions)
    {
        _imageRepository = imageRepository;
        _storageOptions = storageOptions.Value;
    }

    public async Task<Image> CreateAsync(IFormFile photo, IFormFile kmlFile, string fileName,
        CancellationToken cancellationToken)
    {
        CornersCoordinates cornersCoordinates;
        await using (var kmlStream = kmlFile.OpenReadStream())
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(kmlStream);
            cornersCoordinates = KmlParser.Parse(xmlDocument);
        }

        if (_storageOptions is { StoragePath: not null, GeoTiffPath: not null })
        {
            var fullPath = Path.Combine(_storageOptions.StoragePath, fileName + ".jpg");
            var tiffPath = Path.Combine(_storageOptions.GeoTiffPath, fileName + ".tif");
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await photo.CopyToAsync(stream, cancellationToken);
            var image = await _imageRepository.CreateAsync(cornersCoordinates, fullPath, tiffPath, cancellationToken);
            await CreateGeoTiffFile(kmlFile, fileName, cancellationToken);
            return image;
        }

        throw new Exception("Invalid data");
    }

    public async Task<IEnumerable<Image>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _imageRepository.GetAllAsync(cancellationToken);
    }

    public async Task<List<Image>> GetImagesByCoordinateIntersection(CornersCoordinates cornersCoordinates,
        CancellationToken cancellationToken)
    {
        var allImages = await _imageRepository.GetAllAsync(cancellationToken);
        var result = (from image in allImages
            let coord = image.CornersCoordinates
            where DoesRectangleCrossesImage(cornersCoordinates, coord)
            select image).ToList();
        return result;
    }

    public async Task<Image> CutImage(CornersCoordinates rectangle, CancellationToken cancellationToken)
    {
        var imagesByCoordinateIntersection = await GetImagesByCoordinateIntersection(rectangle, cancellationToken);

        if (!imagesByCoordinateIntersection.Any())
            throw new Exception("There are no images for coordinates");

        var inputDatasets = new Dataset[imagesByCoordinateIntersection.Count];
        for (int i = 0; i < imagesByCoordinateIntersection.Count; i++)
        {
            var path = imagesByCoordinateIntersection[i].GeoTiffFilePath;
            inputDatasets[i] = Gdal.Open(path, Access.GA_ReadOnly);
            if (inputDatasets[i] == null)
                throw new Exception($"Не удалось открыть {path}");
        }

        var fileName = $"cut_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        if (_storageOptions.GeoTiffPath == null) throw new Exception("Invalid data");
        var outputTiffPath = Path.Combine(_storageOptions.GeoTiffPath, fileName + ".tif");

        var warpOptions = new GDALWarpAppOptions([
            "-of", "GTiff",
            "-t_srs", "EPSG:4326",
            "-r", "bilinear",
            "-te",
            rectangle.TopLeft.Longitude.ToString(CultureInfo.InvariantCulture),
            rectangle.TopRight.Latitude.ToString(CultureInfo.InvariantCulture),
            rectangle.BottomRight.Longitude.ToString(CultureInfo.InvariantCulture),
            rectangle.BottomLeft.Latitude.ToString(CultureInfo.InvariantCulture)
        ]);

        using var outputDataset = Gdal.Warp(outputTiffPath, inputDatasets, warpOptions, null, null);

        foreach (var ds in inputDatasets)
            ds.Dispose();

        var cutImage = new Image
        {
            GeoTiffFilePath = outputTiffPath,
            CornersCoordinates = rectangle
        };

        return cutImage;
    }

    private async Task CreateGeoTiffFile(IFormFile kmlFile, string fileName,
        CancellationToken cancellationToken)
    {
        await using var kmlStream = kmlFile.OpenReadStream();
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(kmlStream);
        var cornersCoordinates = KmlParser.Parse(xmlDocument);

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
                "-a_srs", "EPSG:4326", "-co", "TFW=YES"
            ]);
            using var src = Gdal.Open(photoPath, Access.GA_ReadOnly);
            using var dst = Gdal.wrapper_GDALTranslate(tiffPath, src, options, null, null);
        }
    }

    private static bool DoesRectangleCrossesImage(CornersCoordinates rect, CornersCoordinates img)
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

    private static bool IsPointInPhoto(
        double pointLat,
        double pointLon,
        CornersCoordinates corners)
    {
        var lat1 = corners.TopLeft.Latitude;
        var lon1 = corners.TopLeft.Longitude;

        var lat2 = corners.TopRight.Latitude;
        var lon2 = corners.TopRight.Longitude;

        var lat3 = corners.BottomRight.Latitude;
        var lon3 = corners.BottomRight.Longitude;

        var lat4 = corners.BottomLeft.Latitude;
        var lon4 = corners.BottomLeft.Longitude;

        var cosLat = Math.Cos(lat1 * Math.PI / 180.0);

        var s1 = CheckSide(lat1, lon1, lat2, lon2);
        var s2 = CheckSide(lat2, lon2, lat3, lon3);
        var s3 = CheckSide(lat3, lon3, lat4, lon4);
        var s4 = CheckSide(lat4, lon4, lat1, lon1);

        return (s1 == s2 && s2 == s3 && s3 == s4);

        bool CheckSide(double lt1, double ln1, double lt2, double ln2)
        {
            var dLon = (ln2 - ln1) * cosLat;
            var pLon = (pointLon - ln1) * cosLat;
            var dLat = lt2 - lt1;
            var pLat = pointLat - lt1;

            return dLon * pLat - dLat * pLon >= 0;
        }
    }
}