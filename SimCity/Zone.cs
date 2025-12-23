namespace SimCity
{
    public enum ZoneType
    {
        Residential,
        Commercial,
        Industrial
    }

    public class Zone : Building
    {
        public ZoneType ZoneType { get; set; }
        public int Level { get; set; } = 0;
        public bool HasBuilding { get; set; } = false;
        public bool IsVisibleInGameMode { get; set; } = true;

        public Zone(ZoneType zoneType)
        {
            ZoneType = zoneType;
            Width = 1;
            Height = 1;
            SpritePath = GetZoneSpritePath();
            OffsetX = 0;
            OffsetY = 0;
            UseSimplePositioning = false;
            IsVisibleInGameMode = true;

            // Размеры спрайта для зоны 1x1
            SpriteWidth = 32;   // 1 тайл = 32px
            SpriteHeight = 16;  // 1 тайл = 16px
        }

        public string GetZoneSpritePath()
        {
            if (!IsVisibleInGameMode)
                return "";

            if (ZoneType == ZoneType.Residential)
                return "Assets/zone_residential.png";
            else if (ZoneType == ZoneType.Commercial)
                return "Assets/zone_commercial.png";
            else if (ZoneType == ZoneType.Industrial)
                return "Assets/zone_industrial.png";
            else
                return "Assets/zone.png";
        }

        public void Build()
        {
            Level = 1;
            HasBuilding = true;

            // Все типы зданий первого уровня - 1x2
            Width = 1;
            Height = 2;
            OffsetX = 0;
            OffsetY = -8;

            // Размер спрайта для здания 1x2
            SpriteWidth = 32;    // 1 тайл в ширину = 32px
            SpriteHeight = 32;   // 2 тайла в высоту = 32px

            UpdateSprite();
        }

        public void Upgrade()
        {
            if (Level < 3)
            {
                Level++;

                if (Level == 2)
                {
                    // Второй уровень - 2x2
                    Width = 2;
                    Height = 2;
                    OffsetX = -16;  // Смещение влево на 1 тайл
                    OffsetY = -8;   // Смещение вверх на 0.5 тайла

                    // Размер спрайта для здания 2x2 - УВЕЛИЧИВАЕМ В 4 РАЗА
                    SpriteWidth = 64;    // 2 тайла в ширину = 64px
                    SpriteHeight = 32;   // 2 тайла в высоту = 32px
                }
                else if (Level == 3)
                {
                    // Третий уровень - 3x3
                    Width = 3;
                    Height = 3;
                    OffsetX = -32;  // Смещение влево на 2 тайла
                    OffsetY = -16;  // Смещение вверх на 1 тайл

                    // Размер спрайта для здания 3x3 - УВЕЛИЧИВАЕМ В 9 РАЗ
                    SpriteWidth = 96;    // 3 тайла в ширину = 96px
                    SpriteHeight = 48;   // 3 тайла в высоту = 48px
                }

                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            SpritePath = GetBuildingSpritePath();
        }

        private string GetBuildingSpritePath()
        {
            if (ZoneType == ZoneType.Residential)
            {
                if (Level == 1)
                    return "Assets/building_residential_1.png"; // 1x2
                else if (Level == 2)
                    return "Assets/building_residential_2.png"; // 2x2
                else if (Level == 3)
                    return "Assets/building_residential_3.png"; // 3x3
                else
                    return GetZoneSpritePath();
            }
            else if (ZoneType == ZoneType.Commercial)
            {
                if (Level == 1)
                    return "Assets/building_commercial_1.png"; // 1x2
                else if (Level == 2)
                    return "Assets/building_commercial_2.png"; // 2x2
                else if (Level == 3)
                    return "Assets/building_commercial_3.png"; // 3x3
                else
                    return GetZoneSpritePath();
            }
            else if (ZoneType == ZoneType.Industrial)
            {
                if (Level == 1)
                    return "Assets/building_industrial_1.png"; // 1x2
                else if (Level == 2)
                    return "Assets/building_industrial_2.png"; // 2x2
                else if (Level == 3)
                    return "Assets/building_industrial_3.png"; // 3x3
                else
                    return GetZoneSpritePath();
            }
            else
            {
                return "Assets/zone.png";
            }
        }

        public void SetVisibility(bool isVisible)
        {
            IsVisibleInGameMode = isVisible;
            SpritePath = GetZoneSpritePath();
        }
    }
}