namespace SimCity
{
    public class PowerPlant : Building
    {
        public PowerPlant()
        {
            Width = 2;
            Height = 2;
            SpritePath = "Assets/powerplant.png";
            OffsetX = 0;    // Смещение по X для точного позиционирования
            OffsetY = 0;     // Смещение по Y для точного позиционирования

            // Размеры спрайта для электростанции 2x2
            SpriteWidth = 64;    // 2 тайла в ширину = 64px
            SpriteHeight = 32;   // 2 тайла в высоту = 32px
        }
    }
}