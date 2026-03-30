namespace GeoImageService.Application.Models.Images;

// Вероятно эту ДТОшку имеет смысл сделать "умнее" и всю логику вычисления мин и макс лат и лон вынести сюда. Или в отдельный класс математики.
public class CornersCoordinates
{
    public Coordinate TopLeft { get; init; }
    public Coordinate TopRight { get; init; }
    public Coordinate BottomRight { get; init; }
    public Coordinate BottomLeft { get; init; }

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

    public CornersCoordinates()
    {
    }
}
