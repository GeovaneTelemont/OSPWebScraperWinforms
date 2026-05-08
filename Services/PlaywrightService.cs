using Microsoft.Playwright;

namespace OSPWebScraper.Services;

public class PlaywrightService
{
    private readonly ConfigService configService;

    private readonly LoginService loginService;

    private readonly LogService logService;

    private readonly string pastaConfig;

    private readonly string arquivoSessao;

    private const string Url =
        "https://devopsredes.vivo.com.br/ospcontrol/home";

    public PlaywrightService(
        LogService logService)
    {
        this.logService = logService;

        configService =
            new ConfigService();

        loginService =
            new LoginService(logService);

        pastaConfig = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder
                    .UserProfile),
            ".osp_web_scraper");

        arquivoSessao = Path.Combine(
            pastaConfig,
            "sessao.json");
    }

    public async Task Iniciar()
    {
        try
        {
            logService.Info(
                "Iniciando navegador...");

            var config =
                configService.Carregar();

            bool headless =
                config.Headless;

            bool possuiSessao =
                File.Exists(
                    arquivoSessao);

            using var playwright =
                await Playwright.CreateAsync();

            // =====================================
            // SEM SESSÃO + HEADLESS TRUE
            // LOGIN VISÍVEL
            // =====================================

            if (!possuiSessao &&
                headless)
            {
                logService.Info(
                    "Sem sessão salva.");

                logService.Info(
                    "Abrindo login visível...");

                await FazerLoginVisivel(
                    playwright,
                    config.Usuario,
                    config.Senha);
            }

            // =====================================
            // ABRIR NAVEGADOR PRINCIPAL
            // =====================================

            logService.Info(
                headless
                ? "Abrindo navegador em modo oculto..."
                : "Abrindo navegador visível...");

            IBrowser browser =
                await playwright.Chromium
                    .LaunchAsync(
                        new BrowserTypeLaunchOptions
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

            BrowserNewContextOptions options =
                new()
                {
                    IgnoreHTTPSErrors = true
                };

            // =====================================
            // CARREGAR STORAGE
            // =====================================

            if (File.Exists(
                    arquivoSessao))
            {
                logService.Info(
                    "Carregando sessão salva...");

                options.StorageStatePath =
                    arquivoSessao;
            }

            var context =
                await browser.NewContextAsync(
                    options);

            var page =
                await context.NewPageAsync();

            page.SetDefaultTimeout(
                60000);

            logService.Info(
                "Acessando portal OSP...");

            await page.GotoAsync(
                Url,
                new PageGotoOptions
                {
                    Timeout = 60000,

                    WaitUntil =
                        WaitUntilState
                            .DOMContentLoaded
                });

            // =====================================
            // VERIFICAR SESSÃO
            // =====================================

            logService.Info(
                "Verificando sessão...");

            bool sessaoAtiva =
                await loginService
                    .VerificarSessaoAtiva(
                        page);

            // =====================================
            // SESSÃO ATIVA
            // =====================================

            if (sessaoAtiva)
            {
                logService.Info(
                    "Sessão ativa encontrada.");
            }

            // =====================================
            // SEM LOGIN
            // =====================================

            if (!sessaoAtiva)
            {
                logService.Info(
                    "Sessão inválida.");

                // =====================================
                // HEADLESS FALSE
                // LOGIN DIRETO
                // =====================================

                if (!headless)
                {
                    logService.Info(
                        "Executando login...");

                    await loginService
                        .RealizarLogin(
                            page,
                            config.Usuario,
                            config.Senha);

                    await SalvarSessao(
                        context);
                }
            }

            logService.Info(
                "Playwright iniciado com sucesso.");

            // =====================================
            // MANTER PROCESSO
            // =====================================

            await page.WaitForTimeoutAsync(
                99999999);

            await browser.CloseAsync();
        }
        catch (Exception ex)
        {
            logService.Info(
                $"Erro: {ex.Message}");
        }
    }

    // =========================================
    // LOGIN VISÍVEL
    // =========================================

    private async Task FazerLoginVisivel(
        IPlaywright playwright,
        string usuario,
        string senha)
    {
        logService.Info(
            "Abrindo navegador para login manual...");

        IBrowser browser =
            await playwright.Chromium
                .LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = false,

                        Channel = "chrome",

                        Args = new[]
                        {
                            "--ignore-certificate-errors",
                            "--allow-insecure-localhost",
                            "--ignore-ssl-errors=yes"
                        }
                    });

        var context =
            await browser.NewContextAsync(
                new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true
                });

        var page =
            await context.NewPageAsync();

        page.SetDefaultTimeout(
            60000);

        logService.Info(
            "Abrindo tela de login...");

        await page.GotoAsync(
            Url,
            new PageGotoOptions
            {
                Timeout = 60000,

                WaitUntil =
                    WaitUntilState
                        .DOMContentLoaded
            });

        logService.Info(
            "Preenchendo credenciais...");

        await loginService.RealizarLogin(
            page,
            usuario,
            senha);

        logService.Info(
            "Salvando sessão...");

        await SalvarSessao(
            context);

        logService.Info(
            "Sessão salva com sucesso.");

        await browser.CloseAsync();
    }

    // =========================================
    // SALVAR STORAGE
    // =========================================

    private async Task SalvarSessao(
        IBrowserContext context)
    {
        if (!Directory.Exists(
                pastaConfig))
        {
            Directory.CreateDirectory(
                pastaConfig);
        }

        await context.StorageStateAsync(
            new BrowserContextStorageStateOptions
            {
                Path = arquivoSessao
            });

        logService.Info(
            "Arquivo sessao.json atualizado.");
    }
}