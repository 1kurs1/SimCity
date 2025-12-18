using SimCity.Map;
using System;

namespace SimCity.Game
{
    public class GameController
    {
        private static GameController m_instance;
        public static GameController Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new GameController();
                return m_instance;
            }
        }

        public GameMap Map { get; }
        private GameState m_state = new GameState();

        public GameState State => m_state;

        // Событие для обновления интерфейса
        public event Action<int> MoneyChanged;
        public event Action<string> BuildingSelectedChanged; // Новое событие

        // Приватное поле для хранения выбранного здания
        private string m_selectedBuildingId = "house_small";

        // Единственный способ выбора здания
        public string SelectedBuildingId
        {
            get => m_selectedBuildingId;
            set
            {
                if (m_selectedBuildingId != value)
                {
                    m_selectedBuildingId = value;
                    System.Diagnostics.Debug.WriteLine($"Выбрано здание: {value}");
                    BuildingSelectedChanged?.Invoke(value);
                }
            }
        }

        // Устаревшее свойство - делаем read-only для обратной совместимости
        public TileType SelectedBuildType
        {
            get
            {
                var building = BuildingRegistry.GetBuilding(SelectedBuildingId);
                return building?.Type ?? TileType.Residential;
            }
        }

        private readonly EconomySystem m_economy = new EconomySystem();

        private GameController()
        {
            Map = new GameMap(20, 20);
        }

        public void Build(int x, int y)
        {
            // Проверяем границы
            if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height)
                return;

            var tile = Map.Tiles[x, y];

            // Получаем информацию о здании
            var buildingInfo = BuildingRegistry.GetBuilding(SelectedBuildingId);
            if (buildingInfo == null)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: здание {SelectedBuildingId} не найдено!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Попытка построить {buildingInfo.Id} в ({x}, {y})");

            // Простая проверка - можно строить только на пустых клетках
            if (tile.Type != TileType.Empty)
            {
                // Для дорог - можно строить поверх всего кроме других дорог
                if (buildingInfo.Type == TileType.Road && tile.Type != TileType.Road)
                {
                    // Можно строить дорогу
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Клетка ({x}, {y}) уже занята: {tile.Type}");
                    return;
                }
            }

            // Проверяем деньги
            int cost = buildingInfo.BaseCost;
            if (State.Money < cost)
            {
                System.Diagnostics.Debug.WriteLine($"Недостаточно денег: нужно {cost}, есть {State.Money}");
                return;
            }

            // Для дорог простая логика
            if (buildingInfo.Type == TileType.Road)
            {
                BuildRoad(x, y, buildingInfo);
                return;
            }

            // Для обычных зданий проверяем размер
            if (!CanBuildHere(x, y, buildingInfo))
            {
                System.Diagnostics.Debug.WriteLine($"Недостаточно места для {buildingInfo.Id} ({buildingInfo.Size.WidthInTiles}x{buildingInfo.Size.HeightInTiles})");
                return;
            }

            // Списание денег
            State.Money -= cost;
            OnMoneyChanged();

            // Устанавливаем здание
            SetBuildingOnMap(x, y, buildingInfo);

            System.Diagnostics.Debug.WriteLine($"✅ Успешно построено {buildingInfo.Id} в ({x}, {y}) за {cost}$");
        }

        private bool CanBuildHere(int x, int y, BuildingInfo buildingInfo)
        {
            // Проверяем границы
            if (x + buildingInfo.Size.WidthInTiles > Map.Width ||
                y + buildingInfo.Size.HeightInTiles > Map.Height)
            {
                return false;
            }

            // Проверяем, что все клетки свободны (или могут быть перезаписаны)
            for (int dx = 0; dx < buildingInfo.Size.WidthInTiles; dx++)
            {
                for (int dy = 0; dy < buildingInfo.Size.HeightInTiles; dy++)
                {
                    var checkTile = Map.Tiles[x + dx, y + dy];
                    if (checkTile.Type != TileType.Empty)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void BuildRoad(int x, int y, BuildingInfo roadInfo)
        {
            int cost = roadInfo.BaseCost;

            if (State.Money < cost)
                return;

            State.Money -= cost;
            OnMoneyChanged();

            var tile = Map.Tiles[x, y];
            tile.Type = TileType.Road;
            tile.SetBuilding("road", x, y, true);

            System.Diagnostics.Debug.WriteLine($"✅ Успешно построена дорога в ({x}, {y}) за {cost}$");
        }

        private void SetBuildingOnMap(int baseX, int baseY, BuildingInfo buildingInfo)
        {
            // Занимаем все клетки под здание
            for (int dx = 0; dx < buildingInfo.Size.WidthInTiles; dx++)
            {
                for (int dy = 0; dy < buildingInfo.Size.HeightInTiles; dy++)
                {
                    int x = baseX + dx;
                    int y = baseY + dy;
                    var tile = Map.Tiles[x, y];

                    // Устанавливаем тип здания
                    tile.Type = buildingInfo.Type;

                    // Устанавливаем информацию о здании
                    tile.SetBuilding(buildingInfo.Id, baseX, baseY, dx == 0 && dy == 0);

                    // ✅ Для отладки
                    System.Diagnostics.Debug.WriteLine($"Tile ({x},{y}) установлен BuildingId: {buildingInfo.Id}");
                }
            }
        }

        private void OnMoneyChanged()
        {
            MoneyChanged?.Invoke(State.Money);
        }

        public void Update()
        {
            // Сохраняем старое количество денег для сравнения
            int oldMoney = State.Money;

            // Обновляем экономику
            m_economy.Tick(Map, State);

            // Если деньги изменились, вызываем событие
            if (State.Money != oldMoney)
            {
                OnMoneyChanged();
            }
        }
    }
}