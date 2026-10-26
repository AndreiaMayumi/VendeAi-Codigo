using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using VendeAi.Models;
using VendeAi.Repositories;
using VendeAi.Controllers;

namespace VendeAi;

public partial class CadastroProdutoForm : Form
{
    // ============================
    // CORES DO VENDEAÍ
    // ============================

    private readonly Color corPrimaria = Color.FromArgb(37, 74, 235);
    private readonly Color corPrimariaHover = Color.FromArgb(29, 60, 200);
    private readonly Color corMenu = Color.FromArgb(15, 30, 60);
    private readonly Color corMenuHover = Color.FromArgb(25, 45, 82);
    private readonly Color corFundo = Color.FromArgb(245, 247, 251);
    private readonly Color corTexto = Color.FromArgb(20, 30, 50);
    private readonly Color corTextoSecundario = Color.FromArgb(100, 110, 130);
    private readonly Color corBorda = Color.FromArgb(220, 225, 235);

    // ============================
    // CAMPOS
    // ============================

    private TextBox txtNome = null!;
    private TextBox txtSku = null!;
    private TextBox txtDescricao = null!;
    private TextBox txtMarca = null!;

    private ComboBox cmbCategoria = null!;
    private ComboBox cmbModalidade = null!;

    private NumericUpDown nudQuantidadeMinima = null!;
    private NumericUpDown nudPrecoVenda = null!;
    private NumericUpDown nudPrecoCusto = null!;

    private PictureBox picProduto = null!;
    private Label lblTextoImagem = null!;
    private Label lblPreviewNome = null!;
    private Label lblPreviewDetalhes = null!;
    private Label lblPreviewPreco = null!;
    private Label lblPreviewModalidade = null!;
    private PictureBox picPreview = null!;

    private Button btnSelecionarImagem = null!;
    private Button btnSalvar = null!;
    private Button btnCancelar = null!;
    private Button btnAdicionarVariacao = null!;


    private string? caminhoImagem;

    private readonly ProdutoRepository produtoRepository;
    private readonly ProdutoController produtoController;

    // ============================
    // CONSTRUTOR
    // ============================

    public CadastroProdutoForm()
    {
        InitializeComponent();

        produtoRepository = new ProdutoRepository();
        produtoController = new ProdutoController(produtoRepository);

        CriarInterface();
    }

    // ============================
    // INTERFACE PRINCIPAL
    // ============================

    private void CriarInterface()
    {
        SuspendLayout();
        Text = "Cadastrar Produto - VendeAí";
        BackColor = corFundo;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1050, 700);
        Font = new Font("Segoe UI", 10);
        AutoScroll = false;
        Controls.Clear();

