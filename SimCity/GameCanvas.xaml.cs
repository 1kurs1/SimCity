using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SimCity.Game;
using SimCity.Map;

namespace SimCity.UI
{
    public partial class GameCanvas : UserControl
    {
        private Point lastMousePosition;
        private bool isDragging;
        private ScaleTransform cameraScale = new ScaleTransform(1, 1);
        private TranslateTransform cameraTranslate = new TranslateTransform();
        private ImageBrush grassBrush = null;

        // Рассчитываем размеры карты для камеры
        private double mapPixelWidth = 0;
        private double mapPixelHeight = 0;

        // Смещение для центрирования карты
        private double mapOffsetX = 0;
        private double mapOffsetY = 0;

        public GameCanvas()
        {
            InitializeComponent();

            // Настройка трансформации камеры
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(cameraScale);
            transformGroup.Children.Add(cameraTranslate);
            MainCanvas.RenderTransform = transformGroup;

            Loaded += OnLoaded;
            MainCanvas.MouseWheel += OnMouseWheel;
            MainCanvas.MouseDown += OnMouseDown;
            MainCanvas.MouseUp += OnMouseUp;
            MainCanvas.MouseMove += OnMouseMove;

            // Отключаем обработку колеса для полигонов
            AddHandler(MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheel), true);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CalculateMapSize();
            CenterCamera();
            RenderMap();
        }

        private void CalculateMapSize()
        {
            var controller = GameController.Instance;
            var map = controller.Map;

            // Рассчитываем размеры карты в пикселях
            var mapSize = OrthographicRenderer.CalculateMapSize(map.Width, map.Height);
            mapPixelWidth = mapSize.Width;
            mapPixelHeight = mapSize.Height;

            // Рассчитываем смещение для центрирования карты на канвасе
            mapOffsetX = mapPixelWidth / 2.0;
            mapOffsetY = mapPixelHeight / 4.0; // 1/4 высоты для лучшего визуального центрирования

            // Устанавливаем размер Canvas (увеличиваем для запаса)
            MainCanvas.Width = mapPixelWidth * 2;
            MainCanvas.Height = mapPixelHeight * 2;
        }

        private void CenterCamera()
        {
            // Центрируем камеру на карте
            if (ActualWidth > 0 && ActualHeight > 0)
            {
                // Позиционируем камеру так, чтобы центр карты был в центре экрана
                cameraTranslate.X = (ActualWidth / 2.0) - (mapOffsetX * cameraScale.ScaleX);
                cameraTranslate.Y = (ActualHeight / 2.0) - (mapOffsetY * cameraScale.ScaleY);
            }
        }

