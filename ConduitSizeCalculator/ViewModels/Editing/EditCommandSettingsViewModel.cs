using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Idibri.RevitPlugin.Common.Infrastructure;
using Idibri.RevitPlugin.ConduitSizeCalculator.Models;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.ViewModels
{
    public class EditCommandSettingsViewModel : ViewModelBase
    {
        #region Properties
        public CommandSettings CommandSettings
        {
            get { return _commandSettings; }
            set
            {
                if (_commandSettings != value)
                {
                    _commandSettings = value;
                    NotifyPropertyChanged("CommandSettings");
                    Initialize();
                }
            }
        }
        private CommandSettings _commandSettings;
        #endregion

        #region Cable Management
        #region Properties
        public string SelectedCableGroup
        {
            get { return _selectedCableGroup; }
            set
            {
                if (_selectedCableGroup != value)
                {
                    _selectedCableGroup = value;
                    NotifyPropertyChanged("SelectedCableGroup");
                    UpdateCablesOfGroup();
                    SelectedCable = null;
                    UpdateCommands(CreateCableCommand);
                }
            }
        }
        private string _selectedCableGroup;

        public Cable SelectedCable
        {
            get { return _selectedCable; }
            set
            {
                if (_selectedCable != value)
                {
                    _selectedCable = value;
                    NotifyPropertyChanged("SelectedCable");
                }
            }
        }
        private Cable _selectedCable;

        public ObservableCollection<Cable> SelectedCables
        {
            get
            {
                if (_selectedCables == null)
                {
                    _selectedCables = new ObservableCollection<Cable>();
                    _selectedCables.CollectionChanged += OnSelectedCableCollectionChanged;
                }
                return _selectedCables;
            }
        }
        private ObservableCollection<Cable> _selectedCables;

        public List<Cable> CablesOfGroup
        {
            get
            {
                return _cablesOfGroup;
            }
            private set
            {
                if (_cablesOfGroup != value)
                {
                    _cablesOfGroup = value;
                    NotifyPropertyChanged("CablesOfGroup");
                }
            }
        }
        private List<Cable> _cablesOfGroup;

        public string NewCableGroupName
        {
            get { return _newCableGroupName; }
            set
            {
                value = ReduceString(value);
                if (_newCableGroupName != value)
                {
                    _newCableGroupName = value;
                    NotifyPropertyChanged("NewCableGroupName");
                    UpdateCommands(CreateCableGroupCommand);
                }
            }
        }
        private string _newCableGroupName;

        private bool IgnoreCableChanges { get; set; }
        #endregion

        #region Commands
        #region Create Cable
        private bool CreateCableCanExecute(object param)
        {
            return SelectedCableGroup != null;
        }

        private void CreateCableExecuted(object param)
        {
            if (CreateCableCanExecute(param))
            {
                Cable newCable = new Cable()
                {
                    GroupName = SelectedCableGroup,
                    Name = CommandSettings.CableSchedule.FirstUniqueName(SelectedCableGroup, "A")
                };
                CommandSettings.CableSchedule.AddCable(newCable);
                UpdateCablesOfGroup();
                SelectedCable = newCable;
            }
        }

        private RelayCommand _createCableCommand;
        public RelayCommand CreateCableCommand
        {
            get
            {
                if (_createCableCommand == null)
                {
                    _createCableCommand = new RelayCommand(CreateCableCanExecute, CreateCableExecuted);
                }
                return _createCableCommand;
            }
        }
        #endregion

        #region Delete Cable
        private bool DeleteCableCanExecute(object param)
        {
            return SelectedCables.Count > 0;
        }

        private void DeleteCableExecuted(object param)
        {
            if (DeleteCableCanExecute(param))
            {
                foreach (Cable cable in SelectedCables.ToList())
                {
                    CommandSettings.CableSchedule.RemoveCable(cable);
                }
                if (CommandSettings.CableSchedule.CableGroups.FirstOrDefault(cb => cb == SelectedCableGroup) == null)
                {
                    SelectedCableGroup = CommandSettings.CableSchedule.CableGroups.FirstOrDefault();
                }
                SelectedCable = null;
                UpdateCablesOfGroup();
            }
        }

        private RelayCommand _deleteCableCommand;
        public RelayCommand DeleteCableCommand
        {
            get
            {
                if (_deleteCableCommand == null)
                {
                    _deleteCableCommand = new RelayCommand(DeleteCableCanExecute, DeleteCableExecuted);
                }
                return _deleteCableCommand;
            }
        }
        #endregion

        #region Create Cable Group
        private bool CreateCableGroupCanExecute(object param)
        {
            return NewCableGroupName != null && !CommandSettings.CableSchedule.CableGroups.Any(cb => cb == NewCableGroupName);
        }

        private void CreateCableGroupExecuted(object param)
        {
            if (CreateCableGroupCanExecute(param))
            {
                Cable newCable = new Cable()
                {
                    GroupName = NewCableGroupName,
                    Name = CommandSettings.CableSchedule.FirstUniqueName(SelectedCableGroup, "A")
                };
                CommandSettings.CableSchedule.AddCable(newCable);
                SelectedCableGroup = newCable.GroupName;
                SelectedCable = newCable;
                NewCableGroupName = null;
            }
        }

        private RelayCommand _createCableGroupCommand;
        public RelayCommand CreateCableGroupCommand
        {
            get
            {
                if (_createCableGroupCommand == null)
                {
                    _createCableGroupCommand = new RelayCommand(CreateCableGroupCanExecute, CreateCableGroupExecuted);
                }
                return _createCableGroupCommand;
            }
        }
        #endregion
        #endregion

        #region Methods
        private void InitializeCableManagement()
        {
            SelectedCableGroup = CommandSettings != null ? CommandSettings.CableSchedule.CableGroups.FirstOrDefault() : null;
            SelectedCable = null;
            NewCableGroupName = null;
        }

        private void UpdateCablesOfGroup()
        {
            CablesOfGroup = new List<Cable>(CommandSettings.CableSchedule.Cables.Where(cb => cb.GroupName == SelectedCableGroup).OrderBy(cb => cb.Name));
        }
        #endregion

        #region Event Handlers
        private void OnSelectedCableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (object o in e.OldItems)
                {
                    (o as Cable).PropertyChanged -= OnSelectedCablePropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (object o in e.NewItems)
                {
                    (o as Cable).PropertyChanged += OnSelectedCablePropertyChanged;
                }
            }

            UpdateCommands(DeleteCableCommand);
        }

        private void OnSelectedCablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IgnoreCableChanges) { return; }

            if (e.PropertyName == "IsActive")
            {
                IgnoreCableChanges = true;
                bool isActive = (sender as Cable).IsActive;

                try
                {
                    foreach (Cable cable in SelectedCables)
                    {
                        cable.IsActive = isActive;
                    }
                }
                finally
                {
                    IgnoreCableChanges = false;
                }
            }
        }
        #endregion
        #endregion

        #region Conduit Management
        #region Properties
        public string SelectedConduitType
        {
            get { return _selectedConduitType; }
            set
            {
                if (_selectedConduitType != value)
                {
                    _selectedConduitType = value;
                    NotifyPropertyChanged("SelectedConduitType");
                    UpdateConduitsOfType();
                    UpdateCommands(CreateConduitCommand);
                }
            }
        }
        private string _selectedConduitType;

        public Conduit SelectedConduit
        {
            get { return _selectedConduit; }
            set
            {
                if (_selectedConduit != value)
                {
                    _selectedConduit = value;
                    NotifyPropertyChanged("SelectedConduit");
                }
            }
        }
        private Conduit _selectedConduit;

        public ObservableCollection<Conduit> SelectedConduits
        {
            get
            {
                if (_selectedConduits == null)
                {
                    _selectedConduits = new ObservableCollection<Conduit>();
                    _selectedConduits.CollectionChanged += OnSelectedConduitCollectionChanged;
                }
                return _selectedConduits;
            }
        }
        private ObservableCollection<Conduit> _selectedConduits;

        public List<Conduit> ConduitsOfType
        {
            get { return _conduitsOfType; }
            private set
            {
                if (_conduitsOfType != value)
                {
                    _conduitsOfType = value;
                    NotifyPropertyChanged("ConduitsOfType");
                }
            }
        }
        private List<Conduit> _conduitsOfType;

        public string NewConduitTypeName
        {
            get { return _newConduitTypeName; }
            set
            {
                if (_newConduitTypeName != value)
                {
                    _newConduitTypeName = value;
                    NotifyPropertyChanged("NewConduitTypeName");
                    UpdateCommands(CreateConduitTypeCommand);
                }
            }
        }
        private string _newConduitTypeName;

        private bool IgnoreConduitChanges { get; set; }
        #endregion

        #region Commands
        #region Create Conduit
        private bool CreateConduitCanExecute(object param)
        {
            return SelectedConduitType != null;
        }

        private void CreateConduitExecuted(object param)
        {
            if (CreateConduitCanExecute(param))
            {
                Conduit newConduit = new Conduit(SelectedConduitType, "0\" " + SelectedConduitType, 0, 0, 0);
                CommandSettings.ConduitSchedule.AddConduit(newConduit);
                UpdateConduitsOfType();
                SelectedConduit = newConduit;
            }
        }

        private RelayCommand _createConduitCommand;
        public RelayCommand CreateConduitCommand
        {
            get
            {
                if (_createConduitCommand == null)
                {
                    _createConduitCommand = new RelayCommand(CreateConduitCanExecute, CreateConduitExecuted);
                }
                return _createConduitCommand;
            }
        }
        #endregion

        #region Delete Conduit
        private bool DeleteConduitCanExecute(object param)
        {
            return SelectedConduits.Count > 0;
        }

        private void DeleteConduitExecuted(object param)
        {
            if (DeleteConduitCanExecute(param))
            {
                foreach (Conduit conduit in SelectedConduits.ToList())
                {
                    CommandSettings.ConduitSchedule.RemoveConduit(conduit);
                }
                if (CommandSettings.ConduitSchedule.ConduitTypes.FirstOrDefault(ct => ct == SelectedConduitType) == null)
                {
                    SelectedConduitType = CommandSettings.ConduitSchedule.ConduitTypes.FirstOrDefault();
                }
                UpdateConduitsOfType();
            }
        }

        private RelayCommand _deleteConduitCommand;
        public RelayCommand DeleteConduitCommand
        {
            get
            {
                if (_deleteConduitCommand == null)
                {
                    _deleteConduitCommand = new RelayCommand(DeleteConduitCanExecute, DeleteConduitExecuted);
                }
                return _deleteConduitCommand;
            }
        }
        #endregion

        #region Create Conduit Type
        private bool CreateConduitTypeCanExecute(object param)
        {
            return NewConduitTypeName != null && !CommandSettings.ConduitSchedule.ConduitTypes.Any(ct => ct == NewConduitTypeName);
        }

        private void CreateConduitTypeExecuted(object param)
        {
            if (CreateConduitTypeCanExecute(param))
            {
                Conduit newConduit = new Conduit()
                {
                    Type = NewConduitTypeName,
                    Name = "0\" " + NewConduitTypeName
                };
                CommandSettings.ConduitSchedule.AddConduit(newConduit);
                SelectedConduitType = newConduit.Type;
                SelectedConduit = newConduit;
                NewConduitTypeName = null;
            }
        }

        private RelayCommand _createConduitTypeCommand;
        public RelayCommand CreateConduitTypeCommand
        {
            get
            {
                if (_createConduitTypeCommand == null)
                {
                    _createConduitTypeCommand = new RelayCommand(CreateConduitTypeCanExecute, CreateConduitTypeExecuted);
                }
                return _createConduitTypeCommand;
            }
        }
        #endregion
        #endregion

        #region Methods
        private void InitializeConduitManagement()
        {
            SelectedConduitType = CommandSettings != null ? CommandSettings.ConduitSchedule.ConduitTypes.FirstOrDefault() : null;
            SelectedConduit = null;
            NewConduitTypeName = null;
        }

        private void UpdateConduitsOfType()
        {
            ConduitsOfType = new List<Conduit>(CommandSettings.ConduitSchedule.Conduits.Where(ct => ct.Type == SelectedConduitType).OrderBy(ct => ct.TradeSizeIn));
        }
        #endregion

        #region Event Handlers
        private void OnSelectedConduitCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (object o in e.OldItems)
                {
                    (o as Conduit).PropertyChanged -= OnSelectedConduitPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (object o in e.NewItems)
                {
                    (o as Conduit).PropertyChanged += OnSelectedConduitPropertyChanged;
                }
            }

            UpdateCommands(DeleteConduitCommand);
        }

        private void OnSelectedConduitPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IgnoreConduitChanges) { return; }

            if (e.PropertyName == "IsActive")
            {
                IgnoreConduitChanges = true;
                bool isActive = (sender as Conduit).IsActive;

                try
                {
                    foreach (Conduit conduit in SelectedConduits)
                    {
                        conduit.IsActive = isActive;
                    }
                }
                finally
                {
                    IgnoreConduitChanges = false;
                }
            }
        }
        #endregion
        #endregion

        #region Constructors
        public EditCommandSettingsViewModel(CommandSettings commandSettings)
        {
            CommandSettings = commandSettings;
            IgnoreCableChanges = false;
            IgnoreConduitChanges = false;
        }
        #endregion

        #region Methods
        private void Initialize()
        {
            InitializeCableManagement();
            InitializeConduitManagement();
        }
        #endregion
    }
}
