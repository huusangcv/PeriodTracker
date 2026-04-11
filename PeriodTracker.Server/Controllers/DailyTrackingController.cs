using Microsoft.AspNetCore.Mvc;
using PeriodTracker.ApiGateways.DailyTracking;
using PeriodTracker.ApiGateways.Network;
using PeriodTracker.Services.MainServices;
using System.Security.Claims;

namespace Phoenix.Hemisphere.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DailyTrackingController(IDailyTrackingService _dailyTrackingService) : SecureApiController
    {
        private int GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdText, out var userId) || userId <= 0)
            {
                throw new UnauthorizedAccessException("Invalid user claim.");
            }

            return userId;
        }

        [HttpPost("take-symptoms")]
        public async Task<BaseResponse> TakeSymptoms([FromBody] TakeSymptomsRequest request)
            => await _dailyTrackingService.TakeSymptoms(request ?? new TakeSymptomsRequest());

        [HttpPost("take-daily-log")]
        public async Task<BaseResponse> TakeDailyLog([FromBody] TakeDailyLogRequest request)
        {
            request ??= new TakeDailyLogRequest();
            request.UserId = GetCurrentUserId();
            return await _dailyTrackingService.TakeDailyLog(request);
        }

        [HttpPost("take-daily-logs")]
        public async Task<BaseResponse> TakeDailyLogs([FromBody] TakeDailyLogsRequest request)
        {
            request ??= new TakeDailyLogsRequest();
            request.UserId = GetCurrentUserId();
            return await _dailyTrackingService.TakeDailyLogs(request);
        }

        [HttpPost("upsert-daily-log")]
        public async Task<BaseResponse> UpsertDailyLog([FromBody] UpsertDailyLogRequest request)
        {
            request ??= new UpsertDailyLogRequest();
            request.UserId = GetCurrentUserId();
            return await _dailyTrackingService.UpsertDailyLog(request);
        }

        [HttpPost("upsert-daily-metric")]
        public async Task<BaseResponse> UpsertDailyMetric([FromBody] UpsertDailyMetricRequest request)
        {
            request ??= new UpsertDailyMetricRequest();
            request.UserId = GetCurrentUserId();
            return await _dailyTrackingService.UpsertDailyMetric(request);
        }

        [HttpPost("set-daily-log-symptoms")]
        public async Task<BaseResponse> SetDailyLogSymptoms([FromBody] SetDailyLogSymptomsRequest request)
        {
            request ??= new SetDailyLogSymptomsRequest();
            request.UserId = GetCurrentUserId();
            return await _dailyTrackingService.SetDailyLogSymptoms(request);
        }
    }
}
