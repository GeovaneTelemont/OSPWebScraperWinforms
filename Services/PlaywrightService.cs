using Microsoft.Playwright;

namespace OSPWebScraper.Services;

public class PlaywrightService
{
    private readonly LogService _logService;
    private readonly string _arquivoSessao;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public IPage Page => _page ?? throw new Exception("Navegador não inicializado");
    public IBrowserContext Context => _context ?? throw new Exception("Contexto não inicializado");

    private const string Url = "https://devopsredes.vivo.com.br/ospcontrol/home";

    public PlaywrightService(LogService logService)
    {
        _logService = logService;
        var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osp_web_scraper");
        _arquivoSessao = Path.Combine(pasta, "sessao.json");
    }

    public async Task Inicializar(bool headless)
    {
        using var playwright = await Playwright.CreateAsync();
        
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless,
            Channel = "chrome",
            Args = new[] { "--ignore-certificate-errors", "--ignore-ssl-errors=yes" }
        });

        var options = new BrowserNewContextOptions { IgnoreHTTPSErrors = true };
        if (File.Exists(_arquivoSessao))
            options.StorageStatePath = _arquivoSessao;

        _context = await _browser.NewContextAsync(options);
        _page = await _context.NewPageAsync();
        await _page.GotoAsync(Url);
        
        _logService.Info("✅ Navegador inicializado");
    }

    public async Task SalvarSessao()
    {
        if (_context == null) return;
        var pasta = Path.GetDirectoryName(_arquivoSessao)!;
        if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);
        await _context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = _arquivoSessao });
        _logService.Info("✅ Sessão salva");
    }

    public async Task Fechar()
    {
        if (_browser != null)
            await _browser.CloseAsync();
    }
}