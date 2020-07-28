using ENet.Managed;
using System;
using System.Linq;
using System.Threading.Tasks;
using static GTServer.Program;

namespace GTServer.Resources
{
    public class DialogHandler
    {
        public static async Task SendDialogAsync(ENetPeer peer, World world, string act)
        {
            try
            {
                bool registerDialog = false;
                string growid = "", password = "", email = "", discord = "";
                foreach (var a in act.Split("\n".ToCharArray()))
                {
                    var info = a.Split('|');
                    var button = info[0] == "buttonClicked" ? info[1] : "";
                    if (info[0] == "dialog_name" && info[1] == "creategrowid") registerDialog = true;
                    if (registerDialog)
                    {
                        if (info[0] == "growid") growid = info[1];
                        else if (info[0] == "password") password = info[1];
                        else if (info[0] == "email") email = info[1];
                        else if (info[0] == "discord") discord = info[1];
                    }
                    if (button.Contains("findbutton"))
                    {
                        int id = Convert.ToInt32(button.Substring(10));
                        var item = ItemsData[id];
                        InventoryItems inv = new InventoryItems
                        {
                            ItemID = id,
                            ItemCount = 1
                        };
                        (peer.Data as Player).PlayerInventory.Items = (peer.Data as Player).PlayerInventory.Items.Append(inv).ToArray();
                        await peer.SendInventory((peer.Data as Player).PlayerInventory);
                    }
                }
                if (registerDialog)
                {
                    await peer.CreateGrowID(growid, password, email, discord);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Source + " " + e.TargetSite); }
        }
    }
}
