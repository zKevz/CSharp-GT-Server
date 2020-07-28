using ENet.Managed;
using System;
using System.Linq;
using System.Threading.Tasks;
using static GTServer.Program;
using static GTServer.Resources.TileUpdate;

namespace GTServer.Resources
{
    public static class PacketType
    {
        public static async Task SendPacketType(this ENetPeer peer, int id, PlayerMoving pMov,
            PlayerMoving data2, World world)
        {
            switch (id)
            {
                case 25:
                    await SendPacket25(peer);
                    break;
                case 7:
                    await SendPacket7(peer, pMov, world);
                    break;
                case 10:
                    await SendPacket10(peer, pMov);
                    break;
                case 11:
                    await SendPacket11(peer,pMov,data2);
                    break;
                case 18:
                    await SendPacket18(peer, pMov);
                    break;
                case 3:
                    await SendPacket3(peer, data2);
                    break;
                default:
                    break;
            }

        }

        private static async Task SendPacket11(ENetPeer peer,PlayerMoving pMov,PlayerMoving data2)
        {
            await peer.TakeDroppedItems((peer.Data as Player).NetID,(int) pMov.X,(int) pMov.Y, data2.PlantingTree);
        }

        private static async Task SendPacket3(ENetPeer peer, PlayerMoving data2)
        {
            int x = (peer.Data as Player).X;
            int y = (peer.Data as Player).Y;
            int PunchX = Convert.ToInt32((float)x / 31.5) - 1;
            int PunchY = Convert.ToInt32((float)y / 31.5) - 1;
            if (data2.PlantingTree == 18)
            {
                await SendTileUpdate(data2.PunchX, data2.PunchY, data2.PlantingTree,
                    (peer.Data as Player).NetID, peer);
                if ((peer.Data as Player).HandClothes == 5480)
                {
                    if (PunchY < data2.PunchY && PunchX > data2.PunchX)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            await SendTileUpdate(data2.PunchX - i, data2.PunchY + i, data2.PlantingTree,
                                (peer.Data as Player).NetID, peer);
                        }
                        return;
                    }
                    else if (PunchY < data2.PunchY && PunchX < data2.PunchX)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            await SendTileUpdate(data2.PunchX + i, data2.PunchY + i, data2.PlantingTree,
                                (peer.Data as Player).NetID, peer);
                        }
                        return;
                    }
                    else if (PunchY > data2.PunchY && PunchX > data2.PunchX)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            await SendTileUpdate(data2.PunchX - i, data2.PunchY - i, data2.PlantingTree,
                                (peer.Data as Player).NetID, peer);
                        }
                        return;
                    }
                    else if (PunchY > data2.PunchY && PunchX < data2.PunchX)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            await SendTileUpdate(data2.PunchX + i, data2.PunchY - i, data2.PlantingTree,
                                (peer.Data as Player).NetID, peer);
                        }
                        return;
                    }
                    else if (PunchY > data2.PunchY)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            await SendTileUpdate(data2.PunchX, data2.PunchY - i, data2.PlantingTree,
                            (peer.Data as Player).NetID, peer);
                        }
                        return;
                    }
                    else if (PunchY < data2.PunchY)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            await SendTileUpdate(data2.PunchX, data2.PunchY + i, data2.PlantingTree,
                            (peer.Data as Player).NetID, peer);
                        }
                        return;
                    }
                    else if ((peer.Data as Player).IsRotatingLeft)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            await SendTileUpdate(data2.PunchX - i, data2.PunchY, data2.PlantingTree,
                            (peer.Data as Player).NetID, peer);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            await SendTileUpdate(data2.PunchX + i, data2.PunchY, data2.PlantingTree,
                            (peer.Data as Player).NetID, peer);
                        }
                    }

                }
                else if ((peer.Data as Player).HandClothes == 2952)
                {
                    for (int i = 0; i < 3; i++) await SendTileUpdate(data2.PunchX, data2.PunchY, data2.PlantingTree,
                                    (peer.Data as Player).NetID, peer);
                }
            }
            else
            {
                await SendTileUpdate(data2.PunchX, data2.PunchY, data2.PlantingTree,
                        (peer.Data as Player).NetID, peer);
            }
            await peer.SendState();
        }

        private static async Task SendPacket18(ENetPeer peer, PlayerMoving pMov)
                => await peer.SendPData(pMov);


        private static async Task SendPacket10(ENetPeer peer, PlayerMoving pMov)
        {
            DataItems def = new DataItems();
            var t = "";
            try
            {
                //itemDefs.ToList().ForEach(x => Console.WriteLine(x));
                var f = CoreData[pMov.PlantingTree].Split('|');
                def.ItemID = Convert.ToInt32(f[0]);
                def.Name = f[1];
                t = f[9];
            }
            catch (Exception e)
            {
                Console.WriteLine("Sendpacket10 error : " + e.Message);
            }

            var p = peer.Data as Player;
            int id = pMov.PlantingTree;

            if (t.Contains("Hair"))
            {
                if ((peer.Data as Player).HairClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).HairClothes = 0;
                }
                else
                    (peer.Data as Player).HairClothes = pMov.PlantingTree;
            }
            else if (t.Contains("Shirt"))
            {
                if ((peer.Data as Player).ShirtClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).ShirtClothes = 0;
                }
                else
                    (peer.Data as Player).ShirtClothes = pMov.PlantingTree;
            }
            else if (t.Contains("Pants"))
            {
                if ((peer.Data as Player).PantsClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).PantsClothes = 0;
                }
                else
                    (peer.Data as Player).PantsClothes = pMov.PlantingTree;
            }
            else if (t.Contains("Feet"))
            {
                if ((peer.Data as Player).PantsClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).PantsClothes = 0;
                }
                else
                    (peer.Data as Player).PantsClothes = pMov.PlantingTree;
            }
            else if (t.Contains("Face"))
            {
                if ((peer.Data as Player).PantsClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).PantsClothes = 0;
                }
                else
                    (peer.Data as Player).PantsClothes = pMov.PlantingTree;
            }
            else if (t.Contains("Hand"))
            {
                if ((peer.Data as Player).HandClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).HandClothes = 0;
                }
                else
                    (peer.Data as Player).HandClothes = pMov.PlantingTree;
                int item = pMov.PlantingTree;
            }
            else if (t.Contains("Back"))
            {
                if ((peer.Data as Player).BackClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).BackClothes = 0;
                    (peer.Data as Player).InWings = false;
                }
                else
                {
                    (peer.Data as Player).BackClothes = pMov.PlantingTree;
                    int item = pMov.PlantingTree;
                    if (def.Name.ToLower().Contains("wing") || def.Name.ToLower().Contains("cape")
                        || def.Name.ToLower().Contains("aura") || def.Name.ToLower().Contains("dragon warrior's shield") ||
                        def.Name.ToLower().Contains("spirit"))
                    {
                        (peer.Data as Player).InWings = true;
                    }
                    await peer.SendState();
                    // ^^^^ wings
                }
            }
            else if (t.Contains("Mask"))
            {
                if ((peer.Data as Player).MaskClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).MaskClothes = 0;
                }
                else (peer.Data as Player).MaskClothes = pMov.PlantingTree;
            }
            else if (t.Contains("Necklace"))
            {
                if ((peer.Data as Player).NecklaceClothes == pMov.PlantingTree)
                {
                    (peer.Data as Player).NecklaceClothes = 0;
                }
                else (peer.Data as Player).NecklaceClothes = pMov.PlantingTree;
            }
            else
            {
                if (def.Name.ToLower().Contains("ances"))
                {
                    if ((peer.Data as Player).AncesClothes == pMov.PlantingTree)
                    {
                        (peer.Data as Player).AncesClothes = 0;
                    }
                    (peer.Data as Player).AncesClothes = pMov.PlantingTree;
                }
                else
                {
                    Console.WriteLine("Invalid item activated: " + pMov.PlantingTree + " by "
                                      + (peer.Data as Player).DisplayName);
                }
            }
            Peers.Where(x => x.InWorld(peer)).ToList().ForEach(async x => await x.SendSound("change_clothes.wav"));
            await peer.UpdateClothes();
        }

        private static async Task SendPacket7(ENetPeer peer, PlayerMoving pMov, World world)
        {
            if (pMov.PunchX < 0 || pMov.PunchY < 0 || pMov.PunchX > 100 || pMov.PunchY > 100) return;
            if ((peer.Data as Player).World == " EXIT") return;
            int x = pMov.PunchX;
            int y = pMov.PunchY;
            int tile = world.Items[x + (y * world.Width)].Foreground;
            if (tile == 6)
            {
                await peer.SendLeave();
                (peer.Data as Player).World = "EXIT";
                await peer.SendWorldOffers();
            }
            else if (tile == 410 || tile == 1832 || tile == 1770)
            {
                (peer.Data as Player).CpX = x * 32;
                (peer.Data as Player).CpY = y * 32;
            }
        }

        private static async Task SendPacket25(ENetPeer peer)
        {
            if ((peer.Data as Player).HaveGrowID == true)
            {
                await peer.OnAddNotification("", "`wWarning from `4System`0: You have been auto banned by system for using : Cheat Engine!", "audio/open_hub.wav");

                peer.DisconnectLater(0);
                foreach (ENetPeer currentPeer in Peers)
                {
                    if (currentPeer.State != ENetPeerState.Connected)
                        continue;

                    await currentPeer.OnConsoleMessage("`4** `$" + (peer.Data as Player).RawName + " `4AUTO-BANNED BY SYSTEM **`o(`$/rules `oto view rules)");
                    await currentPeer.OnPlayPositioned("audio/beep.wav");
                }
                await Task.CompletedTask;
            }
            else
            {
                peer.DisconnectLater(0);
            }
        }
    }
}
