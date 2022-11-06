using DBManager.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace DBManager.View
{
    /// <summary>
    /// Interaction logic for ServersListView.xaml
    /// </summary>
    public partial class ServersListView : Window
    {
        public ServersListView()
        {
            InitializeComponent();
            ServerListViewModel model = new ServerListViewModel();
            this.DataContext = model;

            model.ConnectionChanged += RefreshBindings;
            model.DBNamesLoaded += SetSelectionIndex;
            model.DBConnected += ((vm) => new DBView(vm).Show());
        }

        private void RefreshBindings()
        {
            ConnectionMessage.GetBindingExpression(Label.ContentProperty).UpdateTarget();
        }

        private void SetSelectionIndex()
        {
            if (DBList.Items.Count > 0)
            {
                DBList.SelectedIndex = 0;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
