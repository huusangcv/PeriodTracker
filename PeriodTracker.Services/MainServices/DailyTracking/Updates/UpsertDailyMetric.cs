using Microsoft.EntityFrameworkCore;
using PeriodTracker.ApiGateways.DailyTracking;
using PeriodTracker.ApiGateways.Network;
using PeriodTracker.Entities.PeriodTracker;

namespace PeriodTracker.Services.MainServices
{
    public partial class DailyTrackingService
    {
        public async Task<BaseResponse> UpsertDailyMetric(UpsertDailyMetricRequest request)
        {
            var result = new BaseResponse();
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    result.Message = "Invalid request.";
                    return result;
                }

                var normalizedMetricType = request.MetricType?.Trim() ?? string.Empty;

                if (!AllowedMetricTypes.Contains(normalizedMetricType))
                {
                    result.Message = "MetricType is invalid.";
                    return result;
                }

                if (request.DecimalValue <= 0)
                {
                    result.Message = "DecimalValue must be greater than zero.";
                    return result;
                }

                if (!string.IsNullOrWhiteSpace(request.Unit) && request.Unit.Length > 20)
                {
                    result.Message = "Unit length must be less than or equal to 20.";
                    return result;
                }

                if (!string.IsNullOrWhiteSpace(request.Note) && request.Note.Length > 300)
                {
                    result.Message = "Note length must be less than or equal to 300.";
                    return result;
                }

                var userExists = await _dataContext.Users.AnyAsync(x => x.Id == request.UserId && !x.Deleted);
                if (!userExists)
                {
                    result.Message = "User not found.";
                    return result;
                }

                var measuredDate = NormalizeDate(request.MeasuredDate);
                if (!IsBusinessDate(measuredDate))
                {
                    result.Message = "MeasuredDate is invalid.";
                    return result;
                }

                var metric = await _dataContext.DailyMetrics
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId
                                              && x.MeasuredDate == measuredDate
                                              && x.MetricType == normalizedMetricType);

                if (metric == null)
                {
                    metric = new DailyMetric
                    {
                        UserId = request.UserId,
                        MeasuredDate = measuredDate,
                        MetricType = normalizedMetricType,
                        DecimalValue = request.DecimalValue,
                        Unit = request.Unit,
                        Note = request.Note
                    };
                    _dataContext.DailyMetrics.Add(metric);
                }
                else
                {
                    metric.DecimalValue = request.DecimalValue;
                    metric.Unit = request.Unit;
                    metric.Note = request.Note;
                }

                await _dataContext.SaveChangesAsync();

                result.Success = true;
                result.Data = new
                {
                    metric.Id,
                    metric.UserId,
                    metric.MeasuredDate,
                    metric.MetricType,
                    metric.DecimalValue,
                    metric.Unit,
                    metric.Note
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
