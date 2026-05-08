namespace OSPWebScraper.Services;

public class LogService
{
    public event Action<string>? OnLog;

    public void Info(string mensagem)
    {
        OnLog?.Invoke(mensagem);
    }
}