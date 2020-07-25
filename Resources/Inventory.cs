using System;
using System.Collections.Generic;
using System.Text;

namespace GTServer.Resources
{
    public struct InventoryItems
    {
        public int ItemID { get; set; }
        public byte ItemCount { get; set; }
    }
    public class Inventory
    {
        public InventoryItems[] Items { get; set; } = new InventoryItems[] { };
        public int InventorySize { get; set; } = 100;
    }
}
