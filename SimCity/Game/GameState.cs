using SimCity.Map;
using System.Collections.Generic;

namespace SimCity.Game
{
    public class GameState
    {
        public int Money { get; set; } = 5000;
    }

    public static class BuildingRules
    {
        // Словарь для стоимости зданий
        private static readonly Dictionary<TileType, int> _buildingCosts = new Dictionary<TileType, int>
        {
            { TileType.Residential, 100 },
            { TileType.Industrial, 200 },
            { TileType.Road, 50 },
            { TileType.Commercial, 150 }
        };

        // Словарь для дохода зданий
        private static readonly Dictionary<TileType, int> _buildingIncomes = new Dictionary<TileType, int>
        {
            { TileType.Residential, 5 },
            { TileType.Industrial, 15 },
            { TileType.Commercial, 10 },
            { TileType.Road, 0 }
        };

        // Словарь для базовых размеров зданий
        private static readonly Dictionary<TileType, TileSize> _buildingSizes = new Dictionary<TileType, TileSize>
        {
            { TileType.Residential, new TileSize(2, 2) },
            { TileType.Industrial, new TileSize(3, 3) },
            { TileType.Commercial, new TileSize(2, 2) },
            { TileType.Road, new TileSize(1, 1) }
        };

        // Метод для получения стоимости здания по типу
        public static int Cost(TileType type)
        {
            if (_buildingCosts.ContainsKey(type))
                return _buildingCosts[type];
            return 0;
        }

        // Метод для получения стоимости здания по ID (для новой системы)
        public static int Cost(string buildingId)
        {
            var buildingInfo = BuildingRegistry.GetBuilding(buildingId);
            if (buildingInfo != null)
            {
                // Учитываем размер здания в стоимости
                int baseCost = Cost(buildingInfo.Type);
                int sizeMultiplier = buildingInfo.Size.WidthInTiles * buildingInfo.Size.HeightInTiles;
                return baseCost * sizeMultiplier;
            }
            return 0;
        }

        // Метод для получения дохода по типу
        public static int Income(TileType type)
        {
            if (_buildingIncomes.ContainsKey(type))
                return _buildingIncomes[type];
            return 0;
        }

        // Метод для получения дохода по ID здания (для новой системы)
        public static int Income(string buildingId)
        {
            var buildingInfo = BuildingRegistry.GetBuilding(buildingId);
            if (buildingInfo != null)
            {
                // Учитываем размер здания в доходе
                int baseIncome = Income(buildingInfo.Type);
                int sizeMultiplier = buildingInfo.Size.WidthInTiles * buildingInfo.Size.HeightInTiles;
                return baseIncome * sizeMultiplier;
            }
            return 0;
        }

        // Метод для получения размера здания по типу
        public static TileSize GetSize(TileType type)
        {
            if (_buildingSizes.ContainsKey(type))
                return _buildingSizes[type];
            return TileSize.Single;
        }

        // Метод для получения размера здания по ID
        public static TileSize GetSize(string buildingId)
        {
            var buildingInfo = BuildingRegistry.GetBuilding(buildingId);
            if (buildingInfo != null)
                return buildingInfo.Size;
            return TileSize.Single;
        }

        // Метод для проверки, можно ли построить здание на клетке
        public static bool CanBuildOnTile(Tile tile, string buildingId)
        {
            if (tile == null)
                return false;

            // Пустая клетка всегда доступна для строительства
            if (tile.Type == TileType.Empty)
                return true;

            // Можно строить дороги поверх других объектов (кроме других дорог)
            var buildingInfo = BuildingRegistry.GetBuilding(buildingId);
            if (buildingInfo != null && buildingInfo.Type == TileType.Road && tile.Type != TileType.Road)
                return true;

            return false;
        }

        // Метод для получения спрайта по типу (для обратной совместимости)
        public static string GetSpriteName(TileType type)
        {
            switch (type)
            {
                case TileType.Residential:
                    return "residential.png";
                case TileType.Industrial:
                    return "industrial.png";
                case TileType.Road:
                    return "road.png";
                case TileType.Commercial:
                    return "commercial.png";
                default:
                    return "empty.png";
            }
        }

        // Метод для получения описания здания
        public static string GetDescription(TileType type)
        {
            switch (type)
            {
                case TileType.Residential:
                    return "Жилое здание - приносит жителей и доход";
                case TileType.Industrial:
                    return "Промышленное здание - приносит большой доход, но загрязняет";
                case TileType.Commercial:
                    return "Коммерческое здание - приносит доход и услуги";
                case TileType.Road:
                    return "Дорога - соединяет здания";
                default:
                    return "Пустой участок";
            }
        }

        // Метод для расчета стоимости с учетом скидок/бонусов
        public static int CalculateFinalCost(int baseCost, int x, int y, GameMap map)
        {
            int finalCost = baseCost;

            // Проверяем соседние клетки для бонусов
            // Например, если рядом есть дорога - скидка
            if (HasAdjacentRoad(x, y, map))
            {
                finalCost = (int)(finalCost * 0.9); // 10% скидка
            }

            // Если рядом есть вода - надбавка
            if (IsNearWater(x, y, map))
            {
                finalCost = (int)(finalCost * 1.2); // 20% надбавка
            }

            return finalCost;
        }

        private static bool HasAdjacentRoad(int x, int y, GameMap map)
        {
            // Проверяем все соседние клетки
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < map.Width && ny >= 0 && ny < map.Height)
                    {
                        if (map.Tiles[nx, ny].Type == TileType.Road)
                            return true;
                    }
                }
            }
            return false;
        }

        private static bool IsNearWater(int x, int y, GameMap map)
        {
            // Простая проверка - можно расширить для настоящей проверки воды
            // Временная заглушка
            return false;
        }

        // Метод для получения цвета по типу тайла (для отладки)
        public static System.Windows.Media.Color GetTileColor(TileType type)
        {
            switch (type)
            {
                case TileType.Residential:
                    return System.Windows.Media.Color.FromRgb(173, 216, 230); // LightBlue
                case TileType.Industrial:
                    return System.Windows.Media.Color.FromRgb(128, 128, 128); // Gray
                case TileType.Commercial:
                    return System.Windows.Media.Color.FromRgb(255, 215, 0); // Gold
                case TileType.Road:
                    return System.Windows.Media.Color.FromRgb(105, 105, 105); // DarkGray
                default:
                    return System.Windows.Media.Color.FromRgb(144, 238, 144); // LightGreen
            }
        }

        // Метод для проверки, является ли здание жилым
        public static bool IsResidential(TileType type)
        {
            return type == TileType.Residential;
        }

        // Метод для проверки, является ли здание промышленным
        public static bool IsIndustrial(TileType type)
        {
            return type == TileType.Industrial;
        }

        // Метод для проверки, является ли здание коммерческим
        public static bool IsCommercial(TileType type)
        {
            return type == TileType.Commercial;
        }

        // Метод для проверки, является ли здание дорогой
        public static bool IsRoad(TileType type)
        {
            return type == TileType.Road;
        }
    }
}