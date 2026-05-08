using Microsoft.Playwright;
using OSPWebScraper.Models;

namespace OSPWebScraper.Services;

public class DraftScraperService
{
    private readonly LogService _logService;
    private readonly IPage _page;
    private readonly IBrowserContext _context;

    public DraftScraperService(LogService logService, IPage page, IBrowserContext context)
    {
        _logService = logService;
        _page = page;
        _context = context;
    }

    public async Task NavegarParaRequisicoesEps()
    {
        _logService.Info("🔍 Navegando para Lista Requisições EPS...");
        
        await _page.WaitForSelectorAsync("a[href='/ospcontrol/requisicoes-eps']", new PageWaitForSelectorOptions { Timeout = 15000 });
        await _page.ClickAsync("a[href='/ospcontrol/requisicoes-eps']");
        await _page.WaitForURLAsync("**/requisicoes-eps**", new PageWaitForURLOptions { Timeout = 15000 });
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForSelectorAsync("#filtroId", new PageWaitForSelectorOptions { Timeout = 10000 });
        
        _logService.Info("✅ Página de requisições EPS carregada!");
    }

    public async Task<List<DraftResult>> ProcessarDrafts(List<string> ids, IProgress<(int percentual, string mensagem)>? progress = null)
    {
        var resultados = new List<DraftResult>();
        int total = ids.Count;
        int processados = 0;

        foreach (var id in ids)
        {
            processados++;
            int percentual = (processados * 100) / total;
            progress?.Report((percentual, $"Processando ID {id} ({processados}/{total})"));

            var resultado = new DraftResult { Id = id, DataExtracao = DateTime.Now };

            try
            {
                await _page.Locator("#filtroId").ClearAsync();
                await _page.FillAsync("#filtroId", id);
                await Task.Delay(500);
                await _page.ClickAsync("a.btn-primary:has-text('Buscar')");
                await Task.Delay(3000);
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                bool encontrado = await VerificarRegistroEncontrado(id);
                
                if (encontrado)
                {
                    resultado.Sucesso = true;
                    resultado.DadosExtraidos = await ExtrairDadosResultado();
                    resultado.Status = "SUCESSO";
                    _logService.Info($"✅ ID {id} processado com sucesso!");
                }
                else
                {
                    resultado.Sucesso = false;
                    resultado.Erro = "ID não encontrado no sistema";
                    resultado.Status = "NAO_ENCONTRADO";
                    _logService.Info($"⚠️ ID {id} não encontrado!");
                }
            }
            catch (Exception ex)
            {
                resultado.Sucesso = false;
                resultado.Erro = ex.Message;
                resultado.Status = "ERRO";
                _logService.Info($"❌ Erro no ID {id}: {ex.Message}");
            }

            resultados.Add(resultado);
            await Task.Delay(1000);
        }

        return resultados;
    }

    private async Task<bool> VerificarRegistroEncontrado(string id)
    {
        try
        {
            var tabela = await _page.QuerySelectorAsync("table tbody tr");
            return tabela != null;
        }
        catch
        {
            return false;
        }
    }

    private async Task<Dictionary<string, string>> ExtrairDadosResultado()
    {
        var dados = new Dictionary<string, string>();
        
        try
        {
            var tabela = await _page.QuerySelectorAsync("table");
            if (tabela != null)
            {
                var cabecalhos = await tabela.QuerySelectorAllAsync("thead th");
                var celulas = await tabela.QuerySelectorAllAsync("tbody tr:first-child td");
                
                for (int i = 0; i < celulas.Count && i < cabecalhos.Count; i++)
                {
                    var chave = await cabecalhos[i].TextContentAsync();
                    var valor = await celulas[i].TextContentAsync();
                    if (!string.IsNullOrWhiteSpace(chave))
                    {
                        dados[chave.Trim()] = valor?.Trim() ?? "";
                    }
                }
            }
            
            dados["DataHoraExtracao"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
        catch (Exception ex)
        {
            dados["ErroExtracao"] = ex.Message;
        }
        
        return dados;
    }
}