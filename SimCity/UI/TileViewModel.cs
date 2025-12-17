using SimCity.Game;
using SimCity.Map;
using System.ComponentModel;

namespace SimCity.UI
{
    public class TileViewModel : INotifyPropertyChanged
    {
        private readonly Tile m_tile;
        private readonly GameController m_controller;   
        public int X => m_tile.X;
        public int Y => m_tile.Y;

        public TileType Type
        {
            get => m_tile.Type;
            set
            {
                m_tile.Type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
            }
        }

        public TileViewModel(Tile tile, GameController gc)
        {
            m_tile = tile;
            m_controller = gc;
        }

        public void OnClick()
        {
            m_controller.ToggleResidential(X, Y);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
