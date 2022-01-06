using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calladito_DiscordBot
{
    public class GLOBALS
    {
        public static string[] m_calladitos = new string[4] {
            @"resources\Sounds\calladito.wav",
            @"resources\Sounds\agarrame.wav",
            @"resources\Sounds\ssshCalladito.wav",
            @"resources\Sounds\ssshTontito.wav"
        };
        
        public static string m_bot_token = "token bot";

        public JObject read_insultos_file()
        {
            string m_insultos = System.IO.File.ReadAllText(@"resources\Texts\insultos.json");
            JObject m_insultos_0bject = new JObject();
            m_insultos_0bject = JObject.Parse(m_insultos);
            return m_insultos_0bject;
        }
        public void add_insulto_to_file(JArray _new_insultos)
        {
            JObject insultos_obj = new JObject();
            insultos_obj.Add("insultos",_new_insultos);
            using (StreamWriter file = File.CreateText(@"resources\Texts\insultos.json"))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                insultos_obj.WriteTo(writer);
            }
        }
    }
}
