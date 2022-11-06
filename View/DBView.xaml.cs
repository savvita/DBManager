using DBManager.Model;
using DBManager.ViewModel;
using System.Windows;

namespace DBManager.View
{
    /// <summary>
    /// Interaction logic for DBView.xaml
    /// </summary>
    public partial class DBView : Window
    {
        private DBViewModel? model;
        public DBView()
        {
            InitializeComponent();
        }

        public DBView(DBViewModel vm) : this()
        {
            model = vm;
            model.AddRow += AddRow;
            model.EditRow += EditRow;
            this.DataContext = model;
        }

        private async void AddRow(DBTable? table)
        {
            if (table == null || model == null)
            {
                return;
            }

            DBTableRow row = new DBTableRow(table);

            AddRowView view = new AddRowView(table.TableName!, row);
            view.ShowDialog();
            if (view.DialogResult == true && view.Row != null)
            {
                await model.InsertRow(view.Row);
            }
        }
        private async void EditRow(DBTableRow? row)
        {
            if (row == null || model == null || model.SelectedTableName == null)
            {
                return;
            }

            EditRowView view = new EditRowView(model.SelectedTableName, row);
            view.ShowDialog();
            if (view.DialogResult == true && view.Row != null)
            {
                await model.UpdateRow(view.Row);
            }
        }
    }
}
