using Microsoft.Extensions.DependencyInjection;
using PeriodTracker.Services.Framework;
using PeriodTracker.Services.MainServices;

namespace PeriodTracker.Services.Infrastructure
{
    public class DependencyRegister
    {
        public static void Register(IServiceCollection services)
        {
            #region [Framework]
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDailyTrackingService, DailyTrackingService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ISettingService, SettingService>();
            #endregion
        }
    }
}
