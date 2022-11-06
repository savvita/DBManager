using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace DBManager.View
{
    /// <summary>
    /// Interaction logic for CellInputControl.xaml
    /// </summary>
    public partial class CellInputControl : UserControl, INotifyPropertyChanged
    {
        private bool isReadOnly;
        public bool IsReadOnly
        {
            get => isReadOnly;
            set
            {
                isReadOnly = value;
                OnPropertyChanged(nameof(IsReadOnly));
            }
        }
        public string? ColumnName { get; set; }
        public object? Value { get; set; }
        public CellInputControl()
        {
            InitializeComponent();
        }

        public CellInputControl(string columnName) : this()
        {
            ColumnName = columnName;
            this.DataContext = this;
        }

        public CellInputControl(string columnName, object? value) : this(columnName)
        {
            this.Value = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
