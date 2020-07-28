using Newtonsoft.Json.Linq;

namespace GTServer.Resources
{
    public class ItemsJsonData
    {
        public int ItemCount { get; set; }
        public JArray Items { get; set; }
    }
}
