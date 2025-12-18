namespace SimCity.Map
{
    public class BuildingInfo
    {
        public string Id { get; set; }
        public TileType Type { get; set; }
        public TileSize Size { get; set; }
        public int BaseCost { get; set; }
        public int BaseIncome { get; set; }
        public string SpriteName { get; set; }
        public int SpriteWidth { get; set; }  // Фактическая ширина спрайта в пикселях
        public int SpriteHeight { get; set; } // Фактическая высота спрайта в пикселях

        public BuildingInfo(string id, TileType type, TileSize size, int cost, int income, string sprite, int spriteW, int spriteH)
        {
            Id = id;
            Type = type;
            Size = size;
            BaseCost = cost;
            BaseIncome = income;
            SpriteName = sprite;
            SpriteWidth = spriteW;
            SpriteHeight = spriteH;
        }
    }
}