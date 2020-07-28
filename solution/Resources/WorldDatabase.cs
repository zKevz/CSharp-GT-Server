using ENet.Managed;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static GTServer.Program;

namespace GTServer.Resources
{
    public static class WorldDatabase
    {
        public static List<World> Worlds { get; set; } = new List<World>();
        public static World GenerateClearWorld(string name, int width,int height,
            string ownerName, int worldLockPosition)
        {
            try
            {
                World world = new World
                {
                    Name = name,
                    Width = width,
                    Height = height,
                    Items = new WorldItems[width * height],
                    OwnerName = ownerName,
                };
                Random rand = new Random();
                for (int i = 0; i < world.Width * world.Height; i++)
                {
                    world.Items[i].IsWater = false;
                    world.Items[i].BreakTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    world.Items[i].BreakLevel = 6;
                    world.Items[i].IsBackground = ItemsData[i].IsBackground;
                    world.Items[i].DroppedItems = new List<DroppedItem>();
                    if (i >= 5400)
                    {
                        world.Items[i].Foreground = 8;
                    }
                    else if (i == 3650)
                        world.Items[i].Foreground = 6;
                    else if (i == worldLockPosition)
                        world.Items[i].Foreground = 242;
                    else if (i >= 3600 && i < 3700)
                        world.Items[i].Foreground = 0;
                    if (i == 3750)
                        world.Items[i].Foreground = 8;
                }
                //await Task.CompletedTask;
                return world;
            }
            catch (StackOverflowException e)
            {
                Console.WriteLine(e.Message + " " + e.Data + " " + e.Source);
                return null;
            }
        }
        public static World[] GetRandomWorlds()
        {
            World[] ret = new World[] { };
            for (int i = 0; i < ((Worlds.Count < 10) ? Worlds.Count : 10); i++)
            { // load first four worlds, it is excepted that they are special
                ret = ret.Append(Worlds[i]).ToArray();
            }
            // and lets get up to 6 random
            if (Worlds.Count > 4)
            {
                Random rand = new Random();
                for (int j = 0; j < 6; j++)
                {
                    bool isPossible = true;
                    World world = Worlds[rand.Next(0, Worlds.Count - 4)];
                    for (int i = 0; i < ret.Length; i++)
                    {
                        if (world.Name == ret[i].Name || world.Name == "EXIT")
                        {
                            isPossible = false;
                        }
                    }
                    if (isPossible)
                        ret = ret.Append(world).ToArray();
                }
            }
            return ret;
        }
        public static WorldProperties GetWorld(string name)
        {
            try
            {
                foreach (var w in Worlds)
                {
                    if (w.Name == name)
                    {
                        WorldProperties p = new WorldProperties { Id = name.Length - 1, WorldInfo = w };
                        return p;
                    }
                }

                var world = GenerateWorld(name, 100, 60);
                var worldP = new WorldProperties
                {
                    Id = name.Length - 1,
                    WorldInfo = world
                };
                Worlds.Add(world);
                return worldP;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message + " " + e.Data + " " + e.InnerException + " " + e.Source + " " + e.TargetSite);
                return new WorldProperties();
            }
        }

        private static World GenerateWorld(string name, int width, int height)
        {
            try
            {
                World world = new World
                {
                    Name = name,
                    Width = width,
                    Height = height,
                    Items = new WorldItems[width * height]
                };
                Random rand = new Random();
                for (int i = 0; i < world.Width * world.Height; i++)
                {
                    world.Items[i].IsWater = false;
                    world.Items[i].BreakTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    world.Items[i].BreakLevel = 6;
                    world.Items[i].IsBackground = ItemsData[i].IsBackground;
                    world.Items[i].DroppedItems = new List<DroppedItem>();
                    if (i >= 3800 && i < 5400 && rand.Next(0, 50) == 0)
                        world.Items[i].Foreground = 10;
                    else if (i >= 3700 && i < 5400)
                    {
                        world.Items[i].Foreground = 2;
                    }
                    else if (i >= 5400)
                    {
                        world.Items[i].Foreground = 8;
                    }
                    if (i >= 3700)
                        world.Items[i].Background = 14;
                    if (i == 3650)
                        world.Items[i].Foreground = 6;
                    else if (i >= 3600 && i < 3700)
                        world.Items[i].Foreground = 0;
                    if (i == 3750)
                        world.Items[i].Foreground = 8;
                }
                //await Task.CompletedTask;
                return world;
            }
            catch(StackOverflowException e)
            {
                Console.WriteLine(e.Message + " " + e.Data + " " + e.Source);
                return null;
            }
        }
    }
}
