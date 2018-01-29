using NiceHashMiner.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace NiceHashMiner
{
    public delegate string TelegramGetInfo(TelegramBotService.TelegramOptions options);
    public static class TelegramBotService
    {
        public enum TelegramOptions
        {
            FullStatus = 1,
            Profit = 2 ,
            IsRunning = 3,
            ForceStop = 4,
            ForceStart = 5,
            ForceRestart = 6,
            Info = 7
        }

        private static TelegramBotClient _BotClient;
        private static TelegramGetInfo _GetStatus;
        private static System.Timers.Timer _AutoInfoTimer;
        private static int _AutoInfoTimerPeriod;

        public static bool BotRunning = false;

        public static void StartBot(TelegramGetInfo getStatus)
        {
            if (_BotClient == null)
            {
                _BotClient = new Telegram.Bot.TelegramBotClient(ConfigManager.GeneralConfig.TelegramAPIToken);
                _GetStatus = getStatus;
                _BotClient.StartReceiving(new Telegram.Bot.Types.Enums.UpdateType[] { Telegram.Bot.Types.Enums.UpdateType.MessageUpdate });

                _BotClient.OnMessage += BotClient_OnMessage;
                BotRunning = true;
            }
        }

        private static void SendAutoInfo()
        {
            if (!string.IsNullOrEmpty(ConfigManager.GeneralConfig.TelegramChatID) && _AutoInfoTimer == null)
            {
                var autoEvent = new AutoResetEvent(true);
                _AutoInfoTimerPeriod = 1000 * 20;
                _AutoInfoTimer = new System.Timers.Timer() { Interval = _AutoInfoTimerPeriod };
                _AutoInfoTimer.Elapsed +=  (s,e) => SendText(
                    string.Format("{0}{1}{2}",
                        string.Format("WorkerName - '{0}'", ConfigManager.GeneralConfig.WorkerName)
                      , Environment.NewLine
                      , _GetStatus(TelegramOptions.FullStatus)));
                SendText(string.Format("WorkerName - '{0}'{1}AutoSendInfo: ON"
                    , ConfigManager.GeneralConfig.WorkerName
                    , Environment.NewLine));

                _AutoInfoTimer.Start();
            }
            else
            {
                SendText(string.Format("WorkerName - '{0}'{1}AutoSendInfo already running"
                    , ConfigManager.GeneralConfig.WorkerName
                    , Environment.NewLine));
            }
        }

        private static void StopAutoInfo()
        {
            if(_AutoInfoTimer != null)
            {
                _AutoInfoTimer.Stop();
                _AutoInfoTimer.Dispose();
                SendText(string.Format("WorkerName - '{0}'{1}AutoSendInfo: Off"
                    , ConfigManager.GeneralConfig.WorkerName
                    , Environment.NewLine));
            }
            else
            {
                SendText(string.Format("WorkerName - '{0}'{1}AutoSendInfo already stopped"
                    , ConfigManager.GeneralConfig.WorkerName
                    , Environment.NewLine));
            }
        }

        public static void RestartBot()
        {
            StopBot();
            StartBot(_GetStatus);
        }
        public static void StopBot()
        {
            if (_BotClient != null)
            {
                _BotClient.StopReceiving();
                _BotClient.OnMessage -= BotClient_OnMessage;
                _BotClient = null;
                BotRunning = false;
            }
        }

        private static void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (!e.Message.From.IsBot
                && e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage
                && e.Message.Text.StartsWith("/"))
            {
                StringBuilder responseMessage = new StringBuilder();

                responseMessage.AppendLine(string.Format("Machine - '{0}'", Environment.MachineName));
                responseMessage.AppendLine(string.Format("WorkerName - '{0}'", ConfigManager.GeneralConfig.WorkerName));
                string message = e.Message.Text.ToLower();
                if(message == "/info")
                {
                    responseMessage.AppendLine(_GetStatus(TelegramOptions.FullStatus));
                }
                else if(message == "/running")
                {
                    responseMessage.AppendLine(_GetStatus(TelegramOptions.IsRunning));
                }
                else if (message == "/profit")
                {
                    responseMessage.AppendLine(_GetStatus(TelegramOptions.Profit));
                }
                else if(message == string.Format("/forcestop:{0}", ConfigManager.GeneralConfig.WorkerName))
                {
                    responseMessage.AppendLine(_GetStatus(TelegramOptions.ForceStop));
                }
                else if (message == string.Format("/forcestart:{0}", ConfigManager.GeneralConfig.WorkerName.ToLower()))
                {
                    responseMessage.AppendLine(_GetStatus(TelegramOptions.ForceStart));
                }
                else if (message == string.Format("/forcerestart:{0}", ConfigManager.GeneralConfig.WorkerName))
                {
                    responseMessage.AppendLine(_GetStatus(TelegramOptions.ForceRestart));
                }
                else if(message == "/start")
                {
                    responseMessage.AppendLine(string.Format("Bot start successfully'"));
                }
                else if(message.StartsWith("/sendautoinfo"))
                {
                    if(message == "/sendautoinfo")
                    {
                        SendAutoInfo();
                    }
                }
                else if (message.StartsWith("/stopautoinfo"))
                {
                    if (message == "/stopautoinfo")
                    {
                        StopAutoInfo();
                    }
                }
                else if (message.StartsWith("/addtoautoinfo"))
                {
                    if (ConfigManager.GeneralConfig.TelegramChatID.Split(',').Contains(e.Message.Chat.Id.ToString()))
                    {
                        responseMessage.AppendLine("Already added to the AutoInfo List");
                    }
                    else
                    {
                        ConfigManager.GeneralConfig.TelegramChatID += "," + e.Message.Chat.Id.ToString();
                        responseMessage.AppendLine("successfully added to the AutoInfo List");
                    }
                }
                else
                {
                    responseMessage.AppendLine(string.Format("Invalid Command: '{0}'", e.Message.Text));
                }

                SendText(responseMessage.ToString(), e.Message.Chat.Id.ToString());
            }
        }

        public static async void SendText(string text, string chatId = null)
        {
            if(chatId == null)
            {
                chatId = ConfigManager.GeneralConfig.TelegramChatID;
            }
            try
            {
                foreach(var chat in chatId.Split(','))
                {
                    await _BotClient.SendTextMessageAsync(chat, text);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", "Telegram Bot error: " + ex.Message);
            }
        }


    }
}
