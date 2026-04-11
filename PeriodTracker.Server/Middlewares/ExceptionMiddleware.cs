using Microsoft.AspNetCore.Diagnostics;
using PeriodTracker.ApiGateways.Network;
using PeriodTracker.Entities;
using PeriodTracker.Entities.Framework;
using System.Net;

namespace Phoenix.Hemisphere.Server.Middlewares
{
    public static class ExceptionMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        using (var scope = app.ApplicationServices.CreateAsyncScope())
                        {
                            var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                            await dataContext.Logs.AddAsync(new Log
                            {
                                LogLevel = "Error",
                                CreatedAt = DateTime.Now,
                                Message = contextFeature.Error.Message,
                                Exception = contextFeature.Error.ToString()
                            });
                            await dataContext.SaveChangesAsync();
                        }
                        await context.Response.WriteAsync(new BaseResponse()
                        {
                            Success = context.Response.StatusCode == 200,
                            Message = "Internal Server Error. Please check the logs for details. SeeDu © " + DateTime.Now.Year
                        }.ToString());
                    }
                });
            });
        }
    }
}
