using SimCity.Game;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SimCity.UI
{
    /// <summary>
    /// Interaction logic for BuildingSelector.xaml
    /// </summary>
    public partial class BuildingSelector : UserControl
    {
        public BuildingSelector()
        {
            InitializeComponent();

            // Подписываемся на изменение денег
            GameController.Instance.MoneyChanged += OnMoneyChanged;

            // Подписываемся на событие Unloaded
            this.Unloaded += BuildingSelector_Unloaded;

            // Обновляем начальное значение
            UpdateMoneyDisplay(GameController.Instance.State.Money);

            // Выделяем кнопку по умолчанию
            UpdateButtonSelection(btnHouseSmall);
        }

        private void BuildingSelector_Unloaded(object sender, RoutedEventArgs e)
        {
            // Отписываемся от события
            GameController.Instance.MoneyChanged -= OnMoneyChanged;
        }

        private void OnMoneyChanged(int newMoney)
        {
            // Обновляем UI в основном потоке
            Dispatcher.Invoke(() =>
            {
                UpdateMoneyDisplay(newMoney);
            });
        }

        private void UpdateMoneyDisplay(int money)
        {
            if (MoneyDisplay != null)
            {
                MoneyDisplay.Text = $"{money}$";

                // Меняем цвет если денег мало
                if (money < 100)
                {
                    MoneyDisplay.Foreground = System.Windows.Media.Brushes.Red;
                }
                else if (money < 500)
                {
                    MoneyDisplay.Foreground = System.Windows.Media.Brushes.Yellow;
                }
                else
                {
                    MoneyDisplay.Foreground = System.Windows.Media.Brushes.Gold;
                }
            }
        }

        // Жилые дома
        private void SelectHouseSmall(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Выбран маленький дом");
                GameController.Instance.SelectedBuildingId = "house_small";
                UpdateButtonSelection(sender as Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка выбора здания: {ex.Message}");
            }
        }

        private void SelectHouseMedium(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Выбран средний дом");
                GameController.Instance.SelectedBuildingId = "house_medium";
                UpdateButtonSelection(sender as Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка выбора здания: {ex.Message}");
            }
        }

        private void SelectApartment(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Выбраны апартаменты");
                GameController.Instance.SelectedBuildingId = "apartment";
                UpdateButtonSelection(sender as Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка выбора здания: {ex.Message}");
            }
        }

        // Промышленные
        private void SelectFactorySmall(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Выбрана маленькая фабрика");
                GameController.Instance.SelectedBuildingId = "factory_small";
                UpdateButtonSelection(sender as Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка выбора здания: {ex.Message}");
            }
        }

        private void SelectFactoryLarge(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Выбрана большая фабрика");
                GameController.Instance.SelectedBuildingId = "factory_large";
                UpdateButtonSelection(sender as Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка выбора здания: {ex.Message}");
            }
        }

        // Коммерческие
        private void SelectShop(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Выбран магазин");
                GameController.Instance.SelectedBuildingId = "shop";
                UpdateButtonSelection(sender as Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка выбора здания: {ex.Message}");
            }
        }

        private void SelectMall(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Выбран торговый центр");
                GameController.Instance.SelectedBuildingId = "mall";
                UpdateButtonSelection(sender as Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка выбора здания: {ex.Message}");
            }
        }

        // Инфраструктура
        private void SelectRoad(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Выбрана дорога");
                GameController.Instance.SelectedBuildingId = "road";
                UpdateButtonSelection(sender as Button);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка выбора здания: {ex.Message}");
            }
        }

        private void SelectPower(object sender, RoutedEventArgs e)
        {
            // Тихая заглушка - ничего не делаем
        }

        private void SelectWater(object sender, RoutedEventArgs e)
        {
            // Тихая заглушка - ничего не делаем
        }

        // Метод для обновления визуального выделения выбранной кнопки
        private void UpdateButtonSelection(Button selectedButton)
        {
            // Сбрасываем выделение у всех кнопок
            ResetAllButtons();

            // Выделяем выбранную кнопку
            if (selectedButton != null)
            {
                selectedButton.BorderBrush = System.Windows.Media.Brushes.Gold;
                selectedButton.BorderThickness = new Thickness(3);
                selectedButton.FontWeight = FontWeights.Bold;

                // Логируем
                System.Diagnostics.Debug.WriteLine($"Выделена кнопка: {selectedButton.Content}");
            }
        }

        private void ResetAllButtons()
        {
            // Список всех кнопок зданий
            Button[] buildingButtons = {
                btnHouseSmall, btnHouseMedium, btnApartment,
                btnFactorySmall, btnFactoryLarge,
                btnShop, btnMall,
                btnRoad, btnPower, btnWater
            };

            foreach (var button in buildingButtons)
            {
                if (button != null)
                {
                    button.BorderBrush = System.Windows.Media.Brushes.Transparent;
                    button.BorderThickness = new Thickness(1);
                    button.FontWeight = FontWeights.Normal;
                }
            }
        }
    }
}