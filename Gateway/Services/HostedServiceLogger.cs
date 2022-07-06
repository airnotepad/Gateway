using Gateway.Helper;

namespace Gateway.Services
{
    public class HostedServiceLogger
    {
        protected readonly ILogger<HostedServiceLogger> _logger;
        private RWLock locker = new RWLock();
        private Dictionary<string, LogStack<string>> logs = new Dictionary<string, LogStack<string>>();
        private int maxSize = 200;

        public HostedServiceLogger(ILogger<HostedServiceLogger> logger)
        {
            _logger = logger;
        }

        public void NewLine(string hostedName)
        {
            using (locker.WriteLock())
                if (logs.ContainsKey(hostedName))
                    logs[hostedName].Push(Environment.NewLine);
                else
                {
                    logs.Add(hostedName, new LogStack<string>(maxSize));
                    logs[hostedName].Push(Environment.NewLine);
                }
        }

        public void Push(string hostedName, string log)
        {
            using (locker.WriteLock())
                if (logs.ContainsKey(hostedName))
                    logs[hostedName].Push(FormateLog(log));
                else
                {
                    logs.Add(hostedName, new LogStack<string>(maxSize));
                    logs[hostedName].Push(FormateLog(log));
                }
        }

        public List<string> GetLogs(string hostedName)
        {
            using (locker.ReadLock())
                if (logs.ContainsKey(hostedName))
                    return new List<string>(logs[hostedName]);
                else
                    return new List<string>();
        }

        public List<string> GetLoggedServices()
        {
            using (locker.ReadLock())
                return new List<string>(logs.Keys);
        }

        private string FormateLog(string text) => $"{Date.Now.ToString("HH:mm:ss")} {text}";
    }
}
