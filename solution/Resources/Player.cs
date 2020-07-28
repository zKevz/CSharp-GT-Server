using System;

namespace GTServer.Resources
{
    public class Player
    {
        //set all the properties value to prevent nullable value.
        public string RawName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Discord { get; set; } = "";
        public string TankIDName { get; set; } = "";
        public string TankIDPassword { get; set; } = "";
        public string World { get; set; } = "EXIT";
        public string RequestedName { get; set; }
        public string Mac { get; set; }
        public string Country { get; set; }
        public string RID { get; set; }
        public int AdminID { get; set; } = 0;
        public int X { get; set; } = 0;
        public int Y { get; set; }
        public int CpX { get; set; }
        public int CpY { get; set; }
        public int NetID { get; set; } = 0;
        public int BackClothes { get; set; }
        public int HairClothes { get; set; } = 0; // 0
        public int ShirtClothes { get; set; } = 0; // 1
        public int PantsClothes { get; set; } = 0; // 2
        public int FeetClothes { get; set; } = 0; // 3
        public int FaceClothes { get; set; } = 0; // 4
        public int HandClothes { get; set; } = 0; // 5
        public int MaskClothes { get; set; } = 0; // 7
        public int NecklaceClothes { get; set; } = 0; // 8
        public int AncesClothes { get; set; } = 0;
        public int NeckClothes { get; set; } = 0;
        public int Gems { get; set; }
        public int LastPunchX { get; set; }
        public int LastPunchY { get; set; }
        public int LastWrenchX { get; set; }
        public int LastWrenchY { get; set; }
        public int SmState { get; set; } = 0;
        public uint SkinColor { get; set; } = 0x8295C3FF;
        public bool Updating { get; set; } = false;
        public bool HaveGrowID { get; set; } = false;
        public bool InGame { get; set; } = false;
        public bool ClothesUpdated { get; set; } = false;
        public bool InModState { get; set; } = false;
        public bool InWings { get; set; } = false;
        public bool IsRotatingLeft { get; set; } = false;
        public long LastPlayerEnter { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        public DateTimeOffset LastJoinWorld { get; set; } = DateTimeOffset.Now;
        public World PWorld { get; set; }
        public Inventory PlayerInventory { get; set; }
    }
}
