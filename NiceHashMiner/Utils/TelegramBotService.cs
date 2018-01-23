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
    public delegate string GetStatus();
    public static class TelegramBotService
    {
        private static TelegramBotClient botClient;
        private static GetStatus mGetStatus;
        public static bool BotRunning = false;

        public static void StartBot(GetStatus getStatus)
        {
            if (botClient == null)
            {
                botClient = new Telegram.Bot.TelegramBotClient(ConfigManager.GeneralConfig.TelegramAPIToken);
                mGetStatus = getStatus;
                botClient.StartReceiving(new Telegram.Bot.Types.Enums.UpdateType[] { Telegram.Bot.Types.Enums.UpdateType.MessageUpdate });

                botClient.OnMessage += BotClient_OnMessage;
                BotRunning = true;
            }
        }
        public static void RestartBot()
        {
            StopBot();
            StartBot(mGetStatus);
        }
        public static void StopBot()
        {
            if (botClient != null)
            {
                botClient.StopReceiving();
                botClient.OnMessage -= BotClient_OnMessage;
                botClient = null;
                BotRunning = false;
            }
        }

        private static void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (!e.Message.From.IsBot
                && e.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage
                && e.Message.Text.StartsWith("!"))
            {
                switch (e.Message.Text.ToLower())
                {
                    case "!info":
                        SendText(mGetStatus());
                        break;
                    default:
                        SendText(string.Format("Invalid Command: '{0}'", e.Message.Text));
                        break;
                }

            }
        }

        public static async void SendText(string text)
        {
            try
            {
                await botClient.SendTextMessageAsync(ConfigManager.GeneralConfig.TelegramChatID, text);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("NICEHASH", "Telegram Bot error: " + ex.Message);
            }
        }


    }
}
