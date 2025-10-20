using System.Text.Json.Serialization;

namespace OtpProvider.Infrastructure.OtpSenders { 
    // Root response DTO
    public class SmsSendResponse
    {
        [JsonPropertyName("error")]
        public int Error { get; set; }

        [JsonPropertyName("msg")]
        public string Msg { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public SmsSendResponseData? Data { get; set; }

        // Convenience
        [JsonIgnore]
        public bool IsSuccess => Error == 0;
    }

    // Nested "data" object
    public class SmsSendResponseData
    {
        [JsonPropertyName("balance")]
        public string? Balance { get; set; }
    }
}