using Microsoft.Playwright;

namespace OSPWebScraper.Services;

public class LoginService
{
    private readonly LogService logService;

    public LoginService(
        LogService logService)
    {
        this.logService = logService;
    }

    public async Task<bool> VerificarSessaoAtiva(
        IPage page)
    {
        await page.WaitForTimeoutAsync(5000);

        string url =
            page.Url.ToLower();

        // =====================================
        // LOGIN
        // =====================================

        if (url.Contains(
                "autenticaint.vivo.com.br"))
        {
            return false;
        }

        // =====================================
        // SISTEMA
        // =====================================

        if (url.Contains("/ospcontrol"))
        {
            return true;
        }

        return false;
    }

    public async Task RealizarLogin(
        IPage page,
        string usuario,
        string senha)
    {
        logService.Info(
            "Preenchendo login...");

        await InserirMensagem(
            page,
            "Aguarde o robô vai preencher login e senha");

        await DigitarHumano(
            page,
            "#username",
            usuario);

        logService.Info(
            "Preenchendo senha...");

        await DigitarHumano(
            page,
            "#password",
            senha);

        logService.Info(
            "Aguardando captcha manual...");

        await InserirMensagem(
            page,
            "Preencha os caracteres do captcha manualmente");

        // =====================================
        // AGUARDAR LOGIN
        // =====================================

        while (true)
        {
            await page.WaitForTimeoutAsync(
                3000);

            string url =
                page.Url.ToLower();

            // LOGIN OK

            if (url.Contains(
                    "/ospcontrol"))
            {
                logService.Info(
                    "Login realizado com sucesso.");

                break;
            }

            // CAPTCHA INVÁLIDO

            bool erro =
                await page.Locator(
                    "p.msg").IsVisibleAsync();

            if (erro)
            {
                string texto =
                    await page.Locator(
                        "p.msg")
                        .InnerTextAsync();

                if (texto.Contains(
                        "Acesso inválido"))
                {
                    logService.Info(
                        "Captcha inválido. Tentando novamente...");

                    await page.ReloadAsync();

                    await page.WaitForTimeoutAsync(
                        3000);

                    await DigitarHumano(
                        page,
                        "#username",
                        usuario);

                    await DigitarHumano(
                        page,
                        "#password",
                        senha);

                    await InserirMensagem(
                        page,
                        "Captcha inválido. Digite novamente.");
                }
            }
        }
    }

    private async Task DigitarHumano(
        IPage page,
        string selector,
        string valor)
    {
        await page.ClickAsync(selector);

        await page.FillAsync(selector, "");

        foreach (char letra in valor)
        {
            await page.Keyboard.TypeAsync(
                letra.ToString());

            await page.WaitForTimeoutAsync(
                Random.Shared.Next(80, 180));
        }
    }

    private async Task InserirMensagem(
        IPage page,
        string mensagem)
    {
        await page.EvaluateAsync(
            @$"
            (() => {{

                let div =
                    document.getElementById(
                        'osp-helper');

                if(!div)
                {{
                    div =
                        document.createElement(
                            'div');

                    div.id = 'osp-helper';

                    div.style.position = 'fixed';
                    div.style.top = '20px';
                    div.style.right = '20px';
                    div.style.zIndex = '999999';
                    div.style.background = '#111';
                    div.style.color = '#fff';
                    div.style.padding = '15px';
                    div.style.borderRadius = '10px';
                    div.style.fontSize = '16px';
                    div.style.fontFamily =
                        'Arial';
                }}

                div.innerText =
                    '{mensagem}';

                document.body.appendChild(div);

            }})()
            ");
    }
}