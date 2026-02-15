using System.Collections.Concurrent;

namespace OnlineMarketplace.Models
{
    public class LogService
    {
        private readonly ConcurrentBag<LogFile> _logs = new();

        public IEnumerable<LogFile> Logs => _logs;

        public virtual void Add(LogFile log)
        {
            _logs.Add(log);
        }
    }
}
