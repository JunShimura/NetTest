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
        Random random = new Random();
        public enum GetMode
        {
            connect,     // 複数あったものを連結して返す
            single, // 一つだけ返す
            random,  // 複数あった中のひとつをランダムで返す
        };
        public BotRep(RepDictionary[] aRepDictionaries)
        {
            repDictionaries = new List<RepDictionary>(aRepDictionaries);
        }
        public string GetRep(
            string received,
            GetMode getMode = GetMode.connect,
            string connectString = "")
        {
            List<string> returnStrings = new List<string>();
            for (var i = 0; i < repDictionaries.Count; i++)
            {
                string temp = repDictionaries[i].GetRep(received);
                if (temp != String.Empty)
                {
                    returnStrings.Add(temp);
                    if (getMode == GetMode.single)
                    {
                        break;
                    }
                }
            }
            string s = string.Empty;
            if (returnStrings.Count != 0)
            {
                switch (getMode)
                {
                    case GetMode.single:
                        s = returnStrings[0];
                        break;
                    case GetMode.random:
                        s = returnStrings[random.Next(returnStrings.Count)];
                        break;
                    case GetMode.connect:
                        s = string.Join(connectString, returnStrings);
                        break;
                    default:
                        break;
                }
            }
            return s;
        }
    }
}
