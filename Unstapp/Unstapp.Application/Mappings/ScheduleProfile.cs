using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Unstapp.Application.DTOs;
using Unstapp.Application.DTOs.Horarios;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Application.Mappings
{
    public class ScheduleProfile : Profile
    {
        public ScheduleProfile()
        {
            CreateMap<ScheduleCreateDto, Schedule>()
                .ForMember(dest => dest.StartTime,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Subject,
                    opt => opt.MapFrom(src => src.Subject.Trim()))
                .ForMember(dest => dest.Professor,
                    opt => opt.MapFrom(src =>
                        string.IsNullOrWhiteSpace(src.Professor) ? string.Empty : src.Professor.Trim()))
                .ForMember(dest => dest.Classroom,
                    opt => opt.MapFrom(src =>
                        string.IsNullOrWhiteSpace(src.Classroom) ? string.Empty : src.Classroom.Trim()))
                .ForMember(dest => dest.Day,
                    opt => opt.MapFrom(src => src.Day.Trim()))
                .ForMember(dest => dest.Career,
                    opt => opt.Ignore());

            CreateMap<ScheduleUpdateDto, Schedule>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Career,
                    opt => opt.Ignore())
                .ForMember(dest => dest.StartTime,
                    opt => opt.Ignore())

                .ForMember(dest => dest.CareerId,
                    opt =>
                    {
                        opt.PreCondition(src => src.CareerId.HasValue);
                        opt.MapFrom(src => src.CareerId!.Value);
                    })

                .ForMember(dest => dest.Subject,
                    opt =>
                    {
                        opt.PreCondition(src => src.Subject != null);
                        opt.MapFrom(src => src.Subject!.Trim());
                    })

                .ForMember(dest => dest.Professor,
                    opt =>
                    {
                        opt.PreCondition(src => src.Professor != null);
                        opt.MapFrom(src => src.Professor!.Trim());
                    })

                .ForMember(dest => dest.Classroom,
                    opt =>
                    {
                        opt.PreCondition(src => src.Classroom != null);
                        opt.MapFrom(src => src.Classroom!.Trim());
                    })

                .ForMember(dest => dest.Day,
                    opt =>
                    {
                        opt.PreCondition(src => src.Day != null);
                        opt.MapFrom(src => src.Day!.Trim());
                    })

                .ForMember(dest => dest.DurationHours,
                    opt =>
                    {
                        opt.PreCondition(src => src.DurationHours.HasValue);
                        opt.MapFrom(src => src.DurationHours!.Value);
                    });

            CreateMap<Schedule, ScheduleResponseDto>();
        }
    }
}
