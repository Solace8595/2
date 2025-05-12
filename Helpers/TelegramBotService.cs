using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using System;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Pastar.Helpers
{
    public class TelegramBotService
    {
        private readonly TelegramBot _telegramBot;
        private readonly long _adminChatId; 
        private readonly HashSet<long> _awaitingPassword = new();

        public TelegramBotService(TelegramBot telegramBot)
        {
            _telegramBot = telegramBot;
            _adminChatId = 123456789; 
        }

        public void StartBot()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            var cts = new CancellationTokenSource();

            _telegramBot.BotClient.StartReceiving(
                UpdateHandler,
                ErrorHandler,
                receiverOptions,
                cts.Token
            );
        }

        private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.Type == UpdateType.Message && update.Message is Message message)
            {
                var chatId = message.Chat.Id;
                var text = message.Text?.Trim();

                if (text == "/start")
                {
                    _awaitingPassword.Add(chatId);
                    await _telegramBot.HandleStartCommandAsync(chatId);
                }
                else if (_awaitingPassword.Contains(chatId))
                {
                    if (text == "123456789")
                    {
                        _awaitingPassword.Remove(chatId);
                        await _telegramBot.HandlePasswordInputAsync(chatId, text);
                    }
                    else
                    {
                        await _telegramBot.HandlePasswordInputAsync(chatId, text);
                    }
                }
                else
                {
                    await _telegramBot.SendMessageAsync(chatId, "Неизвестная команда. Напишите /start.");
                }
            }
        }

        private Task ErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }

        public async Task NotifyAdminAsync(string message)
        {
            Console.WriteLine($"[Telegram] Попытка отправки уведомления администраторам");
            Console.WriteLine($"[Telegram] Авторизованные пользователи: {string.Join(", ", _telegramBot.GetAuthorizedUsers())}");

            var authorizedUsers = _telegramBot.GetAuthorizedUsers();

            if (!authorizedUsers.Any())
            {
                Console.WriteLine("[Telegram] Нет авторизованных пользователей для отправки уведомления");
                return;
            }

            foreach (var chatId in authorizedUsers)
            {
                try
                {
                    await _telegramBot.SendMessageAsync(chatId, message);
                    Console.WriteLine($"[Telegram] Уведомление успешно отправлено chatId={chatId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Telegram] Ошибка при отправке уведомления для chatId={chatId}: {ex.Message}");
                }
            }
        }
    }
}