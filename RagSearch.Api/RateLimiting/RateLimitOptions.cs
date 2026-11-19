namespace RagSearch.Api.RateLimiting
{
    public class RateLimitOptions
    {
        internal static readonly string RateLimitSection = "RateLimit";
        internal readonly int PermitLimit;
        internal readonly int QueueLimit;
    }
}
