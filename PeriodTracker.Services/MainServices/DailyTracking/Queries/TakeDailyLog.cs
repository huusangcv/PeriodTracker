using Microsoft.EntityFrameworkCore;
using PeriodTracker.ApiGateways.DailyTracking;
using PeriodTracker.ApiGateways.Network;

namespace PeriodTracker.Services.MainServices
{
    public partial class DailyTrackingService
    {
        public async Task<BaseResponse> TakeDailyLog(TakeDailyLogRequest request)
        {
            var result = new BaseResponse();
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    result.Message = "Invalid request.";
                    return result;
                }

                var logDate = NormalizeDate(request.LogDate);
                if (!IsBusinessDate(logDate))
                {
                    result.Message = "LogDate is invalid.";
                    return result;
                }

                var dailyLog = await _dataContext.DailyLogs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.LogDate == logDate);

                if (dailyLog == null)
                {
                    result.Success = true;
                    result.Data = null;
                    return result;
                }

                var metrics = await _dataContext.DailyMetrics
                    .AsNoTracking()
                    .Where(x => x.UserId == request.UserId && x.MeasuredDate == logDate)
                    .Select(x => new
                    {
                        x.Id,
                        x.MetricType,
                        x.DecimalValue,
                        x.Unit,
                        x.Note
                    })
                    .ToListAsync();

                var symptoms = await _dataContext.DailyLogSymptoms
                    .AsNoTracking()
                    .Where(x => x.DailyLogId == dailyLog.Id)
                    .Select(x => new
                    {
                        x.SymptomId,
                        SymptomName = x.Symptom.Name,
                        x.Severity,
                        x.Note
                    })
                    .ToListAsync();

                result.Data = new
                {
                    dailyLog.Id,
                    dailyLog.UserId,
                    dailyLog.LogDate,
                    dailyLog.FlowLevel,
                    dailyLog.Mood,
                    dailyLog.Note,
                    Metrics = metrics,
                    Symptoms = symptoms
                };
                result.Success = true;
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }

            return result;
        }
    }
}
