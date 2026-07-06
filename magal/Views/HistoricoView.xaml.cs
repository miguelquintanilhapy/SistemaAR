using System.Windows;
using System.Windows.Controls;
using magal.ViewModels;

namespace magal.Views
{
    public partial class HistoricoView : UserControl
    {
        public HistoricoView()
        {
            InitializeComponent();

            var viewModel = new HistoricoViewModel();
            this.DataContext = viewModel;

            Loaded += HistoricoView_Loaded;
        }

        private async void HistoricoView_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is HistoricoViewModel vm)
            {
                await vm.CarregarHistoricoAsync();
            }
        }
    }
}
