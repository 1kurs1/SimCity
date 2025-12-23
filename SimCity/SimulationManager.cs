using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace SimCity
{
    public class SimulationManager
    {
        private DispatcherTimer simulationTimer;
        private Random random = new Random();
        private MapManager mapManager;
        private ZoningManager zoningManager;

        private const double BUILD_CHANCE = 0.01;
        private const double UPGRADE_CHANCE = 0.005;
        private const double ZONE_EXPAND_CHANCE = 0.002; // Шанс расширения зоны

        private TimeSpan gameTime = TimeSpan.Zero;
        private int simulationSpeed = 1;
        private bool isPaused = false;

        public event Action<TimeSpan> GameTimeChanged;
        public event Action SimulationUpdated;

        public SimulationManager(MapManager mapManager, ZoningManager zoningManager)
        {
            this.mapManager = mapManager;
            this.zoningManager = zoningManager;
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromMilliseconds(50);
            simulationTimer.Tick += OnSimulationTick;
        }

        public void StartSimulation()
        {
            isPaused = false;
            simulationTimer.Start();
        }

        public void PauseSimulation()
        {
            isPaused = true;
            simulationTimer.Stop();
        }

        public void ResumeSimulation()
        {
            isPaused = false;
            simulationTimer.Start();
        }

        public void SetSimulationSpeed(int speed)
        {
            if (speed >= 1 && speed <= 3)
            {
                simulationSpeed = speed;
            }
        }

        private void OnSimulationTick(object sender, EventArgs e)
        {
            if (isPaused) return;

            gameTime += TimeSpan.FromMinutes(10 * simulationSpeed);
            GameTimeChanged?.Invoke(gameTime);

            ProcessZoneDevelopment();
            ProcessZoneExpansion();
            ProcessBuildingUpgrades();

            SimulationUpdated?.Invoke();
        }

        private void ProcessZoneDevelopment()
        {
            var tilesToUpdate = new List<Tile>();

            foreach (var tile in mapManager.Tiles)
            {
                if (tile.Building is Zone zone && !(tile.Building is OccupiedTile))
                {
                    tilesToUpdate.Add(tile);
                }
            }

            foreach (var tile in tilesToUpdate)
            {
                var zone = tile.Building as Zone;

                if (!zone.HasBuilding)
                {
                    if (random.NextDouble() < BUILD_CHANCE)
                    {
                        if (mapManager.HasRoadAccess(zone.X, zone.Y, 2))
                        {
                            zone.Build();
                            tile.UpdatePosition(zone);
                            tile.SpritePath = zone.SpritePath;
                        }
                    }
                }
            }
        }

        private void ProcessBuildingUpgrades()
        {
            var readyZones = zoningManager.GetReadyToUpgradeZones();

            foreach (var zone in readyZones)
            {
                if (random.NextDouble() < UPGRADE_CHANCE)
                {
                    if (zoningManager.UpgradeBuilding(zone))
                    {
                        // Обновляем спрайт на карте
                        var tile = mapManager.GetTile(zone.X, zone.Y);
                        if (tile != null)
                        {
                            tile.SpritePath = zone.SpritePath;
                        }
                    }
                }
            }
        }

        private void ProcessZoneExpansion()
        {
            // Развитие существующих зон, но не создание новых
            var allZones = new List<Zone>();

            foreach (var tile in mapManager.Tiles)
            {
                if (tile.Building is Zone zone && !(tile.Building is OccupiedTile))
                {
                    allZones.Add(zone);
                }
            }

            foreach (var zone in allZones)
            {
                // Только развитие существующих зон, но не создание новых
                if (zone.HasBuilding && zone.Level < 3)
                {
                    // Проверяем возможность прокачки (новый метод без параметров)
                    if (zoningManager.CanUpgrade(zone))
                    {
                        if (random.NextDouble() < UPGRADE_CHANCE)
                        {
                            zoningManager.UpgradeBuilding(zone);
                        }
                    }
                }
            }
        }

        private void TryExpandZone(Zone zone)
        {
            // Пробуем расширить зону в случайном направлении
            int direction = random.Next(4); // 0: вверх, 1: вправо, 2: вниз, 3: влево

            int newX = zone.X;
            int newY = zone.Y;
            int newWidth = zone.Width;
            int newHeight = zone.Height;

            switch (direction)
            {
                case 0: // Вверх
                    if (zoningManager.CreateZone(zone.X, zone.Y - 1, zone.ZoneType))
                    {
                        newY--;
                        newHeight++;
                    }
                    break;
                case 1: // Вправо
                    if (zoningManager.CreateZone(zone.X + zone.Width, zone.Y, zone.ZoneType))
                    {
                        newWidth++;
                    }
                    break;
                case 2: // Вниз
                    if (zoningManager.CreateZone(zone.X, zone.Y + zone.Height, zone.ZoneType))
                    {
                        newHeight++;
                    }
                    break;
                case 3: // Влево
                    if (zoningManager.CreateZone(zone.X - 1, zone.Y, zone.ZoneType))
                    {
                        newX--;
                        newWidth++;
                    }
                    break;
            }
        }

        public int GetSimulationSpeed()
        {
            return simulationSpeed;
        }

        public TimeSpan GetGameTime()
        {
            return gameTime;
        }

        public bool IsPaused()
        {
            return isPaused;
        }
    }
}