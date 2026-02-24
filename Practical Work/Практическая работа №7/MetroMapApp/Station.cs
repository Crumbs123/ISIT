// Station.cs - Класс для станции метро (вершина)
using System;
using System.Drawing;

namespace MetroMapApp
{
    public class Station
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Point Location { get; set; }
        public int Radius { get; set; } = 20;
        public Color Color { get; set; } = Color.LightGreen;
        public bool IsSelected { get; set; }
        public MetroLine Line { get; set; }

        public Station(string name, Point location)
        {
            Id = Guid.NewGuid();
            Name = name;
            Location = location;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(
                Location.X - Radius,
                Location.Y - Radius,
                Radius * 2,
                Radius * 2);
        }

        public bool Contains(Point point)
        {
            int dx = point.X - Location.X;
            int dy = point.Y - Location.Y;
            return dx * dx + dy * dy <= Radius * Radius;
        }
    }

    public class MetroLine
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }

        public MetroLine(string name, Color color)
        {
            Id = Guid.NewGuid();
            Name = name;
            Color = color;
        }
    }
}