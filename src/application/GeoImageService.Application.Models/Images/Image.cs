namespace GeoImageService.Application.Models.Images;

public class Image
{
    public long Id { get; set; }
    public string FilePath { get; set; }
    
    public string GeoTiffFilePath { get; set; }
    
    public CornersCoordinates CornersCoordinates { get; set; }

    public Image(long id, string filePath, CornersCoordinates cornersCoordinates)
    {
        Id = id;
        FilePath = filePath;
        CornersCoordinates = cornersCoordinates;
    }
    
    public Image(){}
};