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
        List<string> rep;
        Random random = new Random();
        public RepDictionary(string[] aReceived, string[] aRep)
        {
            this.received = new List<string>(aReceived);
            this.rep = new List<string>(aRep);
        }
        public string GetRep(string aReceived)
        {
            string returnString = string.Empty;
            for (var i = 0; i < received.Count; i++)
            {
                if (aReceived.Contains(received[i]))
                {
                    returnString = rep[random.Next(rep.Count)];
                    break;
                }
            }
            return returnString;
        }
    }
}
