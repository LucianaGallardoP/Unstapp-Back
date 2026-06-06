using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Unstapp.Application.DTOs;
using Unstapp.Infrastructure.Entities;

namespace Unstapp.Application.Mappings
{
    public class CalendarProfile : Profile
    {
        public CalendarProfile()
        {
            CreateMap<CalendarEvent, CalendarEventDto>();
        }
    }
}
