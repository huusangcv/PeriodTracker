using Microsoft.EntityFrameworkCore;
using PeriodTracker.ApiGateways.DailyTracking;
using PeriodTracker.ApiGateways.Network;
using PeriodTracker.Entities.PeriodTracker;

namespace PeriodTracker.Services.MainServices
{
    public partial class DailyTrackingService
    {
        public async Task<BaseResponse> SetDailyLogSymptoms(SetDailyLogSymptomsRequest request)
        {
            var result = new BaseResponse();
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    result.Message = "Invalid request.";
                    return result;
                }

                if (request.Symptoms == null)
                {
                    result.Message = "Symptoms is required.";
                    return result;
                }

                if (request.Symptoms.Any(x => x.SymptomId <= 0))
                {
                    result.Message = "SymptomId must be greater than zero.";
                    return result;
                }

                if (request.Symptoms.Any(x => x.Severity.HasValue && (x.Severity.Value < 1 || x.Severity.Value > 5)))
                {
                    result.Message = "Severity must be between 1 and 5.";
                    return result;
                }

                if (request.Symptoms.Any(x => !string.IsNullOrWhiteSpace(x.Note) && x.Note.Length > 300))
                {
                    result.Message = "Symptom note length must be less than or equal to 300.";
                    return result;
                }

                var userExists = await _dataContext.Users.AnyAsync(x => x.Id == request.UserId && !x.Deleted);
                if (!userExists)
                {
                    result.Message = "User not found.";
                    return result;
                }

                await using var transaction = await _dataContext.Database.BeginTransactionAsync();

                var logDate = NormalizeDate(request.LogDate);
                if (!IsBusinessDate(logDate))
                {
                    result.Message = "LogDate is invalid.";
                    return result;
                }

                var symptomIds = request.Symptoms.Select(x => x.SymptomId).Distinct().ToList();
                if (symptomIds.Count != request.Symptoms.Count)
                {
                    result.Message = "Duplicate symptom ids are not allowed.";
                    return result;
                }

                var validSymptomIds = await _dataContext.Symptoms
                    .Where(x => symptomIds.Contains(x.Id) && x.IsActive)
                    .Select(x => x.Id)
                    .ToListAsync();

                if (validSymptomIds.Count != symptomIds.Count)
                {
                    result.Message = "One or more symptoms are invalid or inactive.";
                    return result;
                }

                var dailyLog = await _dataContext.DailyLogs
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.LogDate == logDate);

                if (dailyLog == null)
                {
                    dailyLog = new DailyLog
                    {
                        UserId = request.UserId,
                        LogDate = logDate
                    };
                    _dataContext.DailyLogs.Add(dailyLog);
                    await _dataContext.SaveChangesAsync();
                }

                var existingSymptoms = await _dataContext.DailyLogSymptoms
                    .Where(x => x.DailyLogId == dailyLog.Id)
                    .ToListAsync();

                if (existingSymptoms.Count > 0)
                {
                    _dataContext.DailyLogSymptoms.RemoveRange(existingSymptoms);
                }

                var newSymptoms = request.Symptoms.Select(x => new DailyLogSymptom
                {
                    DailyLogId = dailyLog.Id,
                    SymptomId = x.SymptomId,
                    Severity = x.Severity,
                    Note = x.Note
                }).ToList();

                _dataContext.DailyLogSymptoms.AddRange(newSymptoms);
                await _dataContext.SaveChangesAsync();
                await transaction.CommitAsync();

                result.Success = true;
                result.Data = new
                {
                    dailyLog.Id,
                    dailyLog.UserId,
                    dailyLog.LogDate,
                    Symptoms = newSymptoms.Select(x => new
                    {
                        x.SymptomId,
                        x.Severity,
                        x.Note
                    })
                };
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }

            return result;
        }
    }
}
