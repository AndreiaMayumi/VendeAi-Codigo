
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VendeAi.Controllers;
using VendeAi.Models;
using VendeAi.Service;

namespace VendeAi;

public class CadastrarPedidoForm : Form
{
    private readonly PedidoController controller;
    private readonly List<ItemPedido> itens = new();

    private readonly Color azul = Color.FromArgb(40, 70, 245);
    private readonly Color marinho = Color.FromArgb(17, 28, 62);
    private readonly Color cinza = Color.FromArgb(105, 117, 145);
    private readonly Color borda = Color.FromArgb(221, 228, 241);
    private readonly Color fundo = Color.FromArgb(246, 248, 253);
    private readonly Color vermelhoEscuro = Color.FromArgb(153, 27, 27);

    private readonly CultureInfo cultura =
        CultureInfo.GetCultureInfo("pt-BR");

    private Cliente? clienteSelecionado;
    private Produto? produtoSelecionado;
    private ProdutoVariacao? variacaoSelecionada;
    private ComboBox cmbVariacao = null!;

    private List<Cliente> clientes = new();
    private List<Produto> produtos = new();

    private TextBox txtCliente = null!;
    private TextBox txtProduto = null!;
    private TextBox txtObservacoes = null!;

    private ListBox listaClientes = null!;
    private ListBox listaProdutos = null!;

    private Label lblClienteInfo = null!;
    private Label lblPreco = null!;
    private Label lblMinimo = null!;
    private Label lblSubtotal = null!;
    private Label lblTotal = null!;
    private Label lblQuantidade = null!;

    private ComboBox cmbModalidade = null!;
    private ComboBox cmbStatus = null!;
    private ComboBox cmbPagamento = null!;

    private NumericUpDown numDesconto = null!;

    private DateTimePicker dtPedido = null!;
    private DateTimePicker dtEntrega = null!;
    private CheckBox chkEntrega = null!;

    private DataGridView tabela = null!;

