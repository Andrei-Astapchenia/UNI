using Stoky_programm.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stoky_programm.Data
{
    public class JsonData
    {
        [JsonInclude]
        public List<MaterialObj> AllItems { get; set; }
        [JsonInclude]
        public List<Order> Orders { get; set; }
        [JsonInclude]
        public int NextId { get; set; }

        [JsonIgnore]
        public string FilePath { get; set; } = "data.json";

        public JsonData()
        {
            AllItems = new List<MaterialObj>();
            Orders = new List<Order>();
            NextId = 1;
        }

        public JsonData(List<MaterialObj> items, List<Order> orders, int nextId)
        {
            AllItems = items;
            Orders = orders;
            NextId = nextId;
        }
    }

}