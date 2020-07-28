using ENet.Managed;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GTServer.Methods;
using static GTServer.Program;

namespace GTServer.Resources
{
    public class TileUpdate
    {
        public static async Task SendTileUpdate(int x, int y, int tile, int causedBy, ENetPeer peer)
        {
            try
            {
                if (tile == 18)
                {
                    (peer.Data as Player).LastPunchX = x;
                    (peer.Data as Player).LastPunchY = y;
                }
                else if (tile == 32)
                {
                    (peer.Data as Player).LastWrenchX = x;
                    (peer.Data as Player).LastWrenchY = y;
                }
                PlayerMoving data = new PlayerMoving
                {
                    PacketType = 0x3,
                    CharacterState = 0x0,
                    X = x,
                    Y = y,
                    PunchX = x,
                    PunchY = y,
                    XSpeed = 0,
                    YSpeed = 0,
                    NetID = causedBy,
                    PlantingTree = tile
                };
                World world = (peer.Data as Player).PWorld;
                if (world.OwnerName != "" && tile == 242) return;
                int b = world.Items[x + y * world.Width].Background;
                if (world == null) return;
                if (x < 0 || y < 0 || x > world.Width || y > world.Height) return;
                await SendNothingHappened(peer, x, y);
                if ((peer.Data as Player).AdminID < 666)
                {
                    if (world.Items[x + (y * world.Width)].Foreground == 6 || world.Items[x + (y * world.Width)].Foreground == 8 || world.Items[x + (y * world.Width)].Foreground == 3760)
                    {
                        return;
                    }
                    if (tile == 6 || tile == 8 || tile == 3760)
                    {
                        return;
                    }
                }
                if ((peer.Data as Player).RawName != world.OwnerName)
                {
                    if (world.Items[x + (y * world.Width)].Foreground == 242)
                    {
                        return;
                    }
                }
                if (world.Name == "ADMIN" && (peer.Data as Player).AdminID > 0)
                {
                    if (world.Items[x + (y * world.Width)].Foreground == 758)
                        await peer.SendRoulette();
                    return;
                }
                if (tile == 32 && world.Items[x + y * world.Width].Foreground == 6016)
                {
                    await SendGrowScanDialog(peer);
                }
                if (tile != 18 && tile != 32 && !ItemsData[tile].IsBackground && world.Items[x + (y * world.Width)].Foreground != 0)
                {
                    if (tile != 822)
                    { // allowed to place water on other blocks
                        await SendNothingHappened(peer, x, y);
                        return;
                    }
                }
                if (world.Name != "ADMIN")
                {
                    if (world.OwnerName != "")
                    {
                        int fg = world.Items[x + (y * world.Width)].Foreground;
                        if (world.OwnerName != "")
                        {
                            var p = peer.Data as Player;
                            if (!world.IsPublic)
                            {
                                if (p.RawName != world.OwnerName)
                                {
                                    await peer.OnPlayPositioned("audio/punch_locked.wav");
                                    await peer.SendSound("punch_locked.wav");
                                    if (ItemsData[fg].Name.ToLower().Contains("lock")) await peer.OnConsoleMessage(world.OwnerName + "'s `9World Lock");
                                }
                            }
                            else
                            {
                                if (p.RawName != world.OwnerName && ItemsData[fg].Name.ToLower().Contains("lock"))
                                {
                                    await peer.OnPlayPositioned("audio/punch_locked.wav");
                                    await peer.SendSound("punch_locked.wav");
                                    await peer.OnConsoleMessage(world.OwnerName + "'s World Lock `o(`$Open to Public`o)");
                                }
                            }
                        }
                        else if (world.IsPublic)
                        {
                            if (world.Items[x + (y * world.Width)].Foreground == 242)
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                if (tile == 32)
                {
                    // TODO
                    return;
                }
                DataItems def = new DataItems();
                try
                {
                    def = ItemsData[tile];
                }
                catch
                {
                    def.BreakHits = 4;
                }

                if (tile == 544 || tile == 546 || tile == 4520 || tile == 382 || tile == 3116 || tile == 4520 || tile == 1792 || tile == 5666 || tile == 2994 || tile == 4368) return;
                if (tile == 5708 || tile == 5709 || tile == 5780 || tile == 5781 || tile == 5782 || tile == 5783 || tile == 5784 || tile == 5785 || tile == 5710 || tile == 5711 || tile == 5786 || tile == 5787 || tile == 5788 || tile == 5789 || tile == 5790 || tile == 5791 || tile == 6146 || tile == 6147 || tile == 6148 || tile == 6149 || tile == 6150 || tile == 6151 || tile == 6152 || tile == 6153 || tile == 5670 || tile == 5671 || tile == 5798 || tile == 5799 || tile == 5800 || tile == 5801 || tile == 5802 || tile == 5803 || tile == 5668 || tile == 5669 || tile == 5792 || tile == 5793 || tile == 5794 || tile == 5795 || tile == 5796 || tile == 5797 || tile == 544 || tile == 546 || tile == 4520 || tile == 382 || tile == 3116 || tile == 1792 || tile == 5666 || tile == 2994 || tile == 4368) return;
                if (tile == 1902 || tile == 1508 || tile == 428)
                {
                    return;
                }
                if (tile == 1770 || tile == 4720 || tile == 4882 || tile == 6392 || tile == 3212 || tile == 1832 || tile == 4742 || tile == 3496 || tile == 3270 || tile == 4722 || tile == 6864) return;
                if (tile >= 7068) return;
                if (tile == 18)
                {
                    if (world.Items[x + (y * world.Width)].Background == 6864 && world.Items[x + (y * world.Width)].Foreground == 0)
                    {
                        return;
                    }
                    if (world.Items[x + (y * world.Width)].Background == 0 && world.Items[x + (y * world.Width)].Foreground == 0)
                    {
                        return;
                    }
                    //data.netID = -1;

                    data.PacketType = 0x8;
                    data.PlantingTree = 4;
                    long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    //if (world->Items[x + (y*world->Width)].foreground == 0) return;
                    int a = world.Items[x + y * world.Width].Foreground;
                    if (time - world.Items[x + (y * world.Width)].BreakTime >= 4000)
                    {
                        world.Items[x + (y * world.Width)].BreakTime = time;
                        world.Items[x + (y * world.Width)].BreakLevel = 6;
                        if (world.Items[x + (y * world.Width)].Foreground == 758)
                            await peer.SendRoulette();
                    }
                    else if (y < world.Height && world.Items[x + (y * world.Width)].BreakLevel + 6 >= ItemsData[a].BreakHits)
                    {
                        data.PacketType = 0x3;
                        data.NetID = -1;
                        data.PlantingTree = 18;
                        data.PunchX = x;
                        data.PunchY = y;
                        world.Items[x + (y * world.Width)].BreakLevel = 0;
                        int brokentile = world.Items[x + (y * world.Width)].Foreground;

                        if (world.Items[x + (y * world.Width)].Foreground != 0)
                        {
                            if (world.Items[x + (y * world.Width)].Foreground == 242)
                            {
                                world.OwnerName = "";
                                world.IsPublic = false;
                                world.AccessedPlayer = "";
                                foreach (var p in Peers)
                                {
                                    if (p.State != ENetPeerState.Connected) continue;
                                    await p.OnConsoleMessage("`oWorld `0" + world.Name + " `ohas had it's World Lock's removed!");
                                }
                            }
                            await SendRandomItems(peer, x * 32, y * 32, world.Items[x + y * world.Width].Foreground,world);
                            //await peer.SendDrop(-1, x * 32, y * 32, world.Items[x + y * world.Width].Foreground, 1, 0);
                            world.Items[x + y * world.Width].Foreground = 0;
                        }
                        else
                        {
                            await SendRandomItems(peer, x * 32, y * 32, world.Items[x + y * world.Width].Background,world);
                            //await peer.SendDrop(-1, x * 32, y * 32, world.Items[x + y * world.Width].Background, 1, 0);
                            world.Items[x + (y * world.Width)].Background = 0;
                        }
                    }
                    else
                    {
                        world.Items[x + (y * world.Width)].BreakTime = time;
                        world.Items[x + (y * world.Width)].BreakLevel += 6; // TODO
                        if (world.Items[x + (y * world.Width)].Foreground == 758)
                            await peer.SendRoulette();
                    }
                }
                else
                {
                    for (int i = 0; i < (peer.Data as Player).PlayerInventory.Items.Length; i++)
                    {
                        if ((peer.Data as Player).PlayerInventory.Items[i].ItemID == tile)
                        {
                            if ((uint)(peer.Data as Player).PlayerInventory.Items[i].ItemCount > 1)
                            {
                                (peer.Data as Player).PlayerInventory.Items[i].ItemCount--;
                            }
                            else
                            {
                                Array.Clear((peer.Data as Player).PlayerInventory.Items, i, 1);
                            }
                        }
                    }
                    if (ItemsData[tile].IsBackground)
                    {
                        world.Items[x + (y * world.Width)].Background = (short)tile;
                    }
                    if (world.Items[x + (y * world.Width)].Foreground != 0)
                    {
                        Console.WriteLine("return 11");
                        return;
                    }
                    else
                    {
                        int xx = data.PunchX;
                        int yy = data.PunchY;
                        foreach (ENetPeer currentPeer in Peers)
                        {
                            if (currentPeer.State != ENetPeerState.Connected)
                                continue;
                            if (peer.InWorld(currentPeer))
                            {
                                //dont allow to put consumbales
                                if (tile == 1368) return;
                                if (tile == 276) return;
                            }
                        }
                        world.Items[x + (y * world.Width)].Foreground = (short)tile;
                        if (tile == 242)
                        {
                            world.OwnerName = (peer.Data as Player).RawName;
                            world.OwnerNetID = (peer.Data as Player).NetID;
                            world.IsPublic = false;
                            await peer.OnPlayPositioned("audio/use_lock.wav");
                            await peer.SendSound("use_lock.wav");
                            foreach (ENetPeer currentPeer in Peers)
                            {
                                if (currentPeer.State != ENetPeerState.Connected)
                                    continue;
                                if (peer.InWorld(currentPeer))
                                {
                                    await currentPeer.OnConsoleMessage("`3[`w" + world.Name + " `ohas been World Locked by `2" + (peer.Data as Player).DisplayName + "`3]");
                                }
                            }
                        }

                    }

                    world.Items[x + (y * world.Width)].BreakLevel = 0;
                }
                Random r = new Random();
                byte xd = (byte)r.Next(1, 5);
                foreach (ENetPeer currentPeer in Peers)
                {
                    byte[] raw = PlayerMovingPack(data);
                    if (currentPeer.State != ENetPeerState.Connected)
                        continue;
                    if (peer.InWorld(currentPeer))
                    {
                        await SendPacketRaw(4, raw, 56, 0, currentPeer);
                    }
                    //cout << "Tile update at: " << data2->punchX << "x" << data2->punchY << endl;
                }
                // To save all the worlds DB (memory)
                await SaveOrAddWorldAsync(world);
            }
            catch (Exception e)
            {
                Console.WriteLine("error in async Task sendtileupdate");
                await peer.OnConsoleMessage(e.Message + " " + e.TargetSite + ",SOURCE : " + e.Source);
            }
        }
        public static async Task SendRandomItems(ENetPeer peer, int x, int y, int items,World world)
        {
            try
            {
                Random r = new Random();
                int rarity = ItemsData[items].Rarity / 3;
                int a = x / 32, b = y / 32;
                if (rarity <= 5) rarity = 3;
                int gems = r.Next(0, rarity);
                int rand = r.Next(0, 20);
                if (rand > 10)
                {
                    await SendGems(peer, x, y, 0, gems, world);
                }
                else if (rand > 4 && rand <= 10)
                {
                    DroppedItem drop = new DroppedItem
                    {
                        ItemID = items + 1,
                        ItemCount = 1,
                        ItemName = ""
                    };
                    world.Items[a + b*world.Width].DroppedItems.Add(drop);
                    await peer.SendDrop(-1, x, y, items + 1, 1, 0);
                }
                else
                {
                    DroppedItem drop = new DroppedItem
                    {
                        ItemID = items,
                        ItemCount = 1,
                        ItemName = ItemsData[items + 1].Name
                    };
                    world.Items[a + b * world.Width].DroppedItems.Add(drop);
                    await peer.SendDrop(-1, x, y, items, 1, 0);
                }
            }
            catch (Exception e) { Console.WriteLine("RandomItems error : " + e.Message); }
        }
        public static async Task SendGems(ENetPeer peer,int x,int y, byte se,int gemsCount,World world)
        {
            try
            {
                int a = x / 32, b = y / 32;
                while (gemsCount != 0)
                {
                    if (gemsCount >= 100)
                    {
                        DroppedItem drop = new DroppedItem
                        {
                            ItemID = 112,
                            ItemCount = 100,
                            ItemName = ItemsData[112].Name
                        };
                        world.Items[a + b * world.Width].DroppedItems.Add(drop);
                        await peer.SendDrop(-1, x, y, 112, 100, se);
                        gemsCount -= 100;
                    }
                    else if (gemsCount >= 50)
                    {
                        DroppedItem drop = new DroppedItem
                        {
                            ItemID = 112,
                            ItemCount = 50,
                            ItemName = ItemsData[112].Name
                        };
                        world.Items[a + b * world.Width].DroppedItems.Add(drop);
                        await peer.SendDrop(-1, x, y, 112, 50, se);
                        gemsCount -= 50;
                    }
                    else if (gemsCount >= 10)
                    {
                        DroppedItem drop = new DroppedItem
                        {
                            ItemID = 112,
                            ItemCount = 10,
                            ItemName = ItemsData[112].Name
                        };
                        world.Items[a + b * world.Width].DroppedItems.Add(drop);
                        await peer.SendDrop(-1, x, y, 112, 10, se);
                        gemsCount -= 10;
                    }
                    else if (gemsCount >= 5)
                    {
                        DroppedItem drop = new DroppedItem
                        {
                            ItemID = 112,
                            ItemCount = 5,
                            ItemName = ItemsData[112].Name
                        };
                        world.Items[a + b * world.Width].DroppedItems.Add(drop);
                        await peer.SendDrop(-1, x, y, 112, 5, se);
                        gemsCount -= 5;
                    }
                    else if (gemsCount >= 1)
                    {
                        DroppedItem drop = new DroppedItem
                        {
                            ItemID = 112,
                            ItemCount = 1,
                            ItemName = ItemsData[112].Name
                        };
                        world.Items[a + b * world.Width].DroppedItems.Add(drop);
                        await peer.SendDrop(-1, x, y, 112, 1, se);
                        gemsCount -= 1;
                    }
                }
            }
            catch (Exception e) { Console.WriteLine("Sendgems error : " +e.Message); }
        }
        public static async Task SendGrowScanDialog(ENetPeer peer)
        {
            World world = peer.GetWorld();
            List<int> items = new List<int>();
            List<int> itemscount = new List<int>();
            for (int i = 0; i < world.Width * world.Height; i++)
            {
                int fg = world.Items[i].Foreground;
                int bg = world.Items[i].Background;
                if (fg == 0 && bg == 0) continue;
                else if (fg == 8 || fg == 6) continue;
                if (items.Contains(fg))
                {
                    itemscount[items.IndexOf(fg)]++;
                }
                else
                {
                    if (fg != 0)
                    {
                        items.Add(fg);
                        itemscount.Add(1);
                    }
                }
                if (items.Contains(bg))
                {
                    itemscount[items.IndexOf(bg)]++;
                }
                else
                {
                    if (bg != 0)
                    {
                        items.Add(bg);
                        itemscount.Add(1);
                    }
                }
            }
            string dd = new Dialog().SetDefaultColor()
                .AddLabelWithIcon("Found items : ", 6016, false)
                .AddSmallSpacer()
                .Result;
            for (int i = 0; i < items.Count; i++)
            {
                dd += new Dialog().AddLabelWithIcon(ItemsData[items[i]].Name + " : " + itemscount[i], items[i], true).Result;
            }
            dd += new Dialog().AddSmallSpacer().EndDialog("a", "Back", "");
            await peer.OnDialogRequest(dd);
        }
    }
}
