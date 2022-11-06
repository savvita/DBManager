using DBManager.Model;
using DBManager.ViewModel;
using System.Windows;

namespace DBManager.View
{
    /// <summary>
    /// Interaction logic for EditRowView.xaml
    /// </summary>
    public partial class EditRowView : Window
    {
        private RowControl? rowControl;

        public EditRowView()
        {
            InitializeComponent();
        }

        public DBTableRow? Row { get; set; }

        public EditRowView(string tableName, DBTableRow row) : this()
        {
            this.DataContext = new RowViewModel(tableName);
            this.Row = row;
            rowControl = new RowControl(row);
            RowContainer.Children.Add(rowControl);
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (rowControl == null)
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
