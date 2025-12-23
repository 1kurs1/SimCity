using System;
using System.Windows;
using System.Windows.Media;

namespace SimCity
{
    public class Camera
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Scale { get; set; } = 1.0;

        private Point? lastMousePos;
        public bool IsDragging { get; private set; }

        public double GetDepth(int x, int y, Building building = null)
        {
            // Базовый z-index - расстояние от камеры
            double depth = (x + y) * 10;

            // Если есть здание, учитываем его высоту
            if (building != null)
            {
                // Высота здания увеличивает z-index
                depth += building.Height * 5;
            }

            return depth;
        }

        public void StartDrag(Point mousePos)
        {
            lastMousePos = mousePos;
            IsDragging = true;
        }

        public void UpdateDrag(Point mousePos)
        {
            if (!IsDragging || lastMousePos == null)
                return;

            X += (mousePos.X - lastMousePos.Value.X);
            Y += (mousePos.Y - lastMousePos.Value.Y);
            lastMousePos = mousePos;
        }

        public void EndDrag()
        {
            IsDragging = false;
            lastMousePos = null;
        }

        public void Zoom(double delta, Point center)
        {
            double oldScale = Scale;
            Scale += delta * 0.001;

            if (Scale < 0.5) Scale = 0.5;
            if (Scale > 2.0) Scale = 2.0;

            double factor = Scale / oldScale;
            X = center.X - (center.X - X) * factor;
            Y = center.Y - (center.Y - Y) * factor;
        }

        public Transform GetTransform()
        {
            TransformGroup g = new TransformGroup();
            g.Children.Add(new ScaleTransform(Scale, Scale));
            g.Children.Add(new TranslateTransform(X, Y));
            return g;
        }

        // TILE → SCREEN
        public Point TileToScreen(int x, int y)
        {
            double wx = (x - y) * 16;
            double wy = (x + y) * 8;
            return new Point(wx * Scale + X, wy * Scale + Y);
        }

        // SCREEN → TILE
        public (int x, int y) ScreenToTile(Point screen)
        {
            double wx = (screen.X - X) / Scale;
            double wy = (screen.Y - Y) / Scale;

            double fx = (wy / 8 + wx / 16) / 2;
            double fy = (wy / 8 - wx / 16) / 2;

            return ((int)Math.Round(fx), (int)Math.Round(fy));
        }
    }
}