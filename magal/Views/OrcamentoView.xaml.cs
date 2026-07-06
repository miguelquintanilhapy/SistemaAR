using System.Collections.Generic;
using System.Windows.Controls;
using magal.Models;
using magal.ViewModels;

namespace magal.Views
{
    public partial class OrcamentoView : UserControl
    {
        public OrcamentoViewModel ViewModel => this.DataContext as OrcamentoViewModel;

        public OrcamentoView()
        {
            InitializeComponent();
            this.DataContext = new OrcamentoViewModel();
        }

        public OrcamentoView(Orcamento orcamentoParaEditar, List<ItemOrcamento> itens)
        {
            InitializeComponent();

            var vm = new OrcamentoViewModel();
            vm.CarregarParaEdicao(orcamentoParaEditar, itens);
            this.DataContext = vm;
        }
    }
}
