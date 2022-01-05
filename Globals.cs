using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calladito_DiscordBot
{
    public static class GLOBALS
    {
        public static string[] m_calladitos = new string[4] {
            @"resources\Sounds\calladito.wav",
            @"resources\Sounds\agarrame.wav",
            @"resources\Sounds\ssshCalladito.wav",
            @"resources\Sounds\ssshTontito.wav"
        };
        static string m_insultos = System.IO.File.ReadAllText(@"resources\Texts\insultos.json");
        static JObject m_insultos_0bject = JObject.Parse(m_insultos);
        public static JArray m_insultos_list = (JArray)m_insultos_0bject["insultos"];
        public static string m_bot_token = "bot token";
    }
}
