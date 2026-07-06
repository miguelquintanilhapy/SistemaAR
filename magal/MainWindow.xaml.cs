using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using magal.Models;
using magal.Views;

namespace magal
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            AbrirOrcamento();
        }

        private void BtnOrcamentos_Click(object sender, RoutedEventArgs e) => AbrirOrcamento();
        private void BtnHistorico_Click(object sender, RoutedEventArgs e) => AbrirHistorico();

        private void AtualizarBotaoAtivo(Button botaoAtivo)
        {
            var botoes = new[] { BtnOrcamentos, BtnHistorico };

            foreach (var btn in botoes)
            {
                if (btn == null) continue;
                btn.Tag = btn == botaoAtivo ? "Ativo" : null;
            }
        }

        public void AbrirOrcamento()
        {
            MainContent.Content = new OrcamentoView();
            AtualizarBotaoAtivo(BtnOrcamentos);
        }

        public void AbrirHistorico()
        {
            MainContent.Content = new HistoricoView();
            AtualizarBotaoAtivo(BtnHistorico);
        }

        /// <summary>
        /// Reabre um orçamento salvo (vindo do histórico) na tela de orçamento para visualização/edição.
        /// </summary>
        public void AbrirOrcamentoParaEdicao(Orcamento orcamento, List<ItemOrcamento> itens)
        {
            MainContent.Content = new OrcamentoView(orcamento, itens);
            AtualizarBotaoAtivo(BtnOrcamentos);
        }
    }
}
