using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GelDesk
{
    public class ObjectSet : ComponentObject
    {
        public JObject Root { get; private set; }
        protected override void OnInitialize(RpcContext rpc, ComponentInfo info, ComponentFrameReader reader)
        {
            Root = ReadRootObject(reader);
            Debug.Print("init: GelDesk.ObjectSet Root: {0}", 
                Root?.ToString(Newtonsoft.Json.Formatting.None) ?? "null");

            
        }
        static JObject ReadRootObject(ComponentFrameReader reader)
        {
            JArray frame = null;
            var count = 0;
            while (reader.MoveNext())
            {
                if (count == 0)
                    frame = (JArray)reader.Current;
                count++;
            }
            // TODO: if (count > 1) throw error.
            if (frame == null || frame.Count < 2 || (string)frame[0] != "root")
                return null;
            return frame[1] as JObject;
        }

        public object ObserveObject(string path)
        {
            return null;
        }

        public JArray ObserveArray(string path)
        {
            return null;
        }
    }
}
