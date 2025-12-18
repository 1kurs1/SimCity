using System;
using System.Windows;

namespace SimCity.Game
{
    public class Camera
    {
        public Point Position { get; set; } = new Point(0, 0);
        public double Scale { get; set; } = 1.0;
        public double MinScale { get; set; } = 0.2;
        public double MaxScale { get; set; } = 3.0;

        // Преобразование мировых координат в экранные
        public Point WorldToScreen(double worldX, double worldY, double screenWidth, double screenHeight)
        {
            double screenX = (worldX - Position.X) * Scale + screenWidth / 2;
            double screenY = (worldY - Position.Y) * Scale + screenHeight / 2;
            return new Point(screenX, screenY);
        }

        // Преобразование экранных координат в мировые
        public Point ScreenToWorld(double screenX, double screenY, double screenWidth, double screenHeight)
        {
            double worldX = (screenX - screenWidth / 2) / Scale + Position.X;
            double worldY = (screenY - screenHeight / 2) / Scale + Position.Y;
            return new Point(worldX, worldY);
        }

        public void Zoom(double delta, Point center)
        {
            double oldScale = Scale;
            Scale *= delta;

            // Вместо Math.Clamp используем свою реализацию
            Scale = Clamp(Scale, MinScale, MaxScale);

            // Корректируем позицию для сохранения центра зума
            double scaleRatio = Scale / oldScale;
            Position = new Point(
                center.X - (center.X - Position.X) * scaleRatio,
                center.Y - (center.Y - Position.Y) * scaleRatio
            );
        }

        public void Pan(Vector delta)
        {
            Position = new Point(
                Position.X + delta.X / Scale,
                Position.Y + delta.Y / Scale
            );
        }

        // Своя реализация Clamp для старых версий .NET
        private double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        // Перегруженные версии для других типов (опционально)
        private int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}