        private void RenderMap()
        {
            MainCanvas.Children.Clear();

            var controller = GameController.Instance;
            var map = controller.Map;

            // Сначала рисуем ландшафт
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    DrawLandscapeTile(x, y);
                }
            }

            // Затем здания
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var tile = map.Tiles[x, y];
                    if (tile.Type != TileType.Empty)
                    {
                        DrawBuildingTile(x, y, tile);
                    }
                }
            }
        }

        private void DrawLandscapeTile(int x, int y)
        {
            // Создаем полигон в форме ромба
            var polygon = new Polygon
            {
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 0.5,
                Fill = GetGrassBrush()
            };

            // Устанавливаем точки ромба с учетом смещения центрирования
            var points = OrthographicRenderer.GetTilePolygon(x, y, mapOffsetX, mapOffsetY);
            foreach (var point in points)
            {
                polygon.Points.Add(point);
            }

            // Добавляем обработчик клика
            polygon.Tag = new Point(x, y);
            polygon.MouseDown += OnTileClicked;

            MainCanvas.Children.Add(polygon);
        }

        private ImageBrush GetGrassBrush()
        {
            if (grassBrush == null)
            {
                try
                {
                    // Загружаем спрайт травы
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Sprites/empty.png", UriKind.Absolute);
                    bitmap.EndInit();

                    // Просто ImageBrush без тайлинга
                    grassBrush = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.Fill, // Заполняет весь полигон
                        AlignmentX = AlignmentX.Center,
                        AlignmentY = AlignmentY.Center
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Не удалось загрузить текстуру травы: {ex.Message}");

                    // Создаем однородную текстуру
                    var drawingVisual = new DrawingVisual();
                    using (var dc = drawingVisual.RenderOpen())
                    {
                        // Однородный зеленый цвет
                        dc.DrawRectangle(Brushes.LightGreen, null, new Rect(0, 0, 32, 16));

                        // Легкая текстура
                        Random rnd = new Random(42); // Фиксированный seed для одинаковой текстуры
                        for (int i = 0; i < 10; i++)
                        {
                            double x = rnd.Next(0, 32);
                            double y = rnd.Next(0, 16);
                            dc.DrawEllipse(Brushes.Green, null, new Point(x, y), 1, 1);
                        }
                    }

                    var drawingImage = new DrawingImage(drawingVisual.Drawing);
                    grassBrush = new ImageBrush(drawingImage)
                    {
                        Stretch = Stretch.Fill
                    };
                }
            }
            return grassBrush;
        }

        private void DrawBuildingTile(int x, int y, Tile tile)
        {
            // Создаем полигон в форме ромба
            var polygon = new Polygon
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            // Создаем ImageBrush со спрайтом здания
            ImageBrush buildingBrush;

            try
            {
                // Определяем имя спрайта в зависимости от типа здания
                string spriteName;

                switch (tile.Type)
                {
                    case TileType.Residential:
                        spriteName = "residential.png";
                        break;
                    case TileType.Industrial:
                        spriteName = "industrial.png";
                        break;
                    case TileType.Road:
                        spriteName = "road.png";
                        break;
                    case TileType.Commercial:
                        spriteName = "commercial.png";
                        break;
                    default:
                        spriteName = "empty.png";
                        break;
                }

                // Загружаем изображение
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri($"pack://application:,,,/Assets/Sprites/{spriteName}", UriKind.Absolute);
                bitmap.EndInit();

                buildingBrush = new ImageBrush(bitmap)
                {
                    Stretch = Stretch.Uniform
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки спрайта: {ex.Message}");

                // Создаем цветную заглушку
                SolidColorBrush colorBrush;

                switch (tile.Type)
                {
                    case TileType.Residential:
                        colorBrush = Brushes.LightBlue;
                        break;
                    case TileType.Industrial:
                        colorBrush = Brushes.Gray;
                        break;
                    case TileType.Road:
                        colorBrush = Brushes.DarkGray;
                        break;
                    case TileType.Commercial:
                        colorBrush = Brushes.Gold;
                        break;
                    default:
                        colorBrush = Brushes.White;
                        break;
                }

                // Конвертируем SolidColorBrush в DrawingImage
                var drawingVisual = new DrawingVisual();
                using (var dc = drawingVisual.RenderOpen())
                {
                    dc.DrawRectangle(colorBrush, null, new Rect(0, 0, 32, 32));
                }
                var drawingImage = new DrawingImage(drawingVisual.Drawing);
                buildingBrush = new ImageBrush(drawingImage);
            }

            polygon.Fill = buildingBrush;

            // Устанавливаем точки ромба с учетом смещения центрирования
            var points = OrthographicRenderer.GetTilePolygon(x, y, mapOffsetX, mapOffsetY);
            foreach (var point in points)
            {
                polygon.Points.Add(point);
            }

            // Поднимаем здание над ландшафтом
            Canvas.SetZIndex(polygon, 10);

            MainCanvas.Children.Add(polygon);
        }

        private void OnTileClicked(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (sender is Polygon polygon && polygon.Tag is Point gridPos)
            {
                int x = (int)gridPos.X;
                int y = (int)gridPos.Y;

                GameController.Instance.Build(x, y);
                UpdateTile(x, y);
            }
        }

        private void UpdateTile(int x, int y)
        {
            // Найти и удалить полигон ландшафта на этой позиции
            for (int i = MainCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (MainCanvas.Children[i] is Polygon polygon && polygon.Tag is Point pos)
                {
                    if ((int)pos.X == x && (int)pos.Y == y)
                    {
                        MainCanvas.Children.RemoveAt(i);
                        break;
                    }
                }
            }

            // Нарисовать новый тайл (ландшафт + здание если есть)
            var tile = GameController.Instance.Map.Tiles[x, y];

            // Нарисовать ландшафт
            DrawLandscapeTile(x, y);

            // Нарисовать здание если есть
            if (tile.Type != TileType.Empty)
            {
                DrawBuildingTile(x, y, tile);
            }
        }

        // Управление камерой
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                isDragging = true;
                lastMousePosition = e.GetPosition(this);
                this.Cursor = Cursors.Hand;
                e.Handled = true;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
            {
                isDragging = false;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(this);
                Vector delta = currentPosition - lastMousePosition;

                // Двигаем камеру
                cameraTranslate.X += delta.X;
                cameraTranslate.Y += delta.Y;

                lastMousePosition = currentPosition;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Получаем позицию курсора относительно Canvas
            Point mousePos = e.GetPosition(MainCanvas);

            // Запоминаем текущие трансформации
            double scaleX = cameraScale.ScaleX;
            double scaleY = cameraScale.ScaleY;
            double translateX = cameraTranslate.X;
            double translateY = cameraTranslate.Y;

            // Вычисляем масштаб
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            double newScaleX = scaleX * zoomFactor;
            double newScaleY = scaleY * zoomFactor;

            // Ограничиваем масштаб
            newScaleX = Math.Max(0.2, Math.Min(3.0, newScaleX));
            newScaleY = Math.Max(0.2, Math.Min(3.0, newScaleY));

            // Корректируем позицию для сохранения точки под курсором
            double relativeX = (mousePos.X * scaleX + translateX) / scaleX;
            double relativeY = (mousePos.Y * scaleY + translateY) / scaleY;

            cameraScale.ScaleX = newScaleX;
            cameraScale.ScaleY = newScaleY;

            cameraTranslate.X = relativeX * newScaleX - mousePos.X * newScaleX;
            cameraTranslate.Y = relativeY * newScaleY - mousePos.Y * newScaleY;

            e.Handled = true;
        }

        // Метод для принудительного обновления карты
        public void RefreshMap()
        {
            RenderMap();
        }

        public void ResetCamera()
        {
            cameraScale.ScaleX = 1.0;
            cameraScale.ScaleY = 1.0;
            CenterCamera();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            CenterCamera();
        }
    }
}