namespace SimCity.Map
{
    public class GameMap
    {
        public int Width { get; }
        public int Height { get; }
        public Tile[,] Tiles { get; }

        public GameMap(int width, int height)
        {
            Width = width; Height = height;
            Tiles = new Tile[Width, Height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Tiles[x, y] = new Tile(x, y);
        }
    }
}
