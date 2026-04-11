using PeriodTracker.Entities;
using PeriodTracker.Entities.Framework;

namespace PeriodTracker.Services.Framework
{
    public interface ILogService
    {
        void Debug(string shortMessage, string longMessage);
        void Error(string shortMessage, string longMessage);
        void Info(string message);
        void Error(Exception exception);
    }

    public class LogService(DataContext _dataContext) : ILogService
    {
        public void Info(string message)
        {
            _dataContext.Logs.Add(new Log
            {
                LogLevel = "Info",
                CreatedAt = DateTime.Now,
                Message = message,
            });
            _dataContext.SaveChanges();
        }
        public void Debug(string shortMessage, string longMessage)
        {
            _dataContext.Logs.Add(new Log
            {
                LogLevel = "Debug",
                CreatedAt = DateTime.Now,
                Message = shortMessage,
                Exception = longMessage
            });
            _dataContext.SaveChanges();
        }
        public void Error(Exception exception)
        {
            _dataContext.Logs.Add(new Log
            {
                LogLevel = "Error",
                CreatedAt = DateTime.Now,
                Message = exception.Message,
                Exception = exception.ToString()
            });
            _dataContext.SaveChanges();
        }
        public void Error(string shortMessage, string longMessage)
        {
            _dataContext.Logs.Add(new Log
            {
                LogLevel = "Error",
                CreatedAt = DateTime.Now,
                Message = shortMessage,
                Exception = longMessage
            });
            _dataContext.SaveChanges();
        }
    }
}
