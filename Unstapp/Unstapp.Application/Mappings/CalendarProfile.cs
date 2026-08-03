using AutoMapper;
using Unstapp.Application.DTOs.Calendar;
using Unstapp.Infrastructure.Entities;
using Unstapp.Shared.Helpers;

namespace Unstapp.Application.Mappings
{
    public class CalendarProfile : Profile
    {
        public CalendarProfile()
        {
            CreateMap<CalendarEvent, CalendarEventDto>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Day,
                    opt => opt.MapFrom(src =>
                        DateHelper.ConvertUtcToArgentina(src.StartDate).ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.StartTime,
                    opt => opt.MapFrom(src =>
                        DateHelper.ConvertUtcToArgentina(src.StartDate).ToString("HH:mm")))
                .ForMember(dest => dest.EndTime,
                    opt => opt.MapFrom(src =>
                        DateHelper.ConvertUtcToArgentina(src.EndDate).ToString("HH:mm")))
                .ForMember(dest => dest.CareerIds,
                    opt => opt.MapFrom(src =>
                        src.CalendarEventCareers
                            .Select(ec => ec.CareerId)
                            .ToList()))
                .ForMember(dest => dest.CareerNames,
                    opt => opt.MapFrom(src =>
                        src.CalendarEventCareers
                            .Where(ec => ec.Career != null)
                            .Select(ec => ec.Career!.Name)
                            .ToList()))
                .ForMember(dest => dest.ReminderEnabledForCurrentUser,
                    opt => opt.Ignore());
        }
    }
}
