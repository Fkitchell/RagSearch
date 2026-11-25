namespace RagSearch.Api.RateLimiting
{
    public class RateLimitOptions
    {
        internal const string RateLimitSection = "RateLimit";
        internal const string ConcurrencyPolicy = "Concurrency";

        public int PermitLimit { get; set; }
        public int QueueLimit { get; set; }
    }
}
