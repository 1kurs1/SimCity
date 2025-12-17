namespace SimCity.Map
{
    public class Tile
    {
        public int X { get; }
        public int Y { get; }
        public TileType Type { get; set; }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
            Type = TileType.Empty;
        }
    }
}
