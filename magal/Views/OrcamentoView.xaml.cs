using System.Collections.Generic;
<<<<<<< HEAD
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
=======
using System.Windows.Controls;
>>>>>>> 4754a8fb0525ce9a449f316ecec94f294eb0475b
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
<<<<<<< HEAD

        private void ValidarEntradaSemNegativo(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "-")
            {
                e.Handled = true;
                return;
            }

            var regex = new Regex("[^0-9,.]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }
=======
>>>>>>> 4754a8fb0525ce9a449f316ecec94f294eb0475b
    }
}
