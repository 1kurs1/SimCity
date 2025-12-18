namespace SimCity.Map
{
    public class Tile
    {
        public int X { get; }
        public int Y { get; }
        public TileType Type { get; set; }
        public LandscapeType Landscape { get; set; }

        // Для зданий, занимающих несколько клеток
        public string BuildingId { get; set; }
        public int BuildingRootX { get; set; }
        public int BuildingRootY { get; set; }
        public bool IsBuildingRoot { get; set; }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
            Type = TileType.Empty;
            Landscape = LandscapeType.Grass;
            BuildingId = null;
            BuildingRootX = -1;
            BuildingRootY = -1;
            IsBuildingRoot = false;
        }

        public void SetBuilding(string buildingId, int rootX, int rootY, bool isRoot)
        {
            BuildingId = buildingId;
            BuildingRootX = rootX;
            BuildingRootY = rootY;
            IsBuildingRoot = isRoot;
        }

        public void ClearBuilding()
        {
            BuildingId = null;
            BuildingRootX = -1;
            BuildingRootY = -1;
            IsBuildingRoot = false;
        }
    }
}