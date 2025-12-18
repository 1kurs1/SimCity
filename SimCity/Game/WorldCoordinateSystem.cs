using SimCity.Map;
using System.Collections.Generic;
using System.Windows;

namespace SimCity.Game
{
    public static class WorldCoordinateSystem
    {
        // Размер базовой клетки (ландшафта) в мировых единицах
        public const float BASE_TILE_WIDTH = 1.0f;
        public const float BASE_TILE_HEIGHT = 0.5f; // Высота меньше из-за изометрии

        // Преобразование координат сетки в мировые координаты
        public static Point GridToWorld(int gridX, int gridY)
        {
            // Для изометрической проекции
            float worldX = (gridX - gridY) * (BASE_TILE_WIDTH / 2);
            float worldY = (gridX + gridY) * (BASE_TILE_HEIGHT / 2);
            return new Point(worldX, worldY);
        }

        // Для топ-даун (если у вас такой стиль)
        public static Point GridToWorldTopDown(int gridX, int gridY)
        {
            return new Point(gridX * BASE_TILE_WIDTH, gridY * BASE_TILE_HEIGHT);
        }

        // Получение всех клеток, которые занимает здание
        public static IEnumerable<(int x, int y)> GetOccupiedTiles(
            int baseX, int baseY, TileSize size)
        {
            for (int dx = 0; dx < size.WidthInTiles; dx++)
            {
                for (int dy = 0; dy < size.HeightInTiles; dy++)
                {
                    yield return (baseX + dx, baseY + dy);
                }
            }
        }
    }
}