    private int quantidade = 1;
    private bool atualizandoBusca;

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        int msg,
        int wParam,
        int lParam);

    public CadastrarPedidoForm()
    {
        controller = new PedidoController(
            new PedidoService(AppContexto.Pedidos));

        ConfigurarJanela();
        CriarInterface();
        CarregarDados();
        AtualizarResumo();
    }

    // =====================================================
    // CONFIGURAÇÃO DA JANELA
    // =====================================================

    private void ConfigurarJanela()
    {
        Text = "Cadastrar pedido";

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;

        ClientSize = new Size(1180, 700);
        MinimumSize = new Size(1180, 700);
        MaximumSize = new Size(1180, 700);

        BackColor = fundo;
        Font = new Font("Segoe UI", 9);

        DoubleBuffered = true;
        ShowInTaskbar = false;

        Shown += (_, _) =>
        {
            AplicarArredondamento(this, 18);
            ActiveControl = null;
        };
    }

    // =====================================================
    // INTERFACE PRINCIPAL
    // =====================================================

    private void CriarInterface()
    {
        var cabecalho = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(1180, 92),
            BackColor = Color.White
        };

        Controls.Add(cabecalho);

        var icone = new Label
        {
            Text = "▤",
            Font = new Font(
                "Segoe UI",
                24,
                FontStyle.Bold),
            ForeColor = azul,
            BackColor = Color.FromArgb(237, 241, 255),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(28, 20),
            Size = new Size(52, 52)
        };

        cabecalho.Controls.Add(icone);
        AplicarArredondamento(icone, 12);

        cabecalho.Controls.Add(
            CriarLabel(
                "Cadastrar pedido",
                94, 20, 19, true));

        var subtitulo = CriarLabel(
            "Preencha as informações para registrar um novo pedido.",
            96, 54, 9);

        subtitulo.ForeColor = cinza;
        cabecalho.Controls.Add(subtitulo);

        var fechar = new Button
        {
            Text = "✕",
            Location = new Point(1123, 25),
            Size = new Size(34, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = cinza,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 12),
            UseVisualStyleBackColor = false
        };

        fechar.FlatAppearance.BorderSize = 0;

        fechar.Click += (_, _) => Close();

        cabecalho.Controls.Add(fechar);

        cabecalho.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        };

        var esquerda = new Panel
        {
            Location = new Point(24, 110),
            Size = new Size(736, 510),
            BackColor = fundo
        };

        var direita = new Panel
        {
            Location = new Point(776, 110),
            Size = new Size(380, 510),
            BackColor = fundo
        };

        Controls.Add(esquerda);
        Controls.Add(direita);

        CriarSecaoCliente(esquerda);
        CriarSecaoProdutos(esquerda);

        CriarSecaoResumo(direita);
        CriarSecaoAdicionais(direita);

        CriarRodape();
    }

    // =====================================================
    // CLIENTE
    // =====================================================

    private void CriarSecaoCliente(Panel pai)
    {
        var card = CriarCard(
            pai, 0, 0, 736, 139);

        card.Controls.Add(
            CriarLabel(
                "Informações do cliente",
                18, 14, 11, true));

        card.Controls.Add(
            CriarLabel(
                "Cliente *",
                18, 46, 9, true));

        var campoCliente = CriarCampo(
            card, 18, 69, 700, 35);

        txtCliente = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Location = new Point(12, 9),
            Width = 672,
            Font = new Font("Segoe UI", 9),
            PlaceholderText =
                "Buscar cliente por nome, CPF ou telefone..."
        };

        campoCliente.Controls.Add(txtCliente);

        lblClienteInfo = CriarLabel(
            "Nenhum cliente selecionado",
            18, 110, 8);

        lblClienteInfo.ForeColor = cinza;
        card.Controls.Add(lblClienteInfo);

        listaClientes = new ListBox
        {
            Location = new Point(18, 104),
            Size = new Size(700, 105),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            Visible = false,
            IntegralHeight = false
        };

        card.Controls.Add(listaClientes);

        txtCliente.TextChanged += (_, _) =>
            FiltrarClientes();

        txtCliente.Enter += (_, _) =>
            FiltrarClientes();

        listaClientes.Click += (_, _) =>
            SelecionarCliente();

        listaClientes.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelecionarCliente();
                e.Handled = true;
            }
        };

        listaClientes.BringToFront();
    }

    // =====================================================
    // PRODUTOS
    // =====================================================

    private void CriarSecaoProdutos(Panel pai)
    {
        var card = CriarCard(
            pai, 0, 151, 736, 359);

        card.Controls.Add(
            CriarLabel(
                "Adicionar produtos",
                18, 12, 11, true));

        var campoProduto = CriarCampo(
            card, 18, 43, 700, 35);

        txtProduto = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Location = new Point(12, 9),
            Width = 670,
            Font = new Font("Segoe UI", 9),
            PlaceholderText =
                "Buscar produto por nome, código ou categoria..."
        };

        campoProduto.Controls.Add(txtProduto);

        card.Controls.Add(CriarLabel("Variação (cor / tamanho)", 18, 86, 9, true));
        var campoVariacao = CriarCampo(card, 18, 105, 700, 35);
        cmbVariacao = CriarComboModerno(campoVariacao, 700);
        cmbVariacao.SelectedIndexChanged += (_, _) => AtualizarVariacao();

        card.Controls.Add(
            CriarLabel(
                "Modalidade",
                18, 147, 9, true));

        card.Controls.Add(
            CriarLabel(
                "Quantidade",
                256, 147, 9, true));

        card.Controls.Add(
            CriarLabel(
                "Preço unitário",
                402, 147, 9, true));

        // MODALIDADE COM BORDA MODERNA

        var campoModalidade = CriarCampo(
            card, 18, 168, 220, 36);

        cmbModalidade = new ComboBox
        {
            Location = new Point(9, 6),
            Size = new Size(202, 27),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 20,
            BackColor = Color.White,
            ForeColor = marinho,
            Font = new Font("Segoe UI", 9)
        };

        cmbModalidade.DrawItem += DesenharCombo;

        campoModalidade.Controls.Add(cmbModalidade);

        cmbModalidade.SelectedIndexChanged += (_, _) =>
            AtualizarModalidade();

        // QUANTIDADE PERSONALIZADA

        var campoQuantidade = CriarCampo(
            card, 256, 168, 126, 36);

        var diminuir = CriarBotaoQuantidade(
            "−", 2, 3);

        var aumentar = CriarBotaoQuantidade(
            "+", 91, 3);

        lblQuantidade = new Label
        {
            Text = "1",
            Location = new Point(35, 5),
            Size = new Size(56, 26),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = marinho,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Bold)
        };

        diminuir.Click += (_, _) =>
        {
            int minimo = ObterQuantidadeMinima();

            if (quantidade > minimo)
            {
                quantidade--;
                AtualizarQuantidade();
            }
        };

        aumentar.Click += (_, _) =>
        {
            if (quantidade < 1000000)
            {
                quantidade++;
                AtualizarQuantidade();
            }
        };

        campoQuantidade.Controls.Add(diminuir);
        campoQuantidade.Controls.Add(lblQuantidade);
        campoQuantidade.Controls.Add(aumentar);

        lblPreco = CriarLabel(
            "R$ 0,00",
            402, 173, 10, true);

        card.Controls.Add(lblPreco);

        lblMinimo = CriarLabel(
            "Quantidade mínima: -",
            18, 207, 8);

        lblMinimo.ForeColor = cinza;
        card.Controls.Add(lblMinimo);

        var adicionar = CriarBotao(
            "+ Adicionar",
            574, 164, 144, 36, true);

        adicionar.Click += AdicionarItem;

        card.Controls.Add(adicionar);

        card.Controls.Add(
            CriarLabel(
                "Itens do pedido",
                18, 237, 10, true));

        // TABELA MODERNA

        tabela = new DataGridView
        {
            Location = new Point(18, 263),
            Size = new Size(700, 79),

            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,

            ReadOnly = true,

            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,

            RowHeadersVisible = false,
            MultiSelect = false,

            SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,

            AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill,

            ColumnHeadersHeight = 36,
            ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing,

            RowTemplate = { Height = 36 },

            EnableHeadersVisualStyles = false,

            CellBorderStyle =
                DataGridViewCellBorderStyle.None,

            ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None,

            GridColor = Color.White,

            Font = new Font("Segoe UI", 9)
        };

        tabela.ColumnHeadersDefaultCellStyle.BackColor =
            Color.FromArgb(243, 246, 252);

        tabela.ColumnHeadersDefaultCellStyle.ForeColor =
            marinho;

        tabela.ColumnHeadersDefaultCellStyle.Font =
            new Font(
                "Segoe UI",
                9,
                FontStyle.Bold);

        tabela.ColumnHeadersDefaultCellStyle.SelectionBackColor =
            Color.FromArgb(243, 246, 252);

        tabela.DefaultCellStyle.BackColor =
            Color.White;

        tabela.DefaultCellStyle.ForeColor =
            marinho;

        tabela.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(239, 243, 255);

        tabela.DefaultCellStyle.SelectionForeColor =
            marinho;

        tabela.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(249, 250, 254);

        tabela.Columns.Add(
            "Produto", "Produto");

        tabela.Columns.Add("Variacao", "Variação");
        tabela.Columns.Add(
            "Modalidade", "Modalidade");

        tabela.Columns.Add(
            "Quantidade", "Qtd.");

        tabela.Columns.Add(
            "Subtotal", "Subtotal");

        var remover = new DataGridViewButtonColumn
        {
            Name = "Remover",
            HeaderText = "",
            Text = "✕",
            UseColumnTextForButtonValue = true,

            FlatStyle = FlatStyle.Flat
        };

        tabela.Columns.Add(remover);

        tabela.Columns["Produto"]!.FillWeight = 125;
        tabela.Columns["Variacao"]!.FillWeight = 120;
        tabela.Columns["Modalidade"]!.FillWeight = 110;
        tabela.Columns["Quantidade"]!.FillWeight = 55;
        tabela.Columns["Subtotal"]!.FillWeight = 105;
        tabela.Columns["Remover"]!.FillWeight = 45;

        tabela.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex !=
                tabela.Columns["Remover"]!.Index)
                return;

            itens.RemoveAt(e.RowIndex);
            AtualizarItens();
        };

        card.Controls.Add(tabela);

        // LISTA DE PESQUISA DE PRODUTOS

        listaProdutos = new ListBox
        {
            Location = new Point(18, 78),
            Size = new Size(700, 105),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            Visible = false,
            IntegralHeight = false
        };

        card.Controls.Add(listaProdutos);

        txtProduto.TextChanged += (_, _) =>
            FiltrarProdutos();

        txtProduto.Enter += (_, _) =>
            FiltrarProdutos();

        listaProdutos.Click += (_, _) =>
            SelecionarProduto();

        listaProdutos.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelecionarProduto();
                e.Handled = true;
            }
        };

        listaProdutos.BringToFront();
    }

    // =====================================================
    // RESUMO DO PEDIDO
    // =====================================================

    private void CriarSecaoResumo(Panel pai)
    {
        var card = CriarCard(
            pai, 0, 0, 380, 183);

        card.Controls.Add(
            CriarLabel(
                "Resumo do pedido",
                18, 15, 11, true));

        lblSubtotal = CriarLabel(
            "Subtotal (0 itens): R$ 0,00",
            18, 58, 9);

        card.Controls.Add(lblSubtotal);

        card.Controls.Add(
            CriarLabel(
                "Desconto",
                18, 101, 9));

        // DESCONTO MODERNO

        var campoDesconto = CriarCampo(
            card, 177, 91, 182, 36);

        var simbolo = CriarLabel(
            "R$",
            10, 9, 9, true);

        simbolo.ForeColor = cinza;

        campoDesconto.Controls.Add(simbolo);

        numDesconto = new NumericUpDown
        {
            Location = new Point(41, 8),
            Size = new Size(130, 24),

            BorderStyle = BorderStyle.None,

            DecimalPlaces = 2,
            Minimum = 0,
            Maximum = 100000000,

            ThousandsSeparator = true,

            Value = 0,

            TextAlign = HorizontalAlignment.Right,

            BackColor = Color.White,
            ForeColor = marinho,

            Font = new Font("Segoe UI", 10)
        };

        campoDesconto.Controls.Add(numDesconto);

        numDesconto.ValueChanged += (_, _) =>
            AtualizarResumo();

        var linha = new Panel
        {
            Location = new Point(18, 135),
            Size = new Size(342, 1),
            BackColor = borda
        };

        card.Controls.Add(linha);

        card.Controls.Add(
            CriarLabel(
                "Total",
                18, 148, 11, true));

        lblTotal = CriarLabel(
            "R$ 0,00",
            230, 145, 14, true);

        lblTotal.ForeColor = azul;

        lblTotal.Size = new Size(130, 28);
        lblTotal.AutoSize = false;

        lblTotal.TextAlign =
            ContentAlignment.MiddleRight;

        card.Controls.Add(lblTotal);
    }

    // =====================================================
    // INFORMAÇÕES ADICIONAIS
    // =====================================================

    private void CriarSecaoAdicionais(Panel pai)
    {
        var card = CriarCard(
            pai, 0, 195, 380, 315);

        card.Controls.Add(
            CriarLabel(
                "Informações adicionais",
                18, 13, 11, true));

        card.Controls.Add(
            CriarLabel(
                "Status do pedido *",
                18, 48, 9, true));

        var campoStatus = CriarCampo(
            card, 18, 69, 342, 36);

        cmbStatus = CriarComboModerno(
            campoStatus, 342);

        foreach (StatusPedido status
            in Enum.GetValues<StatusPedido>())
        {
            cmbStatus.Items.Add(status);
        }

        cmbStatus.SelectedItem =
            StatusPedido.EmAndamento;

        card.Controls.Add(
            CriarLabel(
                "Forma de pagamento",
                18, 110, 9, true));

        var campoPagamento = CriarCampo(
            card, 18, 131, 342, 36);

        cmbPagamento = CriarComboModerno(
            campoPagamento, 342);

        cmbPagamento.Items.Add("Selecione...");

        foreach (FormaPagamento forma
            in Enum.GetValues<FormaPagamento>())
        {
            cmbPagamento.Items.Add(forma);
        }

        cmbPagamento.SelectedIndex = 0;

        // DATA DO PEDIDO

        card.Controls.Add(
            CriarLabel(
                "Data do pedido *",
                18, 174, 9, true));

        var campoDataPedido = CriarCampo(
            card, 18, 195, 155, 36);

        dtPedido = CriarDataModerna(
            campoDataPedido, 155);

        dtPedido.Value = DateTime.Now;

        // PREVISÃO DE ENTREGA

        chkEntrega = new CheckBox
        {
            Text = "Previsão de entrega",
            Location = new Point(189, 176),
            AutoSize = true,
            ForeColor = marinho,
            Font = new Font("Segoe UI", 8)
        };

        card.Controls.Add(chkEntrega);

        var campoDataEntrega = CriarCampo(
            card, 189, 195, 171, 36);

        dtEntrega = CriarDataModerna(
            campoDataEntrega, 171);

        dtEntrega.Value = DateTime.Now;
        dtEntrega.Enabled = false;

        chkEntrega.CheckedChanged += (_, _) =>
        {
            dtEntrega.Enabled = chkEntrega.Checked;

            campoDataEntrega.BackColor =
                chkEntrega.Checked
                    ? Color.White
                    : Color.FromArgb(249, 250, 252);

            campoDataEntrega.Invalidate();
        };

        // OBSERVAÇÕES

        card.Controls.Add(
            CriarLabel(
                "Observações",
                18, 239, 9, true));

        var campoObservacoes = CriarCampo(
            card, 18, 260, 342, 43);

        txtObservacoes = new TextBox
        {
            BorderStyle = BorderStyle.None,

            Location = new Point(9, 7),
            Size = new Size(322, 29),

            Multiline = true,
            MaxLength = 500,

            ScrollBars = ScrollBars.None,

            PlaceholderText =
                "Observações opcionais...",

            Font = new Font("Segoe UI", 9)
        };

        campoObservacoes.Controls.Add(txtObservacoes);
    }

    // =====================================================
    // RODAPÉ
    // =====================================================

    private void CriarRodape()
    {
        var rodape = new Panel
        {
            Location = new Point(0, 635),
            Size = new Size(1180, 65),
            BackColor = Color.White
        };

        Controls.Add(rodape);

        // CANCELAR COM HOVER VERMELHO-ESCURO

        var cancelar = CriarBotao(
            "Cancelar",
            800, 12, 150, 40, false);

        cancelar.Click += (_, _) => Close();

        rodape.Controls.Add(cancelar);

        // SALVAR PEDIDO

        var salvar = CriarBotao(
            "Salvar pedido",
            968, 12, 188, 40, true);

        salvar.Click += SalvarPedido;

        rodape.Controls.Add(salvar);
    }

    // =====================================================
    // CARREGAR CLIENTES E PRODUTOS
    // =====================================================

    private void CarregarDados()
    {
        clientes = AppContexto.Clientes
            .ListarTodos()
            .Where(c =>
                c.StatusCliente == StatusCliente.Ativo)
            .ToList();

        produtos = AppContexto.Produtos
            .ListarTodos()
            .Where(p =>
                p.Status == StatusProduto.Ativo)
            .ToList();

        listaClientes.DisplayMember =
            nameof(Cliente.Nome);

        listaProdutos.DisplayMember =
            nameof(Produto.Nome);

        AtualizarModalidade();
    }

    // =====================================================
    // BUSCA DE CLIENTES
    // =====================================================

    private void FiltrarClientes()
    {
        if (atualizandoBusca)
            return;

        clienteSelecionado = null;

        lblClienteInfo.Text =
            "Nenhum cliente selecionado";

        string busca = txtCliente.Text.Trim();

        var encontrados = clientes
            .Where(c =>
                c.Nome.Contains(
                    busca,
                    StringComparison.OrdinalIgnoreCase)
                ||
                c.CPF.Contains(
                    busca,
                    StringComparison.OrdinalIgnoreCase)
                ||
                (c.Telefone ?? "").Contains(
                    busca,
                    StringComparison.OrdinalIgnoreCase))
            .Take(6)
            .ToList();

        listaClientes.DataSource = null;
        listaClientes.DataSource = encontrados;

        listaClientes.DisplayMember =
            nameof(Cliente.Nome);

        listaClientes.Visible =
            encontrados.Count > 0;

        listaClientes.BringToFront();
    }

    private void SelecionarCliente()
    {
        if (listaClientes.SelectedItem
            is not Cliente cliente)
        {
            return;
        }

        clienteSelecionado = cliente;

        atualizandoBusca = true;

        txtCliente.Text = cliente.Nome;
        txtCliente.SelectionStart =
            txtCliente.Text.Length;

        atualizandoBusca = false;

        lblClienteInfo.Text =
            $"{cliente.CPF}  •  {cliente.Telefone}";

        listaClientes.Visible = false;
    }

    // =====================================================
    // BUSCA DE PRODUTOS
    // =====================================================

    private void FiltrarProdutos()
    {
        if (atualizandoBusca)
            return;

        produtoSelecionado = null;
        variacaoSelecionada = null;
        cmbVariacao.DataSource = null;
        cmbModalidade.DataSource = null;
        AtualizarModalidade();

        string busca = txtProduto.Text.Trim();

        var encontrados = produtos
            .Where(p =>
                p.Nome.Contains(
                    busca,
                    StringComparison.OrdinalIgnoreCase)
                ||
                p.CodigoSku.Contains(
                    busca,
                    StringComparison.OrdinalIgnoreCase)
                ||
                p.Categoria.ToString().Contains(
                    busca,
                    StringComparison.OrdinalIgnoreCase))
            .Take(6)
            .ToList();

        listaProdutos.DataSource = null;
        listaProdutos.DataSource = encontrados;

        listaProdutos.DisplayMember =
            nameof(Produto.Nome);

        listaProdutos.Visible =
            encontrados.Count > 0;

        listaProdutos.BringToFront();
    }

    private void SelecionarProduto()
    {
        if (listaProdutos.SelectedItem
            is not Produto produto)
        {
            return;
        }

        produtoSelecionado = produto;

        atualizandoBusca = true;

        txtProduto.Text = produto.Nome;

        atualizandoBusca = false;

        listaProdutos.Visible = false;

        cmbVariacao.DataSource = null;
        var opcoes = new List<OpcaoVariacao>();
        if (produto.Modalidades.Count > 0)
            opcoes.Add(new OpcaoVariacao(null, "Produto principal"));

        foreach (var variacao in produto.Variacoes.Where(v =>
            v.Status == StatusProduto.Ativo && v.Modalidades.Count > 0))
        {
            string descricao = string.Join(" / ", new[] { variacao.Cor, variacao.Tamanho }
                .Where(valor => !string.IsNullOrWhiteSpace(valor)));
            if (string.IsNullOrWhiteSpace(descricao))
                descricao = variacao.CodigoSku ?? $"Variação {variacao.Id}";
            opcoes.Add(new OpcaoVariacao(variacao, descricao));
        }

        cmbVariacao.DataSource = opcoes;
        cmbVariacao.DisplayMember = nameof(OpcaoVariacao.Descricao);
        AtualizarVariacao();
    }

    private static string DescreverVariacao(ProdutoVariacao? variacao)
    {
        if (variacao == null) return "Principal";
        string descricao = string.Join(" / ",
            new[] { variacao.Cor, variacao.Tamanho }
                .Where(v => !string.IsNullOrWhiteSpace(v)));
        return descricao.Length > 0 ? descricao :
            variacao.CodigoSku ?? $"Variação {variacao.Id}";
    }

    private sealed class OpcaoVariacao
    {
        public ProdutoVariacao? Variacao { get; }
        public string Descricao { get; }

        public OpcaoVariacao(ProdutoVariacao? variacao, string descricao)
        {
            Variacao = variacao;
            Descricao = descricao;
        }
    }

    private void AtualizarVariacao()
    {
        variacaoSelecionada = (cmbVariacao.SelectedItem as OpcaoVariacao)?.Variacao;
        var modalidades = produtoSelecionado == null
            ? new List<ProdutoModalidade>()
            : variacaoSelecionada == null
                ? produtoSelecionado.Modalidades.ToList()
                : variacaoSelecionada.Modalidades.ToList();

        cmbModalidade.DataSource = null;
        cmbModalidade.DisplayMember = nameof(ProdutoModalidade.Modalidade);
        cmbModalidade.DataSource = modalidades;
        AtualizarModalidade();
    }

    // =====================================================
    // MODALIDADE E QUANTIDADE
    // =====================================================

    private void AtualizarModalidade()
    {
        if (cmbModalidade.SelectedItem
            is not ProdutoModalidade modalidade)
        {
            lblPreco.Text = "R$ 0,00";

            lblMinimo.Text =
                "Quantidade mínima: -";

            quantidade = 1;

            AtualizarQuantidade();
            return;
        }

        quantidade = Math.Max(
            1,
            modalidade.QuantidadeMinimaVenda);

        lblPreco.Text =
            modalidade.PrecoVenda.ToString(
                "C2", cultura);

        lblMinimo.Text =
            $"Quantidade mínima: {quantidade}";

        AtualizarQuantidade();
    }

    private int ObterQuantidadeMinima()
    {
        if (cmbModalidade.SelectedItem
            is ProdutoModalidade modalidade)
        {
            return Math.Max(
                1,
                modalidade.QuantidadeMinimaVenda);
        }

        return 1;
    }

    private void AtualizarQuantidade()
    {
        lblQuantidade.Text =
            quantidade.ToString(cultura);
    }

    // =====================================================
    // ADICIONAR ITEM
    // =====================================================

    private void AdicionarItem(
        object? sender, EventArgs e)
    {
        if (produtoSelecionado == null)
        {
            Aviso("Selecione um produto.");
            return;
        }

        if (cmbModalidade.SelectedItem
            is not ProdutoModalidade modalidade)
        {
            Aviso("Selecione uma modalidade.");
            return;
        }

        var existente = itens.FirstOrDefault(i =>
            ReferenceEquals(
                i.Produto, produtoSelecionado)
            &&
            ReferenceEquals(
                i.ProdutoModalidade, modalidade)
            && ReferenceEquals(i.ProdutoVariacao, variacaoSelecionada));

        if (existente != null)
        {
            if (existente.Quantidade >
                1000000 - quantidade)
            {
                Aviso("Quantidade máxima excedida.");
                return;
            }

            existente.Quantidade += quantidade;
        }
        else
        {
            itens.Add(new ItemPedido
            {
                Produto = produtoSelecionado,

                ProdutoModalidade = modalidade,
                ProdutoVariacao = variacaoSelecionada,

                Quantidade = quantidade,

                PrecoUnitario =
                    modalidade.PrecoVenda
            });
        }

        AtualizarItens();

        atualizandoBusca = true;
        txtProduto.Clear();
        atualizandoBusca = false;

        produtoSelecionado = null;

        cmbModalidade.DataSource = null;
        AtualizarModalidade();
    }

    // =====================================================
    // ATUALIZAR TABELA
    // =====================================================

    private void AtualizarItens()
    {
        tabela.Rows.Clear();

        foreach (var item in itens)
        {
            tabela.Rows.Add(
                item.Produto.Nome,
                DescreverVariacao(item.ProdutoVariacao),
                item.ProdutoModalidade.Modalidade,

                item.Quantidade,

                item.Subtotal.ToString(
                    "C2", cultura));
        }

        tabela.ClearSelection();

        AtualizarResumo();
    }

    // =====================================================
    // ATUALIZAR RESUMO
    // =====================================================

    private void AtualizarResumo()
    {
        decimal subtotal =
            itens.Sum(i => i.Subtotal);

        decimal total =
            subtotal - numDesconto.Value;

        int quantidadeItens =
            itens.Sum(i => i.Quantidade);

        lblSubtotal.Text =
            $"Subtotal ({quantidadeItens} itens): " +
            subtotal.ToString("C2", cultura);

        lblTotal.Text =
            total.ToString("C2", cultura);

        lblTotal.ForeColor =
            total < 0
                ? Color.Firebrick
                : azul;
    }

    // =====================================================
    // SALVAR PEDIDO
    // =====================================================

    private void SalvarPedido(
        object? sender, EventArgs e)
    {
        try
        {
            if (clienteSelecionado == null)
            {
                Aviso("Selecione um cliente.");
                return;
            }

            if (itens.Count == 0)
            {
                Aviso(
                    "Adicione pelo menos um produto.");
                return;
            }

            if (numDesconto.Value >
                itens.Sum(i => i.Subtotal))
            {
                Aviso(
                    "O desconto não pode ser maior " +
                    "que o subtotal do pedido.");
                return;
            }

            var pedido = new Pedido
            {
                Cliente = clienteSelecionado,

                Itens = new List<ItemPedido>(itens),

                DataPedido = dtPedido.Value.Date,

                DataPrevistaEntrega =
                    chkEntrega.Checked
                        ? dtEntrega.Value.Date
                        : null,

                Status =
                    (StatusPedido)cmbStatus.SelectedItem!,

                FormaPagamento =
                    cmbPagamento.SelectedItem
                        is FormaPagamento forma
                            ? forma
                            : null,

                Desconto = numDesconto.Value,

                Observacoes =
                    txtObservacoes.Text.Trim()
            };

            controller.CadastrarPedido(pedido);

            MessageBox.Show(
                "Pedido cadastrado com sucesso!",
                "VendeAí",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Aviso(ex.Message);
        }
    }

    private void Aviso(string mensagem)
    {
        MessageBox.Show(
            mensagem,
            "VendeAí",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    // =====================================================
    // COMPONENTES VISUAIS
    // =====================================================

    private Panel CriarCard(
        Control pai,
        int x, int y,
        int largura, int altura)
    {
        var card = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(largura, altura),
            BackColor = Color.White
        };

        pai.Controls.Add(card);

        AplicarArredondamento(card, 12);

        return card;
    }

    private Panel CriarCampo(
        Control pai,
        int x, int y,
        int largura, int altura)
    {
        var campo = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(largura, altura),
            BackColor = Color.White
        };

        campo.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using var caminho =
                CriarCaminhoArredondado(
                    new Rectangle(
                        0, 0,
                        campo.Width - 1,
                        campo.Height - 1),
                    7);

            using var caneta =
                new Pen(borda, 1);

            e.Graphics.DrawPath(
                caneta, caminho);
        };

        pai.Controls.Add(campo);

        return campo;
    }

    private ComboBox CriarComboModerno(
        Control pai, int largura)
    {
        var combo = new ComboBox
        {
            Location = new Point(9, 6),

            Size = new Size(
                largura - 18, 27),

            DropDownStyle =
                ComboBoxStyle.DropDownList,

            FlatStyle = FlatStyle.Flat,

            DrawMode = DrawMode.OwnerDrawFixed,

            ItemHeight = 20,

            BackColor = Color.White,
            ForeColor = marinho,

            Font = new Font("Segoe UI", 9)
        };

        combo.DrawItem += DesenharCombo;

        pai.Controls.Add(combo);

        return combo;
    }

    private void DesenharCombo(
        object? sender,
        DrawItemEventArgs e)
    {
        if (sender is not ComboBox combo)
            return;

        e.Graphics.FillRectangle(
            Brushes.White, e.Bounds);

        if (e.Index < 0)
            return;

        string texto =
            combo.GetItemText(combo.Items[e.Index]);

        using var pincel =
            new SolidBrush(marinho);

        var area = new Rectangle(
            e.Bounds.X + 5,
            e.Bounds.Y,
            e.Bounds.Width - 10,
            e.Bounds.Height);

        TextRenderer.DrawText(
            e.Graphics,
            texto,
            combo.Font,
            area,
            marinho,
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.Left |
            TextFormatFlags.EndEllipsis);
    }

    private Button CriarBotaoQuantidade(
        string texto, int x, int y)
    {
        var botao = new Button
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(32, 29),

            FlatStyle = FlatStyle.Flat,

            BackColor =
                Color.FromArgb(246, 248, 253),

            ForeColor = azul,

            Cursor = Cursors.Hand,

            Font = new Font(
                "Segoe UI",
                12,
                FontStyle.Bold),

            UseVisualStyleBackColor = false
        };

        botao.FlatAppearance.BorderSize = 0;

        botao.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(230, 236, 255);

        AplicarArredondamento(botao, 5);

        return botao;
    }

    private DateTimePicker CriarDataModerna(
        Control pai, int largura)
    {
        var data = new DateTimePicker
        {
            Location = new Point(7, 7),

            Size = new Size(
                largura - 14, 25),

            Format = DateTimePickerFormat.Short,

            Font = new Font("Segoe UI", 9),

            CalendarForeColor = marinho,

            CalendarMonthBackground =
                Color.White
        };

        pai.Controls.Add(data);

        return data;
    }

    private Label CriarLabel(
        string texto,
        int x, int y,
        int tamanho = 9,
        bool negrito = false)
    {
        return new Label
        {
            Text = texto,

            Location = new Point(x, y),

            AutoSize = true,

            ForeColor = marinho,

            Font = new Font(
                "Segoe UI",
                tamanho,
                negrito
                    ? FontStyle.Bold
                    : FontStyle.Regular)
        };
    }

    // =====================================================
    // BOTÃO COM HOVER VERMELHO NO CANCELAR
    // =====================================================

    private Button CriarBotao(
        string texto,
        int x, int y,
        int largura, int altura,
        bool principal)
    {
        Color corNormal =
            principal
                ? azul
                : Color.White;

        Color corTextoNormal =
            principal
                ? Color.White
                : marinho;

        var botao = new Button
        {
            Text = texto,

            Location = new Point(x, y),

            Size = new Size(
                largura, altura),

            FlatStyle = FlatStyle.Flat,

            BackColor = corNormal,

            ForeColor = corTextoNormal,

            Font = new Font(
                "Segoe UI",
                9,
                FontStyle.Bold),

            Cursor = Cursors.Hand,

            UseVisualStyleBackColor = false
        };

        botao.FlatAppearance.BorderSize =
            principal ? 0 : 1;

        botao.FlatAppearance.BorderColor =
            borda;

        botao.FlatAppearance.MouseOverBackColor =
            corNormal;

        botao.FlatAppearance.MouseDownBackColor =
            corNormal;

        // HOVER DO CANCELAR

        if (!principal && texto == "Cancelar")
        {
            botao.FlatAppearance.MouseOverBackColor =
                vermelhoEscuro;

            botao.FlatAppearance.MouseDownBackColor =
                vermelhoEscuro;

            botao.MouseEnter += (_, _) =>
            {
                botao.BackColor =
                    vermelhoEscuro;

                botao.ForeColor =
                    Color.White;

                botao.FlatAppearance.BorderColor =
                    vermelhoEscuro;
            };

            botao.MouseLeave += (_, _) =>
            {
                botao.BackColor =
                    Color.White;

                botao.ForeColor =
                    marinho;

                botao.FlatAppearance.BorderColor =
                    borda;
            };
        }

        AplicarArredondamento(botao, 8);

        return botao;
    }

    // =====================================================
    // BORDAS ARREDONDADAS
    // =====================================================

    private static GraphicsPath CriarCaminhoArredondado(
        Rectangle r, int raio)
    {
        int d = raio * 2;

        var caminho = new GraphicsPath();

        caminho.AddArc(
            r.X, r.Y,
            d, d, 180, 90);

        caminho.AddArc(
            r.Right - d, r.Y,
            d, d, 270, 90);

        caminho.AddArc(
            r.Right - d,
            r.Bottom - d,
            d, d, 0, 90);

        caminho.AddArc(
            r.X,
            r.Bottom - d,
            d, d, 90, 90);

        caminho.CloseFigure();

        return caminho;
    }

    private static void AplicarArredondamento(
        Control controle, int raio)
    {
        using var caminho =
            CriarCaminhoArredondado(
                new Rectangle(
                    0, 0,
                    controle.Width,
                    controle.Height),
                raio);

        controle.Region =
            new Region(caminho);
    }
}
