using AutoMapper;
using Unstapp.Application.DTOs;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Entities.Enums;

namespace Unstapp.Application.Mappings
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationResponseDto>()
                .ForMember(dest => dest.User,
                    opt => opt.MapFrom(src => src.ActorUserName))
                .ForMember(dest => dest.Action,
                    opt => opt.MapFrom(src => GetActionText(src.ActionType)))
                .ForMember(dest => dest.ActorAvatarUrl,
                    opt => opt.MapFrom(src => src.ActorUser != null ? src.ActorUser.AvatarUrl : null))
                .ForMember(dest => dest.Message,
                    opt => opt.MapFrom(src => BuildNotificationMessage(src)));
        }

        private static string BuildNotificationMessage(Notification notification)
        {
            if (!string.IsNullOrWhiteSpace(notification.Message))
                return notification.Message;

            return $"{notification.ActorUserName} {GetActionText(notification.ActionType)}";
        }

        private static string GetActionText(NotificationActionType actionType)
        {
            return actionType switch
            {
                NotificationActionType.Like => "dio me gusta a tu post",
                NotificationActionType.Comment => "comentó en tu post",
                NotificationActionType.Follow => "comenzó a seguirte",
                NotificationActionType.CalendarEventReminder => "te recordó un evento",
                _ => "interactuó con tu post"
            };
        }
    }
}
