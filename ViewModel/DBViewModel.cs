using DBManager.Model;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace DBManager.ViewModel
{

    public class DBViewModel : INotifyPropertyChanged
    {
        private string db;
        private string server;
        private List<DBTable>? tables;
        public ObservableCollection<TreeNode> Nodes { get; set; } = new ObservableCollection<TreeNode>();

        private TreeNode? selectedTable;
        public TreeNode? SelectedTable
        {
            get => selectedTable;
            set
            {
                selectedTable = value;
                if(selectedTable != null)
                {
                    TreeNode table = GetParentNode(selectedTable);
                    FillTable(table.Name);
                    SelectedTableName = table.Name;
                }
            }
        }

        private string? selectedTableName;
        public string? SelectedTableName
        {
            get => selectedTableName;
            set
            {
                selectedTableName = value;
                OnPropertyChanged(nameof(SelectedTableName));
            }
        }

        private int? selectedRowIndex;
        public int? SelectedRowIndex
        {
            get => selectedRowIndex;
            set
            {
                selectedRowIndex = value;
                OnPropertyChanged(nameof(SelectedRowIndex));
            }
        }

        private DataView? tableDataView;
        public DataView? TableDataView
        {
            get => tableDataView;
            set
            {
                tableDataView = value;
                OnPropertyChanged(nameof(TableDataView));
            }
        }

        public DBViewModel(string serverName, string dbName)
        {
            this.server = serverName;
            this.db = dbName;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void FillTable(string? tableName)
        {
            if(tableName == null)
            {
                return;
            }

            DataTable table = await DBData.GetTableValues(server, db, tableName);
            TableDataView = table.DefaultView;
        }

        public async Task FillTree()
        {
            Nodes.Clear();

            tables = await DBData.GetDBSchema(server, db);

            foreach(DBTable table in tables)
            {
                TreeNode tableNode = new TreeNode()
                {
                    Name = table.TableName
                };

                TreeNode columnsNode = new TreeNode()
                {
                    Name = "Columns",
                    ParentNode = tableNode
                };

                foreach(DBTableColumn column in table.Columns)
                {
                    string name = $"{column.ColumnName!} ";                   
                    name += $"({column.ColumnDataType}{(column.CharacterMaximumLength != null ? $"({column.CharacterMaximumLength})" : "")}), ";
                    name += $"{(column.IsPrimaryKey ? "PK, " : "")}";
                    name += $"{(column.IsNullable ? "NULL" : "NOT NULL")}";

                    columnsNode.Nodes.Add(new TreeNode()
                    {
                        Name = name,
                        ParentNode = columnsNode
                    });
                }

                TreeNode fkNode = new TreeNode()
                {
                    Name = "Foreign Keys",
                    ParentNode = tableNode
                };

                foreach (string key in table.ForeignKeys)
                {
                    fkNode.Nodes.Add(new TreeNode()
                    {
                        Name = key,
                        ParentNode = fkNode
                    });
                }

                TreeNode indexesNode = new TreeNode()
                {
                    Name = "Indexes",
                    ParentNode = tableNode
                };

                foreach (string index in table.Indexes)
                {
                    indexesNode.Nodes.Add(new TreeNode()
                    {
                        Name = index,
                        ParentNode = indexesNode
                    });
                }


                tableNode.Nodes.Add(columnsNode);
                tableNode.Nodes.Add(fkNode);
                tableNode.Nodes.Add(indexesNode);

                Nodes.Add(tableNode);
            }
        }

        private TreeNode GetParentNode(TreeNode node)
        {
            TreeNode parentNode = node;

            while(parentNode.ParentNode != null)
            {
                parentNode = parentNode.ParentNode;
            }

            return parentNode;
        }

        private RelayCommand? addRowCmd;

        public RelayCommand AddRowCmd
        {
            get => addRowCmd ?? new RelayCommand(() => AddRow?.Invoke(tables?.Where(table => table.TableName!.Equals(selectedTableName)).FirstOrDefault()));
        }

        private RelayCommand? editRowCmd;

        public RelayCommand EditRowCmd
        {
            get => editRowCmd ?? new RelayCommand(() => EditRow?.Invoke(GetRowByIndex()));
        }

        private RelayCommand? deleteRowCmd;

        public RelayCommand DeleteRowCmd
        {
            get => deleteRowCmd ?? new RelayCommand(async () =>
            {
                DBTableRow? row = GetRowByIndex();

                if (row != null)
                {
                    await DeleteRow(row);
                }
            });
        }


        public event Action<DBTable?>? AddRow;
        public event Action<DBTableRow?>? EditRow;

        public async Task InsertRow(DBTableRow values)
        {
            if (selectedTableName != null)
            {
                await DBData.InsertRow(server, db, selectedTableName, values);
                FillTable(selectedTableName);
            }
        }

        public async Task UpdateRow(DBTableRow values)
        {
            if (selectedTableName != null)
            {
                await DBData.UpdateRow(server, db, selectedTableName, values);
                FillTable(selectedTableName);
            }
        }
        
        private async Task DeleteRow(DBTableRow values)
        {
            if (selectedTableName != null)
            {
                if (MessageBox.Show("Are you sure?", "Delete row", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
                {
                    await DBData.DeleteRow(server, db, selectedTableName, values);
                    FillTable(selectedTableName);
                }
            }
        }

        private DBTableRow? GetRowByIndex()
        {
            DBTable? table = tables?.Where(table => table.TableName!.Equals(selectedTableName)).FirstOrDefault();
            if (table == null || selectedRowIndex == null || TableDataView == null || TableDataView.Table == null)
            {
                return null;
            }

            DBTableRow row = new DBTableRow(table);

            foreach(var column in row.Values.Keys)
            {
                try
                {
                    row.Values[column] = TableDataView.Table.Rows[(int)selectedRowIndex][column];
                }
                catch { }
            }

            return row;
        }
    }
}
