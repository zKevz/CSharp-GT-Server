using ENet.Managed;
using GTServer.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static GTServer.Program;
using static GTServer.Resources.WorldDatabase;

namespace GTServer
{
    public class Methods
    {
        public static async Task SendNothingHappened(ENetPeer peer, int x, int y)
        {
            PlayerMoving data = new PlayerMoving
            {
                NetID = (peer.Data as Player).NetID,
                PacketType = 0x8,
                PlantingTree = 0
            };
            data.NetID = -1;
            data.X = x;
            data.Y = y;
            data.PunchX = x;
            data.PunchY = y;
            await SendPacketRaw(4, PlayerMovingPack(data), 56, 0, peer);
        }
        public static int CountPlayer(string name)
        {
            int count = 0;
            foreach (var a in Peers)
            {
                if (a.State != ENetPeerState.Connected) continue;
                else if ((a.Data as Player).World == name) count++;
            }
            return count;
        }
        public static int CountPlayerOnline()
        {
            int count = 0;
            foreach (var a in Peers)
            {
                if (a.State != ENetPeerState.Connected) continue;
                count++;
            }
            return count;
        }
        public static int GetState(Player player)
        {
            int val = 0;
            val |= player.InModState ? 1 << 0 : 0 << 0;
            val |= player.InWings ? 1 << 1 : 0 << 1;
            return val;
        }
        public static async Task SendPacketRaw(int a1, byte[] packetData, long packetDataSize, int a4, ENetPeer peer)
        {
            try
            {
                if (peer != null) // check if we have it setup
                {

                    if (a1 == 4 && (packetData[12] & 8) == 1)
                    {
                        byte[] p = new byte[packetDataSize + packetData[13]];
                        Array.Copy(BitConverter.GetBytes(4), 0, p, 0, 4); //right
                        Array.Copy(packetData, 0, p, 4, packetDataSize); //right
                                                                         //Array.Copy(BitConverter.GetBytes(a4), 0, p, 4 + packetDataSize, 4);
                        Array.Copy(BitConverter.GetBytes(a4), 0, p, 4 + packetDataSize, 4 + 13);
                        peer.Send(p, 0, ENetPacketFlags.Reliable);
                        string mo = BitConverter.ToString(p);
                        mo = mo.Replace("-", "");

                    }
                    else
                    {
                        byte[] p = new byte[packetDataSize + 5];
                        Array.Copy(BitConverter.GetBytes(a1), 0, p, 0, 4);
                        //Console.WriteLine("1: " + BitConverter.ToString(p).Replace("-", ""));
                        Array.Copy(packetData, 0, p, 4, packetDataSize);
                        //Console.WriteLine("2: " + BitConverter.ToString(p).Replace("-", ""));
                        peer.Send(p, 0, ENetPacketFlags.Reliable);
                        // Console.WriteLine("Bytelen: " + p.Length + ":Len2: " + packetDataSize.ToString());
                        //  Console.WriteLine("Packetdata: " + BitConverter.ToString(p).Replace("-", ""));
                        string mo = BitConverter.ToString(p);
                        mo = mo.Replace("-", "");

                    }
                }
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine("error in sendpacketraw : " + e.Message);
            }
        }
        public static PlayerMoving UnpackPlayerMoving(byte[] data)
        {
            PlayerMoving dataStruct = new PlayerMoving
            {
                PacketType = BitConverter.ToInt32(data, 0),
                NetID = BitConverter.ToInt32(data, 4),
                CharacterState = BitConverter.ToInt32(data, 12),
                PlantingTree = BitConverter.ToInt32(data, 20),
                X = BitConverter.ToInt32(data, 24)
            };
            try
            {
                int i = BitConverter.ToInt32(data, 24);
                byte[] byte2s = BitConverter.GetBytes(i);
                byte b65 = byte2s[1];
                byte b25 = byte2s[2];
                byte b35 = byte2s[3];
                byte[] arr2 = { byte2s[0], b65, b25, b35 };
                float myFloat = System.BitConverter.ToSingle(arr2, 0);
                dataStruct.X = myFloat;


                int i3 = BitConverter.ToInt32(data, 28);
                byte[] byte23s = BitConverter.GetBytes(i3);
                byte b653 = byte23s[1];
                byte b253 = byte23s[2];
                byte b353 = byte23s[3];
                byte[] arr23 = { byte23s[0], b653, b253, b353 };
                float myFloat3 = System.BitConverter.ToSingle(arr23, 0);
                dataStruct.Y = myFloat3;
            }
            catch { Console.WriteLine("error at unpackplayermoving"); }


            try
            {
                int i = BitConverter.ToInt32(data, 32);
                byte[] byte2s = BitConverter.GetBytes(i);
                byte b65 = byte2s[1];
                byte b25 = byte2s[2];
                byte b35 = byte2s[3];
                byte[] arr2 = { byte2s[0], b65, b25, b35 };
                float myFloat = BitConverter.ToSingle(arr2, 0);
                dataStruct.XSpeed = myFloat;


                int i3 = BitConverter.ToInt32(data, 36);
                byte[] byte23s = BitConverter.GetBytes(i3);
                byte b653 = byte23s[1];
                byte b253 = byte23s[2];
                byte b353 = byte23s[3];
                byte[] arr23 = { byte23s[0], b653, b253, b353 };
                float myFloat3 = System.BitConverter.ToSingle(arr23, 0);
                dataStruct.YSpeed = myFloat3;
            }
            catch { Console.WriteLine("unpack playermoving error 2"); }


            //dataStruct.XSpeed = BitConverter.ToInt32(data, 32);
            //dataStruct.YSpeed = BitConverter.ToInt32(data, 36);
            //Console.WriteLine("UNPACKSPEED: " + BitConverter.ToString(BitConverter.GetBytes(dataStruct.XSpeed)));
            dataStruct.PunchX = BitConverter.ToInt32(data, 44);
            dataStruct.PunchY = BitConverter.ToInt32(data, 48);
            return dataStruct;
        }
        public static byte[] PlayerMovingPack(PlayerMoving dataStruct)
        {
            byte[] data = new byte[56];
            for (int i = 0; i < 56; i++)
            {
                data[i] = 0;
            }
            Array.Copy(BitConverter.GetBytes(dataStruct.PacketType), 0, data, 0, 4);
            //Console.WriteLine("packetType: " + dataStruct.packetType.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.NetID), 0, data, 4, 4);
            //Console.WriteLine("netID: " + dataStruct.netID.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.CharacterState), 0, data, 12, 4);
            // Console.WriteLine("Charstate: " + dataStruct.characterState.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.PlantingTree), 0, data, 20, 4);
            //Console.WriteLine("plantingTree: " + dataStruct.plantingTree.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.X), 0, data, 24, 4);
            //Console.WriteLine("x: " + dataStruct.x.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.Y), 0, data, 28, 4);
            // Console.WriteLine("y: " + dataStruct.y.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.XSpeed), 0, data, 32, 4);
            // Console.WriteLine("XSpeed: " + dataStruct.XSpeed.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.YSpeed), 0, data, 36, 4);
            //Console.WriteLine("PACKSPEED: " + BitConverter.ToString(BitConverter.GetBytes(dataStruct.XSpeed)));
            // Console.WriteLine("YSpeed: " + dataStruct.YSpeed.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.PunchX), 0, data, 44, 4);
            // Console.WriteLine("punchX: " + dataStruct.punchX.ToString());
            Array.Copy(BitConverter.GetBytes(dataStruct.PunchY), 0, data, 48, 4);
            // Console.WriteLine("punchY: " + dataStruct.punchY.ToString());
            return data;
        }

