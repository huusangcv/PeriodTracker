using Microsoft.EntityFrameworkCore;
using PeriodTracker.ApiGateways.DailyTracking;
using PeriodTracker.ApiGateways.Network;

namespace PeriodTracker.Services.MainServices
{
    public partial class DailyTrackingService
    {
        public async Task<BaseResponse> TakeDailyLogs(TakeDailyLogsRequest request)
        {
            var result = new BaseResponse();
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    result.Message = "Invalid request.";
                    return result;
                }

                var fromDate = NormalizeDate(request.FromDate);
                var toDate = NormalizeDate(request.ToDate);
                if (!IsBusinessDate(fromDate) || !IsBusinessDate(toDate))
                {
                    result.Message = "Date range is invalid.";
                    return result;
                }

                if (fromDate > toDate)
                {
                    result.Message = "FromDate must be less than or equal to ToDate.";
                    return result;
                }

                if (!IsAllowedRange(fromDate, toDate, 366))
                {
                    result.Message = "Date range is too large.";
                    return result;
                }

                result.Data = await _dataContext.DailyLogs
                    .AsNoTracking()
                    .Where(x => x.UserId == request.UserId && x.LogDate >= fromDate && x.LogDate <= toDate)
                    .OrderByDescending(x => x.LogDate)
                    .Select(x => new
                    {
                        x.Id,
                        x.LogDate,
                        x.FlowLevel,
                        x.Mood,
                        x.Note
                    })
                    .ToListAsync();
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
