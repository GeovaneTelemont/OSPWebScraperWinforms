using System.Text.Json;
using OSPWebScraper.Models;

namespace OSPWebScraper.Services;

public class ConfigService
{
    private readonly string pastaConfig;

    private readonly string arquivoConfig;

    public ConfigService()
    {
        pastaConfig = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".osp_web_scraper");

        arquivoConfig = Path.Combine(
            pastaConfig,
            "config.json");
    }

    public void Salvar(AppSettings config)
    {
        if (!Directory.Exists(pastaConfig))
        {
            Directory.CreateDirectory(pastaConfig);
        }

        var json = JsonSerializer.Serialize(
            config,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(arquivoConfig, json);
    }

    public AppSettings Carregar()
    {
        if (!File.Exists(arquivoConfig))
        {
            return new AppSettings();
        }

        var json = File.ReadAllText(arquivoConfig);

        return JsonSerializer.Deserialize<AppSettings>(json)
               ?? new AppSettings();
    }
}