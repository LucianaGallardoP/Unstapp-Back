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
    public class CareerProfile : Profile
    {
        public CareerProfile() {
            CreateMap<Career, CareerAdminResponseDto>()
                .ForMember(dest => dest.FacultyName,
                    opt => opt.MapFrom(src =>
                        src.Faculty != null ? src.Faculty.Name : "Sin Facultad"));
        }
    }
}
