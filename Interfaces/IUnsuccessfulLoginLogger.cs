namespace RacerBooksAPI.Interfaces
{
    public interface IUnsuccessfulLoginLogger
    {
        Task LogUnsuccessfulLoginAttemptAsync(string email, string errorDescription);
    }
}
