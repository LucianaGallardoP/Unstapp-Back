using AutoMapper;
using Unstapp.Application.DTOs;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class CareerAdminService : ICareerAdminService
    {
        private readonly ICareerRepository _careerRepository;
        private readonly IMapper _mapper;

        public CareerAdminService(
            ICareerRepository careerRepository,
            IMapper mapper)
        {
            _careerRepository = careerRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<List<CareerAdminResponseDto>>> GetAllCareersAsync()
        {
            var careers = await _careerRepository.GetAllCareersAsync();

            var careersDtos = _mapper.Map<List<CareerAdminResponseDto>>(careers);

            return ServiceResult<List<CareerAdminResponseDto>>.Ok(careersDtos);
        }
    }
}