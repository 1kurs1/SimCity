using System.Collections.ObjectModel;

namespace SimCity.UI
{
    public class MainViewModel
    {
        public ObservableCollection<TileViewModel> Tiles { get; }
        public int MapWidth { get; } = 20;
        public int MapHeight { get; } = 20;
        
        public MainViewModel()
        {
            Tiles = new ObservableCollection<TileViewModel>();
            for (int y = 0; y < MapHeight; y++)
                for (int x = 0; x < MapWidth; x++)
                    Tiles.Add(new TileViewModel(new Map.Tile(x, y)));
        }
    }
}
