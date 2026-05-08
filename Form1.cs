using System.Drawing;
using OSPWebScraper.Models;
using OSPWebScraper.Services;

namespace OSPWebScraper;

public partial class Form1 : Form
{   
    // =========================================
    // CAMPOS GLOBAIS
    // =========================================
    
    private TextBox txtUsuario = null!;
    private TextBox txtSenha = null!;
    private Button btnMostrarSenha = null!;
    private CheckBox chkHeadless = null!;
    private Button btnIniciar = null!;
    private RichTextBox txtLogs = null!;
    private Label lblArquivo = null!;
    private Button btnSelecionarCsv = null!;
    private Button btnLimparCsv = null!;
    private ProgressBar barraProgresso = null!;
    private Label lblProgresso = null!;
    private Button btnLimparLogs = null!;
    private Button btnBaixarLogs = null!;
    private Button btnParar = null!;

    private RadioButton rbDraft = null!;
    private RadioButton rbMedicao = null!;
    private RadioButton rbCancelados = null!;
    private RadioButton rbMemoria = null!;

    private CancellationTokenSource? cts;
    private PlaywrightService? _playwrightService;
    private string? caminhoCsv;
    private bool estaExecutando = false;
    
    private readonly ConfigService configService = new();
    private readonly LogService logService = new();
    
    public Form1()
    {
        InitializeComponent();

        logService.OnLog += AdicionarLog;

        ConfigurarJanela();

        CriarInterface();

        CriarRodape();

        AdicionarTooltips();

        CarregarConfiguracoes();
    }

