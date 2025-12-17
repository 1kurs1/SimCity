using SimCity.Game;
using System.Collections.ObjectModel;

namespace SimCity.UI
{
    public class MainViewModel
    {
        public ObservableCollection<TileViewModel> Tiles { get; }
        public int MapWidth { get; } = 20;
        public int MapHeight { get; } = 20;

        private readonly GameController m_controller;
        
        public MainViewModel()
        {
            m_controller = new GameController(MapWidth, MapHeight);
            Tiles = new ObservableCollection<TileViewModel>();

            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    var tile = m_controller.Map.Tiles[x, y];
                    Tiles.Add(new TileViewModel(tile, m_controller));
                }
            }
        }
    }
}
