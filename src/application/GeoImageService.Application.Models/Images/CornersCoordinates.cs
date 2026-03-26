namespace GeoImageService.Application.Models.Images;

public class CornersCoordinates
{
    public Coordinate TopLeft { get; set; }
    public Coordinate TopRight { get; set; }
    public Coordinate BottomRight { get; set; }
    public Coordinate BottomLeft { get; set; }
    
    public CornersCoordinates(
        Coordinate topLeft,
        Coordinate topRight,
        Coordinate bottomRight,
        Coordinate bottomLeft)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomRight = bottomRight;
        BottomLeft = bottomLeft;
    }
    
    public CornersCoordinates(){}
}