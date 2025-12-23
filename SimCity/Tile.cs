using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimCity
{
    public class OccupiedTile : Building
    {
        public Building OriginalBuilding { get; }

        public OccupiedTile(Building originalBuilding)
        {
            OriginalBuilding = originalBuilding;
            Width = 1;
            Height = 1;
            SpritePath = ""; // Пустой спрайт
        }
    }

    public enum TileType
    {
        Grass,
        Water,
        Road
    }

    public enum ToolType
    {
        None,
        Road,
        Zone,
        Building,
        Bulldozer,    // Новый инструмент: снос зданий
        ZoneRemover   // Новый инструмент: удаление зон
    }

    public class Tile : INotifyPropertyChanged
    {
        private TileType type;
        private string spritePath;
        private Building building;
        private double spriteWidth = 32;
        private double spriteHeight = 16;
        private double zIndex;

        public TileType Type
        {
            get => type;
            set { type = value; OnPropertyChanged(); }
        }

        public int X { get; set; }
        public int Y { get; set; }

        private double screenX;
        private double screenY;

        public double ScreenX
        {
            get => screenX;
            set { screenX = value; OnPropertyChanged(); }
        }

        public double ScreenY
        {
            get => screenY;
            set { screenY = value; OnPropertyChanged(); }
        }

        public double SpriteWidth
        {
            get => spriteWidth;
            set { spriteWidth = value; OnPropertyChanged(); }
        }

        public double SpriteHeight
        {
            get => spriteHeight;
            set { spriteHeight = value; OnPropertyChanged(); }
        }

        public string SpritePath
        {
            get => spritePath;
            set { spritePath = value; OnPropertyChanged(); }
        }

        public double ZIndex
        {
            get => zIndex;
            set { zIndex = value; OnPropertyChanged(); }
        }

        public bool HasRoadAccess { get; set; }

        public Building Building
        {
            get => building;
            set
            {
                building = value;
                OnPropertyChanged();

                // Обновляем размеры спрайта при установке здания
                if (building != null)
                {
                    // Используем размеры спрайта из здания
                    SpriteWidth = building.SpriteWidth;
                    SpriteHeight = building.SpriteHeight;

                    // Обновляем ZIndex
                    ZIndex = building.X + building.Y;
                    if (building.Height > 1)
                    {
                        ZIndex += building.Height * 0.5;
                    }

                    // Обновляем путь к спрайту
                    SpritePath = building.SpritePath;
                }
                else
                {
                    SpriteWidth = 32;
                    SpriteHeight = 16;
                    ZIndex = X + Y; // Базовый ZIndex для пустого тайла
                    SpritePath = GetBaseSpritePath();
                }
            }
        }

        private string GetBaseSpritePath()
        {
            switch (Type)
            {
                case TileType.Grass:
                    return HasRoadAccess ? "Assets/grass_road.png" : "Assets/grass.png";
                case TileType.Water:
                    return "Assets/water.png";
                case TileType.Road:
                    return "Assets/road.png";
                default:
                    return "Assets/grass.png";
            }
        }

        public bool IsOccupied => Building is OccupiedTile;

        public void UpdatePosition(Building building)
        {
            if (building == null) return;

            // ПЕРЕСЧИТЫВАЕМ координаты с учетом размера и оффсета
            double screenX = (building.X - building.Y) * 16;
            double screenY = (building.X + building.Y) * 8;

            // Если здание больше 1x1, нужно сместить
            if (building.Width > 1 || building.Height > 1)
            {
                // Смещаем влево и вверх для многотайловых зданий
                screenX -= (building.Width - 1) * 16;
                screenY -= (building.Height - 1) * 8;
            }

            // ВАЖНО: Применяем оффсет самого здания
            screenX += building.OffsetX;
            screenY += building.OffsetY;

            // Сохраняем в самом здании (опционально)
            building.ScreenX = screenX;
            building.ScreenY = screenY;

            // Обновляем тайл
            ScreenX = screenX;
            ScreenY = screenY;

            // ОБЯЗАТЕЛЬНО обновляем размеры спрайта
            SpriteWidth = building.SpriteWidth;
            SpriteHeight = building.SpriteHeight;

            // Обновляем ZIndex
            ZIndex = building.X + building.Y;
            if (building.Height > 1)
            {
                ZIndex += building.Height * 0.5;
            }

            OnPropertyChanged(nameof(ScreenX));
            OnPropertyChanged(nameof(ScreenY));
            OnPropertyChanged(nameof(SpriteWidth));
            OnPropertyChanged(nameof(SpriteHeight));
            OnPropertyChanged(nameof(ZIndex));
            OnPropertyChanged(nameof(SpritePath));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}