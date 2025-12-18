using SimCity.Map;
using System.Collections.Generic;

namespace SimCity.Game
{
    public static class BuildingRegistry
    {
        private static readonly Dictionary<string, BuildingInfo> Buildings =
            new Dictionary<string, BuildingInfo>();
        private static readonly Dictionary<TileType, List<string>> BuildingsByType =
            new Dictionary<TileType, List<string>>();

        static BuildingRegistry()
        {
            InitializeBuildings();
        }

        private static void InitializeBuildings()
        {
            // Жилые дома разных размеров
            RegisterBuilding(new BuildingInfo(
                "house_small", TileType.Residential, TileSize.Small,
                100, 10, "house_small.png", 32, 35)); // 10 дохода в секунду

            RegisterBuilding(new BuildingInfo(
                "house_medium", TileType.Residential, TileSize.Medium,
                250, 25, "house_medium.png", 96, 64)); // 25 дохода в секунду

            RegisterBuilding(new BuildingInfo(
                "apartment", TileType.Residential, new TileSize(4, 3),
                500, 50, "apartment.png", 128, 96)); // 50 дохода в секунду

            // Промышленные здания
            RegisterBuilding(new BuildingInfo(
                "factory_small", TileType.Industrial, TileSize.Medium,
                300, 40, "factory_small.png", 96, 64)); // 40 дохода в секунду

            RegisterBuilding(new BuildingInfo(
                "factory_large", TileType.Industrial, new TileSize(5, 4),
                800, 100, "factory_large.png", 160, 128)); // 100 дохода в секунду

            // Коммерческие здания
            RegisterBuilding(new BuildingInfo(
                "shop", TileType.Commercial, TileSize.Small,
                150, 20, "shop.png", 64, 48)); // 20 дохода в секунду

            RegisterBuilding(new BuildingInfo(
                "mall", TileType.Commercial, new TileSize(4, 4),
                600, 80, "mall.png", 128, 128)); // 80 дохода в секунду

            // Дороги (только один тип)
            RegisterBuilding(new BuildingInfo(
                "road", TileType.Road, TileSize.Single,
                50, 0, "road.png", 32, 16)); // 0 дохода, только связь
        }

        private static void RegisterBuilding(BuildingInfo building)
        {
            Buildings[building.Id] = building;

            if (!BuildingsByType.ContainsKey(building.Type))
                BuildingsByType[building.Type] = new List<string>();

            BuildingsByType[building.Type].Add(building.Id);
        }

        public static BuildingInfo GetBuilding(string id)
        {
            return Buildings.ContainsKey(id) ? Buildings[id] : null;
        }

        public static List<string> GetBuildingsOfType(TileType type)
        {
            return BuildingsByType.ContainsKey(type) ?
                BuildingsByType[type] : new List<string>();
        }

        public static BuildingInfo GetRandomBuildingOfType(TileType type)
        {
            var buildings = GetBuildingsOfType(type);
            if (buildings.Count == 0) return null;

            var random = new System.Random();
            var randomId = buildings[random.Next(buildings.Count)];
            return GetBuilding(randomId);
        }

        // Старый метод для обратной совместимости
        public static int GetCost(string buildingId)
        {
            var building = GetBuilding(buildingId);
            return building?.BaseCost ?? 0;
        }

        public static int GetIncome(string buildingId)
        {
            var building = GetBuilding(buildingId);
            return building?.BaseIncome ?? 0;
        }

        // Новый метод для получения информации о здании
        public static string GetBuildingDescription(string buildingId)
        {
            var building = GetBuilding(buildingId);
            if (building == null) return "Неизвестное здание";

            return $"Стоимость: {building.BaseCost}$\n" +
                   $"Доход в секунду: +{building.BaseIncome}$\n" +
                   $"Размер: {building.Size.WidthInTiles}x{building.Size.HeightInTiles}";
        }
    }
}