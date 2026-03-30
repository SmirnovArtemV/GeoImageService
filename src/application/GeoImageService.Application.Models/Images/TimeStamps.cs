namespace GeoImageService.Application.Models.Images;

public class TimeStamps
{
    // Зачем тут nullable поля? Кажется, что может только путать
    // Возможно, стоит сделать один класс TimeStamp, и еще один - Interval, например
    public double? Start { get; init; }
    public double? End { get; init; }

    public TimeStamps(double start, double end)
    {
        Start = start;
        End = end;
    }
    
    public TimeStamps(){}
}
