using System;

namespace SimCity
{
    public class ZoningManager
    {
        private MapManager mapManager;
        private bool isZoningMode = false;

        public event Action<bool> ZoningModeChanged;

        public ZoningManager(MapManager mapManager)
        {
            this.mapManager = mapManager;
        }

        public bool IsZoningMode
        {
            get => isZoningMode;
            set
            {
                if (isZoningMode != value)
                {
                    isZoningMode = value;
                    UpdateAllZonesVisibility();
                    ZoningModeChanged?.Invoke(value);
                }
            }
        }

        public bool CreateZone(int x, int y, ZoneType zoneType)
        {
            if (mapManager == null) return false;

            // Проверяем только 1 тайл
            var tile = mapManager.GetTile(x, y);
            if (tile == null) return false;
            if (tile.Type != TileType.Grass) return false;

            // НОВАЯ ПРОВЕРКА: нельзя создать зону на занятом тайле
            if (tile.Building != null)
            {
                // Можно создать зону только на пустом месте
                // или на OccupiedTile, который не связан с другим зданием
                if (tile.Building is OccupiedTile occupiedTile)
                {
                    // Проверяем, что это OccupiedTile от другой зоны
                    if (!(occupiedTile.OriginalBuilding is Zone))
                    {
                        return false;
                    }
                }
                else if (!(tile.Building is Zone))
                {
                    // Нельзя создать зону на существующем здании
                    return false;
                }
            }

            // Создаем зону 1x1
            var zone = new Zone(zoneType)
            {
                X = x,
                Y = y,
                Width = 1,
                Height = 1
            };

            // Размещаем зону
            mapManager.PlaceBuilding(x, y, zone);

            return true;
        }


        public bool CanUpgrade(Zone zone)
        {
            if (mapManager == null || zone == null || !zone.HasBuilding) return false;

            int targetWidth, targetHeight;

            // Определяем целевой размер для следующего уровня
            if (zone.Level == 1)
            {
                targetWidth = 2;
                targetHeight = 2;
            }
            else if (zone.Level == 2)
            {
                targetWidth = 3;
                targetHeight = 3;
            }
            else
            {
                return false; // Уже максимальный уровень
            }

            // Проверяем, что вся целевая область состоит из зон того же типа
            for (int dx = 0; dx < targetWidth; dx++)
            {
                for (int dy = 0; dy < targetHeight; dy++)
                {
                    int checkX = zone.X + dx;
                    int checkY = zone.Y + dy;

                    var tile = mapManager.GetTile(checkX, checkY);
                    if (tile == null) return false;

                    // Проверяем, что тайл принадлежит нужной зоне
                    bool isValidZone = false;

                    if (tile.Building is Zone otherZone)
                    {
                        if (otherZone.ZoneType == zone.ZoneType &&
                            otherZone.Level == zone.Level)
                        {
                            isValidZone = true;
                        }
                    }
                    else if (tile.Building is OccupiedTile occupiedTile)
                    {
                        if (occupiedTile.OriginalBuilding is Zone originalZone)
                        {
                            if (originalZone.ZoneType == zone.ZoneType &&
                                originalZone.Level == zone.Level)
                            {
                                isValidZone = true;
                            }
                        }
                    }

                    if (!isValidZone)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool UpgradeBuilding(Zone zone)
        {
            if (!CanUpgrade(zone)) return false;

            int newWidth = 0;
            int newHeight = 0;

            // Определяем размер для следующего уровня
            if (zone.Level == 1)
            {
                newWidth = 2;
                newHeight = 2;
            }
            else if (zone.Level == 2)
            {
                newWidth = 3;
                newHeight = 3;
            }
            else
            {
                return false;
            }

            // Сохраняем координаты базового тайла
            int baseX = zone.X;
            int baseY = zone.Y;

            // Очищаем всю область
            for (int dx = 0; dx < newWidth; dx++)
            {
                for (int dy = 0; dy < newHeight; dy++)
                {
                    var tile = mapManager.GetTile(baseX + dx, baseY + dy);
                    if (tile != null)
                    {
                        tile.Building = null;
                        // Восстанавливаем спрайт травы
                        tile.SpritePath = "Assets/grass.png";
                    }
                }
            }

            // Создаем новое здание большего размера
            zone.X = baseX;
            zone.Y = baseY;
            zone.Width = newWidth;
            zone.Height = newHeight;
            zone.Upgrade(); // Это увеличит Level и обновит размеры спрайта

            // Размещаем новое здание
            mapManager.PlaceBuilding(baseX, baseY, zone);

            return true;
        }

        private void UpdateAllZonesVisibility()
        {
            if (mapManager?.Tiles == null) return;

            foreach (var tile in mapManager.Tiles)
            {
                if (tile.Building is Zone zone)
                {
                    zone.SetVisibility(isZoningMode);
                    tile.SpritePath = zone.SpritePath;
                }
            }
        }

        public System.Collections.Generic.List<Zone> GetZonesOfType(ZoneType zoneType)
        {
            var zones = new System.Collections.Generic.List<Zone>();

            if (mapManager?.Tiles == null) return zones;

            foreach (var tile in mapManager.Tiles)
            {
                if (tile.Building is Zone zone && zone.ZoneType == zoneType)
                {
                    zones.Add(zone);
                }
            }

            return zones;
        }

        public System.Collections.Generic.List<Zone> GetReadyToUpgradeZones()
        {
            var readyZones = new System.Collections.Generic.List<Zone>();

            if (mapManager?.Tiles == null) return readyZones;

            foreach (var tile in mapManager.Tiles)
            {
                if (tile.Building is Zone zone && zone.HasBuilding)
                {
                    if (CanUpgrade(zone))
                    {
                        readyZones.Add(zone);
                    }
                }
            }

            return readyZones;
        }
    }
}