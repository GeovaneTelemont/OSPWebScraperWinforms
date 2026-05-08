namespace OSPWebScraper.Models;

public class DraftResult
{
    public string Id { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Erro { get; set; }
    public DateTime DataExtracao { get; set; }
    public Dictionary<string, string> DadosExtraidos { get; set; } = new();
}
