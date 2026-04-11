using Microsoft.EntityFrameworkCore;
using PeriodTracker.ApiGateways.DailyTracking;
using PeriodTracker.ApiGateways.Network;
using PeriodTracker.Entities.PeriodTracker;

namespace PeriodTracker.Services.MainServices
{
    public partial class DailyTrackingService
    {
        public async Task<BaseResponse> UpsertDailyLog(UpsertDailyLogRequest request)
        {
            var result = new BaseResponse();
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    result.Message = "Invalid request.";
                    return result;
                }

                if (!string.IsNullOrWhiteSpace(request.FlowLevel) && request.FlowLevel.Length > 30)
                {
                    result.Message = "FlowLevel length must be less than or equal to 30.";
                    return result;
                }

                if (!string.IsNullOrWhiteSpace(request.Mood) && request.Mood.Length > 30)
                {
                    result.Message = "Mood length must be less than or equal to 30.";
                    return result;
                }

                if (!string.IsNullOrWhiteSpace(request.Note) && request.Note.Length > 1000)
                {
                    result.Message = "Note length must be less than or equal to 1000.";
                    return result;
                }

                var userExists = await _dataContext.Users.AnyAsync(x => x.Id == request.UserId && !x.Deleted);
                if (!userExists)
                {
                    result.Message = "User not found.";
                    return result;
                }

                var logDate = NormalizeDate(request.LogDate);
                if (!IsBusinessDate(logDate))
                {
                    result.Message = "LogDate is invalid.";
                    return result;
                }

                var dailyLog = await _dataContext.DailyLogs
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.LogDate == logDate);

                if (dailyLog == null)
                {
                    dailyLog = new DailyLog
                    {
                        UserId = request.UserId,
                        LogDate = logDate,
                        FlowLevel = request.FlowLevel,
                        Mood = request.Mood,
                        Note = request.Note
                    };
                    _dataContext.DailyLogs.Add(dailyLog);
                }
                else
                {
                    dailyLog.FlowLevel = request.FlowLevel;
                    dailyLog.Mood = request.Mood;
                    dailyLog.Note = request.Note;
                }

                await _dataContext.SaveChangesAsync();

                result.Success = true;
                result.Data = new
                {
                    dailyLog.Id,
                    dailyLog.UserId,
                    dailyLog.LogDate,
                    dailyLog.FlowLevel,
                    dailyLog.Mood,
                    dailyLog.Note
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
