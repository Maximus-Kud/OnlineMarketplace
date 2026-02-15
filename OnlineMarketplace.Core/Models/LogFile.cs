using OnlineMarketplace.Roles;

namespace OnlineMarketplace.Models
{
    public class LogFile
    {
        public string Date { get; set; } = string.Empty;
        public enum LogLevels
        {
            DEBUG,
            INFO,
            WARNING,
            ERROR,
            CRITICAL
        }
        public LogLevels LogLevel { get; set; }
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public string Role { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
