using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pastar.Helpers
{
    public class TelegramBot
    {
        private readonly ITelegramBotClient _botClient;
        private readonly string _botToken;
        private readonly HashSet<long> _authorizedUsers = new HashSet<long>();

        public ITelegramBotClient BotClient => _botClient;

        public TelegramBot(string botToken)
        {
            _botToken = botToken;
            _botClient = new TelegramBotClient(_botToken);
        }

        public async Task SendMessageAsync(long chatId, string message)
        {
            await _botClient.SendTextMessageAsync(chatId, message, parseMode: ParseMode.Html);
        }

        public async Task HandleStartCommandAsync(long chatId)
        {
            if (_authorizedUsers.Contains(chatId))
            {
                await SendMessageAsync(chatId, "Вы уже авторизованы и получаете уведомления.");
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "Пожалуйста, введите пароль для авторизации.");
            }
        }

        public async Task HandlePasswordInputAsync(long chatId, string inputPassword)
        {
            if (inputPassword == "123456789")
            {
                _authorizedUsers.Add(chatId);
                await SendMessageAsync(chatId, "Вы успешно авторизованы и будете получать уведомления.");
            }
            else
            {
                await SendMessageAsync(chatId, "Неверный пароль. Попробуйте снова.");
            }
        }

        public async Task SendNotificationAsync(long chatId, string notificationMessage)
        {
            if (_authorizedUsers.Contains(chatId))
            {
                await SendMessageAsync(chatId, notificationMessage);
            }
            else
            {
                await SendMessageAsync(chatId, "Вы не авторизованы. Пожалуйста, введите пароль.");
            }
        }
        public HashSet<long> GetAuthorizedUsers()
        {
            return _authorizedUsers;
        }
    }
}