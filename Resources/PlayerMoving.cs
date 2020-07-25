using System;
using System.Collections.Generic;
using System.Text;

namespace GTServer.Resources
{
    public struct PlayerMoving
    {
        public int PacketType;
        public int NetID;
        public float X;
        public float Y;
        public int CharacterState;
        public int PlantingTree;
        public float XSpeed;
        public float YSpeed;
        public int PunchX;
        public int PunchY;
    };
}