        Panel menu = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = corMenu };
        Controls.Add(menu);
        menu.Controls.Add(new Label
        {
            Text = "🛒  VendeAí", ForeColor = Color.White,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            Location = new Point(20, 34), Size = new Size(205, 42)
        });
        menu.Controls.Add(new Label
        {
            Text = "Gestão de vendas", ForeColor = Color.FromArgb(160, 175, 200),
            Font = new Font("Segoe UI", 9), Location = new Point(34, 78), AutoSize = true
        });

        int menuY = 135;
        foreach (var item in new (string Icone, string Texto)[]
        {
            ("⌂", "Painel"), ("▣", "Produtos"), ("♙", "Clientes"),
            ("▤", "Pedidos"), ("▥", "Compras"), ("▥", "Separação"),
            ("⚿", "Permissões")
        })
        {
            CriarBotaoMenu(menu, item.Icone, item.Texto, menuY, item.Texto == "Produtos");
            menuY += 55;
        }
        Button configuracoes = CriarBotaoMenu(menu, "⚙", "Configurações", 0, false);
        configuracoes.Dock = DockStyle.Bottom;
        configuracoes.Height = 60;

        Panel principal = new Panel { Dock = DockStyle.Fill, BackColor = corFundo, AutoScroll = true };
        Controls.Add(principal);
        principal.BringToFront();

        Panel topo = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
        principal.Controls.Add(topo);

        Label usuario = new Label
        {
            Text = "Mayumi  ▾", ForeColor = corTexto,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true
        };
        topo.Controls.Add(usuario);
        topo.Resize += (_, _) => usuario.Location =
            new Point(Math.Max(20, topo.ClientSize.Width - usuario.Width - 35), 26);

        // O formulário e os cards laterais ficam na mesma linha.
        // A largura fixa corresponde ao desenho dos campos internos.
        Panel conteudo = new Panel
        {
            Location = new Point(20, 90),
            Width = 1040,
            Height = 1020,
            BackColor = corFundo
        };
        principal.Controls.Add(conteudo);

        conteudo.Controls.Add(new Label
        {
            Text = "Produtos   ›   Cadastrar produto", ForeColor = corTextoSecundario,
            Font = new Font("Segoe UI", 9), Location = new Point(0, 0), AutoSize = true
        });
        conteudo.Controls.Add(new Label
        {
            Text = "Cadastrar Produto", ForeColor = corTexto,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            Location = new Point(0, 45), AutoSize = true
        });
        conteudo.Controls.Add(new Label
        {
            Text = "Preencha as informações do produto para adicioná-lo ao sistema.",
            ForeColor = corTextoSecundario, Font = new Font("Segoe UI", 10),
            Location = new Point(3, 96), AutoSize = true
        });

        btnAdicionarVariacao = CriarBotaoAcao("+  Adicionar variação", false);
        btnAdicionarVariacao.Name = "btnAdicionarVariacao";
        btnAdicionarVariacao.Size = new Size(205, 42);
        btnAdicionarVariacao.Click += (_, _) => MessageBox.Show(
            "O cadastro de variações (cor e tamanho) ainda será implementado. " +
            "Por enquanto, selecione a modalidade de venda no formulário e salve o produto.",
            "Adicionar variação", MessageBoxButtons.OK, MessageBoxIcon.Information);
        conteudo.Controls.Add(btnAdicionarVariacao);

        Button btnVoltar = CriarBotaoAcao("←  Voltar para a lista", false);
        btnVoltar.Name = "btnVoltar";
        btnVoltar.Size = new Size(195, 42);
        btnVoltar.Click += (_, _) => MessageBox.Show(
            "A tela de listagem de produtos ainda não foi criada.",
            "VendeAí", MessageBoxButtons.OK, MessageBoxIcon.Information);
        conteudo.Controls.Add(btnVoltar);

        Panel card = CriarCard(new Point(0, 155), new Size(710, 675));
        card.Name = "cardProduto";
        conteudo.Controls.Add(card);
        CriarCamposProduto(card);

        Panel cardImagem = CriarCard(new Point(730, 155), new Size(290, 350));
        cardImagem.Name = "cardImagem";
        conteudo.Controls.Add(cardImagem);
        CriarAreaImagem(cardImagem);

        Panel cardPreview = CriarCard(new Point(730, 525), new Size(290, 445));
        cardPreview.Name = "cardPreview";
        conteudo.Controls.Add(cardPreview);
        CriarPreVisualizacao(cardPreview);

        btnCancelar = CriarBotaoAcao("Cancelar", false);
        btnCancelar.Name = "btnCancelar";
        btnCancelar.Size = new Size(145, 44);
        btnCancelar.Location = new Point(30, 610);
        btnCancelar.ForeColor = Color.FromArgb(190, 40, 55);
        btnCancelar.FlatAppearance.BorderColor = Color.FromArgb(220, 90, 100);
        btnCancelar.MouseEnter += (_, _) =>
        {
            btnCancelar.BackColor = Color.FromArgb(190, 40, 55);
            btnCancelar.ForeColor = Color.White;
        };
        btnCancelar.MouseLeave += (_, _) =>
        {
            btnCancelar.BackColor = Color.White;
            btnCancelar.ForeColor = Color.FromArgb(190, 40, 55);
        };
        btnCancelar.Click += (_, _) => Close();
        card.Controls.Add(btnCancelar);

        btnSalvar = CriarBotaoAcao("Salvar", true);
        btnSalvar.Name = "btnSalvar";
        btnSalvar.Size = new Size(190, 44);
        btnSalvar.Location = new Point(490, 610);
        btnSalvar.Click += SalvarProduto;
        card.Controls.Add(btnSalvar);

        txtNome.TextChanged += (_, _) => AtualizarPreVisualizacao();
        txtMarca.TextChanged += (_, _) => AtualizarPreVisualizacao();
        cmbCategoria.SelectedIndexChanged += (_, _) => AtualizarPreVisualizacao();
        cmbModalidade.SelectedIndexChanged += (_, _) => AtualizarPreVisualizacao();
        nudPrecoVenda.ValueChanged += (_, _) => AtualizarPreVisualizacao();
        AjustarLayoutConteudo(conteudo);
        AtualizarPreVisualizacao();
        ResumeLayout();
    }

    private Button CriarBotaoAcao(string texto, bool primario)
    {
        Button botao = new Button
        {
            Text = texto, FlatStyle = FlatStyle.Flat,
            BackColor = primario ? corPrimaria : Color.White,
            ForeColor = primario ? Color.White : corPrimaria,
            Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        botao.FlatAppearance.BorderSize = primario ? 0 : 1;
        botao.FlatAppearance.BorderColor = corPrimaria;
        botao.MouseEnter += (_, _) => { if (primario) botao.BackColor = corPrimariaHover; };
        botao.MouseLeave += (_, _) => { if (primario) botao.BackColor = corPrimaria; };
        return botao;
    }

    private void AjustarLayoutConteudo(Panel conteudo)
    {
        // Evita que os cards de imagem e pré-visualização caiam
        // para baixo do formulário em resoluções comuns.
        Panel card = (Panel)conteudo.Controls["cardProduto"]!;
        Panel imagem = (Panel)conteudo.Controls["cardImagem"]!;
        Panel preview = (Panel)conteudo.Controls["cardPreview"]!;
        Button voltar = (Button)conteudo.Controls["btnVoltar"]!;
        Button variacao = (Button)conteudo.Controls["btnAdicionarVariacao"]!;

        card.Location = new Point(0, 155);
        imagem.Location = new Point(730, 155);
        preview.Location = new Point(730, 525);
        variacao.Location = new Point(conteudo.Width - variacao.Width - 20, 45);
        voltar.Location = new Point(variacao.Left - voltar.Width - 12, 45);
        conteudo.Height = 1020;
    }

    // ============================
    // CAMPOS DO PRODUTO
    // ============================

    private void CriarCamposProduto(Panel card)
    {
        Label tituloBasico = CriarTituloSecao(
            "Informações básicas",
            30,
            25
        );

        card.Controls.Add(tituloBasico);

        // NOME

        CriarLabel(card, "Nome do produto *", 30, 75);

        txtNome = CriarTextBox(
            card,
            30,
            100,
            390
        );

        // SKU

        CriarLabel(card, "Código (SKU) *", 445, 75);

        txtSku = CriarTextBox(
            card,
            445,
            100,
            225
        );

        // DESCRIÇÃO

        CriarLabel(card, "Descrição", 30, 155);

        txtDescricao = CriarTextBoxMultiline(
            card,
            30,
            180,
            640,
            65
        );

        // CATEGORIA

        CriarLabel(card, "Categoria *", 30, 265);

        cmbCategoria = CriarComboBox(
            card,
            30,
            290,
            300
        );

        cmbCategoria.Items.AddRange(
            new object[]
            {
                "Roupas",
                "Cosméticos",
                "Acessórios"
            }
        );

        // MARCA

        CriarLabel(card, "Marca", 355, 265);

        txtMarca = CriarTextBox(
            card,
            355,
            290,
            315
        );

        // ============================
        // INFORMAÇÕES DE VENDA
        // ============================

        Label tituloVenda = CriarTituloSecao(
            "Informações de venda",
            30,
            355
        );

        card.Controls.Add(tituloVenda);

        // MODALIDADE

        CriarLabel(
            card,
            "Modalidade de venda *",
            30,
            405
        );

        cmbModalidade = CriarComboBox(
            card,
            30,
            430,
            300
        );

        cmbModalidade.Items.AddRange(
            new object[]
            {
                "Varejo",
                "Atacado",
                "Pacote Internacional",
                "Lote Nacional"
            }
        );

        // QUANTIDADE MÍNIMA

        CriarLabel(
            card,
            "Quantidade mínima *",
            355,
            405
        );

        nudQuantidadeMinima = CriarNumeric(
            card,
            355,
            430,
            150
        );

        nudQuantidadeMinima.Minimum = 1;
        nudQuantidadeMinima.Maximum = 1000000;
        nudQuantidadeMinima.Value = 1;

        // PREÇO VENDA

        CriarLabel(
            card,
            "Preço de venda (R$) *",
            30,
            495
        );

        nudPrecoVenda = CriarNumeric(
            card,
            30,
            520,
            300
        );

        nudPrecoVenda.DecimalPlaces = 2;
        nudPrecoVenda.Maximum = 1000000;
        nudPrecoVenda.ThousandsSeparator = true;

        // PREÇO CUSTO

        CriarLabel(
            card,
            "Preço de custo (R$)",
            355,
            495
        );

        nudPrecoCusto = CriarNumeric(
            card,
            355,
            520,
            315
        );

        nudPrecoCusto.DecimalPlaces = 2;
        nudPrecoCusto.Maximum = 1000000;
        nudPrecoCusto.ThousandsSeparator = true;

    }

    // ============================
    // OBTER MODALIDADE
    // ============================

    private Modalidade ObterModalidadeSelecionada()
    {
        return cmbModalidade.SelectedIndex switch
        {
            0 => Modalidade.Varejo,
            1 => Modalidade.Atacado,
            2 => Modalidade.Pacote,
            3 => Modalidade.Lote,

            _ => throw new InvalidOperationException(
                "Modalidade de venda inválida."
            )
        };
    }

    // ============================
    // IMAGEM DO PRODUTO
    // ============================

    private void CriarAreaImagem(Panel card)
    {
        card.Controls.Add(CriarTituloSecao("Imagem do produto", 22, 22));
        card.Controls.Add(new Label
        {
            Text = "Adicione uma imagem para facilitar a identificação do produto.",
            ForeColor = corTextoSecundario, Font = new Font("Segoe UI", 9),
            Location = new Point(22, 58), Size = new Size(246, 43)
        });
        picProduto = new PictureBox
        {
            Location = new Point(22, 110), Size = new Size(246, 168),
            BackColor = Color.FromArgb(248, 250, 255),
            BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom,
            Cursor = Cursors.Hand
        };
        picProduto.Click += SelecionarImagem;
        card.Controls.Add(picProduto);
        lblTextoImagem = new Label
        {
            Text = "Clique para adicionar uma imagem\nPNG, JPG ou JPEG (máx. 5 MB)",
            ForeColor = corTextoSecundario, Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleCenter, Size = new Size(226, 60),
            Location = new Point(5, 54), BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        lblTextoImagem.Click += SelecionarImagem;
        picProduto.Controls.Add(lblTextoImagem);
        btnSelecionarImagem = CriarBotaoAcao("Selecionar imagem", false);
        btnSelecionarImagem.Location = new Point(22, 291);
        btnSelecionarImagem.Size = new Size(246, 39);
        btnSelecionarImagem.Click += SelecionarImagem;
        card.Controls.Add(btnSelecionarImagem);
    }

    private void CriarPreVisualizacao(Panel card)
    {
        card.Controls.Add(CriarTituloSecao("Pré-visualização", 22, 22));
        Panel fundo = new Panel
        {
            Location = new Point(22, 63), Size = new Size(246, 355),
            BackColor = Color.FromArgb(249, 250, 254)
        };
        card.Controls.Add(fundo);
        picPreview = new PictureBox
        {
            Location = new Point(43, 15), Size = new Size(160, 175),
            SizeMode = PictureBoxSizeMode.Zoom
        };
        fundo.Controls.Add(picPreview);
        lblPreviewNome = new Label
        {
            Location = new Point(10, 200), Size = new Size(226, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = corTexto, Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };
        fundo.Controls.Add(lblPreviewNome);
        lblPreviewDetalhes = new Label
        {
            Location = new Point(10, 234), Size = new Size(226, 25),
            TextAlign = ContentAlignment.MiddleCenter, ForeColor = corTextoSecundario,
            Font = new Font("Segoe UI", 9)
        };
        fundo.Controls.Add(lblPreviewDetalhes);
        lblPreviewPreco = new Label
        {
            Location = new Point(10, 268), Size = new Size(226, 45),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = corPrimaria, Font = new Font("Segoe UI", 19, FontStyle.Bold)
        };
        fundo.Controls.Add(lblPreviewPreco);
        lblPreviewModalidade = new Label
        {
            Location = new Point(10, 320), Size = new Size(226, 24),
            TextAlign = ContentAlignment.MiddleCenter, ForeColor = corTextoSecundario,
            Font = new Font("Segoe UI", 9)
        };
        fundo.Controls.Add(lblPreviewModalidade);
    }

    private void AtualizarPreVisualizacao()
    {
        if (lblPreviewNome == null || lblPreviewNome.IsDisposed) return;
        lblPreviewNome.Text = string.IsNullOrWhiteSpace(txtNome.Text)
            ? "Nome do produto" : txtNome.Text.Trim();
        string categoria = cmbCategoria.SelectedItem?.ToString() ?? "Categoria";
        string marca = string.IsNullOrWhiteSpace(txtMarca.Text) ? "Marca" : txtMarca.Text.Trim();
        lblPreviewDetalhes.Text = $"{categoria}  •  {marca}";
        decimal preco = nudPrecoVenda.Value;
        lblPreviewPreco.Text = preco > 0 ? $"R$ {preco:N2}" : "R$ 0,00";
        lblPreviewModalidade.Text = cmbModalidade.SelectedItem?.ToString() ?? "Modalidade de venda";
        picPreview.Image = picProduto.Image;
    }

    // ============================
    // SELECIONAR IMAGEM
    // ============================

    private void SelecionarImagem(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new OpenFileDialog
        {
            Title = "Selecionar imagem do produto",
            Filter = "Imagens|*.jpg;*.jpeg;*.png;*.bmp"
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            var arquivo = new System.IO.FileInfo(dialog.FileName);
            if (arquivo.Length > 5 * 1024 * 1024)
            {
                MessageBox.Show("Escolha uma imagem de até 5 MB.", "Imagem muito grande",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using Image original = Image.FromFile(dialog.FileName);
            Image nova = new Bitmap(original);
            Image? antiga = picProduto.Image;
            picPreview.Image = null;
            picProduto.Image = nova;
            antiga?.Dispose();
            caminhoImagem = dialog.FileName;
            lblTextoImagem.Visible = false;
            AtualizarPreVisualizacao();
        }
        catch (Exception)
        {
            MessageBox.Show("Não foi possível carregar a imagem.", "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ============================
    // SALVAR PRODUTO
    // ============================

    private void SalvarProduto(
        object? sender,
        EventArgs e)
    {
        // ============================
        // VALIDAÇÕES DA TELA
        // ============================

        if (string.IsNullOrWhiteSpace(
            txtNome.Text))
        {
            MessageBox.Show(
                "Informe o nome do produto.",
                "Campo obrigatório",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            txtNome.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(
            txtSku.Text))
        {
            MessageBox.Show(
                "Informe o código SKU.",
                "Campo obrigatório",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            txtSku.Focus();
            return;
        }

        if (cmbCategoria.SelectedIndex == -1)
        {
            MessageBox.Show(
                "Selecione uma categoria.",
                "Campo obrigatório",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            cmbCategoria.Focus();
            return;
        }

        if (cmbModalidade.SelectedIndex == -1)
        {
            MessageBox.Show("Selecione uma modalidade de venda.", "Campo obrigatório",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbModalidade.Focus();
            return;
        }
        if (nudPrecoVenda.Value <= 0)
        {
            MessageBox.Show("O preço de venda deve ser maior que zero.", "Valor inválido",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            nudPrecoVenda.Focus();
            return;
        }

        // ============================
        // CRIA O PRODUTO
        // ============================

        Produto produto = new Produto
        {
            Nome =
                txtNome.Text.Trim(),

            CodigoSku =
                txtSku.Text.Trim(),

            Descricao =
                string.IsNullOrWhiteSpace(
                    txtDescricao.Text)
                    ? null
                    : txtDescricao.Text.Trim(),

            Categoria = cmbCategoria.SelectedIndex switch
            {
                0 => Categoria.Roupas,
                1 => Categoria.Cosmeticos,
                2 => Categoria.Acessorios,

                _ => throw new InvalidOperationException(
                "Categoria inválida."
                )
            },

            Marca =
                string.IsNullOrWhiteSpace(
                    txtMarca.Text)
                    ? null
                    : txtMarca.Text.Trim(),

            Imagem =
                caminhoImagem,

            Modalidades = new List<ProdutoModalidade>
            {
                new ProdutoModalidade
                {
                    Modalidade = ObterModalidadeSelecionada(),
                    QuantidadeMinimaVenda = (int)nudQuantidadeMinima.Value,
                    PrecoVenda = nudPrecoVenda.Value,
                    PrecoCusto = nudPrecoCusto.Value > 0 ? nudPrecoCusto.Value : null
                }
            }
        };

        // ============================
        // ENVIA PARA O CONTROLLER
        // ============================

        string resultado =
            produtoController.Cadastrar(
                produto
            );

        bool sucesso =
            resultado ==
            "Produto cadastrado com sucesso.";

        MessageBox.Show(
            resultado,
            sucesso
                ? "VendeAí"
                : "Não foi possível cadastrar",
            MessageBoxButtons.OK,
            sucesso
                ? MessageBoxIcon.Information
                : MessageBoxIcon.Warning
        );

        // ============================
        // LIMPA APÓS CADASTRAR
        // ============================

        if (sucesso)
        {
            LimparFormulario();
        }
    }

    // ============================
    // LIMPAR FORMULÁRIO
    // ============================

    private void LimparFormulario()
    {
        txtNome.Clear();
        txtSku.Clear();
        txtDescricao.Clear();
        txtMarca.Clear();

        cmbCategoria.SelectedIndex = -1;
        cmbModalidade.SelectedIndex = -1;

        nudQuantidadeMinima.Value = 1;
        nudPrecoVenda.Value = 0;
        nudPrecoCusto.Value = 0;

        caminhoImagem = null;

        if (picProduto.Image != null)
        {
            picPreview.Image = null;
            picProduto.Image.Dispose();
            picProduto.Image = null;
        }
        lblTextoImagem.Visible = true;
        AtualizarPreVisualizacao();

        txtNome.Focus();
    }

    // ============================
    // CRIAR BOTÃO DO MENU
    // ============================

    private Button CriarBotaoMenu(
        Panel menu,
        string icone,
        string texto,
        int y,
        bool selecionado)
    {
        Button botao = new Button
        {
            Text =
                $"   {icone}     {texto}",

            TextAlign =
                ContentAlignment.MiddleLeft,

            Location =
                new Point(15, y),

            Size =
                new Size(200, 45),

            FlatStyle =
                FlatStyle.Flat,

            BackColor =
                selecionado
                    ? corPrimaria
                    : corMenu,

            ForeColor =
                Color.White,

            Font =
                new Font(
                    "Segoe UI",
                    10,
                    selecionado
                        ? FontStyle.Bold
                        : FontStyle.Regular
                ),

            Cursor =
                Cursors.Hand
        };

        botao.FlatAppearance.BorderSize = 0;

        if (!selecionado)
        {
            botao.MouseEnter += (s, e) =>
            {
                botao.BackColor =
                    corMenuHover;
            };

            botao.MouseLeave += (s, e) =>
            {
                botao.BackColor =
                    corMenu;
            };
        }

        menu.Controls.Add(botao);

        ArredondarControle(
            botao,
            6
        );

        return botao;
    }

    // ============================
    // CARD
    // ============================

    private Panel CriarCard(
        Point localizacao,
        Size tamanho)
    {
        Panel painel = new Panel
        {
            Location = localizacao,
            Size = tamanho,
            BackColor = Color.White
        };

        ArredondarControle(
            painel,
            10
        );

        return painel;
    }

    // ============================
    // LABEL
    // ============================

    private void CriarLabel(
        Control pai,
        string texto,
        int x,
        int y)
    {
        bool obrigatorio =
            texto.TrimEnd()
                .EndsWith("*");

        if (obrigatorio)
        {
            texto =
                texto.TrimEnd();

            texto =
                texto
                    .Substring(
                        0,
                        texto.Length - 1
                    )
                    .TrimEnd();
        }

        Label label = new Label
        {
            Text = texto,

            ForeColor =
                corTexto,

            Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold
                ),

            AutoSize = true,

            Location =
                new Point(x, y)
        };

        pai.Controls.Add(label);

        if (obrigatorio)
        {
            Label asterisco =
                new Label
                {
                    Text = "*",

                    ForeColor =
                        Color.FromArgb(
                            220,
                            38,
                            38
                        ),

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold
                        ),

                    AutoSize = true
                };

            pai.Controls.Add(asterisco);

            asterisco.Location =
                new Point(
                    label.Right + 3,
                    y
                );
        }
    }

    // ============================
    // TÍTULO DE SEÇÃO
    // ============================

    private Label CriarTituloSecao(
        string texto,
        int x,
        int y)
    {
        return new Label
        {
            Text = texto,

            ForeColor =
                corTexto,

            Font =
                new Font(
                    "Segoe UI",
                    12,
                    FontStyle.Bold
                ),

            AutoSize = true,

            Location =
                new Point(x, y)
        };
    }

    // ============================
    // TEXTBOX NORMAL
    // ============================

    private TextBox CriarTextBox(Control pai, int x, int y, int largura)
    {
        Panel contorno = new Panel
        {
            Location = new Point(x, y), Size = new Size(largura, 38),
            BackColor = corBorda, Padding = new Padding(1)
        };
        TextBox caixa = new TextBox
        {
            BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10),
            BackColor = Color.White, ForeColor = corTexto,
            Location = new Point(8, 9), Width = largura - 16
        };
        contorno.BackColor = Color.White;
        contorno.Paint += (_, e) =>
        {
            using Pen caneta = new Pen(contorno.ContainsFocus ? corPrimaria : corBorda, 1);
            e.Graphics.DrawRectangle(caneta, 0, 0, contorno.Width - 1, contorno.Height - 1);
        };
        contorno.Controls.Add(caixa);
        pai.Controls.Add(contorno);
        caixa.Enter += (_, _) => contorno.Invalidate();
        caixa.Leave += (_, _) => contorno.Invalidate();
        return caixa;
    }

    // ============================
    // TEXTBOX MULTILINE
    // ============================

    private TextBox CriarTextBoxMultiline(Control pai, int x, int y, int largura, int altura)
    {
        Panel contorno = new Panel
        {
            Location = new Point(x, y), Size = new Size(largura, altura),
            BackColor = corBorda
        };
        TextBox caixa = new TextBox
        {
            BorderStyle = BorderStyle.None, Multiline = true,
            Font = new Font("Segoe UI", 10), BackColor = Color.White,
            ForeColor = corTexto, Location = new Point(8, 7),
            Size = new Size(largura - 16, altura - 14), ScrollBars = ScrollBars.Vertical
        };
        contorno.BackColor = Color.White;
        contorno.Paint += (_, e) =>
        {
            using Pen caneta = new Pen(contorno.ContainsFocus ? corPrimaria : corBorda, 1);
            e.Graphics.DrawRectangle(caneta, 0, 0, contorno.Width - 1, contorno.Height - 1);
        };
        contorno.Controls.Add(caixa);
        pai.Controls.Add(contorno);
        caixa.Enter += (_, _) => contorno.Invalidate();
        caixa.Leave += (_, _) => contorno.Invalidate();
        return caixa;
    }

    // ============================
    // COMBOBOX
    // ============================

    private ComboBox CriarComboBox(Control pai, int x, int y, int largura)
    {
        // Painel desenha um contorno fino e visível, inclusive
        // quando o ComboBox está fechado e sem seleção.
        Panel contorno = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(largura, 38),
            BackColor = Color.White
        };
        ComboBox combo = new ComboBox
        {
            Location = new Point(2, 5),
            Width = largura - 4,
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = corTexto
        };
        contorno.Paint += (_, e) =>
        {
            using Pen caneta = new Pen(combo.Focused ? corPrimaria : corBorda, 1);
            e.Graphics.DrawRectangle(caneta, 0, 0,
                contorno.ClientSize.Width - 1, contorno.ClientSize.Height - 1);
        };
        combo.Enter += (_, _) => contorno.Invalidate();
        combo.Leave += (_, _) => contorno.Invalidate();
        contorno.Controls.Add(combo);
        pai.Controls.Add(contorno);
        return combo;
    }

    // ============================
    // NUMERIC UP DOWN
    // ============================

    private NumericUpDown CriarNumeric(Control pai, int x, int y, int largura)
    {
        NumericUpDown numeric = new NumericUpDown
        {
            Location = new Point(x, y), Size = new Size(largura, 38),
            Font = new Font("Segoe UI", 10), BackColor = Color.White,
            ForeColor = corTexto, BorderStyle = BorderStyle.FixedSingle
        };
        pai.Controls.Add(numeric);
        return numeric;
    }

    // ============================
    // BORDAS ARREDONDADAS
    // ============================

    private void ArredondarControle(
        Control controle,
        int raio)
    {
        if (controle.Width <= 0 ||
            controle.Height <= 0)
        {
            return;
        }

        using GraphicsPath path =
            new GraphicsPath();

        int diametro =
            raio * 2;

        path.AddArc(
            0,
            0,
            diametro,
            diametro,
            180,
            90
        );

        path.AddArc(
            controle.Width - diametro,
            0,
            diametro,
            diametro,
            270,
            90
        );

        path.AddArc(
            controle.Width - diametro,
            controle.Height - diametro,
            diametro,
            diametro,
            0,
            90
        );

        path.AddArc(
            0,
            controle.Height - diametro,
            diametro,
            diametro,
            90,
            90
        );

        path.CloseFigure();

        controle.Region =
            new Region(path);
    }
}