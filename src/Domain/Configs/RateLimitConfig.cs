namespace Domain.Configs
{
    public class RateLimitConfig
    {
        public string BypassToken { get; set; }
        public string BypassHeaderName { get; set; }

        // Default rate limiting settings
        public int DefaultTokenLimit { get; set; }
        public int DefaultTokensPerPeriod { get; set; }
        public int DefaultReplenishmentPeriodSeconds { get; set; }

        // Bypass rate limiting settings
        public int BypassTokenLimit { get; set; }
        public int BypassTokensPerPeriod { get; set; }
        public int BypassReplenishmentPeriodSeconds { get; set; }
    }
}