    private void ConfigurarJanela()
    {
        Text = "OSP Vivo Web Scraper";
        Size = new Size(1000, 1160);

        StartPosition = FormStartPosition.CenterScreen;

        BackColor = Color.FromArgb(245, 245, 245);

        Font = new Font("Segoe UI", 10);

        AutoScroll = true;

        MaximizeBox = false;

        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // Se falhar, continua sem ícone
        }
    }

    private void CriarInterface()
    {
        CriarTitulo();
        CriarConfiguracoes();
        CriarCsv();
        CriarModoExtracao();
        CriarProgresso();
        CriarLogs();
    }

    private void CriarTitulo()
    {
        var titulo = new Label();
        titulo.Text = "🕸 OSP Vivo Web Scraper";
        titulo.Font = new Font("Segoe UI", 26, FontStyle.Bold);
        titulo.AutoSize = true;
        titulo.Location = new Point(250, 30);
        Controls.Add(titulo);

        var versao = new Label();
        versao.Text = "v1.0.0";
        versao.ForeColor = Color.Gray;
        versao.AutoSize = true;
        versao.Location = new Point(900, 40);
        Controls.Add(versao);
    }

    private void CriarConfiguracoes()
    {
        var grupo = new GroupBox();
        grupo.Text = "🔧 Configurações";
        grupo.Size = new Size(950, 180);
        grupo.Location = new Point(20, 100);
        Controls.Add(grupo);

        var lblUsuario = new Label();
        lblUsuario.Text = "Usuário:*";
        lblUsuario.Location = new Point(20, 40);
        lblUsuario.AutoSize = true;
        lblUsuario.ForeColor = Color.FromArgb(220, 53, 69);
        lblUsuario.Tag = "required";
        grupo.Controls.Add(lblUsuario);

        txtUsuario = new TextBox();
        txtUsuario.Size = new Size(800, 30);
        txtUsuario.Location = new Point(120, 35);
        txtUsuario.PlaceholderText = "Digite seu usuário";
        txtUsuario.TextChanged += (s, e) => AtualizarCorLabelObrigatorio(lblUsuario, txtUsuario);
        grupo.Controls.Add(txtUsuario);

        var lblSenha = new Label();
        lblSenha.Text = "Senha:*";
        lblSenha.Location = new Point(20, 80);
        lblSenha.AutoSize = true;
        lblSenha.ForeColor = Color.FromArgb(220, 53, 69);
        lblSenha.Tag = "required";
        grupo.Controls.Add(lblSenha);

        txtSenha = new TextBox();
        txtSenha.Size = new Size(760, 30);
        txtSenha.Location = new Point(120, 75);
        txtSenha.UseSystemPasswordChar = true;
        txtSenha.PlaceholderText = "Digite sua senha";
        txtSenha.TextChanged += (s, e) => AtualizarCorLabelObrigatorio(lblSenha, txtSenha);
        grupo.Controls.Add(txtSenha);

        btnMostrarSenha = new Button();
        btnMostrarSenha.Text = "👁";
        btnMostrarSenha.Size = new Size(40, 30);
        btnMostrarSenha.Location = new Point(890, 75);
        btnMostrarSenha.Click += BtnMostrarSenha_Click;
        grupo.Controls.Add(btnMostrarSenha);

        chkHeadless = new CheckBox();
        chkHeadless.Text = "Modo oculto";
        chkHeadless.Location = new Point(20, 120);
        chkHeadless.AutoSize = true;
        grupo.Controls.Add(chkHeadless);

        var btnSalvar = new Button();
        btnSalvar.Text = "💾 Salvar Credenciais";
        btnSalvar.Size = new Size(300, 35);
        btnSalvar.Location = new Point(320, 115);
        btnSalvar.Click += BtnSalvar_Click;
        grupo.Controls.Add(btnSalvar);
    }

    private void BtnMostrarSenha_Click(object? sender, EventArgs e)
    {
        txtSenha.UseSystemPasswordChar = !txtSenha.UseSystemPasswordChar;
        btnMostrarSenha.Text = txtSenha.UseSystemPasswordChar ? "👁" : "🙈";
    }

    private void CriarCsv()
    {
        var grupo = new GroupBox();
        grupo.Text = "📁 Arquivo CSV";
        grupo.Size = new Size(950, 140);
        grupo.Location = new Point(20, 300);
        Controls.Add(grupo);

        lblArquivo = new Label();
        lblArquivo.Text = "Nenhum arquivo selecionado";
        lblArquivo.Location = new Point(20, 40);
        lblArquivo.Size = new Size(850, 25);
        grupo.Controls.Add(lblArquivo);

        btnSelecionarCsv = new Button();
        btnSelecionarCsv.Text = "📂 Selecionar CSV";
        btnSelecionarCsv.Size = new Size(300, 40);
        btnSelecionarCsv.Location = new Point(20, 80);
        btnSelecionarCsv.Click += BtnSelecionarCsv_Click;
        grupo.Controls.Add(btnSelecionarCsv);

        btnLimparCsv = new Button();
        btnLimparCsv.Text = "🗑 Limpar";
        btnLimparCsv.Size = new Size(300, 40);
        btnLimparCsv.Location = new Point(350, 80);
        btnLimparCsv.Click += BtnLimparCsv_Click;
        grupo.Controls.Add(btnLimparCsv);
    }

    private void CriarModoExtracao()
    {
        var grupo = new GroupBox();
        grupo.Text = "🎯 Modo de Extração";
        grupo.Size = new Size(950, 180);
        grupo.Location = new Point(20, 460);
        Controls.Add(grupo);

        rbDraft = new RadioButton();
        rbDraft.Text = "Draft";
        rbDraft.Location = new Point(30, 40);
        rbDraft.AutoSize = true;
        rbDraft.Checked = true;
        grupo.Controls.Add(rbDraft);

        rbMedicao = new RadioButton();
        rbMedicao.Text = "Medição";
        rbMedicao.Location = new Point(30, 75);
        rbMedicao.AutoSize = true;
        grupo.Controls.Add(rbMedicao);

        rbCancelados = new RadioButton();
        rbCancelados.Text = "ID Cancelados";
        rbCancelados.Location = new Point(30, 110);
        rbCancelados.AutoSize = true;
        grupo.Controls.Add(rbCancelados);

        rbMemoria = new RadioButton();
        rbMemoria.Text = "Memória de Cálculo";
        rbMemoria.Location = new Point(30, 145);
        rbMemoria.AutoSize = true;
        grupo.Controls.Add(rbMemoria);
    }

    private void CriarLogs()
    {
        var grupo = new GroupBox();
        grupo.Text = "📝 Logs";
        grupo.Size = new Size(950, 240);
        grupo.Location = new Point(20, 780);
        Controls.Add(grupo);

        txtLogs = new RichTextBox();
        txtLogs.Size = new Size(900, 140);
        txtLogs.Location = new Point(20, 35);
        txtLogs.ReadOnly = true;
        txtLogs.BackColor = Color.White;
        txtLogs.BorderStyle = BorderStyle.FixedSingle;
        grupo.Controls.Add(txtLogs);

        btnLimparLogs = new Button();
        btnLimparLogs.Text = "🗑 Limpar Logs";
        btnLimparLogs.Size = new Size(250, 40);
        btnLimparLogs.Location = new Point(20, 185);
        btnLimparLogs.Click += BtnLimparLogs_Click;
        grupo.Controls.Add(btnLimparLogs);

        btnBaixarLogs = new Button();
        btnBaixarLogs.Text = "💾 Baixar Logs";
        btnBaixarLogs.Size = new Size(250, 40);
        btnBaixarLogs.Location = new Point(290, 185);
        btnBaixarLogs.Click += BtnBaixarLogs_Click;
        grupo.Controls.Add(btnBaixarLogs);
    }

    private void CriarRodape()
    {
        btnIniciar = new Button();
        btnIniciar.Text = "🚀 Iniciar Scraping";
        btnIniciar.BackColor = Color.FromArgb(200, 200, 200);
        btnIniciar.ForeColor = Color.FromArgb(120, 120, 120);
        btnIniciar.FlatStyle = FlatStyle.Flat;
        btnIniciar.Size = new Size(450, 50);
        btnIniciar.Location = new Point(30, 1040);
        btnIniciar.Enabled = false;
        btnIniciar.FlatAppearance.BorderSize = 0;
        Controls.Add(btnIniciar);
        btnIniciar.Click += BtnIniciar_Click;

        btnParar = new Button();
        btnParar.Text = "🛑 Parar";
        btnParar.BackColor = Color.FromArgb(220, 53, 69);
        btnParar.ForeColor = Color.White;
        btnParar.FlatStyle = FlatStyle.Flat;
        btnParar.Size = new Size(450, 50);
        btnParar.Location = new Point(500, 1040);
        btnParar.Enabled = false;
        btnParar.Click += BtnParar_Click;
        Controls.Add(btnParar);
    }

    private void CriarProgresso()
    {
        var grupo = new GroupBox();
        grupo.Text = "📊 Progresso";
        grupo.Size = new Size(950, 100);
        grupo.Location = new Point(20, 660);
        Controls.Add(grupo);

        lblProgresso = new Label();
        lblProgresso.Text = "Aguardando início...";
        lblProgresso.Location = new Point(20, 30);
        lblProgresso.AutoSize = true;
        grupo.Controls.Add(lblProgresso);

        barraProgresso = new ProgressBar();
        barraProgresso.Location = new Point(20, 55);
        barraProgresso.Size = new Size(900, 25);
        barraProgresso.Minimum = 0;
        barraProgresso.Maximum = 100;
        barraProgresso.Value = 0;
        grupo.Controls.Add(barraProgresso);
    }

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        var config = new AppSettings
        {
            Usuario = txtUsuario.Text,
            Senha = txtSenha.Text,
            Headless = chkHeadless.Checked
        };

        configService.Salvar(config);

        MessageBox.Show("Configurações salvas com sucesso!", "Sucesso",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CarregarConfiguracoes()
    {
        var config = configService.Carregar();
        txtUsuario.Text = config.Usuario;
        txtSenha.Text = config.Senha;
        chkHeadless.Checked = config.Headless;
    }

    private async void BtnIniciar_Click(object? sender, EventArgs e)
{
    // Verificações iniciais
    if (!btnIniciar.Enabled)
    {
        AdicionarLog("⚠️ Selecione um arquivo CSV válido primeiro.");
        return;
    }
    
    if (estaExecutando)
    {
        AdicionarLog("⚠️ Já existe um processo em execução.");
        return;
    }
    
    if (!ValidarCredenciais())
    {
        return;
    }
    
    if (string.IsNullOrEmpty(caminhoCsv) || !File.Exists(caminhoCsv))
    {
        MessageBox.Show("Arquivo CSV não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        AtualizarAparenciaBotaoIniciar(false);
        return;
    }
    
    cts = new CancellationTokenSource();
    
    try
    {
        estaExecutando = true;
        SetUIControlsEnabled(false);
        
        AdicionarLog("🚀 Iniciando scraping...");
        AdicionarLog($"👤 Usuário: {txtUsuario.Text}");
        AdicionarLog($"👻 Modo headless: {(chkHeadless.Checked ? "Ativado" : "Desativado")}");
        
        // Inicializa o PlaywrightService
        _playwrightService = new PlaywrightService(logService);
        await _playwrightService.Inicializar(chkHeadless.Checked);
        
        // Verifica se o login foi bem sucedido
        if (!_playwrightService.IsLoggedIn)
        {
            throw new Exception("Não foi possível realizar login. Verifique suas credenciais.");
        }
        
        // Aguarda um pouco para a página estabilizar
        await Task.Delay(2000);
        
        // Executa o scraping
        await ExecutarScraping(cts.Token);
        
        if (!cts.Token.IsCancellationRequested)
        {
            MessageBox.Show("Scraping concluído com sucesso!", "Sucesso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    catch (OperationCanceledException)
    {
        AdicionarLog("⚠️ Processo cancelado pelo usuário.");
        MessageBox.Show("Scraping cancelado!", "Cancelado",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    catch (Exception ex)
    {
        AdicionarLog($"❌ Erro: {ex.Message}");
        MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        estaExecutando = false;
        
        if (_playwrightService != null)
        {
            try
            {
                await _playwrightService.Fechar();
            }
            catch (Exception ex)
            {
                AdicionarLog($"⚠️ Erro ao fechar navegador: {ex.Message}");
            }
            finally
            {
                _playwrightService = null;
            }
        }
        
        SetUIControlsEnabled(true);
        AtualizarProgresso(0, "Aguardando início...");
        
        cts?.Dispose();
        cts = null;
        
        AdicionarLog("🏁 Processo finalizado.");
    }
}
    private async Task ExecutarScraping(CancellationToken cancellationToken)
    {
        try
        {
            // Lê os IDs do CSV
            var linhas = File.ReadAllLines(caminhoCsv!);
            var ids = new List<string>();
            
            for (int i = 1; i < linhas.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(linhas[i]))
                    continue;
                    
                var colunas = linhas[i].Split(',');
                if (colunas.Length > 0)
                {
                    var id = colunas[0].Trim();
                    if (!string.IsNullOrEmpty(id))
                    {
                        ids.Add(id);
                    }
                }
            }
            
            if (ids.Count == 0)
            {
                throw new Exception("Nenhum ID encontrado no arquivo CSV.");
            }
            
            AdicionarLog($"📊 Total de IDs a processar: {ids.Count}");
            
            var modoExtracao = GetModoExtracaoSelecionado();
            AdicionarLog($"🎯 Modo de extração selecionado: {modoExtracao}");
            
            switch (modoExtracao)
            {
                case "Draft":
                    AdicionarLog("🚀 Iniciando extração no modo DRAFT...");
                    await ExecutarScrapingDraft(ids, cancellationToken);
                    break;
                    
                case "Medição":
                    AdicionarLog("📏 Iniciando extração no modo MEDIÇÃO...");
                    await ExecutarScrapingMedicao(ids, cancellationToken);
                    break;
                    
                case "ID Cancelados":
                    AdicionarLog("❌ Iniciando extração no modo ID CANCELADOS...");
                    await ExecutarScrapingIdCancelados(ids, cancellationToken);
                    break;
                    
                case "Memória de Cálculo":
                    AdicionarLog("🧮 Iniciando extração no modo MEMÓRIA DE CÁLCULO...");
                    await ExecutarScrapingMemoriaCalculo(ids, cancellationToken);
                    break;
                    
                default:
                    throw new Exception($"Modo de extração '{modoExtracao}' não é válido.");
            }
        }
        catch (OperationCanceledException)
        {
            AdicionarLog("⚠️ Processo cancelado.");
            throw;
        }
        catch (Exception ex)
        {
            AdicionarLog($"❌ Erro na execução do scraping: {ex.Message}");
            throw;
        }
    }

    private async Task ExecutarScrapingDraft(List<string> ids, CancellationToken cancellationToken)
    {
        if (_playwrightService == null)
        {
            throw new Exception("PlaywrightService não foi inicializado.");
        }
        
        var draftScraper = new DraftScraperService(logService, _playwrightService.Page, _playwrightService.Context);
        
        try
        {
            AtualizarProgresso(5, "Preparando para modo Draft...");
            cancellationToken.ThrowIfCancellationRequested();
            
            AtualizarProgresso(10, "Navegando para página de requisições EPS...");
            await draftScraper.NavegarParaRequisicoesEps();
            
            cancellationToken.ThrowIfCancellationRequested();
            
            AtualizarProgresso(15, "Iniciando extração dos drafts...");
            
            var progress = new Progress<(int percentual, string mensagem)>(p =>
            {
                AtualizarProgresso(15 + (p.percentual * 75 / 100), p.mensagem);
            });
            
            var resultados = await draftScraper.ProcessarDrafts(ids, progress);
            
            cancellationToken.ThrowIfCancellationRequested();
            
            AtualizarProgresso(95, "Salvando resultados...");
            
            var caminhoResultado = Path.Combine(
                Path.GetDirectoryName(caminhoCsv!)!,
                $"resultados_draft_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            );
            
            await SalvarResultadosDraft(resultados, caminhoResultado);
            
            AtualizarProgresso(100, "Processamento concluído!");
            
            int sucessos = resultados.Count(r => r.Sucesso);
            int erros = resultados.Count(r => !r.Sucesso && r.Status != "NAO_ENCONTRADO");
            int naoEncontrados = resultados.Count(r => r.Status == "NAO_ENCONTRADO");
            
            AdicionarLog($"📊 RESUMO FINAL - MODO DRAFT:");
            AdicionarLog($"   ✅ Sucessos: {sucessos}");
            AdicionarLog($"   ❌ Erros: {erros}");
            AdicionarLog($"   ⚠️ Não encontrados: {naoEncontrados}");
            
            MessageBox.Show(
                $"Extrações de DRAFT concluídas!\n\n✅ Sucessos: {sucessos}\n❌ Erros: {erros}\n⚠️ IDs não encontrados: {naoEncontrados}\n\nResultados salvos em:\n{caminhoResultado}",
                "Processamento Finalizado - Modo Draft",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AdicionarLog($"❌ Erro no modo Draft: {ex.Message}");
            throw;
        }
    }

    private async Task ExecutarScrapingMedicao(List<string> ids, CancellationToken cancellationToken)
    {
        AdicionarLog("⚠️ Modo MEDIÇÃO será implementado em breve.");
        MessageBox.Show("Modo 'Medição' em desenvolvimento.", "Em breve",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        await Task.CompletedTask;
    }

    private async Task ExecutarScrapingIdCancelados(List<string> ids, CancellationToken cancellationToken)
    {
        AdicionarLog("⚠️ Modo ID CANCELADOS será implementado em breve.");
        MessageBox.Show("Modo 'ID Cancelados' em desenvolvimento.", "Em breve",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        await Task.CompletedTask;
    }

    private async Task ExecutarScrapingMemoriaCalculo(List<string> ids, CancellationToken cancellationToken)
    {
        AdicionarLog("⚠️ Modo MEMÓRIA DE CÁLCULO será implementado em breve.");
        MessageBox.Show("Modo 'Memória de Cálculo' em desenvolvimento.", "Em breve",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        await Task.CompletedTask;
    }

    private async Task SalvarResultadosDraft(List<DraftResult> resultados, string caminho)
    {
        try
        {
            using var writer = new StreamWriter(caminho, false, System.Text.Encoding.UTF8);
            
            var todosCampos = new HashSet<string>();
            foreach (var resultado in resultados.Where(r => r.Sucesso))
            {
                foreach (var campo in resultado.DadosExtraidos.Keys)
                {
                    todosCampos.Add(campo);
                }
            }
            
            var cabecalho = new List<string> { "ID", "Status", "DataExtracao", "Erro" };
            cabecalho.AddRange(todosCampos.OrderBy(c => c));
            await writer.WriteLineAsync(string.Join(";", cabecalho));
            
            foreach (var resultado in resultados)
            {
                var linha = new List<string>
                {
                    resultado.Id,
                    resultado.Status,
                    resultado.DataExtracao.ToString("dd/MM/yyyy HH:mm:ss"),
                    resultado.Erro ?? ""
                };
                
                foreach (var campo in todosCampos.OrderBy(c => c))
                {
                    if (resultado.Sucesso && resultado.DadosExtraidos.ContainsKey(campo))
                    {
                        var valor = resultado.DadosExtraidos[campo];
                        linha.Add(valor?.Replace(";", ",").Replace("\n", " ").Replace("\r", "") ?? "");
                    }
                    else
                    {
                        linha.Add("");
                    }
                }
                
                await writer.WriteLineAsync(string.Join(";", linha));
            }
            
            AdicionarLog($"✅ Resultados salvos: {resultados.Count} registros");
        }
        catch (Exception ex)
        {
            AdicionarLog($"❌ Erro ao salvar: {ex.Message}");
            throw;
        }
    }

    public void AdicionarLog(string mensagem)
    {
        if (InvokeRequired)
        {
            Invoke(() => AdicionarLog(mensagem));
            return;
        }

        txtLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {mensagem}\n");
        txtLogs.ScrollToCaret();
    }

    public void AtualizarProgresso(int valor, string texto)
    {
        if (InvokeRequired)
        {
            Invoke(() => AtualizarProgresso(valor, texto));
            return;
        }

        barraProgresso.Value = valor;
        lblProgresso.Text = texto;
    }

    private void BtnSelecionarCsv_Click(object? sender, EventArgs e)
    {
        if (estaExecutando)
        {
            AdicionarLog("⚠️ Aguarde o término do processo atual.");
            return;
        }
        
        using var dialog = new OpenFileDialog();
        dialog.Title = "Selecionar arquivo CSV";
        dialog.Filter = "Arquivo CSV (*.csv)|*.csv";
        dialog.Multiselect = false;

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            string caminho = dialog.FileName;
            string primeiraLinha = File.ReadLines(caminho).First();
            var colunas = primeiraLinha.Split(',');
            
            bool possuiIds = colunas.Any(x => x.Trim().Equals("IDs", StringComparison.OrdinalIgnoreCase));

            if (!possuiIds)
            {
                MessageBox.Show("O CSV precisa conter a coluna IDs.", "CSV inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                AdicionarLog("CSV inválido. Coluna IDs não encontrada.");
                AtualizarAparenciaBotaoIniciar(false);
                return;
            }

            caminhoCsv = caminho;
            lblArquivo.Text = caminho;
            AdicionarLog("CSV carregado com sucesso.");
            
            int totalLinhas = File.ReadLines(caminho).Count() - 1;
            AdicionarLog($"Total de linhas: {totalLinhas} IDs encontrados");
            
            AtualizarAparenciaBotaoIniciar(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao ler CSV:\n{ex.Message}");
            AdicionarLog($"Erro CSV: {ex.Message}");
            AtualizarAparenciaBotaoIniciar(false);
        }
    }

    private void BtnLimparCsv_Click(object? sender, EventArgs e)
    {
        if (estaExecutando)
        {
            AdicionarLog("⚠️ Não é possível limpar o CSV durante a execução.");
            return;
        }
        
        caminhoCsv = null;
        lblArquivo.Text = "Nenhum arquivo selecionado";
        AdicionarLog("Arquivo CSV removido.");
        AtualizarAparenciaBotaoIniciar(false);
    }

    private void BtnLimparLogs_Click(object? sender, EventArgs e)
    {
        txtLogs.Clear();
        AdicionarLog("Logs limpos.");
    }

    private void BtnBaixarLogs_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog();
        dialog.Title = "Salvar logs";
        dialog.Filter = "Arquivo TXT (*.txt)|*.txt";
        dialog.FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            File.WriteAllText(dialog.FileName, txtLogs.Text);
            MessageBox.Show("Logs salvos com sucesso.", "Sucesso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            AdicionarLog("Logs exportados para TXT.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar logs:\n{ex.Message}");
            AdicionarLog($"Erro ao exportar logs: {ex.Message}");
        }
    }

    private void BtnParar_Click(object? sender, EventArgs e)
    {
        if (estaExecutando && cts != null)
        {
            AdicionarLog("🛑 Solicitando parada do processo...");
            cts.Cancel();
            btnParar.Enabled = false;
            btnParar.Text = "⏹️ Parando...";
        }
        else
        {
            AdicionarLog("Nenhum processo em execução para parar.");
        }
    }

    private void SetUIControlsEnabled(bool habilitado)
    {
        btnSelecionarCsv.Enabled = habilitado;
        btnLimparCsv.Enabled = habilitado;
        btnLimparLogs.Enabled = habilitado;
        btnBaixarLogs.Enabled = habilitado;
        txtUsuario.Enabled = habilitado;
        txtSenha.Enabled = habilitado;
        btnMostrarSenha.Enabled = habilitado;
        chkHeadless.Enabled = habilitado;
        
        btnParar.Enabled = !habilitado;
        
        if (habilitado && !estaExecutando)
        {
            btnIniciar.Enabled = !string.IsNullOrEmpty(caminhoCsv) && File.Exists(caminhoCsv);
            btnIniciar.BackColor = btnIniciar.Enabled ? Color.FromArgb(40, 167, 69) : Color.FromArgb(200, 200, 200);
            btnIniciar.Text = btnIniciar.Enabled ? "🚀 Iniciar Scraping" : "⏳ Aguardando CSV...";
        }
        else if (!habilitado)
        {
            btnIniciar.Enabled = false;
            btnIniciar.BackColor = Color.FromArgb(200, 200, 200);
            btnIniciar.Text = "🔄 Processando...";
        }
    }

    private void AtualizarAparenciaBotaoIniciar(bool habilitado)
    {
        if (InvokeRequired)
        {
            Invoke(() => AtualizarAparenciaBotaoIniciar(habilitado));
            return;
        }
        
        if (!estaExecutando)
        {
            btnIniciar.Enabled = habilitado;
            btnIniciar.BackColor = habilitado ? Color.FromArgb(40, 167, 69) : Color.FromArgb(200, 200, 200);
            btnIniciar.ForeColor = habilitado ? Color.White : Color.FromArgb(120, 120, 120);
            btnIniciar.Text = habilitado ? "🚀 Iniciar Scraping" : "⏳ Aguardando CSV...";
        }
    }

    private bool ValidarCredenciais()
    {
        if (string.IsNullOrWhiteSpace(txtUsuario.Text))
        {
            MessageBox.Show("Usuário é obrigatório.", "Campos Obrigatórios",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            AdicionarLog("❌ Campo USUÁRIO está vazio.");
            txtUsuario.Focus();
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            MessageBox.Show("Senha é obrigatória.", "Campos Obrigatórios",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            AdicionarLog("❌ Campo SENHA está vazio.");
            txtSenha.Focus();
            return false;
        }
        
        AdicionarLog("✅ Credenciais validadas.");
        return true;
    }

    private void AdicionarTooltips()
    {
        ToolTip toolTip = new ToolTip();
        toolTip.SetToolTip(txtUsuario, "Campo obrigatório - Digite seu usuário");
        toolTip.SetToolTip(txtSenha, "Campo obrigatório - Digite sua senha");
        toolTip.SetToolTip(btnIniciar, "Preencha usuário, senha e selecione um CSV válido");
    }

    private void AtualizarCorLabelObrigatorio(Label label, TextBox textBox)
    {
        label.ForeColor = string.IsNullOrWhiteSpace(textBox.Text) 
            ? Color.FromArgb(220, 53, 69) 
            : Color.FromArgb(120, 120, 120);
    }

    private string GetModoExtracaoSelecionado()
    {
        if (rbDraft.Checked) return "Draft";
        if (rbMedicao.Checked) return "Medição";
        if (rbCancelados.Checked) return "ID Cancelados";
        if (rbMemoria.Checked) return "Memória de Cálculo";
        return "Draft";
    }
}