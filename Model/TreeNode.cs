using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBManager.Model
{
    public class TreeNode : INotifyPropertyChanged
    {
        public string? name;
        public string? Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public TreeNode? ParentNode { get; set; }
        public ObservableCollection<TreeNode> Nodes { get; set; } = new ObservableCollection<TreeNode>();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
