namespace GTServer.Resources
{
    public class World
    {
        public string Name { get; set; }
        public string OwnerName { get; set; } = "";
        public string AccessedPlayer { get; set; } = "";
        public int OwnerNetID { get; set; } = 0;
        public int Weather { get; set; } = 0;
        public int APlayerID { get; set; } = 0;
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsPublic { get; set; } = false;
        public bool Nuked { get; set; } = false;
        public WorldItems[] Items { get; set; }
    }

    public struct WorldItems
    {
        public int Foreground { get; set; }
        public int Background { get; set; }
        public int BreakLevel { get; set; }
        public int LastForeground => Foreground;
        public int LastBackground => Background;
        public long BreakTime { get; set; }
        public bool IsWater { get; set; }
        public bool IsBackground { get; set; }
        public bool BoolType { get; set; }
        public bool IsBreak { get; set; }
    }

    public struct WorldProperties
    {
        public World WorldInfo { get; set; }
        public int Id { get; set; }
    }
}