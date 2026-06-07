using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;


namespace Unstapp.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRealtimeSender _notificationRealtimeSender;

        public NotificationService(
            INotificationRepository notificationRepository,
            IPostRepository postRepository,
            IUserRepository userRepository,
            INotificationRealtimeSender notificationRealtimeSender)
        {
            _notificationRepository = notificationRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
            _notificationRealtimeSender = notificationRealtimeSender;
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

        public async Task<ServiceResult<List<NotificationResponseDto>>> GetMyNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository.GetAllByUserIdAsync(userId);

            var dto = notifications.Select(n => new NotificationResponseDto
            {
                NotificationId = n.NotificationId,
                User = n.ActorUserName,
                Action = GetActionText(n.ActionType),
                PostId = n.PostId,
                IsPriority = n.IsPriority,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                Message = $"{n.ActorUserName} {GetActionText(n.ActionType)}"
            }).ToList();

            return ServiceResult<List<NotificationResponseDto>>.Ok(dto);
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

            var notificationDto = new NotificationResponseDto
            {
                NotificationId = notification.NotificationId,
                User = notification.ActorUserName,
                Action = GetActionText(notification.ActionType),
                IsPriority = notification.IsPriority,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                Message = $"{notification.ActorUserName} {GetActionText(notification.ActionType)}"
            };

            await _notificationRealtimeSender.SendNotificationAsync(followedUserId, notificationDto);
        }

        private async Task<Notification?> CreateNotification(
            int actorUserId,
            int postId,
            NotificationActionType actionType, bool isPriority)
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

        private static string GetActionText(NotificationActionType actionType)
        {
            return actionType switch
            {
                NotificationActionType.Like => "dio me gusta a tu post",
                NotificationActionType.Comment => "comentó en tu post",
                NotificationActionType.Follow => "comenzó a seguirte",
                _ => "interactuó con tu post"
            };
        }
    }
}
