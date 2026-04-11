using Newtonsoft.Json;

namespace PeriodTracker.ApiGateways.Network
{
    public class BaseResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = false;
        [JsonProperty("message")]
        public string Message { get; set; } = "";

        [JsonProperty("errorType")]
        public string? ErrorType { get; set; }

        [JsonProperty("data")]
        public object? Data { get; set; }
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
