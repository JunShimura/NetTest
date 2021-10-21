using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem
{
    class BotRep
    {
        List<RepDictionary> repDictionaries;
        public BotRep(RepDictionary[] aRepDictionaries)
        {
            repDictionaries = new List<RepDictionary>(aRepDictionaries);
        }
        public string GetRep(string received)
        {
            string retrnStr = string.Empty;
            for (var i = 0; i < repDictionaries.Count; i++)
            {
                retrnStr += repDictionaries[i].GetRep(received);
            }
            return retrnStr;
        }
    }
}
