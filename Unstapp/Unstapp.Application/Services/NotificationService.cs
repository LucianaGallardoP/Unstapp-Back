using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;
using Unstapp.Shared.Helpers;


namespace Unstapp.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRealtimeSender _notificationRealtimeSender;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public NotificationService(
            INotificationRepository notificationRepository,
            IPostRepository postRepository,
            IUserRepository userRepository,
            INotificationRealtimeSender notificationRealtimeSender,
            IMapper mapper,
            IConfiguration configuration)
        {
            _notificationRepository = notificationRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
            _notificationRealtimeSender = notificationRealtimeSender;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task CreateLikeNotificationAsync(int actorUserId, int postId)
        {
            Notification? notification = await CreateNotification(
                                                    actorUserId,
                                                    postId,
                                                    NotificationActionType.Like,
                                                    false);

            if(notification == null)
                return;

            await _notificationRepository.AddAsync(notification);
        }

        public async Task CreateCommentNotificationAsync(int actorUserId, int postId)
        {
            Notification? notification = await CreateNotification(
                                                    actorUserId,
                                                    postId,
                                                    NotificationActionType.Comment,
                                                    false);

            if (notification == null)
                return;

            await _notificationRepository.AddAsync(notification);
        }
        public async Task CreateCalendarEventReminderNotificationAsync(
            int recipientUserId,
            int calendarEventId,
            string eventTitle,
            DateTime eventStartDate)
        {

            if(recipientUserId <= 0 || calendarEventId <= 0)
                return;

            var systemActorUserId = GetSystemActorUserId();

            var systemUser = await _userRepository.GetByIdAsync(systemActorUserId);

            if(systemUser == null)
                return;

            var eventStartArgentina = DateHelper.ConvertUtcToArgentina(eventStartDate);

            var message = $"Te recordamos: '{eventTitle}' el {eventStartArgentina:dd/MM/yyyy} a las {eventStartDate:HH:mm}.";

            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                ActorUserId = systemActorUserId,
                ActorUserName = "Unstapp",
                ActionType = NotificationActionType.CalendarEventReminder,
                CalendarEventId = calendarEventId,
                IsPriority = true,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Message = message
            };

            await _notificationRepository.AddAsync(notification);

            var notificationDto = _mapper.Map<NotificationResponseDto>(notification);

            await _notificationRealtimeSender.SendNotificationAsync(recipientUserId, notificationDto);
        }

        public async Task<ServiceResult<List<NotificationResponseDto>>> GetMyNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository.GetAllByUserIdAsync(userId);

            var dtos = _mapper.Map<List<NotificationResponseDto>>(notifications);

            return ServiceResult<List<NotificationResponseDto>>.Ok(dtos);
        }

        public async Task<ServiceResult<bool>> MarkAsReadAsync(int userId, int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null || notification.RecipientUserId != userId || notification.IsDeleted)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "NOTIFICATION_NOT_FOUND",
                    "Notificación no encontrada."
                );

            notification.IsRead = true;

            await _notificationRepository.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int userId, int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null || notification.RecipientUserId != userId || notification.IsDeleted)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "NOTIFICATION_NOT_FOUND",
                    "Notificación no encontrada."
                );

            notification.IsDeleted = true;

            await _notificationRepository.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> MarkAllAsReadAsync(int userId)
        {
            await _notificationRepository.MarkAllAsReadByUserIdAsync(userId);
            
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteAllAsync(int userId)
        {
            await _notificationRepository.SoftDeleteAllByUserIdAsync(userId);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> HasUnreadNotificationsAsync(int userId)
        {
            var hasUnread = await _notificationRepository.HasUnreadByUserIdAsync(userId);
            
            return ServiceResult<bool>.Ok(hasUnread);
        }

        public async Task CreateFollowNotificationAsync(int actorUserId, int followedUserId)
        {
            if(actorUserId == followedUserId)
                return;

            var actor = await _userRepository.GetByIdAsync(actorUserId);

            if(actor == null)
                return;

            var followedUser = await _userRepository.GetByIdAsync(followedUserId);

            if(followedUser == null)
                return;

            var actorUserName = $"{actor.Name} {actor.LastName}".Trim();

            var notification = new Notification
            {
                RecipientUserId = followedUserId,
                ActorUserId = actorUserId,
                ActorUserName = actorUserName,
                ActionType = NotificationActionType.Follow,
                IsPriority = false,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _notificationRepository.AddAsync(notification);

            var notificationDto = _mapper.Map<NotificationResponseDto>(notification);
            notificationDto.ActorAvatarUrl = actor.AvatarUrl;

            await _notificationRealtimeSender.SendNotificationAsync(followedUserId, notificationDto);
        }

        private async Task<Notification?> CreateNotification(
            int actorUserId,
            int postId,
            NotificationActionType actionType,
            bool isPriority)
        {
            var post = await _postRepository.GetByIdWithRelationsAsync(postId);

            if (post == null)
                return null;

            if (post.UserId == actorUserId)
                return null;

            var actor = await _userRepository.GetByIdAsync(actorUserId);

            if (actor == null)
                return null;

            var actorUserName = $"{actor.Name} {actor.LastName}".Trim();

            var notification = new Notification
            {
                RecipientUserId = post.UserId,
                ActorUserId = actorUserId,
                ActorUserName = actorUserName,
                ActionType = actionType,
                PostId = postId,
                IsPriority = isPriority,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            return notification;
        }

        private int GetSystemActorUserId()
        {
            var value = _configuration["Notifications:SystemActorUserId"];

            if(int.TryParse(value, out var systemActorUserId) && systemActorUserId > 0)
            {
                return systemActorUserId;
            }

            return 1;
        }
    }
}
