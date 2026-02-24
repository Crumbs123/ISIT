// Track.cs - Класс для перегона между станциями (ребро)
using System;
using System.Drawing;

namespace MetroMapApp
{
    public class Track
    {
        public Guid Id { get; set; }
        public Station Station1 { get; set; }
        public Station Station2 { get; set; }
        public int Distance { get; set; }
        public MetroLine Line { get; set; }
        public bool IsSelected { get; set; }

        public Track(Station station1, Station station2, int distance, MetroLine line)
        {
            Id = Guid.NewGuid();
            Station1 = station1;
            Station2 = station2;
            Distance = distance;
            Line = line;
        }

        public bool Contains(Point point)
        {
            // Проверка попадания точки на линию (с небольшим допуском)
            float tolerance = 5;
            float dist = DistanceFromPointToLine(point, Station1.Location, Station2.Location);
            return dist <= tolerance;
        }

        private float DistanceFromPointToLine(Point pt, Point p1, Point p2)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if (dx == 0 && dy == 0) return (float)Math.Sqrt((pt.X - p1.X) * (pt.X - p1.X) + (pt.Y - p1.Y) * (pt.Y - p1.Y));

            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            float projX = p1.X + t * dx;
            float projY = p1.Y + t * dy;

            return (float)Math.Sqrt((pt.X - projX) * (pt.X - projX) + (pt.Y - projY) * (pt.Y - projY));
        }

        public Point GetMidPoint()
        {
            return new Point(
                (Station1.Location.X + Station2.Location.X) / 2,
                (Station1.Location.Y + Station2.Location.Y) / 2);
        }
    }
}