using ENet.Managed;
using GTServer.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GTServer.Methods;

namespace GTServer
{
    class Program
    {
        public static List<DataItems> ItemsData { get; set; }
        public static List<ENetPeer> Peers { get; set; } = new List<ENetPeer>();
        public static ENetHost Server { get; set; }
        public static ItemsJsonData ItemsDatJson { get; set; }
        public static int ItemsDatLength { get; set; } = 0;
        public static int PeerNetID { get; set; } = 1;
        public static byte[] ItemsDat { get; set; }
        public static string[,] LoginArray { get; set; }
        public static int ItemsDatHash { get; set; }
        public static string[] CoreData { get; set; }
        public static async Task Main()
        {
            try
            {
                Console.WriteLine("Gt server testing");
                ManagedENet.Startup();
                await UpdateItemsDat();

                IPEndPoint address = new IPEndPoint(IPAddress.Any, 17091);
                Server = new ENetHost(address, 1024, 10);
                Server.ChecksumWithCRC32();
                Server.CompressWithRangeCoder();

                await SetItemsDataDB();
                await SetItemsDB();
                await DbContext.OpenConnection();
                Console.WriteLine("Success at opening MySql connection!");
                Server.OnConnect += Server_OnConnect;
                Server.StartServiceThread();
                Thread.Sleep(-1);
            }
            catch(Exception e)
            {
                Console.WriteLine("Critical error occured ! Message : " + e.Message);
            }
        }

        private static async void Server_OnConnect(object sender, ENetConnectEventArgs connectEventArgs)
        {
            int count = 1;
            //WorldDatabase.GetWorld("START");
            //WorldDatabase.GetWorld("ADMIN");
            //WorldDatabase.GetWorld("HELLO");
            ENetPeer Peer = connectEventArgs.Peer;
            Console.WriteLine("Peer connected. IP : " + Peer.RemoteEndPoint.Address);
            foreach (var currentPeer in Peers)
            {
                if (currentPeer.State != ENetPeerState.Connected)
                    continue;
                if (currentPeer.RemoteEndPoint.Equals(Peer.RemoteEndPoint))
                    count++;
            }
            Peer.Data = new Player();
            if (count > 3)
            {
                await Peer.OnConsoleMessage("`oToo many accounts are logged in this IP. Please try again later.");
            }
            else
            {
                await Peer.SendData(1, BitConverter.GetBytes(0), 1);
                Peers.Add(Peer);
            }
            connectEventArgs.Peer.OnReceive += Peer_OnReceive;
            connectEventArgs.Peer.OnDisconnect += Peer_OnDisconnect;
        }

        private static async void Peer_OnDisconnect(object sender, uint e)
        {
            ENetPeer peer = sender as ENetPeer;
            Console.WriteLine("Peer disconnected");
            await peer.SendLeave();
            peer.Data = null;
            Peers.Remove(peer);
        }

