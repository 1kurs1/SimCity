using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SimCity
{
    public partial class MainWindow : Window
    {
        private MapManager mapManager = new MapManager();
        private Camera camera = new Camera();
        private SimulationManager simulationManager;
        private ZoningManager zoningManager;

        private ToolType currentTool = ToolType.None;
        private Building currentBuilding;
        private ZoneType currentZoneType = ZoneType.Residential;
        // УБИРАЕМ currentZoneSize так как зоны теперь только 1x1

        private const int MAP_SIZE = 30;
        private ImageSource powerPlantGhostImage;
        private bool isZoningMode = false;
        private bool isInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGhostImage();

            mapManager.GenerateMap(MAP_SIZE);
            MapItemsControl.ItemsSource = mapManager.Tiles;

            zoningManager = new ZoningManager(mapManager);
            simulationManager = new SimulationManager(mapManager, zoningManager);

            simulationManager.GameTimeChanged += OnGameTimeChanged;
            simulationManager.SimulationUpdated += OnSimulationUpdated;
            zoningManager.ZoningModeChanged += OnZoningModeChanged;

            simulationManager.StartSimulation();

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double mapWidth = MAP_SIZE * 32;
            double mapHeight = MAP_SIZE * 16;

            camera.X = (screenWidth - mapWidth) / 2;
            camera.Y = (screenHeight - mapHeight) / 2;

            UpdateCamera();

            isInitialized = true;
            UpdateStatistics();

            // Тестовое размещение PowerPlant
            // TestPowerPlantPlacement();

            // Исправляем позиции всех зданий
            Dispatcher.BeginInvoke(new Action(() =>
            {
                mapManager.FixAllBuildingPositions();
            }), DispatcherPriority.Background);
        }


        private void LoadGhostImage()
        {
            try
            {
                string[] possiblePaths =
                {
                    "Assets/powerplant.png",
                    "pack://application:,,,/Assets/powerplant.png",
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "powerplant.png")
                };

                BitmapImage originalImage = null;

                foreach (var path in possiblePaths)
                {
                    try
                    {
                        originalImage = new BitmapImage();
                        originalImage.BeginInit();
                        originalImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                        originalImage.CacheOption = BitmapCacheOption.OnLoad;
                        originalImage.EndInit();

                        if (originalImage.PixelWidth > 0)
                            break;
                    }
                    catch
                    {
                        originalImage = null;
                    }
                }

                if (originalImage != null && originalImage.PixelWidth > 0)
                {
                    powerPlantGhostImage = CreateTransparentImage(originalImage, 0.6);
                }
                else
                {
                    powerPlantGhostImage = CreateSimpleColoredGhost(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить ghost изображение: {ex.Message}");
                powerPlantGhostImage = CreateSimpleColoredGhost(true);
            }
        }

        private ImageSource CreateTransparentImage(BitmapImage original, double opacity)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                ImageBrush brush = new ImageBrush(original);
                brush.Opacity = opacity;
                drawingContext.DrawRectangle(brush, null,
                    new Rect(0, 0, original.PixelWidth, original.PixelHeight));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                original.PixelWidth, original.PixelHeight,
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);

            return rtb;
        }

        private ImageSource CreateSimpleColoredGhost(bool canPlace)
        {
            int width = 64;
            int height = 32;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                Rect rect = new Rect(0, 0, width, height);

                Color fillColor = canPlace ? Colors.Green : Colors.Red;
                fillColor.A = 153;

                drawingContext.DrawRectangle(
                    new SolidColorBrush(fillColor),
                    null,
                    rect
                );

                FormattedText text = new FormattedText(
                    "PP",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    14,
                    Brushes.White,
                    1.0
                );

                drawingContext.DrawText(text, new Point(width / 2 - 10, height / 2 - 7));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            return rtb;
        }

        private void UpdateCamera()
        {
            var presenter = VisualTreeHelper.GetChild(MapItemsControl, 0) as FrameworkElement;
            if (presenter != null)
            {
                presenter.RenderTransform = camera.GetTransform();
            }
        }

        private (int x, int y) GetTileFromScreen(Point screenPos)
        {
            return camera.ScreenToTile(screenPos);
        }

        private void UpdateHover(int x, int y)
        {
            var p = camera.TileToScreen(x, y);
            Canvas.SetLeft(HoverRect, p.X);
            Canvas.SetTop(HoverRect, p.Y);
            HoverRect.Visibility = Visibility.Visible;
        }

        private void UpdateGhost(int x, int y)
        {
            if (currentTool == ToolType.Bulldozer || currentTool == ToolType.ZoneRemover)
            {
                // Для инструментов сноса показываем красный квадрат
                var tile = mapManager.GetTile(x, y);
                bool canRemove = false;

                if (currentTool == ToolType.Bulldozer)
                {
                    // Можно снести если есть здание
                    canRemove = tile != null && tile.Building != null;
                }
                else if (currentTool == ToolType.ZoneRemover)
                {
                    // Можно удалить зону если есть зона
                    canRemove = tile != null &&
                               (tile.Building is Zone ||
                                tile.Building is OccupiedTile occupiedTile && occupiedTile.OriginalBuilding is Zone);
                }

                var p = camera.TileToScreen(x, y);
                Canvas.SetLeft(BuildingGhost, p.X);
                Canvas.SetTop(BuildingGhost, p.Y);
                BuildingGhost.Width = 32 * camera.Scale;
                BuildingGhost.Height = 16 * camera.Scale;
                BuildingGhost.Source = CreateDemolitionGhost(canRemove);
                BuildingGhost.Opacity = 0.7;
                BuildingGhost.Visibility = Visibility.Visible;
                return;
            }

            if (currentBuilding == null) return;

            bool canPlace = false;

            if (currentBuilding is Zone zone)
            {
                var tile = mapManager.GetTile(x, y);
                if (tile != null && tile.Type == TileType.Grass)
                {
                    if (tile.Building == null)
                    {
                        canPlace = true;
                    }
                    else if (tile.Building is OccupiedTile occupiedTile)
                    {
                        canPlace = occupiedTile.OriginalBuilding is Zone;
                    }
                    else if (tile.Building is Zone)
                    {
                        // Можно перезаписать существующую зону
                        canPlace = true;
                    }
                }
            }
            else
            {
                canPlace = mapManager.CanPlaceBuilding(x, y, currentBuilding);
            }

            // ТОЧНО ТАКОЙ ЖЕ расчет что и в MapManager.PlaceBuilding и Tile.UpdatePosition
            double screenX = (x - y) * 16;
            double screenY = (x + y) * 8;

            if (!(currentBuilding is Zone))
            {
                screenX -= (currentBuilding.Width - 1) * 16;
                screenY -= (currentBuilding.Height - 1) * 8;
            }

            // ВАЖНО: Применяем оффсет
            screenX += currentBuilding.OffsetX;
            screenY += currentBuilding.OffsetY;

            // Применяем масштаб камеры и смещение камеры
            screenX = screenX * camera.Scale + camera.X;
            screenY = screenY * camera.Scale + camera.Y;

            Canvas.SetLeft(BuildingGhost, screenX);
            Canvas.SetTop(BuildingGhost, screenY);

            if (currentBuilding is Zone)
            {
                BuildingGhost.Width = currentBuilding.SpriteWidth * camera.Scale;
                BuildingGhost.Height = currentBuilding.SpriteHeight * camera.Scale;
                BuildingGhost.Source = CreateZoneGhost(canPlace, currentZoneType);
                BuildingGhost.Opacity = canPlace ? 0.6 : 0.3;
            }
            else
            {
                BuildingGhost.Width = currentBuilding.SpriteWidth * camera.Scale;
                BuildingGhost.Height = currentBuilding.SpriteHeight * camera.Scale;

                if (powerPlantGhostImage != null && currentBuilding is PowerPlant)
                {
                    BuildingGhost.Source = powerPlantGhostImage;
                    BuildingGhost.Opacity = canPlace ? 0.6 : 0.3;
                }
                else
                {
                    BuildingGhost.Source = CreateSimpleColoredGhost(canPlace);
                    BuildingGhost.Opacity = 0.7;
                }
            }

            BuildingGhost.Visibility = Visibility.Visible;
        }

        private ImageSource CreateDemolitionGhost(bool canRemove)
        {
            int width = 32;
            int height = 16;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                Rect rect = new Rect(0, 0, width, height);

                Color fillColor = canRemove ? Colors.Red : Colors.DarkGray;
                fillColor.A = 150;

                drawingContext.DrawRectangle(
                    new SolidColorBrush(fillColor),
                    new Pen(Brushes.White, 1),
                    rect
                );

                string text = "X";
                FormattedText formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    12,
                    Brushes.White,
                    1.0
                );

                drawingContext.DrawText(formattedText, new Point(width / 2 - 4, height / 2 - 6));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            return rtb;
        }

        private ImageSource CreateZoneGhost(bool canPlace, ZoneType zoneType)
        {
            int width = 32;    // 1 тайл
            int height = 16;   // 1 тайл

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                Rect rect = new Rect(0, 0, width, height);

                Color fillColor = canPlace ? Colors.Green : Colors.Red;
                fillColor.A = 100;

                drawingContext.DrawRectangle(
                    new SolidColorBrush(fillColor),
                    new Pen(Brushes.White, 1),
                    rect
                );

                string zoneText = "1x1";
                if (zoneType == ZoneType.Residential)
                    zoneText = "Ж " + zoneText;
                else if (zoneType == ZoneType.Commercial)
                    zoneText = "К " + zoneText;
                else if (zoneType == ZoneType.Industrial)
                    zoneText = "П " + zoneText;

                FormattedText text = new FormattedText(
                    zoneText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    8, // Меньший размер текста
                    Brushes.White,
                    1.0
                );

                drawingContext.DrawText(text, new Point(width / 2 - 15, height / 2 - 6));
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            return rtb;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(MainCanvas);

            if (camera.IsDragging)
            {
                camera.UpdateDrag(pos);
                UpdateCamera();
                return;
            }

            var tileCoords = GetTileFromScreen(pos);
            int x = tileCoords.x;
            int y = tileCoords.y;

            if (x < 0 || y < 0 || x >= MAP_SIZE || y >= MAP_SIZE)
            {
                HoverRect.Visibility = Visibility.Collapsed;
                BuildingGhost.Visibility = Visibility.Collapsed;
                return;
            }

            UpdateHover(x, y);

            if ((currentTool == ToolType.Building || currentTool == ToolType.Zone) && currentBuilding != null)
                UpdateGhost(x, y);
            else if (currentTool == ToolType.Bulldozer || currentTool == ToolType.ZoneRemover)
                UpdateGhost(x, y); // Для инструментов сноса тоже показываем призрак
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MainCanvas);

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                camera.StartDrag(pos);
                return;
            }

            var tileCoords = GetTileFromScreen(pos);
            int x = tileCoords.x;
            int y = tileCoords.y;

            if (x < 0 || y < 0 || x >= MAP_SIZE || y >= MAP_SIZE)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (currentTool == ToolType.Road)
                {
                    mapManager.SetTileType(x, y, TileType.Road);
                }
                else if (currentTool == ToolType.Zone)
                {
                    // Создаем зону 1x1
                    if (zoningManager.CreateZone(x, y, currentZoneType))
                    {
                        UpdateStatistics();
                    }
                }
                else if (currentTool == ToolType.Building && currentBuilding != null)
                {
                    if (mapManager.CanPlaceBuilding(x, y, currentBuilding))
                    {
                        mapManager.PlaceBuilding(x, y, currentBuilding);
                        ResetToolSelection();
                    }
                }
                else if (currentTool == ToolType.Bulldozer)
                {
                    // Снос здания/строения
                    if (mapManager.RemoveBuilding(x, y))
                    {
                        UpdateStatistics();
                    }
                }
                else if (currentTool == ToolType.ZoneRemover)
                {
                    // Удаление зоны
                    if (mapManager.RemoveZone(x, y))
                    {
                        UpdateStatistics();
                    }
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                // ПКМ - удаление дороги и всего на ней
                if (mapManager.RemoveRoad(x, y))
                {
                    UpdateStatistics();
                }
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            camera.EndDrag();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            camera.Zoom(e.Delta, e.GetPosition(MainCanvas));
            UpdateCamera();
        }

        // Обработчики для инструментов
        private void RoadToolRadio_Checked(object sender, RoutedEventArgs e)
        {
            currentTool = ToolType.Road;
            currentBuilding = null;
            BuildingGhost.Visibility = Visibility.Collapsed;
        }

        private void ZoneToolRadio_Checked(object sender, RoutedEventArgs e)
        {
            currentTool = ToolType.Zone;
            currentBuilding = new Zone(currentZoneType);
            BuildingGhost.Visibility = Visibility.Visible;
        }

        private void ZoneTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio == ResidentialZoneRadio)
            {
                currentZoneType = ZoneType.Residential;
            }
            else if (radio == CommercialZoneRadio)
            {
                currentZoneType = ZoneType.Commercial;
            }
            else if (radio == IndustrialZoneRadio)
            {
                currentZoneType = ZoneType.Industrial;
            }

            if (currentTool == ToolType.Zone)
            {
                currentBuilding = new Zone(currentZoneType);
                var pos = Mouse.GetPosition(MainCanvas);
                var tileCoords = GetTileFromScreen(pos);
                if (tileCoords.x >= 0 && tileCoords.y >= 0 && tileCoords.x < MAP_SIZE && tileCoords.y < MAP_SIZE)
                {
                    UpdateGhost(tileCoords.x, tileCoords.y);
                }
            }
        }

        private void PowerPlantToolRadio_Checked(object sender, RoutedEventArgs e)
        {
            currentTool = ToolType.Building;
            currentBuilding = new PowerPlant();
            BuildingGhost.Visibility = Visibility.Visible;
        }

        private void ViewModeRadio_Checked(object sender, RoutedEventArgs e)
        {
            // Проверяем, что инициализация завершена
            if (!isInitialized || zoningManager == null) return;

            var radio = sender as RadioButton;
            if (radio == GameModeRadio)
            {
                zoningManager.IsZoningMode = false;
            }
            else if (radio == ZoningModeRadio)
            {
                zoningManager.IsZoningMode = true;
            }
        }

        // Обработчики событий симуляции
        private void OnGameTimeChanged(TimeSpan gameTime)
        {
            Dispatcher.Invoke(() =>
            {
                TimeTextBlock.Text = $"Время: {gameTime.Days}д {gameTime.Hours:00}:{gameTime.Minutes:00}";
                Title = $"SimCity 3000 - {TimeTextBlock.Text}";
            });
        }

        private void OnSimulationUpdated()
        {
            Dispatcher.Invoke(() =>
            {
                UpdateStatistics();
            });
        }

        private void OnZoningModeChanged(bool isZoningMode)
        {
            this.isZoningMode = isZoningMode;

            if (mapManager?.Tiles != null)
            {
                foreach (var tile in mapManager.Tiles)
                {
                    if (tile.Building is Zone zone)
                    {
                        tile.SpritePath = zone.SpritePath;
                    }
                }
            }
        }

        private void UpdateStatistics()
        {
            if (!isInitialized || zoningManager == null) return;

            try
            {
                var residential = zoningManager.GetZonesOfType(ZoneType.Residential);
                var commercial = zoningManager.GetZonesOfType(ZoneType.Commercial);
                var industrial = zoningManager.GetZonesOfType(ZoneType.Industrial);
                var readyToUpgrade = zoningManager.GetReadyToUpgradeZones();

                ResidentialCountText.Text = $"Жилые: {residential?.Count ?? 0}";
                CommercialCountText.Text = $"Коммерческие: {commercial?.Count ?? 0}";
                IndustrialCountText.Text = $"Промышленные: {industrial?.Count ?? 0}";
                ReadyToUpgradeText.Text = $"Готовы к прокачке: {readyToUpgrade?.Count ?? 0}";
            }
            catch
            {
                // Игнорируем ошибки при обновлении статистики
            }
        }

        // Вспомогательные методы
        private void ResetToolSelection()
        {
            currentTool = ToolType.None;
            currentBuilding = null;
            BuildingGhost.Visibility = Visibility.Collapsed;

            foreach (var child in ((Grid)Content).Children)
            {
                if (child is StackPanel panel)
                {
                    foreach (var c in panel.Children)
                    {
                        if (c is RadioButton radio && radio.GroupName == "Tool")
                        {
                            radio.IsChecked = false;
                        }
                    }
                }
            }
        }

        private void TogglePause()
        {
            if (simulationManager == null) return;

            if (simulationManager.IsPaused())
            {
                simulationManager.ResumeSimulation();
                PauseButton.Content = "Пауза (P)";
            }
            else
            {
                simulationManager.PauseSimulation();
                PauseButton.Content = "Продолжить (P)";
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void BulldozerToolRadio_Checked(object sender, RoutedEventArgs e)
        {
            currentTool = ToolType.Bulldozer;
            currentBuilding = null;
            BuildingGhost.Visibility = Visibility.Collapsed;
        }

        private void ZoneRemoverToolRadio_Checked(object sender, RoutedEventArgs e)
        {
            currentTool = ToolType.ZoneRemover;
            currentBuilding = null;
            BuildingGhost.Visibility = Visibility.Collapsed;
        }

        private void RoadRemoverToolRadio_Checked(object sender, RoutedEventArgs e)
        {
            currentTool = ToolType.Road; // Используем тот же инструмент, но с другим действием
            currentBuilding = null;
            BuildingGhost.Visibility = Visibility.Collapsed;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.P)
            {
                TogglePause();
            }
            else if (e.Key == Key.Z)
            {
                if (!isInitialized || zoningManager == null) return;

                if (isZoningMode)
                {
                    GameModeRadio.IsChecked = true;
                }
                else
                {
                    ZoningModeRadio.IsChecked = true;
                }
            }
            else if (e.Key == Key.Add || e.Key == Key.OemPlus)
            {
                if (simulationManager != null)
                {
                    simulationManager.SetSimulationSpeed(
                        Math.Min(3, simulationManager.GetSimulationSpeed() + 1));
                }
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                if (simulationManager != null)
                {
                    simulationManager.SetSimulationSpeed(
                        Math.Max(1, simulationManager.GetSimulationSpeed() - 1));
                }
            }
            else if (e.Key == Key.D1)
            {
                // Горячая клавиша для дорог
                foreach (var child in ((Grid)Content).Children)
                {
                    if (child is StackPanel panel)
                    {
                        foreach (var c in panel.Children)
                        {
                            if (c is RadioButton radio && radio.Content.ToString() == "Дорога")
                            {
                                radio.IsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
            else if (e.Key == Key.D2)
            {
                // Горячая клавиша для зон
                foreach (var child in ((Grid)Content).Children)
                {
                    if (child is StackPanel panel)
                    {
                        foreach (var c in panel.Children)
                        {
                            if (c is RadioButton radio && radio.Content.ToString() == "Инструмент зона")
                            {
                                radio.IsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
            else if (e.Key == Key.D3)
            {
                // Горячая клавиша для электростанции
                foreach (var child in ((Grid)Content).Children)
                {
                    if (child is StackPanel panel)
                    {
                        foreach (var c in panel.Children)
                        {
                            if (c is RadioButton radio && radio.Content.ToString() == "Электростанция")
                            {
                                radio.IsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
            else if (e.Key == Key.D4)
            {
                // Горячая клавиша для бульдозера
                foreach (var child in ((Grid)Content).Children)
                {
                    if (child is StackPanel panel)
                    {
                        foreach (var c in panel.Children)
                        {
                            if (c is RadioButton radio && radio.Content.ToString() == "Бульдозер")
                            {
                                radio.IsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}