using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Unstapp.Infrastructure.Interfaces;

namespace Unstapp.Infrastructure.Services
{
    public class WhatsAppNotificationDispatcher : IWhatsAppNotificationDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WhatsAppNotificationDispatcher> _logger;

        public WhatsAppNotificationDispatcher(
            IServiceScopeFactory scopeFactory,
            ILogger<WhatsAppNotificationDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void DispatchImportantPostNotification(int postId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<IWhatsAppNotificationService>();

                    await notificationService.NotifyImportantPostAsync(postId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error enviando notificaciones de WhatsApp para el post {PostId}",
                        postId
                    );
                }
            });
        }
    }
}
