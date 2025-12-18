using SimCity.Game;
using SimCity.Map;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace SimCity.UI
{
    public class MainViewModel
    {
        public ObservableCollection<TileViewModel> Tiles { get; }
        public int MapWidth { get; } = 20;
        public int MapHeight { get; } = 20;

        public MainViewModel()
        {
            var controller = GameController.Instance;

            Tiles = new ObservableCollection<TileViewModel>();

            for (int y = 0; y < controller.Map.Height; y++)
            {
                for (int x = 0; x < controller.Map.Width; x++)
                {
                    var tile = controller.Map.Tiles[x, y];
                    Tiles.Add(new TileViewModel(tile));
                }
            }
        }
    }
}
