namespace SimCity
{
    public abstract class Building
    {
        public int Width { get; set; } = 1;   // размер в тайлах
        public int Height { get; set; } = 1;  // размер в тайлах
        public string SpritePath { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public double ScreenX { get; set; }
        public double ScreenY { get; set; }

        public double OffsetX { get; set; } = 0;
        public double OffsetY { get; set; } = 0;

        public bool UseSimplePositioning { get; set; } = false;

        public string BuildingType { get; set; } = "Unknown";

        // Новые свойства для размеров спрайта в пикселях
        public double SpriteWidth { get; set; } = 32;   // по умолчанию 1 тайл
        public double SpriteHeight { get; set; } = 16;  // по умолчанию 1 тайл
    }
}