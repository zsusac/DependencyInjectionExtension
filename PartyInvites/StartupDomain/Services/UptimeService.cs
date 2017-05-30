using System.Diagnostics;

namespace PartyInvites.StartupDomain.Services
{
    public class UptimeService : IUptimeService
    {
        private Stopwatch _timer;

        public UptimeService()
        {
            _timer = Stopwatch.StartNew();
        }

        public long Uptime => _timer.ElapsedMilliseconds;
    }
}
