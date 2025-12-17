using SimCity.Map;
using System.ComponentModel;

namespace SimCity.UI
{
    public class TileViewModel : INotifyPropertyChanged
    {
        public int X {  get; }
        public int Y { get; }

        private TileType m_type;
        public TileType Type
        {
            get => m_type;
            set
            {
                m_type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(m_type)));
            }
        }

        public TileViewModel(Tile tile)
        {
            X = tile.X;
            Y = tile.Y;
            m_type = tile.Type;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
