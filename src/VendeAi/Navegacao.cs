using System;
using System.Windows.Forms;

namespace VendeAi;

public sealed class Navegacao : ApplicationContext
{
    private static Navegacao? instancia;
    private Form? telaAtual;

    public Navegacao(Form telaInicial)
    {
        instancia = this;
        Exibir(telaInicial);
    }

    public static void Abrir(Form novaTela)
    {
        if (instancia == null)
        {
            throw new InvalidOperationException(
                "A navegação ainda não foi inicializada.");
        }

        instancia.Exibir(novaTela);
    }

    private void Exibir(Form novaTela)
    {
        Form? anterior = telaAtual;

        if (anterior != null)
        {
            novaTela.StartPosition = FormStartPosition.Manual;

            if (anterior.WindowState == FormWindowState.Normal)
            {
                novaTela.Bounds = anterior.Bounds;
            }
            else
            {
                novaTela.Bounds = anterior.RestoreBounds;
            }

            novaTela.WindowState = anterior.WindowState;

            anterior.Hide();
        }

        telaAtual = novaTela;
        MainForm = novaTela;

        novaTela.FormClosed += (_, _) =>
        {
            if (ReferenceEquals(telaAtual, novaTela))
            {
                ExitThread();
            }
        };

        novaTela.Show();

        if (anterior != null && !anterior.IsDisposed)
        {
            anterior.Close();
        }
    }
}