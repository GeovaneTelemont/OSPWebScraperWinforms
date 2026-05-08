using Microsoft.Playwright;

namespace OSPWebScraper.Services;

public class PlaywrightService
{
    private readonly LogService _logService;
    private readonly ConfigService _configService;
    private readonly LoginService _loginService;
    private readonly string _arquivoSessao;
    
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private bool _isLoggedIn;

    private const string Url = "https://devopsredes.vivo.com.br/ospcontrol/home";

    public IPage Page => _page ?? throw new Exception("Navegador não inicializado");
    public IBrowserContext Context => _context ?? throw new Exception("Contexto não inicializado");
    public bool IsLoggedIn => _isLoggedIn;

    public PlaywrightService(LogService logService)
    {
        _logService = logService;
        _configService = new ConfigService();
        _loginService = new LoginService(logService);
        var pastaConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".osp_web_scraper");
        _arquivoSessao = Path.Combine(pastaConfig, "sessao.json");
    }

    public async Task Inicializar(bool headless)
    {
        try
        {
            _logService.Info("Iniciando navegador...");
            
            var config = _configService.Carregar();
            bool possuiSessao = File.Exists(_arquivoSessao);
            
            // Se não tem sessão e está em modo headless, faz login manual primeiro
            if (!possuiSessao && headless)
            {
                _logService.Info("Primeira execução em modo oculto. Abrindo navegador visível para login...");
                await FazerLoginManual(config.Usuario, config.Senha);
            }

            // Cria o Playwright
            _playwright = await Playwright.CreateAsync();
            
            // Abre o navegador
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Channel = "chrome",
                Args = new[] 
                { 
                    "--ignore-certificate-errors", 
                    "--allow-insecure-localhost", 
                    "--ignore-ssl-errors=yes"
                }
            });

            // Configura o contexto
            var options = new BrowserNewContextOptions 
            { 
                IgnoreHTTPSErrors = true,
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
            };
            
            if (File.Exists(_arquivoSessao))
            {
                _logService.Info("Carregando sessão salva...");
                options.StorageStatePath = _arquivoSessao;
            }

            _context = await _browser.NewContextAsync(options);
            _page = await _context.NewPageAsync();
            _page.SetDefaultTimeout(120000);

            _logService.Info("Acessando portal OSP...");
            await _page.GotoAsync(Url, new PageGotoOptions
            {
                Timeout = 90000,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            // Verifica sessão
            bool sessaoAtiva = await _loginService.VerificarSessaoAtiva(_page);
            
            if (sessaoAtiva)
            {
                _logService.Info("Sessão ativa encontrada.");
                _isLoggedIn = true;
            }
            else if (!headless)
            {
                _logService.Info("Sessão inválida. Realizando login...");
                await _loginService.RealizarLogin(_page, config.Usuario, config.Senha);
                await SalvarSessao();
                _isLoggedIn = true;
            }
            else
            {
                _logService.Info("⚠️ Sessão inválida e modo headless ativo. Execute em modo visível primeiro.");
            }
            
            _logService.Info("✅ Navegador pronto!");
        }
        catch (Exception ex)
        {
            _logService.Info($"❌ Erro ao inicializar: {ex.Message}");
            throw;
        }
    }

    private async Task FazerLoginManual(string usuario, string senha)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            Channel = "chrome"
        });

        try
        {
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            
            _logService.Info("Acessando portal para login manual...");
            await page.GotoAsync(Url);
            
            _logService.Info("⚠️ Por favor, faça o login manualmente no navegador que abriu.");
            _logService.Info("Após fazer o login, pressione ENTER neste terminal para continuar...");
            
            // Aguarda o usuário pressionar ENTER
            await Task.Run(() => Console.ReadLine());
            
            // Salva a sessão
            var pasta = Path.GetDirectoryName(_arquivoSessao)!;
            if (!Directory.Exists(pasta)) 
                Directory.CreateDirectory(pasta);
            
            await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = _arquivoSessao });
            _logService.Info("✅ Sessão salva com sucesso!");
        }
        finally
        {
            await browser.CloseAsync();
            playwright.Dispose();
        }
    }

    private async Task SalvarSessao(IBrowserContext? context = null)
    {
        var ctx = context ?? _context;
        if (ctx == null) return;
        
        var pasta = Path.GetDirectoryName(_arquivoSessao)!;
        if (!Directory.Exists(pasta)) 
            Directory.CreateDirectory(pasta);
        
        await ctx.StorageStateAsync(new BrowserContextStorageStateOptions { Path = _arquivoSessao });
        _logService.Info("Sessão salva!");
    }

    public async Task Fechar()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _logService.Info("Navegador fechado.");
        }
        
        _playwright?.Dispose();
    }
}