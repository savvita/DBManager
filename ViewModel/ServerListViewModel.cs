using DBManager.Model;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DBManager.ViewModel
{
    internal class ServerListViewModel
    {
        public ObservableCollection<string> Servers { get; private set; }
        public ObservableCollection<string> DataBases { get; private set; }

        public string? ConnectionMessage { get; private set; }

        private string? selectedServer;
        public string? SelectedServer
        {
            get => selectedServer;

            set
            {
                selectedServer = value;
                if (selectedServer != null)
                {
                    Initialize();
                }
            }
        }
        public string? SelectedDB { get; set; }


        public ServerListViewModel()
        {
            Servers = new ObservableCollection<string>
            {
                @"localhost\SQLEXPRESS"
            };
            DataBases = new ObservableCollection<string>();
        }

        public async void Initialize()
        {
            if(SelectedServer == null)
            {
                return;
            }

            ConnectionMessage = "Connecting...";
            ConnectionChanged?.Invoke();
            try
            {
                List<string> db = await DBData.GetDBNames(selectedServer!);

                foreach(string dbName in db)
                {
                    DataBases.Add(dbName);
                }
                ConnectionMessage = String.Empty;
                
            }
            catch(Exception ex)
            {
                ConnectionMessage = ex.Message;
            }
            ConnectionChanged?.Invoke();
            DBNamesLoaded?.Invoke();
        }


        public event Action? ConnectionChanged;
        public event Action? DBNamesLoaded;
        public event Action<DBViewModel>? DBConnected;

        private readonly RelayCommand? connectCmd;

        public RelayCommand ConnectCmd
        {
            get => connectCmd ?? new RelayCommand(async () => await ConnectToDB());
        }

        private async Task ConnectToDB()
        {
            if(SelectedServer == null || SelectedDB == null)
            {
                return;
            }

            ConnectionMessage = "Loading...";
            ConnectionChanged?.Invoke();
            DBViewModel vm = new DBViewModel(SelectedServer, SelectedDB);
            await vm.FillTree();

            ConnectionMessage = String.Empty;
            ConnectionChanged?.Invoke();

            DBConnected?.Invoke(vm);
        }

    }
}
