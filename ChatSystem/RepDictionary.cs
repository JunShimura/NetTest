using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem
{
    class RepDictionary
    {
        List<string> received;
        string rep;
        public RepDictionary(string[] aReceived, string aRep)
        {
            this.received = new List<string>(aReceived);
            this.rep = aRep;
        }
        public string GetRep(string aReceived)
        {
            string returnString = string.Empty;
            for (var i = 0; i < received.Count; i++)
            {
                if (aReceived.Contains(received[i]))
                {
                    returnString = rep;
                    break;
                }
            }
            return returnString;
        }
    }
}
