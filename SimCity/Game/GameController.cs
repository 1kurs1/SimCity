using SimCity.Map;

namespace SimCity.Game
{
    public class GameController
    {
        public GameMap Map { get; }
        public GameController(int width, int height)
        {
            Map = new GameMap(width, height);
        }

        public void ToggleResidential(int x, int y)
        {
            var tile = Map.Tiles[x, y];
            tile.Type = tile.Type == TileType.Empty ? TileType.Residential : TileType.Empty;
        }
    }
}
