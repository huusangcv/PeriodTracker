namespace PeriodTracker.ApiGateways.Network
{
    public class BaseRequest
    {
        public DateTime? FromDt { get; set; }
        public DateTime? ToDt { get; set; }
        public string? Term { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Sorter? Sorter { get; set; }
    }
    public class Sorter
    {
        public string? Field { get; set; }
        public string? Order { get; set; }

        public bool HasValue()
        {
            return !string.IsNullOrEmpty(Field) && !string.IsNullOrEmpty(Order);
        }
    }
}
