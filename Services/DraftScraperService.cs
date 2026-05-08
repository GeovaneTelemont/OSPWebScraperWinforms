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

    /// <summary>
    /// Navega para a página de Lista Requisições EPS
    /// </summary>
    public async Task NavegarParaRequisicoesEps()
    {
        try
        {
            _logService.Info("🔍 Navegando para Lista Requisições EPS...");
            
            // Aguarda a página carregar
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2000);
            
            // 1. Expande o menu lateral se necessário
            _logService.Info("Expandindo menu lateral...");
            
            var selectorsMenu = new[]
            {
                "#ott-sidebar-collapse",
                ".oi-menu",
                "span.oi.oi-menu",
                "[class*='sidebar-toggle']"
            };
            
            bool menuExpandido = false;
            
            foreach (var selector in selectorsMenu)
            {
                try
                {
                    var elemento = await _page.QuerySelectorAsync(selector);
                    if (elemento != null && await elemento.IsVisibleAsync())
                    {
                        _logService.Info($"Clicando no menu usando: {selector}");
                        await elemento.ClickAsync();
                        await Task.Delay(1500);
                        menuExpandido = true;
                        break;
                    }
                }
                catch { }
            }
            
            if (!menuExpandido)
            {
                _logService.Info("Menu lateral já expandido ou não encontrado");
            }
            
            // 2. Clica no link
            _logService.Info("Aguardando link 'Lista Requisições EPS'...");
            
            var link = await _page.WaitForSelectorAsync("a[href='/ospcontrol/requisicoes-eps']", 
                new PageWaitForSelectorOptions { Timeout = 15000, State = WaitForSelectorState.Visible });
            
            if (link == null)
            {
                throw new Exception("Link 'Lista Requisições EPS' não encontrado");
            }
            
            await link.ClickAsync();
            
            // 3. Aguarda a página carregar
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2000);
            
            // 4. Aguarda o campo de filtro
            await _page.WaitForSelectorAsync("#filtroId", new PageWaitForSelectorOptions { Timeout = 10000 });
            
            _logService.Info("✅ Página de requisições EPS carregada!");
        }
        catch (Exception ex)
        {
            _logService.Info($"❌ Erro ao navegar: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Processa uma lista de IDs para extrair drafts
    /// </summary>
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
                _logService.Info($"📄 [ID: {id}] Iniciando processamento ({processados}/{total})...");
                
                // 1. Limpa o campo de busca
                await _page.Locator("#filtroId").ClearAsync();
                
                // 2. Insere o ID no campo de busca
                await _page.FillAsync("#filtroId", id);
                _logService.Info($"  ✓ ID {id} inserido no campo de busca");
                
                // 3. Clica no botão Buscar
                await _page.ClickAsync("a.btn-primary:has-text('Buscar')");
                _logService.Info($"  ✓ Botão Buscar clicado");
                
                // 4. Aguarda o resultado da busca
                await Task.Delay(3000);
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                // 5. Aguarda a tabela ou mensagem de resultado
                await Task.Delay(2000);
                
                // 6. Verifica se encontrou o registro
                bool encontrado = await VerificarRegistroEncontrado(id);
                
                if (encontrado)
                {
                    _logService.Info($"  ✓ Registro encontrado para ID {id}");
                    
                    // 7. Verifica se existe o botão Editar
                    bool botaoEditarEncontrado = await VerificarBotaoEditar();
                    
                    if (botaoEditarEncontrado)
                    {
                        _logService.Info($"  ✓ Botão Editar encontrado, clicando...");
                        await ClicarBotaoEditar();
                        
                        // Aguarda a página de edição carregar
                        await Task.Delay(3000);
                        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        
                        // Extrai os dados da página de edição
                        var dados = await ExtrairDadosEdicao();
                        resultado.DadosExtraidos = dados;
                        resultado.DadosExtraidos["StatusBotaoEditar"] = "ENCONTRADO";
                        
                        _logService.Info($"  ✓ Dados extraídos da página de edição: {dados.Count} campos");
                        
                        // Volta para a página de lista
                        await VoltarParaLista();
                    }
                    else
                    {
                        _logService.Info($"  ⚠️ Botão Editar NÃO encontrado para ID {id}");
                        resultado.DadosExtraidos = new Dictionary<string, string>();
                        resultado.DadosExtraidos["StatusBotaoEditar"] = "NAO_ENCONTRADO";
                    }
                    
                    resultado.Sucesso = true;
                    resultado.Status = "SUCESSO";
                    _logService.Info($"✅ [ID: {id}] Processado com sucesso!");
                }
                else
                {
                    _logService.Info($"  ⚠️ Nenhum registro encontrado para ID {id}");
                    resultado.Sucesso = false;
                    resultado.Erro = "ID não encontrado no sistema";
                    resultado.Status = "NAO_ENCONTRADO";
                    resultado.DadosExtraidos = new Dictionary<string, string>();
                    resultado.DadosExtraidos["StatusBusca"] = "NAO_ENCONTRADO";
                }
            }
            catch (Exception ex)
            {
                resultado.Sucesso = false;
                resultado.Erro = ex.Message;
                resultado.Status = "ERRO";
                _logService.Info($"❌ [ID: {id}] Erro: {ex.Message}");
            }

            resultados.Add(resultado);
            await Task.Delay(1000);
        }

        int sucessos = resultados.Count(r => r.Sucesso);
        int erros = resultados.Count(r => !r.Sucesso && r.Status != "NAO_ENCONTRADO");
        int naoEncontrados = resultados.Count(r => r.Status == "NAO_ENCONTRADO");
        
        _logService.Info($"🏁 Extração concluída! ✅ Sucessos: {sucessos}, ❌ Erros: {erros}, ⚠️ Não encontrados: {naoEncontrados}");
        
        return resultados;
    }

    /// <summary>
    /// Verifica se o registro foi encontrado na busca
    /// </summary>
    private async Task<bool> VerificarRegistroEncontrado(string id)
    {
        try
        {
            // Verifica se aparece mensagem de "Nenhum registro encontrado"
            var mensagemNaoEncontrado = await _page.QuerySelectorAsync(".alert-warning, .alert-info, .no-records");
            if (mensagemNaoEncontrado != null)
            {
                var texto = await mensagemNaoEncontrado.TextContentAsync();
                if (texto != null && (texto.Contains("Nenhum") || texto.Contains("nenhum") || texto.Contains("não encontrado")))
                {
                    return false;
                }
            }
            
            // Verifica se a tabela de resultados existe e tem linhas
            var tabela = await _page.QuerySelectorAsync("table tbody");
            if (tabela != null)
            {
                var linhas = await tabela.QuerySelectorAllAsync("tr");
                if (linhas.Count > 0)
                {
                    foreach (var linha in linhas)
                    {
                        var textoLinha = await linha.TextContentAsync();
                        if (textoLinha != null && textoLinha.Contains(id))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logService.Info($"⚠️ Erro ao verificar registro: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifica se o botão Editar está visível na página
    /// </summary>
    private async Task<bool> VerificarBotaoEditar()
    {
        try
        {
            var selectors = new[]
            {
                "span.badge.bg-primary:has-text('Editar')",
                "span.badge:has-text('Editar')",
                ".badge.bg-primary",
                "button:has-text('Editar')",
                "a:has-text('Editar')"
            };
            
            foreach (var selector in selectors)
            {
                try
                {
                    var elemento = await _page.QuerySelectorAsync(selector);
                    if (elemento != null && await elemento.IsVisibleAsync())
                    {
                        _logService.Info($"Botão Editar encontrado com seletor: {selector}");
                        return true;
                    }
                }
                catch { }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logService.Info($"Erro ao verificar botão Editar: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clica no botão Editar
    /// </summary>
    private async Task ClicarBotaoEditar()
    {
        try
        {
            var selectors = new[]
            {
                "span.badge.bg-primary:has-text('Editar')",
                "span.badge:has-text('Editar')",
                ".badge.bg-primary",
                "button:has-text('Editar')"
            };
            
            foreach (var selector in selectors)
            {
                try
                {
                    var elemento = await _page.QuerySelectorAsync(selector);
                    if (elemento != null && await elemento.IsVisibleAsync())
                    {
                        await elemento.ClickAsync();
                        _logService.Info($"Clicou no botão Editar via seletor: {selector}");
                        return;
                    }
                }
                catch { }
            }
            
            _logService.Info("Não foi possível clicar no botão Editar");
        }
        catch (Exception ex)
        {
            _logService.Info($"Erro ao clicar no botão Editar: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Extrai dados da página de edição/detalhes
    /// </summary>
    private async Task<Dictionary<string, string>> ExtrairDadosEdicao()
    {
        var dados = new Dictionary<string, string>();
        
        try
        {
            _logService.Info("Extraindo dados da página de edição...");
            
            // Extrai dados de campos de formulário
            var inputs = await _page.QuerySelectorAllAsync("input, select, textarea");
            foreach (var input in inputs)
            {
                try
                {
                    var id = await input.GetAttributeAsync("id");
                    var name = await input.GetAttributeAsync("name");
                    var label = id ?? name;
                    
                    if (!string.IsNullOrEmpty(label))
                    {
                        var valor = await input.GetAttributeAsync("value") ?? await input.TextContentAsync();
                        if (!string.IsNullOrEmpty(valor))
                        {
                            dados[$"Campo_{label}"] = valor.Trim();
                        }
                    }
                }
                catch { }
            }
            
            // Extrai dados de labels e valores
            var linhasInfo = await _page.QuerySelectorAllAsync(".form-group, .row, .info-row, .mb-3");
            foreach (var linha in linhasInfo)
            {
                var labels = await linha.QuerySelectorAllAsync("label, .label, strong, .col-form-label");
                var values = await linha.QuerySelectorAllAsync("span, .value, .form-control-static, .col-sm-10");
                
                for (int i = 0; i < labels.Count && i < values.Count; i++)
                {
                    var chave = await labels[i].TextContentAsync();
                    var valor = await values[i].TextContentAsync();
                    
                    if (!string.IsNullOrWhiteSpace(chave) && !string.IsNullOrWhiteSpace(valor))
                    {
                        dados[chave.Trim().Replace(":", "")] = valor.Trim();
                    }
                }
            }
            
            // Extrai dados de tabelas
            var tables = await _page.QuerySelectorAllAsync("table");
            for (int t = 0; t < tables.Count; t++)
            {
                var rows = await tables[t].QuerySelectorAllAsync("tr");
                foreach (var row in rows)
                {
                    var cells = await row.QuerySelectorAllAsync("td, th");
                    if (cells.Count >= 2)
                    {
                        var chave = await cells[0].TextContentAsync();
                        var valor = await cells[1].TextContentAsync();
                        if (!string.IsNullOrWhiteSpace(chave) && !string.IsNullOrWhiteSpace(valor))
                        {
                            dados[$"Tabela_{t+1}_{chave.Trim()}"] = valor.Trim();
                        }
                    }
                }
            }
            
            // Adiciona metadados
            dados["URL_Edicao"] = _page.Url;
            dados["DataHoraExtracao"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            
            _logService.Info($"✅ Extraídos {dados.Count} campos da página de edição");
        }
        catch (Exception ex)
        {
            _logService.Info($"⚠️ Erro ao extrair dados da edição: {ex.Message}");
            dados["ErroExtracao"] = ex.Message;
        }
        
        return dados;
    }

    /// <summary>
    /// Volta para a página de lista
    /// </summary>
    private async Task VoltarParaLista()
    {
        try
        {
            // Tenta fechar modal se existir
            var closeButton = await _page.QuerySelectorAsync(".close, .btn-close, button[aria-label='Close'], .modal-header .close");
            if (closeButton != null && await closeButton.IsVisibleAsync())
            {
                await closeButton.ClickAsync();
                await Task.Delay(1000);
                _logService.Info("Modal fechado, voltando para lista");
                return;
            }
            
            // Tenta voltar via botão de voltar
            var backButton = await _page.QuerySelectorAsync("a:has-text('Voltar'), button:has-text('Voltar'), .btn-secondary:has-text('Voltar')");
            if (backButton != null && await backButton.IsVisibleAsync())
            {
                await backButton.ClickAsync();
                await Task.Delay(1000);
                _logService.Info("Voltando para lista via botão Voltar");
                return;
            }
            
            // Se não encontrou botão, volta no navegador
            await _page.GoBackAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2000);
            _logService.Info("Voltando para lista via navegação");
            
            // Verifica se voltou para a página correta
            var filtroId = await _page.QuerySelectorAsync("#filtroId");
            if (filtroId == null)
            {
                _logService.Info("Página de lista não encontrada, navegando novamente...");
                await NavegarParaRequisicoesEps();
            }
        }
        catch (Exception ex)
        {
            _logService.Info($"⚠️ Erro ao voltar para lista: {ex.Message}");
            // Tenta navegar novamente
            try
            {
                await NavegarParaRequisicoesEps();
            }
            catch { }
        }
    }
}