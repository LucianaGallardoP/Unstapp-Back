using Unstapp.Application.Interfaces;

namespace Unstapp.API.Services.BackgroundServices
{
    public class CalendarEventReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CalendarEventReminderBackgroundService> _logger;
        private readonly IConfiguration _config;

        public CalendarEventReminderBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CalendarEventReminderBackgroundService> logger,
            IConfiguration config)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CalendarEventReminderBackgroundService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Ejecutando proceso de recordatorios de eventos.");
                    using var scope = _serviceScopeFactory.CreateScope();

                    var reminderService = scope.ServiceProvider.GetRequiredService<ICalendarEventReminderService>();

                    await reminderService.ProcessDueRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando recordatorios de eventos.");
                }

                var intervalMinutes = getIntervalMinutes();

                _logger.LogInformation(
                    "Próxima ejecución de recordatorios en {IntervalMinutes} minutos.",
                    intervalMinutes
                );

                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }

        private int getIntervalMinutes()
        {
            var intervalMinutes = _config.GetValue<int?>("BackgroundJobs:CalendarEventReminderIntervalMinutes") ?? 60;

            if (intervalMinutes < 1)
                return 1;

            return intervalMinutes;
        }
    }
}
