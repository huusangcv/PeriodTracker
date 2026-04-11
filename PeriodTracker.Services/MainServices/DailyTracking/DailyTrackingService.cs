using PeriodTracker.ApiGateways.DailyTracking;
using PeriodTracker.ApiGateways.Network;
using PeriodTracker.Entities;
using PeriodTracker.Services.Framework;

namespace PeriodTracker.Services.MainServices
{
    public interface IDailyTrackingService
    {
        Task<BaseResponse> TakeSymptoms(TakeSymptomsRequest request);
        Task<BaseResponse> TakeDailyLog(TakeDailyLogRequest request);
        Task<BaseResponse> TakeDailyLogs(TakeDailyLogsRequest request);
        Task<BaseResponse> UpsertDailyLog(UpsertDailyLogRequest request);
        Task<BaseResponse> UpsertDailyMetric(UpsertDailyMetricRequest request);
        Task<BaseResponse> SetDailyLogSymptoms(SetDailyLogSymptomsRequest request);
    }

    public partial class DailyTrackingService(
        ILogService _logService,
        DataContext _dataContext
    ) : IDailyTrackingService
    {
        private static readonly HashSet<string> AllowedMetricTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Weight",
                "Temperature",
                "Water"
            };

        private static DateTime NormalizeDate(DateTime value) => value.Date;

        private static bool IsBusinessDate(DateTime value)
        {
            var date = value.Date;
            return date >= new DateTime(1900, 1, 1) && date <= DateTime.UtcNow.Date.AddYears(1);
        }

        private static bool IsAllowedRange(DateTime fromDate, DateTime toDate, int maxDays)
            => (toDate - fromDate).TotalDays <= maxDays;

        private BaseResponse HandleException(Exception ex)
        {
            _logService.Error(ex);
            return new BaseResponse
            {
                Success = false,
                Message = "Request failed. Please try again later."
            };
        }
    }
}
