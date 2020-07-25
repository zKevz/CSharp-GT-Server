using ENet.Managed;
using System;
using System.Linq;
using System.Threading.Tasks;
using static GTServer.Program;

namespace GTServer.Resources
{
    public class Commands
    {
        public static async Task SendCommands(ENetPeer peer,string cch)
        {
            // When adding a command with more than 1 param , example : "/hello world",
            // Make sure you use 'Contains' instead of 'Substring'!
            string str = cch.Split('|').Last();
            if (str == "/test")
            {
                await peer.OnConsoleMessage("Worked!");
            }
            else if (str.Contains("/item "))
            {
                try
                {
                    string[] arr = str.Split(' ');
                    if (arr.Length != 3) return;
                    if (Convert.ToInt32(arr[2]) > 200)
                    {
                        await peer.OnTalkBubble("Maximum is 200 items only!");
                    }
                    var a = (peer.Data as Player).PlayerInventory.Items.Where(x => x.ItemID == Convert.ToInt32(arr[1])).ToList();
                    if(a.Count == 0)
                    {
                        InventoryItems items = new InventoryItems
                        {
                            ItemCount = (byte) Convert.ToInt32(arr[2]),
                            ItemID = Convert.ToInt32(arr[1])
                        };
                        (peer.Data as Player).PlayerInventory.Items = (peer.Data as Player)
                            .PlayerInventory.Items.Append(items).ToArray();
                        await peer.OnConsoleMessage(ItemsData[items.ItemID].Name + " with ID " + items.ItemID + " " +
                            "has been added to your inventory.");
                    }
                    else
                    {
                        InventoryItems items = new InventoryItems
                        {
                            ItemCount = (byte) (Convert.ToInt32(arr[2]) + a[0].ItemCount),
                            ItemID = (byte) Convert.ToInt32(arr[1])
                        };
                        if(items.ItemCount > 200)
                        {
                            await peer.OnTalkBubble("You can only add " + (200 - (items.ItemCount - Convert.ToInt32(arr[2]))) + " items more!");
                            return;
                        }
                        int index = Array.IndexOf((peer.Data as Player).PlayerInventory.Items, a[0]);
                        (peer.Data as Player).PlayerInventory.Items[index] = items;
                        await peer.OnConsoleMessage(ItemsData[items.ItemID].Name + " with ID " + items.ItemID + " " +
                            "has been added to your inventory.");
                    }
                    await peer.SendInventory((peer.Data as Player).PlayerInventory);
                }
                catch { return; }
            }
            else if (str.Contains("/find "))
            {
                try
                {
                    string f = str.Substring(6);
                    var arr = ItemsData.Where
                        (x => x.Name.ToLower()
                        .Contains(f.ToLower()) && x.ItemID % 2 == 0).ToList();
                    if(arr.Count > 100)
                    {
                        await peer.OnConsoleMessage("`4More than 100 items found. " +
                            "Please specify more!");
                        await peer.OnTalkBubble("More than 100 items found. " +
                            "Please specify more!");
                        return;
                    }
                    string s = "set_default_color|\nadd_label|small|Found " + arr.Count + " items|left|420|\nadd_spacer|small|\n";
                    arr.ForEach(x => s += "add_button_with_icon|findbutton" + x.ItemID + "|`9" + x.Name + "|left|" + x.ItemID + "||\n");
                    s += "\nadd_spacer|small|\nend_dialog|finddialog|Cancel||";
                    await peer.OnDialogRequest(s);
                }
                catch { return; }
            }
            else if (str == "/testing")
            {
                string s = "set_default_color|\nadd_label|small|testing|left|420|\nadd_text_input|textinput|`oWow : ||10|\nadd_spacer|small|\nadd_text_input|textinput2|`oWow||10|\nend_dialog|f|cancel|ok|";
                await peer.OnDialogRequest(s);
            }
            else
            {
                if (str[0] == '/')
                {
                    string s = str.ToLower();
                    if (s == "/fp" || s == "/dance" || s == "/dance2" ||
                       s == "/lol" || s == "/cheer")
                    {
                        await peer.OnAction(str);
                    }
                    else
                    {
                        await peer.OnConsoleMessage("Invalid Command!");
                    }
                }
                else {
                    if (str.All(char.IsWhiteSpace)) return;
                    await peer.SendChat(str);
                }
            }
        }
    }
}
