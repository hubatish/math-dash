using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MathDash
{
    public class MainReader : MonoBehaviour
    {
        public void ReadJson()
        {
            var json = "{ \"id\": \"123\", \"data\": [ { \"key1\": \"val1\" }, { \"key2\" : \"val2\" } ] }";

            //SomeData objects = JsonConvert.DeserializeObject<SomeData>(json);
            var rawObj = JObject.Parse(json);

            foreach (var item in rawObj["data"])
            {
                foreach (var prop in item)
                {
                    JProperty property = prop as JProperty;

                    //check if we have an object with that key name
                    if (destObjs.ContainsKey(property.Name))
                    {
                        //then set data
                        destObjs[property.Name].SetData(property.Value.ToString());
                    }

/*                    if (property != null)
                    {
                        var d = new Dictionary<string, string>();
                        d.Add(property.Name, property.Value.ToString());
                        allProperties.Add(d);
                    }*/

                }
            }
        }

        public Dictionary<string, SettableData> destObjs = new Dictionary<string,SettableData>();
//        public List<Dictionary<string, string>> allProperties = new List<Dictionary<string, string>>();

    }

   /* public class SomeData
    {
        public string Id { get; set; }

        public IEnumerable<IDictionary<string, string>> Data { get; set; }
    }*/

}
