using Microsoft.EntityFrameworkCore;
using PeriodTracker.ApiGateways.DailyTracking;
using PeriodTracker.ApiGateways.Network;

namespace PeriodTracker.Services.MainServices
{
    public partial class DailyTrackingService
    {
        public async Task<BaseResponse> TakeSymptoms(TakeSymptomsRequest request)
        {
            var result = new BaseResponse();
            try
            {
                var query = _dataContext.Symptoms.AsNoTracking();
                if (request.OnlyActive)
                {
                    query = query.Where(x => x.IsActive);
                }

                result.Data = await query
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .Select(x => new
                    {
                        x.Id,
                        x.Code,
                        x.Name,
                        x.Category,
                        x.SortOrder,
                        x.IsActive
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
