using System;
using System.Text;
using static GTServer.Methods;

namespace GTServer.Resources
{
    public class Packet
    {
        public byte[] Data { get; set; }
        public int Len { get; set; }
        public int Index { get; set; }

        public static Packet AppendFloat(Packet p, float val)
        {
            byte[] Data = new byte[p.Len + 2 + 4];
            Array.Copy(p.Data, 0, Data, 0, p.Len);
            byte[] num = BitConverter.GetBytes(val);
            Data[p.Len] = (byte)p.Index;
            Data[p.Len + 1] = 1;
            Array.Copy(num, 0, Data, p.Len + 2, 4);
            p.Len = p.Len + 2 + 4;
            p.Index++;
            p.Data = Data;
            return p;
        }

        public static Packet AppendFloat(Packet p, float val, float val2)
        {
            byte[] Data = new byte[p.Len + 2 + 8];
            Array.Copy(p.Data, 0, Data, 0, p.Len);
            byte[] fl1 = BitConverter.GetBytes(val);
            byte[] fl2 = BitConverter.GetBytes(val2);
            Data[p.Len] = (byte)p.Index;
            Data[p.Len + 1] = 3;
            Array.Copy(fl1, 0, Data, p.Len + 2, 4);
            Array.Copy(fl2, 0, Data, p.Len + 6, 4);
            p.Len = p.Len + 2 + 8;
            p.Index++;
            p.Data = Data;
            return p;
        }

        public static Packet AppendFloat(Packet p, float val, float val2, float val3)
        {
            byte[] Data = new byte[p.Len + 2 + 12];
            Array.Copy(p.Data, 0, Data, 0, p.Len);
            byte[] fl1 = BitConverter.GetBytes(val);
            byte[] fl2 = BitConverter.GetBytes(val2);
            byte[] fl3 = BitConverter.GetBytes(val3);
            Data[p.Len] = (byte)p.Index;
            Data[p.Len + 1] = 4;
            Array.Copy(fl1, 0, Data, p.Len + 2, 4);
            Array.Copy(fl2, 0, Data, p.Len + 6, 4);
            Array.Copy(fl3, 0, Data, p.Len + 10, 4);
            p.Len = p.Len + 2 + 12;
            p.Index++;
            p.Data = Data;
            return p;
        }

        public static Packet AppendInt(Packet p, Int32 val)
        {
            byte[] Data = new byte[p.Len + 2 + 4];
            Array.Copy(p.Data, 0, Data, 0, p.Len);
            byte[] num = BitConverter.GetBytes(val);
            Data[p.Len] = (byte)p.Index;
            Data[p.Len + 1] = 9;
            Array.Copy(num, 0, Data, p.Len + 2, 4);
            p.Len = p.Len + 2 + 4;
            p.Index++;
            p.Data = Data;
            return p;
        }

        public static Packet AppendIntx(Packet p, int val)
        {
            byte[] Data = new byte[p.Len + 2 + 4];
            Array.Copy(p.Data, 0, Data, 0, p.Len);
            byte[] num = BitConverter.GetBytes(val);
            Data[p.Len] = (byte)p.Index;
            Data[p.Len + 1] = 5;
            Array.Copy(num, 0, Data, p.Len + 2, 4);
            p.Len = p.Len + 2 + 4;
            p.Index++;
            p.Data = Data;
            return p;
        }

        public static Packet AppendString(Packet p, string str)
        {
            byte[] Data = new byte[p.Len + 2 + str.Length + 4];
            Array.Copy(p.Data, 0, Data, 0, p.Len);
            byte[] strn = Encoding.ASCII.GetBytes(str);
            Data[p.Len] = (byte)p.Index;
            Data[p.Len + 1] = 2;
            byte[] Len = BitConverter.GetBytes(str.Length);
            Array.Copy(Len, 0, Data, p.Len + 2, 4);
            Array.Copy(strn, 0, Data, p.Len + 6, str.Length);
            p.Len = p.Len + 2 + str.Length + 4;
            p.Index++;
            p.Data = Data;
            return p;
        }

        public static Packet CreatePacket()
        {
            byte[] Data = new byte[61];
            string asdf = "0400000001000000FFFFFFFF00000000080000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
            for (int i = 0; i < asdf.Length; i += 2)
            {
                byte x = CharToByte(asdf[i]);
                x = (byte)(x << 4);
                x += CharToByte(asdf[i + 1]);
                Data[i / 2] = x;
                if (asdf.Length > 61 * 2) throw new Exception("?");
            }
            Packet packet = new Packet
            {
                Data = Data,
                Len = 61,
                Index = 0
            };
            return packet;
        }

        public static Packet PacketEnd(Packet p)
        {
            byte[] n = new byte[p.Len + 1];
            Array.Copy(p.Data, 0, n, 0, p.Len);
            p.Data = n;
            p.Data[p.Len] = 0;
            p.Len += 1;
            p.Data[56] = (byte)p.Index;
            p.Data[60] = (byte)p.Index;
            //*(BYTE*)(p.Data + 60) = p.Index;
            return p;
        }
    }
}
