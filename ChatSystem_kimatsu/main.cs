using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatSystem;

namespace ChatSystem
{
    class main
    {
        static ChatSystem chatSystem;
        const Int32 portNo = 11000;
        const string EOF = "<EOF>";
        static readonly int maxLength = 200 + EOF.Length;
        static ChatSystem.ConnectMode connectMode;
        enum FunctionMode { chat, bot, janken, shiritori };
        static FunctionMode functionMode = FunctionMode.chat;

        static void Main(string[] args)
        {
            chatSystem = new ChatSystem(maxLength);
            Console.WriteLine($"this hostName is {chatSystem.hostName}.");
            functionMode = SelectFunction();
            connectMode = SelectMode();
            switch (functionMode)
            {
                case FunctionMode.chat:
                    InChat();
                    break;
                case FunctionMode.bot:
                    InChatBot();
                    break;
                case FunctionMode.janken:
                    InChatJanken();
                    break;
                case FunctionMode.shiritori:
                    InChatshiritori();
                    break;
                default:
                    Console.WriteLine("not suported");
                    break;
            }
        }
        static FunctionMode SelectFunction()
        {
            Console.WriteLine("Select Function\n0= chat\n1=bot\n2=janken\n3=shiritori ");
            int select = int.Parse(Console.ReadLine());
            FunctionMode[] function = { FunctionMode.chat, FunctionMode.bot, FunctionMode.janken, FunctionMode.shiritori };
            return function[select];
        }
        static ChatSystem.ConnectMode SelectMode()
        {
            ChatSystem.ConnectMode connectMode = ChatSystem.ConnectMode.host;
            Console.Write("Select Mode: 0=Host,1=Client\n");
            int select = int.Parse(Console.ReadLine());
            switch (select)
            {
                case 0: //Host
                    Console.WriteLine("Running Host mode");
                    InitializeHost();
                    connectMode = ChatSystem.ConnectMode.host;
                    break;
                case 1: //Client
                    Console.WriteLine("Running Client mode");
                    InitializeClient();
                    connectMode = ChatSystem.ConnectMode.client;
                    break;
                default:
                    Console.WriteLine("ERROR undefind");
                    break;
            }
            return connectMode;
        }
        static void InitializeHost()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(chatSystem.hostName);
            foreach (var addresslist in ipHostInfo.AddressList)
            {
                Console.WriteLine($"found own address:{addresslist.ToString()}");
            }
            Console.Write($"Select address to listen(0 - {ipHostInfo.AddressList.Length - 1}):");
            IPAddress ipAddress = ipHostInfo.AddressList[int.Parse(Console.ReadLine())];
            ChatSystem.EResult re = chatSystem.InitializeHost(ipAddress, portNo);
            if (re != ChatSystem.EResult.success)
            {
                Console.WriteLine($"failed to initialize,ERROR={re.ToString()}");
            }
        }
        static void InitializeClient()
        {
            Console.Write("Input IP address to connect:");
            var ipAddress = IPAddress.Parse(Console.ReadLine());
            ChatSystem.EResult re = chatSystem.InitializeClient(ipAddress, portNo);
            if (re == ChatSystem.EResult.success)
            {
                Console.WriteLine($"Connected host {ipAddress.ToString()}");
            }
            else
            {
                Console.WriteLine($"failed to connect to host,ERROR={chatSystem.resultMessage}");
            }
        }
        static void InChatBot()
        {
            RepDictionary[] repDictionaries = {
                new RepDictionary(new string[] { "おはよう", "おはようございます", "Good morning" },new string[]{"おはよう！","おはようごぜえます、旦那様" }),
                new RepDictionary(new string[] { "こんにちは", "hello" },new string[]{"こんにちは！" }),
                new RepDictionary(new string[] {"すき","好き","I love you" } ,new string[]{"私も好き！" }),
                new RepDictionary(new string[] { "おはよう","うんこ", "poop" },new string[]{"うんこもりもり森鴎外！" }),
                };
            BotRep botRep = new BotRep(repDictionaries);
            Random random = new Random();
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            string received = string.Empty;
            while (true)
            {
                if (turn)
                {   // 受信
                    received = string.Empty;
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received.Length!=0)
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    string inputSt = string.Empty;
                    Console.Write("送るメッセージ：");
                    if (connectMode == ChatSystem.ConnectMode.host)
                    {   // Host
                        inputSt = botRep.GetRep(received);
                        if (inputSt == string.Empty)
                        {
                            //　ランダムに言葉を返す
                            string[] randomRep = { "それは良い質問だ", "Me Too", "私それ気になる！", "そうだね！", "分かるわぁそれ！", "いいね！", "今北産業" };
                            inputSt = randomRep[random.Next(randomRep.Length)];
                        }
                        Console.Write(inputSt);
                    }
                    else
                    {   // Client
                        inputSt = Console.ReadLine();    // 入力文字で送信
                        if (inputSt.Length > maxLength)
                        {
                            inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                        }
                    }

                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }

        static void InChat()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            while (true)
            {
                if (turn)
                {   // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received.Length!=0)
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    Console.Write("送るメッセージ：");
                    string inputSt = Console.ReadLine();    // 入力文字で送信
                    if (inputSt.Length > maxLength)
                    {
                        inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }

        static string[] hand = { "ぐう", "ちょき", "ぱあ" };
        static void InChatJanken()
        {
            Random random = new Random();
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            while (true)
            {
                if (turn)
                {   // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received.Length!=0)
                        {   // 正常にメッセージを受信
                            if (connectMode == ChatSystem.ConnectMode.host)
                            {
                                int hostHand = random.Next(hand.Length);
                                int clientHand = int.Parse(received);
                                //(ホストークライアント + 3) % 3で勝敗を判定
                                //int result = (hostHand - clientHand + hand.Length) % hand.Length;
                                string[] resultMessagge = { "あいこ", "負け", "勝ち" };
                                int[,,] judgeTable
                                    = {
                                        {   // host
                                            {0,  2,1 },{1,0,2},{ 2,1,0 }
                                        },
                                        {   // client
                                            {0,1,2},{2,0,1 },{ 1,2,0}
                                        },
                                };
                                Console.WriteLine(
                                    $"自分は{hand[hostHand]}、相手は{hand[clientHand]}\n{resultMessagge[judgeTable[0, hostHand, clientHand]]}です");
                                int clientResult = (clientHand - hostHand + hand.Length) % hand.Length;
                                string inputSt = $"あいては{hand[hostHand]}、{resultMessagge[judgeTable[1, hostHand, clientHand]]}です";
                                if (inputSt.Length > maxLength)
                                {
                                    inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                                }
                                inputSt += EOF;
                                buffer.content = Encoding.UTF8.GetBytes(inputSt);
                                buffer.length = buffer.content.Length;
                                re = chatSystem.Send(buffer);
                                if (re != ChatSystem.EResult.success)
                                {
                                    Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine(received);
                            }
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    if (connectMode == ChatSystem.ConnectMode.client)
                    {
                        string inputSt = GetJankenHand().ToString();

                        if (inputSt.Length > maxLength)
                        {
                            inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                        }
                        inputSt += EOF;
                        buffer.content = Encoding.UTF8.GetBytes(inputSt);
                        buffer.length = buffer.content.Length;
                        ChatSystem.EResult re = chatSystem.Send(buffer);
                        if (re != ChatSystem.EResult.success)
                        {
                            Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                            break;
                        }
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }

        static int GetJankenHand()
        {
            Console.WriteLine("じゃんけんをしましょう！");
            for (var i = 0; i < hand.Length; i++)
            {
                Console.WriteLine($"{i}:{hand[i]}");
            }
            string inputSt = string.Empty;
            int inputNum;
            while (true)
            {
                inputSt = Console.ReadLine();    // 入力文字
                if (int.TryParse(inputSt, out inputNum) && inputNum >= 0 && inputNum < hand.Length)
                {
                    Console.WriteLine($"あなたの手は{hand[inputNum]}ですね");
                    break;
                }
                Console.WriteLine("規定の数値を入力してください");
            }
            return inputNum;
        }
        static void InChatshiritori()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            string lastReceived = string.Empty;
            string lastSent = string.Empty;
            const string ERROR_HEADER = "エラー：";
            while (true)
            {
                if (turn)
                {   // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content, 0, buffer.length).Replace(EOF, "");
                        if (received.Length != 0)
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"相手から：{received}");
                            if (!received.Contains(ERROR_HEADER))
                            {
                                var receivedLastCharacter = GetLastchar(received);
                                if (receivedLastCharacter == 'ん' || receivedLastCharacter == 'ン')
                                {
                                    Console.WriteLine("最後に「ん」が付いてるのであなたの勝ちです！");
                                    SendString("最後に「ん」が付いてるのであなたの負けです");
                                    break;
                                }
                                else if (GetLastchar(lastSent) == received[0] || lastReceived == string.Empty)
                                {
                                    Console.WriteLine("正常");
                                    lastReceived = received;
                                }
                                else
                                {
                                    Console.WriteLine("言葉がつながりません");
                                    var sendResult = SendString(ERROR_HEADER + "言葉がつながりません");
                                    if (sendResult != ChatSystem.EResult.success)
                                    {
                                        Console.WriteLine("相手に送信できませんでした");
                                        break;
                                    }
                                    turn = !turn;
                                }
                            }
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    Console.Write("送るメッセージ：");
                    string inputSt;  // 入力文字
                    while (true)
                    {
                        inputSt = Console.ReadLine();    // 入力文字
                        if (inputSt.Length >= maxLength)
                        {   //　文字列が長すぎ
                            Console.WriteLine("文字列が長すぎます");
                            continue;
                        }
                        else if (lastReceived != string.Empty && lastReceived[lastReceived.Length - 1] != inputSt[0])
                        {   //　文字がつながらない
                            Console.WriteLine("文字がつながりません");
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    lastSent = inputSt;
                    if (inputSt.Length > maxLength)
                    {
                        inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();

        }
        static ChatSystem.EResult SendString(string s)
        {
            if (s.Length > maxLength)
            {
                s = s.Substring(0, maxLength - EOF.Length);
            }
            s += EOF;
            var buffer = new ChatSystem.Buffer(maxLength);
            buffer.content = Encoding.UTF8.GetBytes(s);
            buffer.length = buffer.content.Length;
            ChatSystem.EResult re = chatSystem.Send(buffer);
            if (re != ChatSystem.EResult.success)
            {
                Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
            }
            return re;
        }
        static char GetLastchar(string s)
        {
            char result = '\0';
            for (var i = s.Length - 1; i > 0; i--)
            {
                result = s[i];
                if (result != '\0')
                {
                    break;
                }
            }
            return result;
        }

    }
}
