namespace OSPWebScraper.Models;

public class AppSettings
{
    public string Usuario { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;

    public bool Headless { get; set; }
}