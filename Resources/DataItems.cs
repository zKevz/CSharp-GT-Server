namespace GTServer.Resources
{
    public class DataItems
    {
        public string Name { get; set; }
        public string Description { get; set; } = "This item has no description.";
        public int ItemID { get; set; }
        public int Rarity { get; set; }
        public int BreakHits { get; set; }
        public bool IsBackground { get; set; }
    }
}
