using ENet.Managed;
using System;
using System.Threading.Tasks;
using static GTServer.Program;
using static GTServer.Resources.Packet;
using static GTServer.Methods;
using System.Text;
using System.Linq;
using MySql.Data.MySqlClient;

namespace GTServer.Resources
{
    public static class ExtensionPacket
    {
        public static async Task SendClothes(this ENetPeer peer)
        {
            try
            {
                var p3 = PacketEnd(AppendFloat(
                        AppendIntx(
                            AppendFloat(
                                AppendFloat(
                                    AppendFloat(AppendString(CreatePacket(), "OnSetClothing"),
                                        (peer.Data as Player).HairClothes, (peer.Data as Player).ShirtClothes,
                                        (peer.Data as Player).PantsClothes),
                                    (peer.Data as Player).FeetClothes, (peer.Data as Player).FaceClothes,
                                    (peer.Data as Player).HandClothes), (peer.Data as Player).BackClothes,
                                (peer.Data as Player).MaskClothes, (peer.Data as Player).NecklaceClothes),
                            (int)(peer.Data as Player).SkinColor), (peer.Data as Player).AncesClothes, 0.0f,
                        0.0f));
                for (var i = 0; i < Peers.Count; i++)
                {
                    var currentPeer = Peers[i];
                    if (Peers[i].State != ENetPeerState.Connected)
                        continue;
                    if (peer.InWorld(currentPeer) && peer != currentPeer)
                    {
                        Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p3.Data, 8, 4); // ffloor
                        currentPeer.Send(p3.Data, 0, ENetPacketFlags.Reliable);
                    }
                }

                await Task.CompletedTask;
            }
            catch
            {
                Console.WriteLine("error in async Task sendclothes");
            }
        }
        public static async Task SendCreateGrowIDError(this ENetPeer peer, string errorMessage)
        {
            Dialog d = new Dialog();
            string dialog = d.SetDefaultColor()
                .AddBigLabel("Create GrowID")
                .AddSmallSpacer().AddSmallLabel(errorMessage)
                .AddTextInput("growid", "GrowID : ", "", 15)
                .AddTextInput("password", "Password : ", "", 20)
                .AddTextInput("email", "Email", "", 20)
                .AddTextInput("discord", "Discord", "", 20)
                .EndDialog("creategrowid", "Cancel", "Create!").Result;
            await peer.OnDialogRequest(dialog);
        }
        public static async Task CreateGrowID(this ENetPeer peer, string growid, string password, string email, string discord)
        {
            try
            {
                await TryCreateTableAsync();
                string cString = "SELECT * FROM player WHERE TankIDName = '" + growid + "'";
                MySqlCommand c = new MySqlCommand(cString, DbContext.Connection);
                var reader = c.ExecuteReaderAsync().Result;
                if (reader.HasRows)
                {
                    await reader.CloseAsync();
                    await c.DisposeAsync();
                    await peer.SendCreateGrowIDError("`4Username have been existed! Pick another username!");
                    await peer.OnConsoleMessage("`4Username have been existed! Pick another username!");
                    return;
                }
                else if (growid.All(char.IsWhiteSpace))
                {
                    await reader.CloseAsync();
                    await c.DisposeAsync();
                    await peer.SendCreateGrowIDError("`4Username can't be blank!");
                    await peer.OnConsoleMessage("`4Username can't be blank!");
                    return;
                }
                else if (growid.Any(char.IsSymbol) || growid.Any(char.IsWhiteSpace))
                {
                    await reader.CloseAsync();
                    await c.DisposeAsync();
                    await peer.SendCreateGrowIDError("`4Username can't contains symbols or space!");
                    await peer.OnConsoleMessage("`4Username can't contains symbols or space!");
                    return;
                }
                else if (growid.Length < 3 || growid.Length > 15)
                {
                    await reader.CloseAsync();
                    await c.DisposeAsync();
                    await peer.SendCreateGrowIDError("`4Username length must between 3 and 15!");
                    await peer.OnConsoleMessage("`4Username length must between 3 and 15!");
                    return;
                }
                else if (password.Length < 6)
                {
                    await peer.SendCreateGrowIDError("`4Password length must be above than 6 characters!");
                    await peer.OnConsoleMessage("`4Password length must be above than 6 characters!");
                    await reader.CloseAsync();
                    await c.DisposeAsync();
                    return;
                }
                else if (!discord.Contains('#') || discord.Length < 5 || !discord.Substring(discord.Length - 4).All(char.IsNumber))
                {
                    await peer.SendCreateGrowIDError("`4Invalid discord username!");
                    await peer.OnConsoleMessage("`4Invalid discord username!");
                    await reader.CloseAsync();
                    await c.DisposeAsync();
                    return;
                }
                else if (!email.Contains('@') || !email.Contains(".com"))
                {
                    await peer.SendCreateGrowIDError("`4Invalid email address!");
                    await peer.OnConsoleMessage("`4Invalid email address!");
                    await reader.CloseAsync();
                    await c.DisposeAsync();
                    return;
                }
                else
                {
                    await reader.CloseAsync();
                    await c.DisposeAsync();
                    string str = "INSERT INTO player(TankIDName,TankIDPassword,Email,Discord,AdminID) " +
                        "VALUES ('" + growid + "','" + password + "','" + email + "','" + discord + "', 0)";
                    MySqlCommand command = new MySqlCommand(str, DbContext.Connection);
                    await command.ExecuteNonQueryAsync();
                    await command.DisposeAsync();
                    Dialog d = new Dialog();
                    string dialog = d.SetDefaultColor()
                        .AddBigLabel("Succeed!")
                        .AddSmallLabel("Account with name : " + growid + " has been created!")
                        .EndDialog("nothing", "", "Continue").Result;
                    await peer.OnDialogRequest(dialog);
                    (peer.Data as Player).RawName = growid;
                    (peer.Data as Player).TankIDName = growid;
                    (peer.Data as Player).Email = email;
                    (peer.Data as Player).DisplayName = growid;
                    (peer.Data as Player).Discord = discord;
                    (peer.Data as Player).TankIDPassword = password;
                    (peer.Data as Player).HaveGrowID = true;
                    await peer.OnNameChanged(growid);
                    await peer.SendSound("piano_nice.wav");
                }
            }
            catch (Exception e) { Console.WriteLine("Error at creating growid! Message : " + e.Message); }
        }
        public static async Task OnNameChanged(this ENetPeer peer, string growid)
        {
            Packet p = PacketEnd(AppendString(AppendString(CreatePacket(), "OnNameChanged"), growid));
            Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p.Data, 8, 4);
            foreach(var a in Peers)
            {
                if (a.State != ENetPeerState.Connected) continue;
                a.Send(p.Data, 0, ENetPacketFlags.Reliable);
            }
            await Task.CompletedTask;
        }
        private static async Task TryCreateTableAsync()
        {
            try
            {
                string str = "CREATE TABLE IF NOT EXISTS player (TankIDName TEXT, TankIDPassword TEXT, Email TEXT, Discord TEXT,AdminID INTEGER)";
                MySqlCommand command = new MySqlCommand(str, DbContext.Connection);
                await command.ExecuteNonQueryAsync();
                await command.DisposeAsync(); // To prevent multiple DataReader
            }
            catch(Exception e)
            {
                Console.WriteLine("Creating table failed! Message : " + e.Message);
            }
        }

        public static async Task SendSound(this ENetPeer peer, string soundName)
        {
            var text = "action|play_sfx\nfile|audio/" + soundName + "\ndelayMS|0\n";
            var data = new byte[5 + text.Length];
            var zero = 0;
            var type = 3;
            Array.Copy(BitConverter.GetBytes(type), 0, data, 0, 4);
            Array.Copy(Encoding.ASCII.GetBytes(text), 0, data, 4, text.Length);
            Array.Copy(BitConverter.GetBytes(zero), 0, data, 4 + text.Length, 1);
            peer.Send(data, 0, ENetPacketFlags.Reliable);
            await Task.CompletedTask;
        }
        public static async Task UpdateClothes(this ENetPeer peer)
        {
            await peer.OnSetClothing(peer);
            foreach (var currentPeer in Peers)
            {
                if (currentPeer.State != ENetPeerState.Connected)
                    continue;
                if (peer.InWorld(currentPeer) && peer != currentPeer)
                {
                    await peer.OnSetClothing(currentPeer);
                    //enet_host_flush(server);
                    await currentPeer.OnSetClothing(peer);
                }
            }
            await Task.CompletedTask;
        }
        public static async Task OnSetClothing(this ENetPeer peer, ENetPeer sendTo)
        {
            var p3 = PacketEnd(AppendFloat(
                        AppendIntx(
                            AppendFloat(
                                AppendFloat(
                                    AppendFloat(AppendString(CreatePacket(), "OnSetClothing"),
                                        (peer.Data as Player).HairClothes, (peer.Data as Player).ShirtClothes,
                                        (peer.Data as Player).PantsClothes),
                                    (peer.Data as Player).FeetClothes, (peer.Data as Player).FaceClothes,
                                    (peer.Data as Player).HandClothes), (peer.Data as Player).BackClothes,
                                (peer.Data as Player).MaskClothes, (peer.Data as Player).NecklaceClothes),
                            (int)(peer.Data as Player).SkinColor), (peer.Data as Player).AncesClothes, 0.0f,
                        0.0f));
            Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p3.Data, 8, 4);
            sendTo.Send(p3.Data, 0, ENetPacketFlags.Reliable);
            await Task.CompletedTask;
        }
        public static async Task SendChat(this ENetPeer peer,string message)
        {
            var name = (peer.Data as Player).DisplayName;
            int id = (peer.Data as Player).NetID;
            foreach(var a in Peers)
            {
                if (a.State != ENetPeerState.Connected) continue;
                if (a.InWorld(peer))
                {
                    await a.OnConsoleMessage("CP:0_PL:4_OID:_CT:[W]_ `o<`w" + name + "`o> " + message);
                    await a.OnTalkBubble(message, id);
                }
            }
        }
        
        public static async Task OnSetBux(this ENetPeer peer, int gems)
        {
            Packet p = PacketEnd(AppendInt(AppendString(CreatePacket(), "OnSetBux"), gems));
            peer.Send(p.Data, 0, ENetPacketFlags.Reliable);
            await Task.CompletedTask;
        }
        public static async Task SendRoulette(this ENetPeer peer)
        {
            try
            {
                Random rand = new Random();
                int val = rand.Next(0, 37);
                foreach (ENetPeer currentPeer in Peers)
                {
                    if (currentPeer.State != ENetPeerState.Connected)
                        continue;
                    if (peer.InWorld(currentPeer))
                    {
                        await peer.OnTalkBubble("`w[" + (peer.Data as Player).DisplayName + " `wspun the wheel and got `6" + val + "`w!]");
                        await currentPeer.OnConsoleMessage("`w[" + (peer.Data as Player).DisplayName + " `wspun the wheel and got `6" + val + "`w!]");
                    }

                    //cout << "Tile update at: " << data2->punchX << "x" << data2->punchY << endl;
                }
                await Task.CompletedTask;
            }
            catch
            {
                Console.WriteLine("error in async Task sendRoulete");
            }
        }
        public static async Task SendPData(this ENetPeer peer, PlayerMoving data)
        {
            try
            {
                foreach (ENetPeer currentPeer in Peers)
                {
                    if (currentPeer.State != ENetPeerState.Connected)
                        continue;
                    if (peer != currentPeer)
                    {
                        if (peer.InWorld(currentPeer))
                        {
                            data.NetID = (peer.Data as Player).NetID;
                            await SendPacketRaw(4, PlayerMovingPack(data), 56, 0, currentPeer);
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("error in async Task sendPData");
            }
        }
        public static async Task OnAddNotification(this ENetPeer peer,string symbol,string text, string audioName)
        {
            Packet ps2 = PacketEnd(AppendInt(AppendString(AppendString(AppendString(AppendString(CreatePacket(), "OnAddNotification"),
                symbol), text), audioName), 0));
            peer.Send(ps2.Data, 0, ENetPacketFlags.Reliable);
            await Task.CompletedTask;
        }
        public static async Task SetRespawnPos(this ENetPeer peer,int x, int y, World world)
        {
            Packet p3 = PacketEnd(AppendInt(AppendString(CreatePacket(), "SetRespawnPos"), x + (y * world.Width)));
            Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p3.Data, 8, 4);
            peer.Send(p3.Data, 0, ENetPacketFlags.Reliable);
            await Task.CompletedTask;
        }
        public static async Task SendLeave(this ENetPeer peer)
        {
            try
            {
                var player = peer.Data as Player;
                Packet p = PacketEnd(AppendString(AppendString(CreatePacket(), "OnRemove"), "netID|" + player.NetID + "\n")); // ((Player*)(server->peers[i].data))->tankIDName
                foreach (ENetPeer currentPeer in Peers)
                {
                    if (currentPeer.State != ENetPeerState.Connected)
                        continue;
                    if (peer.InWorld(currentPeer))
                    {
                        currentPeer.Send(p.Data, 0, ENetPacketFlags.Reliable);
                        await currentPeer.OnConsoleMessage("`5<`w" + player.DisplayName + "`` left, `w" + CountPlayer(player.World) + "`` others here>``");
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("SendLeave error! Message : " + e.Message);
            }
        }
        public static async Task JoinWorld(this ENetPeer peer,string act, int x2, int y2)
        {
            try
            {
                act = act.ToUpper();
                World info = WorldDatabase.GetWorld(act).WorldInfo;
                string name = act;
                if (name == "CON" || name == "PRN" || name == "AUX" || name == "NUL" || name == "COM1" || name == "COM2" || name == "COM3" || name == "COM4" || name == "COM5" || name == "COM6" || name == "COM7" || name == "COM8" || name == "COM9" || name == "LPT1" || name == "LPT2" || name == "LPT3" || name == "LPT4" || name == "LPT5" || name == "LPT6" || name == "LPT7" || name == "LPT8" || name == "LPT9" || name == "con" || name == "prn" || name == "aux" || name == "nul" || name == "com1" || name == "com2" || name == "com3" || name == "com4" || name == "com5" || name == "com6" || name == "com7" || name == "com8" || name == "com9" || name == "lpt1" || name == "lpt2" || name == "lpt3" || name == "lpt4" || name == "lpt5" || name == "lpt6" || name == "lpt7" || name == "lpt8" || name == "lpt9")
                {
                    await peer.OnConsoleMessage("`4Sorry `w this world is used by the system");
                    await peer.OnFailedToEnterWorld();
                    return;
                }
                else if (act.Length > 25)
                {
                    await peer.OnConsoleMessage("`4Sorry, System doesnt accept more than 30 letter in world name, you will be disconnected");
                    await peer.OnFailedToEnterWorld();
                    return;
                }
                else if (name == "EXIT")
                {
                    await peer.OnConsoleMessage("`4You can't enter this world!");
                    await peer.OnFailedToEnterWorld();
                    return;
                }
                await peer.SendWorld(info);// sendWorld(peer, info);
                int x = 3040;
                int y = 736;

                for (int j = 0; j < info.Width * info.Height; j++)
                {
                    if (info.Items[j].Foreground == 6)
                    {
                        x = (j % info.Width) * 32;
                        y = (j / info.Width) * 32;
                    }
                }
                (peer.Data as Player).CpX = x;
                (peer.Data as Player).CpY = y;
                if (x2 != 0 && y2 != 0)
                {
                    x = x2;
                    y = y2;
                }
                //enet_host_flush(server);
                (peer.Data as Player).NetID = PeerNetID;
                await peer.OnPeerConnect();
                Packet p = PacketEnd(AppendString(AppendString(CreatePacket(), "OnSpawn"), "spawn|avatar\nnetID|" + PeerNetID + "\nuserID|" + PeerNetID + "\ncolrect|0|0|20|30\nposXY|" + x + "|" + y + "\nname|``" + (peer.Data as Player).DisplayName + "``\ncountry|" + (peer.Data as Player).Country + "|" + 244 + "\ninvis|0\nmstate|0\nsmstate|0\ntype|local\n"));
                peer.Send(p.Data, 0, ENetPacketFlags.Reliable);
                
                PeerNetID++;
                await peer.UpdateClothes();
                await peer.SendInventory((peer.Data as Player).PlayerInventory);
            }
            catch (Exception ee)
            {
                Console.WriteLine("NOOO " + ee.Message + " " + ee.Source + " " + ee.Data + " " + ee.InnerException);
                int e = 0;
                if (e == 1)
                {
                    await peer.OnConsoleMessage("You have exited the world.");
                    await peer.OnFailedToEnterWorld();
                    return;
                    //enet_host_flush(server);
                }
                else if (e == 2)
                {
                    
                    await peer.OnConsoleMessage("You have entered bad characters in the world name!");
                    await peer.OnFailedToEnterWorld();
                    return;
                    //enet_host_flush(server);
                }
                else if (e == 3)
                {
                    await peer.OnConsoleMessage("Exit from what? Click back if you're done playing.");
                    await peer.OnFailedToEnterWorld();
                    return;
                    //enet_host_flush(server);
                }
                else
                {
                    await peer.OnConsoleMessage("I know this menu is magical and all, but it has its limitations! You can't visit this world!");
                    await peer.OnFailedToEnterWorld();
                    return;
                    //enet_host_flush(server);
                }
            }
        }
        public static async Task SendWorld(this ENetPeer peer,World worldInfo)
        {
            try
            {
                (peer.Data as Player).ClothesUpdated = false;
                var asdf =
                    "0400000004A7379237BB2509E8E0EC04F8720B050000000000000000FBBB0000010000007D920100FDFDFDFD04000000040000000000000000000000070000000000"; // 0400000004A7379237BB2509E8E0EC04F8720B050000000000000000FBBB0000010000007D920100FDFDFDFD04000000040000000000000000000000080000000000000000000000000000000000000000000000000000000000000048133A0500000000BEBB0000070000000000
                var worldName = worldInfo.Name;
                var xSize = worldInfo.Width;
                var ySize = worldInfo.Height;
                var square = xSize * ySize;
                var nameLen = (short)worldName.Length;
                var payloadLen = asdf.Length / 2;
                var dataLen = payloadLen + 2 + nameLen + 12 + square * 8 + 4 + 100;
                int offsetData;
                var allocMem = payloadLen + 2 + nameLen + 12 + square * 8 + 4 + 16000 + 100;
                var data = new byte[allocMem];
                for (var io = 0; io < allocMem; io++) data[io] = 0;
                for (var i = 0; i < asdf.Length; i += 2)
                {
                    var x = (char)CharToByte(asdf[i]);
                    x = Convert.ToChar(x << 4);
                    x += Convert.ToChar(CharToByte(asdf[i + 1]));
                    Array.Copy(BitConverter.GetBytes(x), 0, data, i / 2, 1);
                }

                short item = 0;
                var smth = 0;
                var zero = 0;
                for (var i = 0; i < square * 8; i += 4)
                    Array.Copy(BitConverter.GetBytes(zero), 0, data, payloadLen + i + 14 + nameLen, 4);
                for (var i = 0; i < square * 8; i += 8)
                    Array.Copy(BitConverter.GetBytes(item), 0, data, payloadLen + i + 14 + nameLen, 2);
                Array.Copy(BitConverter.GetBytes(nameLen), 0, data, payloadLen, 2);
                Array.Copy(Encoding.ASCII.GetBytes(worldName), 0, data, payloadLen + 2, nameLen);
                Array.Copy(BitConverter.GetBytes(xSize), 0, data, payloadLen + 2 + nameLen, 4);
                Array.Copy(BitConverter.GetBytes(ySize), 0, data, payloadLen + 6 + nameLen, 4);
                Array.Copy(BitConverter.GetBytes(square), 0, data, payloadLen + 10 + nameLen, 4);
                var blockPtr = payloadLen + 14 + nameLen;

                for (var i = 0; i < square; i++)
                {
                    int sizeofblockstruct = 8;
                    if (worldInfo.Items[i].Foreground == 0 || worldInfo.Items[i].Foreground == 2 ||
                        worldInfo.Items[i].Foreground == 8 || worldInfo.Items[i].Foreground == 100)
                    {
                        Array.Copy(BitConverter.GetBytes(worldInfo.Items[i].Foreground), 0, data, blockPtr, 2);
                        long type = 0x00000000;
                        // type 1 = locked
                        if (worldInfo.Items[i].IsWater)
                            type |= 0x04000000;
                        Array.Copy(BitConverter.GetBytes(type), 0, data, blockPtr + 4, 4);
                    }
                    else
                    {
                        Array.Copy(BitConverter.GetBytes(zero), 0, data, blockPtr, 2);
                    }

                    Array.Copy(BitConverter.GetBytes(worldInfo.Items[i].Background), 0, data, blockPtr + 2, 2);
                    blockPtr += sizeofblockstruct;
                }

                offsetData = dataLen - 100;
                var asdf2 =
                    "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
                var data2 = new byte[101];
                for (var i = 0; i < asdf2.Length; i += 2)
                {
                    var x = (char)CharToByte(asdf2[i]);
                    x = Convert.ToChar(x << 4);
                    x += Convert.ToChar(CharToByte(asdf2[i + 1]));
                    Array.Copy(BitConverter.GetBytes(x), 0, data2, i / 2, 1);
                }

                var weather = worldInfo.Weather;
                Array.Copy(BitConverter.GetBytes(weather), 0, data2, 4, 4);
                Array.Copy(data2, 0, data, offsetData, 100);
                Array.Copy(BitConverter.GetBytes(smth), 0, data, dataLen - 4, 4);
                peer.Send(data, 0, ENetPacketFlags.Reliable);
                for (var i = 0; i < square; i++)
                    //if (worldInfo.droppedItems[i].IsDrop)
                    //{
                    //    sendDrop(peer, (peer.Data as Player).netID, i % worldInfo.width, i / worldInfo.height, worldInfo.droppedItems[i].ItemID, worldInfo.droppedItems[i].ItemCount, 0);
                    //}
                    if (worldInfo.Items[i].Foreground == 0 || worldInfo.Items[i].Foreground == 2 ||
                        worldInfo.Items[i].Foreground == 8 || worldInfo.Items[i].Foreground == 100)
                    {
                    }
                    else if (worldInfo.Items[i].Foreground == 6) await UpdateDoor(peer, worldInfo.Items[i].Foreground, i % worldInfo.Width,i/worldInfo.Width, "fuck off");
                    else
                    {
                        var data1 = new PlayerMoving
                        {
                            PacketType = 0x3,
                            CharacterState = 0x0,
                            X = i % worldInfo.Width,
                            Y = i / worldInfo.Height,
                            PunchX = i % worldInfo.Width,
                            PunchY = i / worldInfo.Width,
                            XSpeed = 0,
                            YSpeed = 0,
                            NetID = -1,
                            PlantingTree = worldInfo.Items[i].Foreground
                        };
                        //data.packetType = 0x14;

                        //data.characterState = 0x924; // animation
                        // animation
                        await SendPacketRaw(4, PlayerMovingPack(data1), 56, 0, peer);
                    }

                var def = ItemsData[(peer.Data as Player).BackClothes];
                if (def.Name.ToLower().Contains("wing") || def.Name.ToLower().Contains("cape")
                                                        || def.Name.ToLower().Contains("aura") ||
                                                        def.Name.ToLower().Contains("dragon warrior's shield") ||
                                                        def.Name.ToLower().Contains("spirit"))
                    (peer.Data as Player).InWings = true;
                await peer.SendState();
                var wname = worldInfo.Name;
                (peer.Data as Player).World = wname;
                await peer.OnConsoleMessage(
                    "World `w" + wname + "`` entered.  There are `w" + (CountPlayer(wname) - 1) +
                    "`` other people here, `w" + CountPlayerOnline() + "`` online.``");

                if (worldInfo.OwnerName != "")
                {
                    var x = "`5[`w" + wname + " `$World Locked `oby `2" + worldInfo.OwnerName;
                    if (worldInfo.OwnerName == (peer.Data as Player).RawName) x += " `o(`2ACCESS GRANTED`o)";
                    await peer.OnConsoleMessage(x + "`5]``");
                }
                await peer.UpdateClothes();
                await peer.OnPlayPositioned("audio/door_open.wav");
                await peer.SendSound("door_open.wav");
            }
            catch (Exception e) { Console.WriteLine(e.Message + "  SENDWORLD ERROR"); }
        }
        public static async Task SendState(this ENetPeer peer)
        {
            try
            {
                //return; // TODO
                var info = peer.Data as Player;
                int netID = info.NetID;
                int state = GetState(info);

                foreach (ENetPeer currentPeer in Peers)
                {
                    if (currentPeer.State != ENetPeerState.Connected)
                        continue;
                    if (peer.InWorld(currentPeer))
                    {
                        PlayerMoving data;
                        data.PacketType = 0x14;
                        data.CharacterState = 0; // animation
                        data.X = 1000;
                        data.Y = 100;
                        data.PunchX = 0;
                        data.PunchY = 0;
                        data.XSpeed = 300; //300
                        data.YSpeed = 600; // 600
                        data.NetID = netID;
                        data.PlantingTree = state;
                        byte[] raw = PlayerMovingPack(data);
                        int var = 0x808000; // placing and breking
                        Array.Copy(BitConverter.GetBytes(var), 0, raw, 1, 3);
                        await SendPacketRaw(4, raw, 56, 0, currentPeer);
                    }
                }
            }
            catch
            {
                Console.WriteLine("error in async Task sendstate");
            }
        }
        public static bool InWorld(this ENetPeer peer,ENetPeer currentPeer)
        {
            return (peer.Data as Player).World == (currentPeer.Data as Player).World;
        }
        public static async Task OnPeerConnect(this ENetPeer peer)
        {
            try
            {
                foreach(var currentPeer in Peers)
                {
                    if (currentPeer.State != ENetPeerState.Connected) continue;
                    else if (peer.InWorld(currentPeer) && peer != currentPeer)
                    {
                        var netIdS = (currentPeer.Data as Player).NetID.ToString();
                        var p = PacketEnd(AppendString(AppendString(CreatePacket(), "OnSpawn"),
                            "spawn|avatar\nnetID|" + netIdS + "\nuserID|" + (currentPeer.Data as Player).NetID +
                            "\ncolrect|0|0|20|30\nposXY|" + (currentPeer.Data as Player).X + "|" +
                            (currentPeer.Data as Player).Y + "\nname|``" +
                            (currentPeer.Data as Player).DisplayName + "``\ncountry|" +
                            (currentPeer.Data as Player).Country +
                            "\ninvis|0\nmstate|0\nsmstate|0\n")); // ((Player*)(server->peers[i].data))->tankIDName
                                                                  //GamePacket p = packetEnd(appendString(appendString(createPacket(), "OnSpawn"), "spawn|avatar\nnetID|" + netIdS + "\nuserID|" + netIdS + "\ncolrect|0|0|20|30\nposXY|1600|1154\nname|``" + (currentPeer.Data as Player).displayName + "``\ncountry|" + (currentPeer.Data as Player).country + "\ninvis|0\nmstate|0\nsmstate|0\n")); // ((Player*)(server->peers[i].data))->tankIDName
                        peer.Send(p.Data, 0, ENetPacketFlags.Reliable);


                        var netIdS2 = (peer.Data as Player).NetID.ToString();
                        var p2 = PacketEnd(AppendString(AppendString(CreatePacket(), "OnSpawn"),
                            "spawn|avatar\nnetID|" + netIdS2 + "\nuserID|" + (peer.Data as Player).NetID +
                            "\ncolrect|0|0|20|30\nposXY|" + (peer.Data as Player).X + "|" +
                            (peer.Data as Player).Y + "\nname|``" + (peer.Data as Player).DisplayName +
                            "``\ncountry|" + (peer.Data as Player).Country +
                            "\ninvis|0\nmstate|0\nsmstate|0\n")); // ((Player*)(server->peers[i].data))->tankIDName
                                                                  //GamePacket p2 = packetEnd(appendString(appendString(createPacket(), "OnSpawn"), "spawn|avatar\nnetID|" + netIdS2 + "\nuserID|" + netIdS2 + "\ncolrect|0|0|20|30\nposXY|1600|1154\nname|``" + (peer.Data as Player).displayName + "``\ncountry|" + (peer.Data as Player).country + "\ninvis|0\nmstate|0\nsmstate|0\n")); // ((Player*)(server->peers[i].data))->tankIDName
                        currentPeer.Send(p2.Data, 0, ENetPacketFlags.Reliable);
                        await Task.CompletedTask;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("OnPeerConnect ERROR! Message : " + e.Message);
            }
        }
        public static async Task SendCreateGID(this ENetPeer peer)
        {
            Dialog d = new Dialog();
            string dialog = d.SetDefaultColor()
                .AddBigLabel("Create GrowID")
                .AddSmallSpacer().AddTextInput("growid", "GrowID : ", "", 15)
                .AddTextInput("password", "Password : ", "", 20)
                .AddTextInput("email", "Email", "", 20)
                .AddTextInput("discord", "Discord", "", 20)
                .EndDialog("creategrowid", "Cancel", "Create!").Result;
            await peer.OnDialogRequest(dialog);
        }
        public static async Task OnSpawn(this ENetPeer peer,string text)
        {
            Packet p = PacketEnd(AppendString(AppendString(CreatePacket(), "OnSpawn"), text));
            peer.Send(p.Data, 0, ENetPacketFlags.Reliable);
            await Task.CompletedTask;
        }
        public static bool Is(this string str, string action)
        {
            return str.IndexOf(action) == 0;
        }
        public static async Task SendCommands(this ENetPeer peer,string action) => await Commands.SendCommands(peer,action);
        public static async Task Respawn(this ENetPeer peer,bool die)
        {
            try
            {
                if (die == false) await peer.OnKilled();
                await peer.OnSetFreezeState(0);
                await peer.OnSetFreezeState(2);
                await peer.OnSetPos((peer.Data as Player).CpX, (peer.Data as Player).CpY);
                await peer.OnPlayPositioned("audio/teleport.wav");
                await peer.SendSound("teleport.wav");
                await Task.CompletedTask;
            }
            catch (Exception e) { Console.WriteLine("Respawn error! Message : " + e.Message); }
        }
        public static async Task OnKilled(this ENetPeer peer)
        {
            try
            {
                Packet p1 = PacketEnd(AppendString(CreatePacket(), "OnKilled"));
                Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p1.Data, 8, 4);
                peer.Send(p1.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch (Exception e) { Console.WriteLine("OnKilled error! Message : " + e.Message); }
        }
        public static async Task OnSetFreezeState(this ENetPeer peer, int x)
        {
            try {
            if (x == 2)
            {
                Packet pf = PacketEnd(AppendInt(AppendString(CreatePacket(), "OnSetFreezeState"), 2));
                Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, pf.Data, 8, 4);
                peer.Send(pf.Data, 0, ENetPacketFlags.Reliable);
            }
            else if (x == 0)
            {
                Packet p2x = PacketEnd(AppendInt(AppendString(CreatePacket(), "OnSetFreezeState"), 0));
                Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p2x.Data, 8, 4);
                int respawnTimeout = 2000;
                int deathFlag = 0x19;
                Array.Copy(BitConverter.GetBytes(respawnTimeout), 0, p2x.Data, 24, 4);
                Array.Copy(BitConverter.GetBytes(deathFlag), 0, p2x.Data, 56, 4);
                peer.Send(p2x.Data, 0, ENetPacketFlags.Reliable);
            }
            await Task.CompletedTask;
            }
            catch (Exception e) { Console.WriteLine("OnSetFreezeState error! Message : " + e.Message); }
        }
        public static async Task OnSetPos(this ENetPeer peer,int x, int y)
        {
            try
            {
                Packet p2 = PacketEnd(AppendFloat(AppendString(CreatePacket(), "OnSetPos"), x, y));
                Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p2.Data, 8, 4);
                int respawnTimeout = 2000;
                int deathFlag = 0x19;
                Array.Copy(BitConverter.GetBytes(respawnTimeout), 0, p2.Data, 24, 4);
                Array.Copy(BitConverter.GetBytes(deathFlag), 0, p2.Data, 56, 4);
                peer.Send(p2.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch (Exception e) { Console.WriteLine("OnSetPos error! Message : " + e.Message); }
        }
        public static async Task OnPlayPositioned(this ENetPeer peer,string audioName)
        {
            try
            {
                Packet p2a = PacketEnd(AppendString(AppendString(CreatePacket(), "OnPlayPositioned"), audioName));
                Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p2a.Data, 8, 4);
                int deathFlag = 0x19;
                int respawnTimeout = 2000;
                Array.Copy(BitConverter.GetBytes(respawnTimeout), 0, p2a.Data, 24, 4);
                Array.Copy(BitConverter.GetBytes(deathFlag), 0, p2a.Data, 56, 4);
                peer.Send(p2a.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch (Exception e) { Console.WriteLine("OnPlayPositioned error! Message : " + e.Message); }
        }
        public static async Task OnSuperMain(this ENetPeer peer)
        {
            var hash = 1720493140;
            Packet p = PacketEnd(AppendString(
                AppendString(
                    AppendString(
                        AppendString(
                            AppendInt(
                                AppendString(CreatePacket(),
                            //														"OnSuperMainStartAcceptLogonHrdxs47254722215a"), -703607114),
                            "OnSuperMainStartAcceptLogonHrdxs47254722215a"), hash),
                            "ubistatic-a.akamaihd.net"), "0098/CDNContent75/cache/"),
                    "cc.cz.madkite.freedom org.aqua.gg idv.aqua.bulldog com.cih.gamecih2 com.cih.gamecih com.cih.game_cih cn.maocai.gamekiller com.gmd.speedtime org.dax.attack com.x0.strai.frep com.x0.strai.free org.cheatengine.cegui org.sbtools.gamehack com.skgames.traffikrider org.sbtoods.gamehaca com.skype.ralder org.cheatengine.cegui.xx.multi1458919170111 com.prohiro.macro me.autotouch.autotouch com.cygery.repetitouch.free com.cygery.repetitouch.pro com.proziro.zacro com.slash.gamebuster"),
                "proto=42|choosemusic=audio/mp3/about_theme.mp3|active_holiday=0|"));
            //for (int i = 0; i < p.len; i++) cout << (int)*(p.data + i) << " ";
            peer.Send(p.Data, 0, ENetPacketFlags.Reliable);
            await Task.CompletedTask;
        }
        public static async Task SendWorldOffers(this ENetPeer peer)
        {
            if (!(peer.Data as Player).InGame) return;
            World[] worldz = WorldDatabase.GetRandomWorlds();
            string worldOffers = "default|";
            if (worldz.Length > 0)
            {
                worldOffers += worldz[0].Name;
            }

            worldOffers += "\nadd_button|Showing: `wWorlds``|_catselect_|0.6|3529161471|\n";
            for (int i = 0; i < worldz.Length; i++)
            {
                worldOffers += "add_floater|" + worldz[i].Name + "|" + 69 + "|0.55|3529161471\n";
            }
            Packet p3 = PacketEnd(AppendString(AppendString(CreatePacket(), "OnRequestWorldSelectMenu"), worldOffers));
            peer.Send(p3.Data, 0, ENetPacketFlags.Reliable);
            await Task.CompletedTask;
            await Task.CompletedTask;
        }
        public static async Task OnConsoleMessage(this ENetPeer peer, string text)
        {
            try
            {
                Packet p = PacketEnd(AppendString(AppendString(CreatePacket()
                    , "OnConsoleMessage"),
                                    text));
                peer.Send(p.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch(Exception e)
            {
                Console.WriteLine("OnConsoleMessage error! Message : " + e.Message);
            }
        }
        public static async Task<bool> SendLogin(this ENetPeer peer)
        {
            string str = "SELECT * FROM player WHERE TankIDName = '" + (peer.Data as Player).TankIDName + "' AND TankIDPassword = '" + (peer.Data as Player).TankIDPassword + "'";
            MySqlCommand command = new MySqlCommand(str, DbContext.Connection);
            var reader = command.ExecuteReaderAsync().Result;
            try
            {
                if (!reader.HasRows)
                {
                    await reader.CloseAsync();
                    await command.DisposeAsync();
                    await peer.OnConsoleMessage("`5Invalid username or password!");
                    (peer.Data as Player).HaveGrowID = true;
                    peer.DisconnectNow(0);
                    return false;
                }
                else
                {
                    if (reader.Read())
                    {
                        (peer.Data as Player).TankIDName = reader["TankIDName"].ToString();
                        (peer.Data as Player).TankIDPassword = reader["TankIDPassword"].ToString();
                        (peer.Data as Player).RawName = reader["TankIDName"].ToString();
                        (peer.Data as Player).DisplayName = reader["TankIDName"].ToString();
                        (peer.Data as Player).Discord = reader["Discord"].ToString();
                        (peer.Data as Player).Email = reader["Email"].ToString();
                        (peer.Data as Player).AdminID = Convert.ToInt32(reader["AdminID"].ToString());
                        await reader.CloseAsync();
                        await command.DisposeAsync();
                    }
                    (peer.Data as Player).HaveGrowID = true;
                    return true;
                }
            }
            catch (Exception e) { Console.WriteLine("Error at login! Message : " +  e.Message + " " + e.TargetSite); await reader.CloseAsync(); return false; }
        }
        public static async Task OnDialogRequest(this ENetPeer peer,string dialog)
        {
            try
            {
                Packet p = PacketEnd(AppendString(AppendString(
                    CreatePacket(), "OnDialogRequest"),
                                    dialog + "\n"));
                peer.Send(p.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine("OnDialogRequest error! Message : " + e.Message);
            }
        }
        public static async Task OnTextOverlay(this ENetPeer peer, string text)
        {
            try
            {
                Packet p = PacketEnd(AppendString(AppendString(
                    CreatePacket(), "OnTextOverlay"),
                                    text));
                peer.Send(p.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine("OnTextOverlay error! Message : " + e.Message);
            }
        }
        public static async Task OnTalkBubble(this ENetPeer peer, string textBubble,int id = 0)
        {
            try
            {
                if(id==0)
                    id = (peer.Data as Player).NetID;
                Packet p3o = PacketEnd(AppendString
                    (AppendIntx(AppendString(CreatePacket(),
                    "OnTalkBubble"), id),
                    textBubble));
                peer.Send(p3o.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine("OnTalkBubble error! Message : " + e.Message);
            }
        }
        public static async Task SetHasGrowID(this ENetPeer peer,string username, string password)
        {
            try
            {
                Packet p2 = PacketEnd(AppendString(AppendString
                    (AppendInt(AppendString(CreatePacket(), "SetHasGrowID"), 1),
                        username), password));
                peer.Send(p2.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine("SetHasGrowID error! Message : " + e.Message);
            }
        }
        public static async Task OnFailedToEnterWorld(this ENetPeer peer)
        {
            try
            {
                Packet p3 = PacketEnd(AppendString(AppendInt(AppendString(CreatePacket(), "OnFailedToEnterWorld"), 1), "Failed"));
                peer.Send(p3.Data, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine("OnFailedToEnterWorld error! Message : " + e.Message);
            }
        }
        public static async Task SendData(this ENetPeer peer, int num, byte[] Data, int len)
        {
            try
            {
                byte[] packet = new byte[len + 5];
                Array.Copy(BitConverter.GetBytes(num), 0, packet, 0, 4);
                if (Data != null)
                {
                    Array.Copy(Data, 0, packet, 4, len);
                }

                packet[4 + len] = 0;
                peer.Send(packet, 0, ENetPacketFlags.Reliable);
                Server.Flush();
                await Task.CompletedTask;
            }
            catch(Exception e)
            {
                Console.WriteLine("Error in SendData! Message : " + e.Message);
            }
        }
        public static async Task OnAction(this ENetPeer peer, string text)
        {
            var p2 = PacketEnd(AppendString(AppendString(CreatePacket(), "OnAction"), text));

            foreach (var currentPeer in Peers)
            {
                if (currentPeer.State != ENetPeerState.Connected)
                    continue;
                if (peer.InWorld(currentPeer))
                {
                    Array.Copy(BitConverter.GetBytes((peer.Data as Player).NetID), 0, p2.Data, 8, 4);

                    currentPeer.Send(p2.Data, 0, ENetPacketFlags.Reliable);
                    //enet_host_flush(server);
                }
            }
            await Task.CompletedTask;
        }
        public static async Task SendInventory(this ENetPeer peer, Inventory inventory)
        {
            try
            {
                string asdf2 = "0400000009A7379237BB2509E8E0EC04F8720B050000000000000000FBBB0000010000007D920100FDFDFDFD04000000040000000000000000000000000000000000";
                int inventoryLen = inventory.Items.Length;
                int packetLen = (asdf2.Length / 2) + (inventoryLen * 4) + 4;
                byte[] data2 = new byte[packetLen];
                for (int i = 0; i < asdf2.Length; i += 2)
                {
                    byte x = CharToByte(asdf2[i]);
                    x = (byte)(x << 4);
                    x += CharToByte(asdf2[i + 1]);
                    data2[i / 2] = x;
                }
                byte[] endianInvVal = BitConverter.GetBytes(inventoryLen);
                Array.Reverse(endianInvVal);
                Array.Copy(endianInvVal, 0, data2, asdf2.Length / 2 - 4, 4);
                endianInvVal = BitConverter.GetBytes(inventory.InventorySize);
                Array.Reverse(endianInvVal);
                Array.Copy(endianInvVal, 0, data2, asdf2.Length / 2 - 8, 4);
                int val = 0;
                for (int i = 0; i < inventoryLen; i++)
                {
                    val = 0;
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
                    val |= inventory.Items[i].ItemID;
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
                    val |= inventory.Items[i].ItemCount << 16;
                    val &= 0x00FFFFFF;
                    val |= 0x00 << 24;
                    byte[] value = BitConverter.GetBytes(val);
                    Array.Copy(value, 0, data2, asdf2.Length / 2 + (i * 4), 4);
                }

                peer.Send(data2, 0, ENetPacketFlags.Reliable);
                await Task.CompletedTask;
                //enet_host_flush(server);
            }
            catch(Exception e)
            {
                Console.WriteLine("error in async Task sendinventory! Message : " + e.Message);
            }
        }
        public static World GetWorld(this ENetPeer peer)
        {
            return WorldDatabase.GetWorld((peer.Data as Player).World).WorldInfo;
        }
    }
}