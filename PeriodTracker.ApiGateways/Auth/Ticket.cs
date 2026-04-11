namespace PeriodTracker.ApiGateways.Auth
{
    public class Ticket
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserDisplayName { get; set; }
        public string? Role { get; set; }
    }
}
