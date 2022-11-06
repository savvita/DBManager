using DBManager.Model;
using DBManager.ViewModel;
using System.Windows;

namespace DBManager.View
{
    /// <summary>
    /// Interaction logic for AddRowView.xaml
    /// </summary>
    public partial class AddRowView : Window
    {
        public DBTableRow? Row { get; set; }
        private RowControl? rowControl;
        public AddRowView()
        {
            InitializeComponent();
        }

        public AddRowView(string tableName, DBTableRow row) : this()
        {
            this.DataContext = new RowViewModel(tableName);
            this.Row = row;
            rowControl = new RowControl(row);
            RowContainer.Children.Add(rowControl);
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if(rowControl == null)
            {
                this.DialogResult = false;
                this.Close();
            }

            this.Row = rowControl!.GetRow();

            this.DialogResult = true;
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