        public static async Task UpdateDoor(ENetPeer peer, int foreground, int x, int y, string text)
        {
            try
            {
                PlayerMoving sign = new PlayerMoving
                {
                    PacketType = 0x3,
                    CharacterState = 0x0,
                    X = x,
                    Y = y,
                    PunchX = x,
                    PunchY = y,
                    NetID = -1,
                    PlantingTree = foreground
                };
                await SendPacketRaw(4, PlayerMovingPack(sign), 56, 0, peer);
                int hmm = 8; int text_len = text.Length; int lol = 0; int wut = 5; int yeh = hmm + 3 + 1; int idk = 15 + text_len;
                int is_locked = 0; int bubble_type = 1; int ok = 52 + idk; int kek = ok + 4; int yup = ok - 8 - idk; int magic = 56;
                int wew = ok + 5 + 4; int wow = magic + 4 + 5; int four = 4;
                byte[] data = new byte[kek]; byte[] p = new byte[wew];
                for (int i = 0; i < kek; i++) data[i] = 0;
                Array.Copy(BitConverter.GetBytes(wut), 0, data, 0, four);
                Array.Copy(BitConverter.GetBytes(hmm), 0, data, yeh, four);
                Array.Copy(BitConverter.GetBytes(x), 0, data, yup, 4);
                Array.Copy(BitConverter.GetBytes(y), 0, data, yup + 4, 4);
                Array.Copy(BitConverter.GetBytes(idk), 0, data, 4 + yup + 4, four);
                Array.Copy(BitConverter.GetBytes(foreground), 0, data, magic, 2);
                Array.Copy(BitConverter.GetBytes(lol), 0, data, four + magic, four);
                Array.Copy(BitConverter.GetBytes(bubble_type), 0, data, magic + 4 + four, 1);
                Array.Copy(BitConverter.GetBytes(text_len), 0, data, wow, 2);
                Array.Copy(Encoding.ASCII.GetBytes(text), 0, data, 2 + wow, text_len);
                Array.Copy(BitConverter.GetBytes(is_locked), 0, data, ok, four);
                Array.Copy(BitConverter.GetBytes(four), 0, p, 0, four);
                Array.Copy(data, 0, p, four, kek);
                foreach (ENetPeer currentPeer in Peers)
                {
                    if (currentPeer.State != ENetPeerState.Connected)
                        continue;
                    if (peer.InWorld(currentPeer))
                    {
                        currentPeer.Send(p, 0, ENetPacketFlags.Reliable);
                    }
                }
            }
            catch
            {
                Console.WriteLine("error in updatedoor");
            }
        }
        public static async Task UpdateItemsDat()
        {
            if (File.Exists("items.dat"))
            {
                byte[] itemsData = File.ReadAllBytesAsync("items.dat").Result;
                ItemsDatLength = itemsData.Length;

                ItemsDat = new byte[60 + ItemsDatLength];
                string remainder = "0400000010000000FFFFFFFF000000000800000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
                for (int i = 0; i < remainder.Length; i += 2)
                {
                    byte x = CharToByte(remainder[i]);
                    x = (byte)(x << 4);
                    x += CharToByte(remainder[i + 1]);
                    ItemsDat[i / 2] = x;
                    if (remainder.Length > 60 * 2) throw new Exception("Error");
                }
                Array.Copy(BitConverter.GetBytes(ItemsDatLength), 0, ItemsDat, 56, 4);

                byte[] itData = File.ReadAllBytesAsync("items.dat").Result;

                string kkb = BitConverter.ToString(ItemsDat);
                kkb = kkb.Replace("-", "");

                if (kkb.Length > 60 || kkb.Length == 60)
                {
                    kkb = kkb.Substring(0, 120);
                    string kkc = BitConverter.ToString(itData);
                    kkc = kkc.Replace("-", "");
                    string bitreal = kkb + kkc;
                    byte[] abcf = HexToByte(bitreal);
                    // Console.WriteLine(bitreal);
                    ItemsDat = abcf;
                }
                SendDateTime();
                Console.WriteLine("Updating items data succeed!!");
            }
            else
            {
                Console.WriteLine("Failed to update items data! [NO FILE]");
                Environment.Exit(1);
            }
            await Task.CompletedTask;
        }
        public static async Task SaveOrAddWorldAsync(World world)
        {
            var worldFromList = Worlds.Where(x => x.Name == world.Name).ToList();
            if (worldFromList.Count != 0)
            {
                int index = Worlds.IndexOf(worldFromList.First());
                Worlds[index] = world;
            }
            else
            {
                Worlds.Add(world);
            }
            await Task.CompletedTask;
        }
        public static async Task SetItemsDataDB()
        {
            try
            {
                ItemsDatJson = new ItemsJsonData();
                string itemsDat = File.ReadAllTextAsync("data.json").Result;
                ItemsDatJson = JsonConvert.DeserializeObject<ItemsJsonData>(itemsDat);
                SendDateTime();
                Console.WriteLine("Gathering items.dat DB Succeed!");
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                SendDateTime();
                Console.WriteLine("Gathering Items.dat DB Failed! Message : " + e.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static byte CharToByte(char x)
        {
            return x switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'A' => 10,
                'B' => 11,
                'C' => 12,
                'D' => 13,
                'E' => 14,
                'F' => 15,
                _ => 0
            };
        }
        public static byte[] HexToByte(string hex)
        {

            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++) raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return raw;
        }
        public static async Task SetItemsDB()
        {
            try
            {
                ItemsData = new List<DataItems>();
                for (int i = 0; i < ItemsDatJson.Items.Count; i++)
                {
                    DataItems data = JsonConvert.DeserializeObject<DataItems>(ItemsDatJson.Items[i].ToString());
                    string d = data.Name.ToLower();
                    if (d.Contains("background") || d.Contains("sheet") || d.Contains("wallpaper"))
                    {
                        data.IsBackground = true;
                    }
                    else data.IsBackground = false;
                    ItemsData.Add(data);
                }
                CoreData = File.ReadAllText("CoreData.txt").Split("\n".ToCharArray());
                SendDateTime();
                Console.WriteLine("Setting items data DB Succeed!");
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                SendDateTime();
                Console.WriteLine("Setting items data DB Failed! Message : " + e.Message);
                Environment.Exit(1);
            }
        }
    }
}
