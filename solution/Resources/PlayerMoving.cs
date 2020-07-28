namespace GTServer.Resources
{
    public struct PlayerMoving
    {
        public int PacketType { get; set; }
        public int NetID { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int CharacterState { get; set; }
        public int PlantingTree { get; set; }
        public float XSpeed { get; set; }
        public float YSpeed { get; set; }
        public int PunchX { get; set; }
        public int PunchY { get; set; }
    };
}
