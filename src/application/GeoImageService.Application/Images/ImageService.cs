using System.Globalization;
using System.Xml;
using GeoImageSerrvice.Abstractions.Repositories;
using GeoImageService.Application.Helpers;
using GeoImageService.Application.Mappers;
using GeoImageService.Application.Models.DTO;
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
    private readonly GdalHelper _gdalHelper;

    public ImageService(IImageRepository imageRepository, IOptions<StorageOptions> storageOptions,
        GdalHelper gdalHelper)
    {
        _imageRepository = imageRepository;
        _gdalHelper = gdalHelper;
        _storageOptions = storageOptions.Value;
    }

    public async Task<ImageDto> CreateAsync(IFormFile photo, IFormFile kmlFile, string fileName,
        CancellationToken cancellationToken)
    {
        CornersCoordinates? cornersCoordinates;
        TimeStamps? timeStamps;
        await using (var kmlStream = kmlFile.OpenReadStream())
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(kmlStream);
            // Удобнее будет, если кмл парсер будет возвращать какую-нибудь структуру "KML" с координатами и длительностью.
            cornersCoordinates = KmlParser.TryParseCoordinates(xmlDocument);
            timeStamps = KmlParser.TryParseTimeStamps(xmlDocument);
        }


        // if nesting hell - инвертировать и будет проще для чтения
        if (_storageOptions is { StoragePath: not null, GeoTiffPath: not null })
        {
            var fullPath = Path.Combine(_storageOptions.StoragePath, fileName + ".jpg");
            var tiffPath = Path.Combine(_storageOptions.GeoTiffPath, fileName + ".tif");
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await photo.CopyToAsync(stream, cancellationToken);
            if (cornersCoordinates != null)
            {
                if (timeStamps != null)
                {
                    var image = ImageMapper.ToDto(await _imageRepository.CreateAsync(cornersCoordinates, fullPath,
                        tiffPath, timeStamps, cancellationToken));
                    await _gdalHelper.CreateGeoTiffFile(kmlFile, fileName, cancellationToken);
                    return image;
                }
            }
        }

        throw new Exception("Invalid data");
    }

    public async Task<IEnumerable<ImageDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var images = await _imageRepository.GetAllAsync(cancellationToken);
        var dtos = images.Select(ImageMapper.ToDto).ToList();
        return dtos;
    }

    public async Task<List<ImageDto>> GetImagesByCoordinateIntersection(CornersCoordinates cornersCoordinates,
        CancellationToken cancellationToken)
    {
        var allImages = await _imageRepository.GetAllAsync(cancellationToken);
        var dtos = allImages.Select(ImageMapper.ToDto).ToList();

        // Если используешь Fluent linq (вроде так называется?), то его и придерживаться, а не приплетать еще query
        var result = (from dto in dtos
            let coord = dto.CornersCoordinates
            where GdalHelper.DoesRectangleCrossesImage(cornersCoordinates, coord)
            select dto).ToList();
        return result;
    }

    public async Task<CutImageDto> CutImage(CornersCoordinates rectangle, TimeStamps timeStamps,
        CancellationToken cancellationToken)
    {
        var imagesByCoordinateIntersection =
            FilterByTimeStamps(await GetImagesByCoordinateIntersection(rectangle, cancellationToken), timeStamps);

        if (!imagesByCoordinateIntersection.Any())
            throw new Exception("There are no images for coordinates");

        var inputDatasets = new Dataset[imagesByCoordinateIntersection.Count];
        for (int i = 0; i < imagesByCoordinateIntersection.Count; i++)
        {
            var path = imagesByCoordinateIntersection[i].GeoTiffFilePath;
            inputDatasets[i] = Gdal.Open(path, Access.GA_ReadOnly);
            if (inputDatasets[i] == null)
                throw new Exception($"Can't open file {path}");
        }

        var fileName = $"cut_{DateTime.UtcNow:yyyyMMdd_HHmmss}";

        // проверка после полезной работы, можно сделать в начале метода
        if (_storageOptions.GeoTiffPath == null) throw new Exception("Invalid data");
        var outputTiffPath = Path.Combine(_storageOptions.GeoTiffPath, fileName + ".tif");

        // в ImageService протекла логика GDAL
        var warpOptions = new GDALWarpAppOptions([
            "-of", "GTiff",
            "-t_srs", "EPSG:4326",
            "-r", "bilinear",
            "-te",

            // какие-то странные детали реализации, почему именно у этих углов берем лат и лон? а не иначе?
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

        return ImageMapper.ToCutDto(cutImage);
    }

    private static List<ImageDto> FilterByTimeStamps(List<ImageDto> images, TimeStamps timeStamps)
    {
        return images.Where(image =>
            image.TimeStamps.Start >= timeStamps.Start && image.TimeStamps.End <= timeStamps.End).ToList();
    }
}
