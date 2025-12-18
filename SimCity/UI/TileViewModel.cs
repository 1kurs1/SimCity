using SimCity.Game;
using SimCity.Map;
using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimCity.UI
{
    public class TileViewModel : INotifyPropertyChanged
    {
        private readonly Tile m_tile;
        public int X => m_tile.X;
        public int Y => m_tile.Y;

        public TileType Type
        {
            get => m_tile.Type;
            set
            {
                m_tile.Type = value;
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(Sprite));
            }
        }

        public TileViewModel(Tile tile)
        {
            m_tile = tile;
        }

        public ImageSource Sprite
        {
            get
            {
                // Сначала проверяем, есть ли конкретное здание
                if (!string.IsNullOrEmpty(m_tile.BuildingId))
                {
                    var building = BuildingRegistry.GetBuilding(m_tile.BuildingId);
                    if (building != null)
                    {
                        return LoadImage(building.SpriteName);
                    }
                }

                // Иначе используем стандартные спрайты по типу
                switch (m_tile.Type)
                {
                    case TileType.Residential:
                        return LoadImage("residential.png");
                    case TileType.Industrial:
                        return LoadImage("industrial.png");
                    case TileType.Road:
                        return LoadImage("road.png");
                    case TileType.Commercial:
                        return LoadImage("commercial.png");
                    default:
                        return LoadImage("empty.png");
                }
            }
        }

        // Переименован метод с Load на LoadImage
        private ImageSource LoadImage(string name)
        {
            try
            {
                // Путь к спрайтам - проверьте, правильный ли у вас путь
                string uriString = $"pack://application:,,,/Assets/Sprites/{name}";
                Uri uri = new Uri(uriString, UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch (Exception)
            {
                // Создаем заглушку, если спрайт не найден
                return CreatePlaceholderImage(name);
            }
        }

        private ImageSource CreatePlaceholderImage(string name)
        {
            // Создаем DrawingVisual для заглушки
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                // Разные цвета для разных типов
                Color color;
                switch (m_tile.Type)
                {
                    case TileType.Residential:
                        color = Colors.LightBlue;
                        break;
                    case TileType.Industrial:
                        color = Colors.Gray;
                        break;
                    case TileType.Road:
                        color = Colors.DarkGray;
                        break;
                    case TileType.Commercial:
                        color = Colors.Gold;
                        break;
                    default:
                        color = Colors.LightGreen;
                        break;
                }

                drawingContext.DrawRectangle(
                    new SolidColorBrush(color),
                    new Pen(Brushes.Black, 0.5),
                    new System.Windows.Rect(0, 0, 32, 32));

                // Добавляем текст с названием для отладки
                if (!string.IsNullOrEmpty(name))
                {
                    var text = new FormattedText(
                        name.Replace(".png", ""),
                        System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        6,
                        Brushes.Black);

                    drawingContext.DrawText(text, new System.Windows.Point(2, 10));
                }
            }

            return new DrawingImage(drawingVisual.Drawing);
        }

        public void OnClick()
        {
            GameController.Instance.Build(X, Y);
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(Sprite));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}