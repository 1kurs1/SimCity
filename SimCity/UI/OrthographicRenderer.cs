using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SimCity.Game;
using SimCity.Map;

namespace SimCity.UI
{
    public class OrthographicRenderer
    {
        // Размеры тайла в ортографической проекции
        public const int TileWidth = 32;   // Ширина ромба
        public const int TileHeight = 16;  // Высота ромба

        // Преобразование координат сетки в экранные координаты
        public static Point GridToScreen(int gridX, int gridY, double mapOffsetX = 0, double mapOffsetY = 0)
        {
            // Для ортографической проекции (вид сверху под углом)
            double screenX = (gridX - gridY) * (TileWidth / 2.0) + mapOffsetX;
            double screenY = (gridX + gridY) * (TileHeight / 2.0) + mapOffsetY;
            return new Point(screenX, screenY);
        }

        // Преобразование экранных координат в сетку
        public static Point ScreenToGrid(double screenX, double screenY, double mapOffsetX = 0, double mapOffsetY = 0)
        {
            double x = screenX - mapOffsetX;
            double y = screenY - mapOffsetY;

            double gridX = (x / (TileWidth / 2.0) + y / (TileHeight / 2.0)) / 2.0;
            double gridY = (y / (TileHeight / 2.0) - x / (TileWidth / 2.0)) / 2.0;
            return new Point(gridX, gridY);
        }

        // Получение всех углов ромба для отрисовки
        public static Point[] GetTilePolygon(int gridX, int gridY, double mapOffsetX = 0, double mapOffsetY = 0)
        {
            Point center = GridToScreen(gridX, gridY, mapOffsetX, mapOffsetY);

            return new Point[]
            {
                new Point(center.X, center.Y - TileHeight / 2.0),                // Верх
                new Point(center.X + TileWidth / 2.0, center.Y),                // Право
                new Point(center.X, center.Y + TileHeight / 2.0),               // Низ
                new Point(center.X - TileWidth / 2.0, center.Y),                // Лево
                new Point(center.X, center.Y - TileHeight / 2.0)                // Замыкаем
            };
        }

        // Новый метод для расчета центра карты
        public static Point GetMapCenterOffset(int mapWidth, int mapHeight)
        {
            // Для центрирования карты: находим координаты центрального тайла
            double centerX = (mapWidth - 1) / 2.0;
            double centerY = (mapHeight - 1) / 2.0;

            // Преобразуем координаты центра
            return GridToScreen((int)centerX, (int)centerY);
        }

        // Метод для расчета размера карты в пикселях
        public static Size CalculateMapSize(int mapWidth, int mapHeight)
        {
            double width = (mapWidth + mapHeight) * (TileWidth / 2.0);
            double height = (mapWidth + mapHeight) * (TileHeight / 2.0);

            return new Size(width, height);
        }
    }
}