using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordQuoteBot
{
    public class Data
    {
        internal static readonly string DataFile = "savedData.json";
        
        private Dictionary<ulong, ServerData> ServerData = new();

        internal void LoadData()
        {
            try
            {
                string fileText = File.ReadAllText(DataFile);
                ServerData = JsonConvert.DeserializeObject<Dictionary<ulong, ServerData>>(fileText) ?? new();
            }
            catch
            {
                ServerData = new();
                SaveData();
            }
        }
        internal void SaveData() => File.WriteAllText(DataFile, JsonConvert.SerializeObject(ServerData));

        internal ServerData GetDataForServer(ulong serverId)
        {
            ServerData? data;
            if (ServerData.TryGetValue(serverId, out data))
            {
                return data;
            }
            else
            {
                data = new ServerData();
                ServerData.Add(serverId, data);
                return data;
            }
        }
    }

    internal class ServerData
    {
        public List<Quote> QuoteList = new();

        
        
    }

    internal class Quote
    {
        public DateTime TimeAdded { get; set; } = DateTime.UtcNow;
        public ulong AddedBy { get; set; }
        public string QuoteText { get; set; } = "";
    }
}
