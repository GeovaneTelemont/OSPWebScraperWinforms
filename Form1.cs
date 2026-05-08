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

    private string? caminhoCsv;

    private readonly ConfigService configService = new();
    private readonly LogService logService = new();
    
    
    public Form1()
    {
        InitializeComponent();

        logService.OnLog += AdicionarLog;

        ConfigurarJanela();

        CriarInterface();

        CriarRodape();


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

        //Icon = new Icon("Assets/ico_osp.ico");

        // Carrega o ícone de forma segura
         // Usa o ícone do próprio executável (já está embutido)
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
            // Se falhar, continua sem ícone (não quebra o app)
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

        // =========================
        // USUÁRIO
        // =========================

        var lblUsuario = new Label();

        lblUsuario.Text = "Usuário:";

        lblUsuario.Location = new Point(20, 40);

        lblUsuario.AutoSize = true;

        grupo.Controls.Add(lblUsuario);

        txtUsuario = new TextBox();

        txtUsuario.Size = new Size(800, 30);

        txtUsuario.Location = new Point(120, 35);

        grupo.Controls.Add(txtUsuario);

        // =========================
        // SENHA
        // =========================

        var lblSenha = new Label();

        lblSenha.Text = "Senha:";

        lblSenha.Location = new Point(20, 80);

        lblSenha.AutoSize = true;

        grupo.Controls.Add(lblSenha);

        txtSenha = new TextBox();

        txtSenha.Size = new Size(760, 30);

        txtSenha.Location = new Point(120, 75);

        txtSenha.UseSystemPasswordChar = true;

        grupo.Controls.Add(txtSenha);

        // =========================
        // BOTÃO OLHO
        // =========================

        btnMostrarSenha = new Button();

        btnMostrarSenha.Text = "👁";

        btnMostrarSenha.Size = new Size(40, 30);

        btnMostrarSenha.Location = new Point(890, 75);

        btnMostrarSenha.Click += BtnMostrarSenha_Click;

        grupo.Controls.Add(btnMostrarSenha);

        // =========================
        // CHECKBOX
        // =========================

        chkHeadless = new CheckBox();

        chkHeadless.Text = "Modo oculto";

        chkHeadless.Location = new Point(20, 120);

        chkHeadless.AutoSize = true;

        grupo.Controls.Add(chkHeadless);

        // =========================
        // BOTÃO SALVAR
        // =========================

        var btnSalvar = new Button();

        btnSalvar.Text = "💾 Salvar Credenciais";

        btnSalvar.Size = new Size(300, 35);

        btnSalvar.Location = new Point(320, 115);

        btnSalvar.Click += BtnSalvar_Click;

        grupo.Controls.Add(btnSalvar);
    }

    private void BtnMostrarSenha_Click(object? sender, EventArgs e)
    {
        txtSenha.UseSystemPasswordChar =
            !txtSenha.UseSystemPasswordChar;

        btnMostrarSenha.Text =
            txtSenha.UseSystemPasswordChar
            ? "👁"
            : "🙈";
    }

    private void CriarCsv()
    {
        var grupo = new GroupBox();

        grupo.Text = "📁 Arquivo CSV";

        grupo.Size = new Size(950, 140);

        grupo.Location = new Point(20, 300);

        Controls.Add(grupo);

        // =====================================
        // LABEL ARQUIVO
        // =====================================

        lblArquivo = new Label();

        lblArquivo.Text =
            "Nenhum arquivo selecionado";

        lblArquivo.Location =
            new Point(20, 40);

        lblArquivo.Size =
            new Size(850, 25);

        grupo.Controls.Add(lblArquivo);

        // =====================================
        // BOTÃO SELECIONAR
        // =====================================

        btnSelecionarCsv = new Button();

        btnSelecionarCsv.Text =
            "📂 Selecionar CSV";

        btnSelecionarCsv.Size =
            new Size(300, 40);

        btnSelecionarCsv.Location =
            new Point(20, 80);

        btnSelecionarCsv.Click +=
            BtnSelecionarCsv_Click;

        grupo.Controls.Add(
            btnSelecionarCsv);

        // =====================================
        // BOTÃO LIMPAR
        // =====================================

        btnLimparCsv = new Button();

        btnLimparCsv.Text =
            "🗑 Limpar";

        btnLimparCsv.Size =
            new Size(300, 40);

        btnLimparCsv.Location =
            new Point(350, 80);

        btnLimparCsv.Click +=
            BtnLimparCsv_Click;

        grupo.Controls.Add(
            btnLimparCsv);
    }
        

    private void CriarModoExtracao()
    {
        var grupo = new GroupBox();

        grupo.Text = "🎯 Modo de Extração";

        grupo.Size = new Size(950, 180);

        grupo.Location = new Point(20, 460);

        Controls.Add(grupo);

        // =====================================
        // DRAFT
        // =====================================

        var rbDraft = new RadioButton();

        rbDraft.Text = "Draft";

        rbDraft.Location =
            new Point(30, 40);

        rbDraft.AutoSize = true;

        rbDraft.Checked = true;

        grupo.Controls.Add(rbDraft);

        // =====================================
        // MEDIÇÃO
        // =====================================

        var rbMedicao = new RadioButton();

        rbMedicao.Text = "Medição";

        rbMedicao.Location =
            new Point(30, 75);

        rbMedicao.AutoSize = true;

        grupo.Controls.Add(rbMedicao);

        // =====================================
        // ID CANCELADOS
        // =====================================

        var rbCancelados = new RadioButton();

        rbCancelados.Text = "ID Cancelados";

        rbCancelados.Location =
            new Point(30, 110);

        rbCancelados.AutoSize = true;

        grupo.Controls.Add(rbCancelados);

        // =====================================
        // MEMÓRIA DE CÁLCULO
        // =====================================

        var rbMemoria = new RadioButton();

        rbMemoria.Text =
            "Memória de Cálculo";

        rbMemoria.Location =
            new Point(30, 145);

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

        // =====================================
        // TEXTAREA LOGS
        // =====================================

        txtLogs = new RichTextBox();

        txtLogs.Size =
            new Size(900, 140);

        txtLogs.Location =
            new Point(20, 35);

        // BLOQUEAR DIGITAÇÃO

        txtLogs.ReadOnly = true;

        txtLogs.BackColor = Color.White;

        txtLogs.BorderStyle =
            BorderStyle.FixedSingle;

        grupo.Controls.Add(txtLogs);

        // =====================================
        // BOTÃO LIMPAR LOGS
        // =====================================

        var btnLimparLogs =
            new Button();

        btnLimparLogs.Text =
            "🗑 Limpar Logs";

        btnLimparLogs.Size =
            new Size(250, 40);

        btnLimparLogs.Location =
            new Point(20, 185);

        btnLimparLogs.Click +=
            BtnLimparLogs_Click;

        grupo.Controls.Add(
            btnLimparLogs);

        // =====================================
        // BOTÃO BAIXAR LOGS
        // =====================================

        var btnBaixarLogs =
            new Button();

        btnBaixarLogs.Text =
            "💾 Baixar Logs";

        btnBaixarLogs.Size =
            new Size(250, 40);

        btnBaixarLogs.Location =
            new Point(290, 185);

        btnBaixarLogs.Click +=
            BtnBaixarLogs_Click;

        grupo.Controls.Add(
            btnBaixarLogs);
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

        MessageBox.Show(
            "Configurações salvas com sucesso!",
            "Sucesso",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void CarregarConfiguracoes()
    {
        var config = configService.Carregar();

        txtUsuario.Text = config.Usuario;

        txtSenha.Text = config.Senha;

        chkHeadless.Checked = config.Headless;
    }

    private async void BtnIniciar_Click(
    object? sender,
        EventArgs e)
    {
        try
        {
            btnIniciar.Enabled = false;

            var playwright = new PlaywrightService(logService);
            await playwright.Iniciar();

            MessageBox.Show(
                "Navegador iniciado com sucesso!");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnIniciar.Enabled = true;
        }
    }

    public void AdicionarLog(string mensagem)
    {
        if (InvokeRequired)
        {
            Invoke(() => AdicionarLog(mensagem));
            return;
        }

        txtLogs.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] {mensagem}\n");

        txtLogs.ScrollToCaret();
    }

    private void BtnSelecionarCsv_Click(
    object? sender,
    EventArgs e)
    {
        using var dialog =
            new OpenFileDialog();

        dialog.Title =
            "Selecionar arquivo CSV";

        dialog.Filter =
            "Arquivo CSV (*.csv)|*.csv";

        dialog.Multiselect = false;

        if (dialog.ShowDialog()
            != DialogResult.OK)
        {
            return;
        }

        try
        {
            string caminho =
                dialog.FileName;

            // =====================================
            // VALIDAR CSV
            // =====================================

            string primeiraLinha =
                File.ReadLines(caminho)
                    .First();

            var colunas =
                primeiraLinha
                    .Split(',');

            bool possuiIds =
                colunas.Any(x =>
                    x.Trim()
                        .Equals(
                            "IDs",
                            StringComparison
                                .OrdinalIgnoreCase));

            if (!possuiIds)
            {
                MessageBox.Show(
                    "O CSV precisa conter a coluna IDs.",
                    "CSV inválido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                AdicionarLog(
                    "CSV inválido. Coluna IDs não encontrada.");

                return;
            }

            caminhoCsv = caminho;

            lblArquivo.Text =
                caminho;

            AdicionarLog(
                "CSV carregado com sucesso.");

            AdicionarLog(
                $"Arquivo: {caminho}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao ler CSV:\n{ex.Message}");

            AdicionarLog(
                $"Erro CSV: {ex.Message}");
        }
    }

    private void BtnLimparCsv_Click(
    object? sender,
    EventArgs e)
    {
        caminhoCsv = null;

        lblArquivo.Text =
            "Nenhum arquivo selecionado";

        AdicionarLog(
            "Arquivo CSV removido.");
    }

    private void BtnLimparLogs_Click(
    object? sender,
    EventArgs e)
    {
        txtLogs.Clear();

        AdicionarLog(
            "Logs limpos.");
    }

    private void BtnBaixarLogs_Click(
    object? sender,
    EventArgs e)
    {
        using var dialog =
            new SaveFileDialog();

        dialog.Title =
            "Salvar logs";

        dialog.Filter =
            "Arquivo TXT (*.txt)|*.txt";

        dialog.FileName =
            $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

        if (dialog.ShowDialog()
            != DialogResult.OK)
        {
            return;
        }

        try
        {
            File.WriteAllText(
                dialog.FileName,
                txtLogs.Text);

            MessageBox.Show(
                "Logs salvos com sucesso.",
                "Sucesso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            AdicionarLog(
                "Logs exportados para TXT.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao salvar logs:\n{ex.Message}");

            AdicionarLog(
                $"Erro ao exportar logs: {ex.Message}");
        }
    }

    private void CriarRodape()
    {
        btnIniciar = new Button();

        btnIniciar.Text =
            "🚀 Iniciar Scraping";

        btnIniciar.BackColor =
            Color.FromArgb(40, 167, 69);

        btnIniciar.ForeColor =
            Color.White;

        btnIniciar.FlatStyle =
            FlatStyle.Flat;

        btnIniciar.Size =
            new Size(450, 50);

        btnIniciar.Location =
            new Point(30, 1040);

        Controls.Add(btnIniciar);

        // EVENTO CLICK

        btnIniciar.Click += BtnIniciar_Click;

        var btnParar =
            new Button();

        btnParar.Text =
            "🛑 Parar";

        btnParar.BackColor =
            Color.FromArgb(220, 53, 69);

        btnParar.ForeColor =
            Color.White;

        btnParar.FlatStyle =
            FlatStyle.Flat;

        btnParar.Size =
            new Size(450, 50);

        btnParar.Location =
            new Point(500, 1040);


        //EVENTO CLICK
        btnParar.Click += BtnParar_Click;

        Controls.Add(btnParar);

        // var btnLimpar =
        //     new Button();

        // btnLimpar.Text =
        //     "🗑 Limpar";

        // btnLimpar.Size =
        //     new Size(300, 50);

        // btnLimpar.Location =
        //     new Point(660, 920);

        // Controls.Add(btnLimpar);
    }

    private void BtnParar_Click(
    object? sender,
    EventArgs e)
    {
        AdicionarLog(
            "Processo parado.");
    }

    private void CriarProgresso()
    {
        var grupo = new GroupBox();

        grupo.Text = "📊 Progresso";

        grupo.Size = new Size(950, 100);

        grupo.Location = new Point(20, 660);

        Controls.Add(grupo);

        // =====================================
        // LABEL STATUS
        // =====================================

        lblProgresso = new Label();

        lblProgresso.Text =
            "Aguardando início...";

        lblProgresso.Location =
            new Point(20, 30);

        lblProgresso.AutoSize = true;

        grupo.Controls.Add(lblProgresso);

        // =====================================
        // BARRA
        // =====================================

        barraProgresso = new ProgressBar();

        barraProgresso.Location =
            new Point(20, 55);

        barraProgresso.Size =
            new Size(900, 25);

        barraProgresso.Minimum = 0;

        barraProgresso.Maximum = 100;

        barraProgresso.Value = 0;

        grupo.Controls.Add(barraProgresso);
    }

    public void AtualizarProgresso(
    int valor,
    string texto)
    {
        if (InvokeRequired)
        {
            Invoke(() =>
                AtualizarProgresso(
                    valor,
                    texto));

            return;
        }

        barraProgresso.Value = valor;

        lblProgresso.Text = texto;
    }
}