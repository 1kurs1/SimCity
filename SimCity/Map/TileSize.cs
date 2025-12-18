namespace SimCity.Map
{
    public struct TileSize
    {
        public int WidthInTiles { get; }
        public int HeightInTiles { get; }

        public TileSize(int width, int height)
        {
            WidthInTiles = width;
            HeightInTiles = height;
        }

        public static readonly TileSize Single = new TileSize(1, 1);
        public static readonly TileSize Small = new TileSize(2, 2);
        public static readonly TileSize Medium = new TileSize(3, 3);
        public static readonly TileSize Large = new TileSize(4, 4);

        public bool IsMultiTile
        {
            get { return WidthInTiles > 1 || HeightInTiles > 1; }
        }

        public int TotalCells
        {
            get { return WidthInTiles * HeightInTiles; }
        }

        public bool Contains(int relativeX, int relativeY)
        {
            return relativeX >= 0 && relativeX < WidthInTiles &&
                   relativeY >= 0 && relativeY < HeightInTiles;
        }

        public System.Collections.Generic.IEnumerable<(int x, int y)> GetAllPositions()
        {
            for (int x = 0; x < WidthInTiles; x++)
            {
                for (int y = 0; y < HeightInTiles; y++)
                {
                    yield return (x, y);
                }
            }
        }
    }
}