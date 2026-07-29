using System.Globalization;
using System.Text;
using AutoMapper;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Unstapp.Application.DTOs.Horarios;
using Unstapp.Application.Interfaces;
using Unstapp.Infrastructure.Entities;
using Unstapp.Infrastructure.Interfaces;
using Unstapp.Shared.DTOs.Common;

namespace Unstapp.Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ICareerRepository _careerRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IMapper _mapper;

        public ScheduleService(
            ICareerRepository careerRepository,
            IScheduleRepository scheduleRepository,
            IMapper mapper
        )
        {
            _careerRepository = careerRepository;
            _scheduleRepository = scheduleRepository;
            _mapper = mapper;
        }


        public async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByDayAsync(int userId, string day, int year)
        {

            var userCareer = await _careerRepository.GetUserCareerAsync(userId);
            if (userCareer == null)
                return ServiceResult<List<ScheduleResponseDto>>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_ASSIGNED",
                    "El usuario no tiene una carrera asignada."
                );

            if (userCareer.Career == null)
                return ServiceResult<List<ScheduleResponseDto>>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_FOUND",
                    "La carrera asignada al usuario no fue encontrada."
                );

            var careerId = userCareer.CareerId;
            return await GetSchedulesCoreAsync(careerId, day, year);
        }


        public async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesByCareerAsync(int careerId, string day, int year)
        {
            var careerExists = await _careerRepository.CareerExistsAsync(careerId);
            if (!careerExists)
                return ServiceResult<List<ScheduleResponseDto>>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_FOUND",
                    "La carrera especificada no existe."
                );

            return await GetSchedulesCoreAsync(careerId, day, year);
        }

        private async Task<ServiceResult<List<ScheduleResponseDto>>> GetSchedulesCoreAsync(
            int careerId,
            string day,
            int year
        )
        {

            if (string.IsNullOrWhiteSpace(day))
                return ServiceResult<List<ScheduleResponseDto>>.Fail(
                    StatusCodes.Status400BadRequest,
                    "DAY_REQUIRED",
                    "El día es obligatorio."
                );

            if (year <= 0)
                return ServiceResult<List<ScheduleResponseDto>>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_YEAR",
                    "El año no es válido."
                );

            var schedules = await _scheduleRepository.GetSchedulesByCareerAndDayAsync(careerId, day, year);

            var resultDto = schedules.Select(s => new ScheduleResponseDto
            {
                Id = s.Id,
                Subject = s.Subject,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                DurationHours = s.DurationHours,
                Professor = s.Professor,
                Classroom = s.Classroom,
                Year = s.Year,
            }).ToList();

            return ServiceResult<List<ScheduleResponseDto>>.Ok(resultDto);
        }


        public async Task<ServiceResult<ScheduleResponseDto>> CreateScheduleAsync(ScheduleCreateDto dto)
        {
            if(dto.Year <= 0)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_YEAR",
                    "El año especificado es inválido."
                );

            var careerExists = await _careerRepository.CareerExistsAsync(dto.CareerId);
            if (!careerExists)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "CAREER_NOT_FOUND",
                    "La carrera especificada no existe."
                );

            if (!TryParseExcelTime(dto.StartTime, out TimeSpan parsedTime))
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_TIME_FORMAT",
                    "El formato de la hora de inicio es inválido."
                );

            var newSchedule = _mapper.Map<Schedule>(dto);
            newSchedule.StartTime = parsedTime;

            await _scheduleRepository.AddNewScheduleAsync(newSchedule);
            var schedule = await _scheduleRepository.GetScheduleByIdAsync(newSchedule.Id);

            if (schedule == null)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "SCHEDULE_NOT_FOUND",
                    "El horario no se registró en nuestra base de datos."
                );

            var response = _mapper.Map<ScheduleResponseDto>(schedule);

            return ServiceResult<ScheduleResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<ScheduleResponseDto>> UpdateScheduleAsync(
            int id,
            ScheduleUpdateDto dto)
        {
            if (id <= 0)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_SCHEDULE_ID",
                    "El ID del horario no es válido."
                );

            var schedule = await _scheduleRepository.GetScheduleByIdAsync(id);

            if (schedule == null)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "SCHEDULE_NOT_FOUND",
                    "Horario no encontrado."
                );

            if (dto.CareerId.HasValue)
            {
                if (dto.CareerId.Value <= 0)
                    return ServiceResult<ScheduleResponseDto>.Fail(
                        StatusCodes.Status400BadRequest,
                        "INVALID_CAREER_ID",
                        "La carrera no es válida."
                    );

                var careerExists = await _careerRepository.CareerExistsAsync(dto.CareerId.Value);

                if (!careerExists)
                    return ServiceResult<ScheduleResponseDto>.Fail(
                        StatusCodes.Status404NotFound,
                        "CAREER_NOT_FOUND",
                        "La carrera especificada no existe."
                    );
            }

            if (dto.Subject != null && string.IsNullOrWhiteSpace(dto.Subject))
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "SUBJECT_REQUIRED",
                    "La materia no puede estar vacía."
                );
            }

            if (dto.Day != null && string.IsNullOrWhiteSpace(dto.Day))
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "DAY_REQUIRED",
                    "El día no puede estar vacío."
                );
            }

            if (dto.DurationHours.HasValue && dto.DurationHours.Value <= 0)
            {
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_DURATION",
                    "La duración debe ser mayor a cero."
                );
            }

            if (dto.StartTime != null)
            {
                if (string.IsNullOrWhiteSpace(dto.StartTime))
                {
                    return ServiceResult<ScheduleResponseDto>.Fail(
                        StatusCodes.Status400BadRequest,
                        "INVALID_TIME_FORMAT",
                        "La hora de inicio no puede estar vacía."
                    );
                }

                if (!TryParseExcelTime(dto.StartTime, out TimeSpan parsedTime))
                {
                    return ServiceResult<ScheduleResponseDto>.Fail(
                        StatusCodes.Status400BadRequest,
                        "INVALID_TIME_FORMAT",
                        "El formato de la hora de inicio es inválido."
                    );
                }

                schedule.StartTime = parsedTime;
            }

            _mapper.Map(dto, schedule);

            await _scheduleRepository.UpdateScheduleAsync(schedule);

            var updatedSchedule = await _scheduleRepository.GetScheduleByIdAsync(schedule.Id);

            if (updatedSchedule == null)
                return ServiceResult<ScheduleResponseDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "SCHEDULE_NOT_FOUND",
                    "El horario no se encontró en nuestra base de datos."
                );

            var response = _mapper.Map<ScheduleResponseDto>(updatedSchedule);

            return ServiceResult<ScheduleResponseDto>.Ok(response);
        }

        public async Task<ServiceResult<bool>> DeleteScheduleAsync(int id)
        {
            if (id <= 0)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_SCHEDULE_ID",
                    "El ID del horario no es válido."
                );

            var schedule = await _scheduleRepository.GetScheduleByIdAsync(id);

            if (schedule == null)
                return ServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "SCHEDULE_NOT_FOUND",
                    "Horario no encontrado."
                );

            await _scheduleRepository.SoftDeleteAsync(schedule);

            return ServiceResult<bool>.Ok(true);
        }

        // Se asume que el excel viene en este formato: Carrera | Año | Materia | Día | HoraInicio | Duracion | Profesor | Aula
        public async Task<ServiceResult<ScheduleImportResponseDto>> ImportSchedulesAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return ServiceResult<ScheduleImportResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "FILE_REQUIRED",
                    "El archivo está vacío o no se proporcionó."
                );

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".xlsx" && extension != ".xls")
                return ServiceResult<ScheduleImportResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_FILE_FORMAT",
                    "El archivo debe ser un Excel (.xlsx o .xls)."
                );

            var careers = await _careerRepository.GetAllCareersAsync();

            var careersByName = careers
                .GroupBy(c => NormalizeText(c.Name))
                .ToDictionary(g => g.Key, g => g.ToList());

            var schedules = new List<Schedule>();

            await using var stream = file.OpenReadStream();

            using var reader = ExcelReaderFactory.CreateReader(stream);

            var rowNumber = 0;

            try
            {
                while (reader.Read())
                {
                    rowNumber++;

                    // Saltar encabezado
                    if (rowNumber == 1)
                        continue;

                    var careerName = GetCellValue(reader, 0)?.ToString()?.Trim();
                    var yearValue = GetCellValue(reader, 1);
                    var subject = GetCellValue(reader, 2)?.ToString()?.Trim();
                    var day = GetCellValue(reader, 3)?.ToString()?.Trim();
                    var startTimeValue = GetCellValue(reader, 4);
                    var durationValue = GetCellValue(reader, 5);
                    var professor = GetCellValue(reader, 6)?.ToString()?.Trim();
                    var classroom = GetCellValue(reader, 7)?.ToString()?.Trim();

                    var isEmptyRow = string.IsNullOrWhiteSpace(careerName) &&
                                     IsEmptyExcelValue(yearValue) &&
                                     string.IsNullOrWhiteSpace(subject) &&
                                     string.IsNullOrWhiteSpace(day) &&
                                     IsEmptyExcelValue(startTimeValue) &&
                                     IsEmptyExcelValue(durationValue) &&
                                     string.IsNullOrWhiteSpace(professor) &&
                                     string.IsNullOrWhiteSpace(classroom);

                    if (isEmptyRow)
                        continue;

                    if (string.IsNullOrWhiteSpace(careerName))
                        return ServiceResult<ScheduleImportResponseDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "CAREER_REQUIRED",
                            $"Fila {rowNumber}: La Carrera es obligatoria."
                        );

                    var normalizedCareerName = NormalizeText(careerName);

                    if (!careersByName.TryGetValue(normalizedCareerName, out var matchingCareers))
                        return ServiceResult<ScheduleImportResponseDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "CAREER_NOT_FOUND",
                            $"Fila {rowNumber}: La carrera '{careerName}' no existe en la base de datos."
                        );

                    if (matchingCareers.Count > 1)
                        return ServiceResult<ScheduleImportResponseDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "AMBIGUOUS_CAREER_NAME",
                            $"Fila {rowNumber}: La carrera '{careerName}' coincide con más de una carrera. Por favor, asegúrese de que el nombre de la carrera sea único."
                        );

                    var career = matchingCareers.First();

                    if (!TryParseExcelInt(yearValue, out var year) || year <= 0)
                    {
                        return ServiceResult<ScheduleImportResponseDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "INVALID_YEAR",
                            $"Fila {rowNumber}: el año es inválido."
                        );
                    }

                    if (string.IsNullOrWhiteSpace(subject))
                        return ServiceResult<ScheduleImportResponseDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "SUBJECT_REQUIRED",
                            $"Fila {rowNumber}: La materia es requerida."
                        );

                    if (string.IsNullOrWhiteSpace(day))
                        return ServiceResult<ScheduleImportResponseDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "DAY_REQUIRED",
                            $"Fila {rowNumber}: El día es requerido."
                        );

                    if (!TryParseExcelTime(startTimeValue, out var startTime))
                        return ServiceResult<ScheduleImportResponseDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "INVALID_START_TIME_FORMAT",
                            $"Fila {rowNumber}: El formato de la hora de inicio es inválido."
                        );

                    if (!TryParseExcelDecimal(durationValue, out var durationHours) || durationHours <= 0)
                        return ServiceResult<ScheduleImportResponseDto>.Fail(
                            StatusCodes.Status400BadRequest,
                            "INVALID_DURATION",
                            $"Fila {rowNumber}: La duración debe ser un número mayor a cero."
                        );

                    schedules.Add(new Schedule
                    {
                        CareerId = career.CareerId,
                        Year = year,
                        Subject = subject.Trim(),
                        Day = day.Trim(),
                        StartTime = startTime,
                        DurationHours = durationHours,
                        Professor = string.IsNullOrWhiteSpace(professor) ? string.Empty : professor.Trim(),
                        Classroom = string.IsNullOrWhiteSpace(classroom) ? string.Empty : classroom.Trim(),
                        IsDeleted = false
                    });
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<ScheduleImportResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_EXCEL_FILE",
                    "No se pudo procesar el archivo Excel. Verificá que respete el formato de la plantilla."
                );
            }

            if (schedules.Count == 0)
                return ServiceResult<ScheduleImportResponseDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "NO_VALID_ROWS",
                    "No se encontraron filas válidas para importar."
                );

            await _scheduleRepository.AddRangeAsync(schedules);

            return ServiceResult<ScheduleImportResponseDto>.Ok(new ScheduleImportResponseDto
            {
                CreatedCount = schedules.Count,
                Message = $"{schedules.Count} horarios importados exitosamente."
            });
        }
        private static string NormalizeText(string value)
        {
            var normalized = value.Trim().Normalize(NormalizationForm.FormD);

            var chars = normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(chars)
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
        }

        private static bool TryParseExcelTime(object? value, out TimeSpan time)
        {
            time = default;

            if (value == null)
                return false;

            if (value is TimeSpan timeSpan)
            {
                time = timeSpan;
                return IsValidHour(time);
            }

            if(value is DateTime dateTime)
            {
                time = dateTime.TimeOfDay;
                return IsValidHour(time);
            }

            if (value is double doubleValue)
            {
                if(doubleValue > 0 && doubleValue < 1)
                {
                    time = TimeSpan.FromDays(doubleValue);
                    return IsValidHour(time);
                }

                if(doubleValue >= 0 && doubleValue < 24)
                {
                    time = TimeSpan.FromHours(doubleValue);
                    return IsValidHour(time);
                }

                return false;
            }

            var text = value.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text
                .Replace(" hs", "", StringComparison.OrdinalIgnoreCase)
                .Replace("hs", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" h", "", StringComparison.OrdinalIgnoreCase)
                .Replace("h", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" pm", "", StringComparison.OrdinalIgnoreCase)
                .Replace("pm", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" am", "", StringComparison.OrdinalIgnoreCase)
                .Replace("am", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if(int.TryParse(text, out int hourOnly))
            {
                if(hourOnly >= 0 && hourOnly < 24)
                {
                    time = TimeSpan.FromHours(hourOnly);
                    return true;
                }

                return false;
            }

            if(decimal.TryParse(text, NumberStyles.Number, new CultureInfo("es-AR"), out var decimalHour) ||
               decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimalHour)
            )
            {
                if(decimalHour >= 0 && decimalHour < 24)
                {
                    time = TimeSpan.FromHours((double)decimalHour);
                    return true;
                }

                return false;
            }

            if (TimeSpan.TryParse(text, out time))
                return IsValidHour(time);

            if (DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate
            ))
            {
                time = parsedDate.TimeOfDay;
                return IsValidHour(time);
            }

            if (DateTime.TryParse(
                text,
                CultureInfo.CurrentCulture,
                DateTimeStyles.None,
                out parsedDate
            ))
            {
                time = parsedDate.TimeOfDay;
                return IsValidHour(time);
            }

            return false;
        }

        private static bool IsValidHour(TimeSpan time)
        {
            return time >= TimeSpan.Zero && time < TimeSpan.FromDays(1);
        }

        private static bool TryParseExcelDecimal(object? value, out decimal number)
        {
            number = default;

            if (value == null)
                return false;

            if (value is decimal decimalValue)
            {
                number = decimalValue;
                return true;
            }

            if (value is double doubleValue)
            {
                number = Convert.ToDecimal(doubleValue, CultureInfo.InvariantCulture);
                return true;
            }

            if(value is int intValue)
            {
                number = intValue;
                return true;
            }

            var text = value.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return false;

            if(decimal.TryParse(
                text,
                NumberStyles.Number,
                new CultureInfo("es-AR"),
                out number
            ))
                return true;

            if (decimal.TryParse(text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out number
            ))
                return true;

            return false;
        }

        private static bool TryParseExcelInt(object? value, out int number)
        {
            number = default;

            if(value == null)
                return false;

            if(value is int intValue)
            {
                number = intValue;
                return true;
            }

            if (value is double doubleValue)
            {
                if(doubleValue % 1 != 0)
                    return false;

                number = Convert.ToInt32(doubleValue);
                return true;
            }

            if (value is decimal decimalValue)
            {
                if(decimalValue % 1 != 0)
                    return false;

                number = Convert.ToInt32(decimalValue);
                return true;
            }

            var text = value?.ToString()?.Trim();

            if(string.IsNullOrWhiteSpace(text))
                return false;

            return int.TryParse(text, out number);
        }

        private static object? GetCellValue(IExcelDataReader dataReader, int index)
        {
            if (index < 0 || index >= dataReader.FieldCount)
                return null;

            return dataReader.GetValue(index);
        }

        private static bool IsEmptyExcelValue(object? value)
        {
            return value == null || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}