using SimCity.Game;
using SimCity.Map;
using SimCity.UI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SimCity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) => GameController.Instance.Update();
            timer.Start();
        }

        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TileViewModel tile)
            {
                tile.OnClick();
            }
        }

        private void ResetCamera_Click(object sender, RoutedEventArgs e)
        {
            if (GameCanvas is GameCanvas canvas)
            {
                canvas.ResetCamera();
            }
        }
    }
}