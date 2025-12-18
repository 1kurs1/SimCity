using SimCity.Map;

namespace SimCity.Game
{
    public class EconomySystem
    {
        public void Tick(GameMap map, GameState state)
        {
            int totalIncome = 0;
            int totalUpkeep = 0;

            // Используем HashSet для отслеживания уже обработанных зданий
            var processedBuildings = new System.Collections.Generic.HashSet<(int, int)>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.Tiles[x, y];

                    // Пропускаем пустые клетки
                    if (tile.Type == TileType.Empty)
                        continue;

                    // Если это часть большого здания, обрабатываем только корневую клетку
                    if (tile.IsBuildingRoot || !tile.BuildingId.Contains("_"))
                    {
                        // ✅ Используем реальный BuildingId, а не фиктивный метод
                        string buildingId = tile.BuildingId;

                        // ❌ УБЕРИТЕ ЭТО: GetDefaultBuildingId(tile.Type)

                        if (!string.IsNullOrEmpty(buildingId))
                        {
                            var buildingInfo = BuildingRegistry.GetBuilding(buildingId);

                            if (buildingInfo != null)
                            {
                                // Добавляем доход
                                totalIncome += buildingInfo.BaseIncome;

                                // Добавляем расходы (например, 10% от стоимости)
                                int upkeep = (int)(buildingInfo.BaseCost * 0.1f);
                                totalUpkeep += upkeep;
                            }
                        }
                    }
                }
            }

            // Применяем чистый доход
            int netIncome = totalIncome - totalUpkeep;
            state.Money += netIncome;

            // Для отладки
            if (netIncome != 0)
            {
                System.Diagnostics.Debug.WriteLine($"Экономика: +{totalIncome}$ -{totalUpkeep}$ = {netIncome}$. Баланс: {state.Money}$");
            }
        }

    }
}