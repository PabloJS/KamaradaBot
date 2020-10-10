using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace KamaradaBot.Core
{
    public class BotKamarada
    {
        private static TelegramBotClient Bot;
        private static Logger _logger;

        public BotKamarada(string botToken)
        {
            Bot = new TelegramBotClient(botToken);
            _logger = new Logger();
        }

        public async Task<string> GetName()
        {
            var me = await Bot.GetMeAsync();
            return me.Username;
        }

        public void StartReceiving(CancellationTokenSource cts)
        {
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            Bot.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                cts.Token
            );
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Task handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                // UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            await _logger.Fatal(ErrorMessage);
        }

        private static async Task BotOnMessageReceived(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            var action = (message.Text.Split(' ').First()) switch
            {
                "/startgame" => StartGame(message),
                "/join" => JoinGame(message),
                _ => Usage(message)
            };
            await action;
        }

        private static async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQuery.Id);

            if (Convert.ToBoolean(callbackQuery.Data))
            {
                await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"{callbackQuery.From.FirstName} quiere jugar!"
            );
            }
        }

        private static async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
            await _logger.Info($"Received inline result: {chosenInlineResult.ResultId}");
        }

        private static async Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            await _logger.Error($"Unknown update type: {update.Type}");
        }

        private static async Task StartGame(Message message)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            // Simulate longer running task
            // await Task.Delay(500);

            /*var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Sí", "true"),
                        InlineKeyboardButton.WithCallbackData("No", "false"),
                    }
            });
            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "¿Quieres jugar?",
                replyMarkup: inlineKeyboard
            );*/
            var logMessage = $"Iniciando juego en chat {message.Chat.Id} por petición de {message.From.FirstName} ({message.From.Id})";
            Console.WriteLine(logMessage);
            await _logger.Info(logMessage);

            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Empezando una nueva partida.\n" +
                        "Si quieres jugar usa el comando\n" +
                        "/join"
            );
        }

        private static async Task JoinGame(Message message)
        {
            var logMessage = $"{message.From.FirstName} ({message.From.Id}) se ha unido a la partida del chat {message.Chat.Id}";
            Console.WriteLine(logMessage);
            await _logger.Info(logMessage);

            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"*{message.From.FirstName}* se ha unido a la partida.",
                parseMode: ParseMode.Markdown
            );
        }

        private static async Task Usage(Message message)
        {
            const string usage = "Usage:\n" +
                                    "/startgame   - Starts a new game\n";
            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove()
            );
        }
    }
}
