using DBManager.Model;
using System.Windows.Controls;

namespace DBManager.View
{
    /// <summary>
    /// Interaction logic for RowControl.xaml
    /// </summary>
    public partial class RowControl : UserControl
    {
        private DBTableRow? row;

        public RowControl()
        {
            InitializeComponent();
        }

        public RowControl(DBTableRow row) : this()
        {
            this.row = row;
            foreach (string columnName in row.Values.Keys)
            {
                if (!columnName.ToLower().StartsWith("id"))
                {
                    this.CellsContainer.Children.Add(new CellInputControl(columnName, row.Values[columnName]));
                }
                else
                {
                    this.CellsContainer.Children.Add(new CellInputControl(columnName, row.Values[columnName]) { IsReadOnly = true });
                }
            }

        }

        public DBTableRow? GetRow()
        {
            if(row == null)
            {
                return null;
            }

            foreach (var cell in this.CellsContainer.Children)
            {
                if(cell is CellInputControl control)
                {
                    try
                    {
                        row.Values[control.ColumnName!] = control.Value;
                    }
                    catch { }
                }
            }

            return row;
        }
    }
}
