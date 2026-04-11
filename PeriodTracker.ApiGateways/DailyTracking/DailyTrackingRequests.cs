namespace PeriodTracker.ApiGateways.DailyTracking
{
    public class TakeDailyLogRequest
    {
        public int UserId { get; set; }
        public DateTime LogDate { get; set; }
    }

    public class TakeDailyLogsRequest
    {
        public int UserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class UpsertDailyLogRequest
    {
        public int UserId { get; set; }
        public DateTime LogDate { get; set; }
        public string? FlowLevel { get; set; }
        public string? Mood { get; set; }
        public string? Note { get; set; }
    }

    public class UpsertDailyMetricRequest
    {
        public int UserId { get; set; }
        public DateTime MeasuredDate { get; set; }
        public string MetricType { get; set; } = string.Empty;
        public decimal DecimalValue { get; set; }
        public string? Unit { get; set; }
        public string? Note { get; set; }
    }

    public class SetDailyLogSymptomsRequest
    {
        public int UserId { get; set; }
        public DateTime LogDate { get; set; }
        public List<DailyLogSymptomRequest> Symptoms { get; set; } = [];
    }

    public class DailyLogSymptomRequest
    {
        public int SymptomId { get; set; }
        public byte? Severity { get; set; }
        public string? Note { get; set; }
    }

    public class TakeSymptomsRequest
    {
        public bool OnlyActive { get; set; } = true;
    }
}
