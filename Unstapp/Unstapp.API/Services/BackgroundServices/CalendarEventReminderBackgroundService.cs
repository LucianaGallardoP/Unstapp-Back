using Unstapp.Application.Interfaces;

namespace Unstapp.API.Services.BackgroundServices
{
    public class CalendarEventReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CalendarEventReminderBackgroundService> _logger;

        public CalendarEventReminderBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CalendarEventReminderBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var reminderService = scope.ServiceProvider.GetRequiredService<ICalendarEventReminderService>();

                    await reminderService.ProcessDueRemindersAsync(stoppingToken);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error procesando recordatorios de eventos.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
