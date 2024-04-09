using RacerBooksAPI.Interfaces;

namespace RacerBooksAPI.Services
{
    public class UnsuccessfulLoginLogger:IUnsuccessfulLoginLogger
    {
        private readonly string _logFilePath;

        public UnsuccessfulLoginLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public async Task LogUnsuccessfulLoginAttemptAsync(string email, string errorDescription)
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Email: {email}, Error: {errorDescription}{Environment.NewLine}";

            await File.AppendAllTextAsync(_logFilePath, logEntry);
        }
    }
}