        private static async void Peer_OnReceive(object sender, ENetPacket e)
        {
            byte[] payLoad = e.GetPayloadCopy();
            var peer = sender as ENetPeer;
            var pData = peer.Data as Player;
            if (pData.Updating)
            {
                Console.WriteLine("PACKET DROP");
                return;
            }
            var world = peer.GetWorld();
            (peer.Data as Player).PWorld = world;
            int type = payLoad[0];
            string action = Encoding.ASCII.GetString(payLoad.Take(payLoad.Length - 1).Skip(4).ToArray());
            string[] actionArr = action.Split("\n".ToCharArray());
            string[,] loginArr = new string[actionArr.Length, 2];
            if (actionArr.Length > 17)
            {
                try
                {
                    for (int i = 0; i < actionArr.Length; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            loginArr[i, j] = actionArr[i].Split('|')[j];
                            LoginArray = loginArr;
                        }
                    }
                }
                catch { }
            }
            switch (type)
            {
                case 2:
                    //    long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    //    if(pData.LastPlayerEnter + 350 < time)
                    //    {
                    //        (peer.Data as Player).LastPlayerEnter = time;
                    //        goto b;
                    //    }
                    //    else
                    //    {
                    //        if (action != "action|refresh_item_data\n") return;
                    //    }
                    //b:;
                    if (action.Is("action|respawn"))
                    {
                        try
                        {
                            if (action.Is("action|respawn_spike"))
                            {
                                await peer.Respawn(true);
                            }
                            else
                            {
                                await peer.Respawn(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Respawn Error! Message : " + ex.Message);
                        }
                    }
                    if (action.Is("action|growid"))
                    {
                        try
                        {
                            await peer.SendCreateGID();
                            return;
                        }
                        catch (Exception ex) { Console.WriteLine("Action|growid error! Message : " + ex.Message); }
                    }
                    else if (action.Is("action|store"))
                    {
                        try
                        {
                            await peer.OnConsoleMessage("Store is not implemented yet!");
                            Dialog d = new Dialog();
                            // to create the dialog
                            string dialog = d.SetDefaultColor()
                                .AddBigLabel("Store is not implemented yet!")
                                .EndDialog("nothing", "Cancel", "Ok").Result;
                            await peer.OnDialogRequest(dialog);
                            return;
                        }
                        catch (Exception ex) { Console.WriteLine("Action|store error! Message : " + ex.Message); }
                    }
                    else if (action.Is("action|info"))
                    {
                        try
                        {
                            if (!(peer.Data as Player).HaveGrowID)
                            {
                                await peer.SendCreateGID();
                                return;
                            }
                            int id = -1;
                            int count = -1;
                            foreach (string to in action.Split("\n".ToCharArray()))
                            {
                                string[] infoDat = action.Split("|".ToCharArray());
                                if (infoDat.Length == 3)
                                {
                                    if (infoDat[1] == "itemID")
                                    {
                                        int a = Convert.ToInt32(infoDat[2]);
                                        if (a > 9279)
                                        {
                                            return;
                                            //thanks for raiter for the fix
                                        }
                                        else if (a < 0)
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            id = a;
                                        }
                                    }
                                    if (infoDat[1] == "count")
                                    {
                                        int a1 = Convert.ToInt32(infoDat[2]);
                                        if (a1 > 200)
                                        {
                                            return;
                                        }
                                        else if (a1 < 200)
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            count = a1;
                                        }
                                    }
                                }
                            }

                            if (id == -1 || count == -1) return;
                            if (ItemsData.Count < id || id < 0) return;
                            await peer.OnDialogRequest("set_default_color|`o\n\nadd_label_with_icon|big|`w" + ItemsData[id].Name +
                                "``|left|" + id + "|\n\nadd_spacer|small|\nadd_textbox|" +
                                ItemsData[id].Description +
                                "|left|\nadd_spacer|small|\nadd_quick_exit|\nadd_button|chc0|Close|noflags|0|0|\nnend_dialog|gazette||OK|");
                            //enet_host_flush(server);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error in action|info! Message : " + ex.Message);
                        }
                    }
                    else if (action.Is("action|dialog_return"))
                    {
                        await DialogHandler.SendDialogAsync(peer, world, action);
                    }
                    else if (action.Is("action|drop\n|itemID|"))
                    {
                        Console.WriteLine("TODO => ACTION_DROP");
                    }
                    else if (action.Contains("text|"))
                    {
                        try
                        {
                            if (!(peer.Data as Player).HaveGrowID)
                            {
                                await peer.SendCreateGID();
                                return;
                            }
                            await peer.SendCommands(action);
                        }
                        catch (Exception ex) { Console.WriteLine("action text error! Message : " + ex.Message); }
                    }
                    if (!(peer.Data as Player).InGame)
                    {
                        try
                        {
                            int a = 0;
                            await peer.OnSuperMain();
                            if (LoginArray[0, 0] == "tankIDName")
                            {
                                a = 2;
                                (peer.Data as Player).TankIDName = LoginArray[0, 1];
                                (peer.Data as Player).RawName = LoginArray[0, 1];
                                (peer.Data as Player).DisplayName = LoginArray[0, 1];
                                (peer.Data as Player).TankIDPassword = LoginArray[1, 1];
                                (peer.Data as Player).HaveGrowID = true;
                            }
                            if (LoginArray[17 + a, 1].Length < 15 || LoginArray[17 + a, 1].Length > 20 ||
                               LoginArray[12 + a, 1].Length < 30 || LoginArray[12 + a, 1].Length > 38)
                            {
                                peer.DisconnectLater(0);
                            }
                            (peer.Data as Player).RequestedName = LoginArray[0 + a, 1];
                            foreach (char c in (peer.Data as Player).RequestedName)
                            {
                                if (c < 0x20 || c > 0x7A)
                                {
                                    await peer.OnConsoleMessage("Bad characters detected in your name! Please remove them!");
                                    peer.DisconnectLater(0);
                                    return;
                                }
                            }
                            (peer.Data as Player).Mac = LoginArray[17 + a, 1];
                            (peer.Data as Player).RID = LoginArray[12 + a, 1];
                            (peer.Data as Player).Country = LoginArray[15 + a, 1];
                            if (!(peer.Data as Player).HaveGrowID)
                            {
                                Random r = new Random();
                                int randomInt = r.Next(100, 1001);
                                (peer.Data as Player).RawName = "";
                                (peer.Data as Player).DisplayName = (peer.Data as Player).RequestedName + "_" + randomInt;
                            }
                            else
                            {
                                if (!peer.SendLogin().Result)
                                {
                                    return;
                                }
                            }
                            await peer.SetHasGrowID((peer.Data as Player).TankIDName, (peer.Data as Player).TankIDPassword);
                        }
                        catch (Exception ex) { Console.WriteLine("Error occured at OnSuperMain! Message : " + ex.Message); }
                    }
                    if (action.Contains("action|enter_game") && (peer.Data as Player).InGame == false)
                    {
                        try
                        {
                            //long joinTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                            //if ((peer.Data as Player).LastPlayerEnter + 4000 < joinTime)
                            //{
                            //    (peer.Data as Player).LastPlayerEnter = joinTime;
                            //}
                            //else
                            //{
                            //    peer.DisconnectLater(0);
                            //    return;
                            //}
                            (peer.Data as Player).InGame = true;
                            if ((peer.Data as Player).TankIDName.Any(char.IsSymbol))
                            {
                                await peer.OnConsoleMessage("Please remove bad characters in your name!");
                                peer.DisconnectLater(0);
                                return;
                            }
                            await peer.OnConsoleMessage("Welcome to the server!");
                            await peer.SendWorldOffers();
                            await peer.UpdateClothes();
                            Inventory inventory = new Inventory();
                            InventoryItems items = new InventoryItems
                            {
                                ItemID = 18,
                                ItemCount = 1
                            };
                            inventory.Items = inventory.Items.Append(items).ToArray();
                            items.ItemID = 32;
                            items.ItemCount = 1;
                            inventory.Items = inventory.Items.Append(items).ToArray();
                            items.ItemID = 2;
                            items.ItemCount = 200;
                            inventory.Items = inventory.Items.Append(items).ToArray();
                            await peer.SendInventory(inventory);
                            (peer.Data as Player).PlayerInventory = inventory;
                            //await peer.OnDialogRequest("set_default_color|\nadd_label|big|`9Hello!|left|420|\nadd_spacer|small|" +
                            //    "\nadd_button|nothing|`oContinue|noflags|0|0|");
                        }
                        catch (Exception ex) { Console.WriteLine("action enter_game error! Message : " + ex.Message); }
                    }
                    if (Encoding.ASCII.GetString(payLoad.Take(payLoad.Length - 1).Skip(4).ToArray()) == "action|refresh_item_data\n")
                    {
                        try
                        {
                            if (ItemsDat != null)
                            {
                                await peer.OnConsoleMessage("`oOne moment, updating items data...");
                                peer.Send(ItemsDat, 0, ENetPacketFlags.Reliable);
                                (peer.Data as Player).Updating = true;
                                string iStr = BitConverter.ToString(ItemsDat);
                                iStr = iStr.Replace("-", "");
                                peer.DisconnectLater(0);
                            }
                        }
                        catch (Exception ex) { Console.WriteLine("refresh_item_data error! Message : " + ex.Message); }
                    }
                    break;
                case 3:
                    try
                    {
                        bool isJoinReq = false;
                        string[] arr = Encoding.ASCII.GetString(payLoad.Take(payLoad.Length - 1).Skip(4).ToArray()).Split("\n".ToCharArray());
                        foreach (var a in arr)
                        {
                            if (a == "") continue;
                            string id = a.Substring(0, a.IndexOf("|"));
                            string act = a.Substring(a.IndexOf("|") + 1, a.Length - a.IndexOf("|") - 1);
                            if (id == "name" && isJoinReq)
                            {
                                await peer.JoinWorld(act, 0, 0);
                                (peer.Data as Player).World = act;
                            }
                            if (id == "action")
                            {

                                if (act == "join_request")
                                {
                                    isJoinReq = true;
                                }

                                if (act == "quit_to_exit")
                                {
                                    await peer.SendLeave();
                                    (peer.Data as Player).World = "EXIT";
                                    await peer.SendWorldOffers();
                                }

                                if (act == "quit")
                                {
                                    peer.DisconnectLater(0);
                                }
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error at case 3 of action! Message : " + ex.Message); }
                    break;
                case 4:
                    byte[] tankUpdatePacket = payLoad.Skip(4).ToArray();
                    if (tankUpdatePacket.Length != 0)
                    {
                        try
                        {
                            var pMov = UnpackPlayerMoving(tankUpdatePacket);
                            switch (pMov.PacketType)
                            {
                                case 0:
                                    (peer.Data as Player).X = (int)pMov.X;
                                    (peer.Data as Player).Y = (int)pMov.Y;
                                    (peer.Data as Player).IsRotatingLeft = (pMov.CharacterState & 0x10) != 0;
                                    await peer.SendPData(pMov);
                                    if (!(peer.Data as Player).ClothesUpdated)
                                    {
                                        (peer.Data as Player).ClothesUpdated = true;
                                        await peer.UpdateClothes();
                                    }
                                    break;
                                default:
                                    break;
                            }
                            var data2 = UnpackPlayerMoving(tankUpdatePacket);
                            if (data2.PacketType != 3)
                                await peer.SendPacketType(data2.PacketType, pMov, data2, world);
                            else
                            {
                                await peer.SendPacketType(data2.PacketType, pMov, data2,
                                                        world);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error occured in packet types! Message : " + ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Bad Tank Packet detected!");
                    }
                    break;
                case 5:
                    break;
                case 6:
                    break;
            }
        }
    }
}
