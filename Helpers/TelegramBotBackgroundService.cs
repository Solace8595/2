namespace Pastar.Helpers
{
    public class TelegramBotBackgroundService : IHostedService
    {
        private readonly TelegramBotService _telegramBotService;

        public TelegramBotBackgroundService(TelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _telegramBotService.StartBot();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
