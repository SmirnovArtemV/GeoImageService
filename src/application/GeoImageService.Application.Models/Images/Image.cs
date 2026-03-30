namespace GeoImageService.Application.Models.Images;

public class Image
{
    public long Id { get; init; }
    public string FilePath { get; init; }

    public string GeoTiffFilePath { get; init; }

    public CornersCoordinates CornersCoordinates { get; init; }

    public TimeStamps TimeStamps { get; init; }

    public Image(long id, string filePath, string geoTiffPath, CornersCoordinates cornersCoordinates,
        TimeStamps timeStamps)
    {
        Id = id;
        FilePath = filePath;
        GeoTiffFilePath = geoTiffPath;
        CornersCoordinates = cornersCoordinates;
        TimeStamps = timeStamps;
    }

    public Image()
    {
    }
};