using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimCity
{
    public class MapManager
    {
        private const int TILE_HALF_WIDTH = 16;
        private const int TILE_HALF_HEIGHT = 8;
        private Tile[,] tileGrid;
        private ObservableCollection<Tile> tiles = new ObservableCollection<Tile>();
        public ObservableCollection<Tile> Tiles => tiles;

        public void GenerateMap(int size)
        {
            tiles.Clear();
            tileGrid = new Tile[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    TileType tileType = TileType.Grass;

                    if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                    {
                        tileType = TileType.Water;
                    }

                    double screenX = (x - y) * TILE_HALF_WIDTH;
                    double screenY = (x + y) * TILE_HALF_HEIGHT;

                    var tile = new Tile
                    {
                        Type = tileType,
                        X = x,
                        Y = y,
                        ScreenX = screenX,
                        ScreenY = screenY,
                        HasRoadAccess = false,
                        Building = null
                    };

                    tile.SpritePath = GetSpritePath(tile);
                    tiles.Add(tile);
                    tileGrid[x, y] = tile;
                }
            }

            UpdateRoadAccess();
        }

        public void SetTileType(int x, int y, TileType type)
        {
            if (IsValidCoordinate(x, y) && tileGrid[x, y].Building == null)
            {
                if (type == TileType.Road)
                {
                    if (tileGrid[x, y].Type == TileType.Water)
                    {
                        return;
                    }
                }

                tileGrid[x, y].Type = type;
                tileGrid[x, y].SpritePath = GetSpritePath(tileGrid[x, y]);
                UpdateRoadAccess();
            }
        }

        public Tile GetTile(int x, int y)
        {
            if (IsValidCoordinate(x, y))
                return tileGrid[x, y];
            return null;
        }

        public bool HasRoadAccess(int x, int y, int radius = 2)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    Tile tile = GetTile(x + dx, y + dy);
                    if (tile != null && tile.Type == TileType.Road)
                        return true;
                }
            }
            return false;
        }

        public bool RemoveBuilding(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;

            var tile = GetTile(x, y);
            if (tile == null) return false;

            if (tile.Building is Zone zone)
            {
                // Если это зона со зданием - сносим только здание
                if (zone.HasBuilding)
                {
                    // Возвращаем зону к базовому состоянию
                    zone.HasBuilding = false;
                    zone.Level = 0;

                    // Восстанавливаем размеры и офсеты зоны
                    zone.Width = 1;
                    zone.Height = 1;
                    zone.OffsetX = 0;
                    zone.OffsetY = 0;
                    zone.SpriteWidth = 32;
                    zone.SpriteHeight = 16;
                    zone.SpritePath = zone.GetZoneSpritePath();

                    // Освобождаем занятые тайлы
                    int oldWidth = zone.Width;
                    int oldHeight = zone.Height;

                    for (int dx = 0; dx < oldWidth; dx++)
                    {
                        for (int dy = 0; dy < oldHeight; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            var occupiedTile = GetTile(x + dx, y + dy);
                            if (occupiedTile != null && occupiedTile.Building is OccupiedTile occupied)
                            {
                                if (occupied.OriginalBuilding == zone)
                                {
                                    occupiedTile.Building = null;
                                    occupiedTile.SpritePath = GetSpritePath(occupiedTile);
                                }
                            }
                        }
                    }

                    tile.SpriteWidth = zone.SpriteWidth;
                    tile.SpriteHeight = zone.SpriteHeight;
                    tile.SpritePath = zone.SpritePath;

                    return true;
                }
                else
                {
                    // Если это просто зона без здания - сносится вместе с зоной
                    return RemoveZone(x, y);
                }
            }
            else if (tile.Building != null)
            {
                // Находим основное здание (если это OccupiedTile)
                Building mainBuilding = tile.Building;
                if (tile.Building is OccupiedTile occupiedTile)
                {
                    mainBuilding = occupiedTile.OriginalBuilding;
                }

                // Удаляем все тайлы этого здания
                if (mainBuilding != null)
                {
                    for (int dx = 0; dx < mainBuilding.Width; dx++)
                    {
                        for (int dy = 0; dy < mainBuilding.Height; dy++)
                        {
                            var buildingTile = GetTile(mainBuilding.X + dx, mainBuilding.Y + dy);
                            if (buildingTile != null)
                            {
                                buildingTile.Building = null;
                                buildingTile.SpritePath = GetSpritePath(buildingTile);
                                buildingTile.SpriteWidth = 32;
                                buildingTile.SpriteHeight = 16;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public bool RemoveZone(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;

            var tile = GetTile(x, y);
            if (tile == null || tile.Building == null) return false;

            // Находим основную зону
            Zone mainZone = null;

            if (tile.Building is Zone zone)
            {
                mainZone = zone;
            }
            else if (tile.Building is OccupiedTile occupiedTile)
            {
                if (occupiedTile.OriginalBuilding is Zone originalZone)
                {
                    mainZone = originalZone;
                }
            }

            if (mainZone == null) return false;

            // Удаляем все тайлы этой зоны
            for (int dx = 0; dx < mainZone.Width; dx++)
            {
                for (int dy = 0; dy < mainZone.Height; dy++)
                {
                    var zoneTile = GetTile(mainZone.X + dx, mainZone.Y + dy);
                    if (zoneTile != null)
                    {
                        // Проверяем, что это действительно часть той же зоны
                        if (zoneTile.Building is Zone z && z == mainZone)
                        {
                            zoneTile.Building = null;
                            zoneTile.SpritePath = GetSpritePath(zoneTile);
                        }
                        else if (zoneTile.Building is OccupiedTile ot && ot.OriginalBuilding == mainZone)
                        {
                            zoneTile.Building = null;
                            zoneTile.SpritePath = GetSpritePath(zoneTile);
                        }
                    }
                }
            }

            return true;
        }

        public bool RemoveRoad(int x, int y)
        {
            if (!IsValidCoordinate(x, y)) return false;

            var tile = GetTile(x, y);
            if (tile == null || tile.Type != TileType.Road) return false;

            // Сносим все на этой клетке (включая здания и зоны)
            tile.Building = null;
            tile.Type = TileType.Grass;
            tile.SpritePath = GetSpritePath(tile);

            // Обновляем доступ к дорогам
            UpdateRoadAccess();

            return true;
        }

        public void PlaceBuilding(int x, int y, Building building)
        {
            if (!CanPlaceBuilding(x, y, building)) return;

            // Для зон ограничиваем размер 1x1
            if (building is Zone)
            {
                building.Width = 1;
                building.Height = 1;
            }

            building.X = x;
            building.Y = y;

            // Расчет координат с учетом размера здания И оффсета
            double screenX = (x - y) * 16;
            double screenY = (x + y) * 8;

            // Если здание больше 1x1, нужно сместить
            if (building.Width > 1 || building.Height > 1)
            {
                screenX -= (building.Width - 1) * 16;
                screenY -= (building.Height - 1) * 8;
            }

            // ВАЖНО: Применяем оффсет самого здания
            screenX += building.OffsetX;
            screenY += building.OffsetY;

            // Находим основной тайл для здания
            var buildingTile = GetTile(x, y);
            if (buildingTile != null)
            {
                // Очищаем тайл перед размещением нового здания
                buildingTile.Building = null;

                // Размещаем новое здание
                buildingTile.Building = building;

                // Устанавливаем ScreenX и ScreenY в Tile - они вызовут OnPropertyChanged через сеттеры
                buildingTile.ScreenX = screenX;
                buildingTile.ScreenY = screenY;

                buildingTile.SpriteWidth = building.SpriteWidth;
                buildingTile.SpriteHeight = building.SpriteHeight;
                buildingTile.SpritePath = building.SpritePath;

                // Обновляем ZIndex
                buildingTile.ZIndex = x + y;
                if (building.Height > 1)
                {
                    buildingTile.ZIndex += building.Height * 0.5;
                }

                // НЕ вызываем OnPropertyChanged вручную - сеттеры делают это автоматически
            }

            // Занимаем остальные тайлы под здание (если оно больше 1x1)
            for (int dx = 0; dx < building.Width; dx++)
            {
                for (int dy = 0; dy < building.Height; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Основной тайл уже обработан

                    var occupiedTile = GetTile(x + dx, y + dy);
                    if (occupiedTile != null)
                    {
                        // Очищаем тайл перед созданием OccupiedTile
                        occupiedTile.Building = null;

                        // Создаем OccupiedTile
                        occupiedTile.Building = new OccupiedTile(building);
                        if (occupiedTile.Type != TileType.Road)
                        {
                            occupiedTile.SpritePath = "";
                            occupiedTile.SpriteWidth = 32;
                            occupiedTile.SpriteHeight = 16;
                        }
                    }
                }
            }

            // Обновляем спрайты
            UpdateBuildingSprites();
        }


        public bool CanPlaceBuilding(int x, int y, Building building)
        {
            // Для зон проверяем только 1 тайл
            int widthToCheck = building is Zone ? 1 : building.Width;
            int heightToCheck = building is Zone ? 1 : building.Height;

            for (int dx = 0; dx < widthToCheck; dx++)
            {
                for (int dy = 0; dy < heightToCheck; dy++)
                {
                    var tile = GetTile(x + dx, y + dy);
                    if (tile == null) return false;               // тайл за пределами карты
                    if (tile.Type != TileType.Grass) return false; // можно строить только на Grass

                    // НОВАЯ ПРОВЕРКА: нельзя строить на занятых тайлах
                    if (tile.Building != null)
                    {
                        // Для зоны нельзя строить ни на чем, кроме пустого места
                        if (building is Zone) return false;

                        // Для других зданий можно строить только на пустых тайлах
                        // или OccupiedTile (но это уже проверяется в OccupiedTile)
                        if (!(tile.Building is OccupiedTile)) return false;
                    }
                }
            }

            // Проверка доступа к дороге
            bool hasRoadAccess = false;
            for (int dx = 0; dx < widthToCheck; dx++)
            {
                for (int dy = 0; dy < heightToCheck; dy++)
                {
                    if (HasRoadAccess(x + dx, y + dy, 2))
                    {
                        hasRoadAccess = true;
                        break;
                    }
                }
                if (hasRoadAccess) break;
            }

            return hasRoadAccess;
        }

        public void FixAllBuildingPositions()
        {
            foreach (var tile in Tiles)
            {
                if (tile.Building != null && !(tile.Building is OccupiedTile))
                {
                    var building = tile.Building;

                    // Пересчитываем позицию с учетом оффсета
                    double screenX = (building.X - building.Y) * 16;
                    double screenY = (building.X + building.Y) * 8;

                    if (building.Width > 1 || building.Height > 1)
                    {
                        screenX -= (building.Width - 1) * 16;
                        screenY -= (building.Height - 1) * 8;
                    }

                    screenX += building.OffsetX;
                    screenY += building.OffsetY;

                    // Устанавливаем новые координаты - сеттеры вызовут OnPropertyChanged
                    tile.ScreenX = screenX;
                    tile.ScreenY = screenY;

                    // Обновляем размеры спрайта
                    tile.SpriteWidth = building.SpriteWidth;
                    tile.SpriteHeight = building.SpriteHeight;

                    // НЕ вызываем OnPropertyChanged вручную
                }
            }
        }

        public void UpdateBuilding(Tile tile, Building updatedBuilding)
        {
            if (tile == null || tile.Building == null) return;

            // Пересчитываем координаты с учетом оффсета
            double screenX = (updatedBuilding.X - updatedBuilding.Y) * 16;
            double screenY = (updatedBuilding.X + updatedBuilding.Y) * 8;

            if (updatedBuilding.Width > 1 || updatedBuilding.Height > 1)
            {
                screenX -= (updatedBuilding.Width - 1) * 16;
                screenY -= (updatedBuilding.Height - 1) * 8;
            }

            // ПРИМЕНЯЕМ ОФФСЕТ
            screenX += updatedBuilding.OffsetX;
            screenY += updatedBuilding.OffsetY;

            // Основной тайл
            tile.Building = updatedBuilding;
            tile.SpritePath = updatedBuilding.SpritePath;
            tile.SpriteWidth = updatedBuilding.SpriteWidth;
            tile.SpriteHeight = updatedBuilding.SpriteHeight;
            tile.ScreenX = screenX;
            tile.ScreenY = screenY;

            // OccupiedTile для остальных тайлов
            for (int dx = 0; dx < updatedBuilding.Width; dx++)
            {
                for (int dy = 0; dy < updatedBuilding.Height; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    var t = GetTile(updatedBuilding.X + dx, updatedBuilding.Y + dy);
                    if (t != null && t.Type == TileType.Grass)
                    {
                        t.Building = new OccupiedTile(updatedBuilding);
                        t.SpritePath = "";
                        t.SpriteWidth = 32;
                        t.SpriteHeight = 16;
                    }
                }
            }
        }

        private void UpdateBuildingSprites()
        {
            foreach (var tile in tiles)
            {
                // НЕ сбрасываем дороги!
                if (tile.Type == TileType.Road)
                {
                    tile.SpritePath = "Assets/road.png";
                    continue;
                }

                tile.SpritePath = GetSpritePath(tile);
            }
        }

        private void UpdateRoadAccess()
        {
            if (tileGrid == null) return;

            int size = tileGrid.GetLength(0);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    tileGrid[x, y].HasRoadAccess = HasRoadAccess(x, y);
                    tileGrid[x, y].SpritePath = GetSpritePath(tileGrid[x, y]);
                }
            }
        }

        private bool IsValidCoordinate(int x, int y)
        {
            return tileGrid != null && x >= 0 && y >= 0 && x < tileGrid.GetLength(0) && y < tileGrid.GetLength(1);
        }

        private string GetSpritePath(Tile tile)
        {
            if (tile.Building != null)
            {
                if (tile.Building is OccupiedTile)
                {
                    // Для OccupiedTile возвращаем пустую строку
                    return "";
                }

                return tile.Building.SpritePath;
            }

            // Если нет здания - показываем тайл
            switch (tile.Type)
            {
                case TileType.Grass:
                    return tile.HasRoadAccess
                        ? "Assets/grass_road.png"
                        : "Assets/grass.png";
                case TileType.Water:
                    return "Assets/water.png";
                case TileType.Road:
                    return "Assets/road.png";
                default:
                    return "Assets/grass.png";
            }
        }
    }
}