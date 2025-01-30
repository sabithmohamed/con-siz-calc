using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Idibri.RevitPlugin.Common;
using Idibri.RevitPlugin.Common.Infrastructure;
using Idibri.RevitPlugin.ConduitSizeCalculator.Models;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.ViewModels
{
    public class ViewModelIoc
    {
        public delegate void OnDoneDelegate();
        public delegate string GetSaveFileNameDelegate(FileSearchOptions fileSearchOptions);
        public delegate void AlertUserDelegate(string message, string title);
        public delegate CommandSettings LoadSettingsDelegate();

        public OnDoneDelegate OnDone { get; set; }
        public GetSaveFileNameDelegate GetSaveFileName { get; set; }
        public AlertUserDelegate AlertUser { get; set; }
        public LoadSettingsDelegate LoadSettings { get; set; }
    }

    public class ViewModeParts
    {
        public Predicate<EditMasterViewModel> CanGoToViewMode { get; set; }
        public string AssociatedTitle { get; set; }
    }

    public class EditMasterViewModel : ViewModelBase
    {
        #region Properties
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        private string _title = "Conduit Calculator Settings";

        public CommandSettings CommandSettings
        {
            get { return _commandSettings; }
            set
            {
                if (_commandSettings != value)
                {
                    _commandSettings = value;
                    NotifyPropertyChanged("CommandSettings");
                    EditElementsViewModel.CommandSettings = _commandSettings;
                    EditCommandSettingsViewModel.CommandSettings = _commandSettings;
                }
            }
        }

        private CommandSettings _commandSettings;
        public UIDocument _uiDoc_master;
        public EditElementsViewModel EditElementsViewModel
        {
            get
            {
                if (_editElementsViewModel == null)
                {
                    EditElementsViewModel = new EditElementsViewModel(CommandSettings, ConduitUpdater, null, _uiDoc_master);
                    EditElementsViewModel.RequestCloseWindow += OnRequestCloseWindow;

                }
                return _editElementsViewModel;
            }
            private set
            {
                if (_editElementsViewModel != value)
                {
                    if (_editElementsViewModel != null)
                    {
                        _editElementsViewModel.CommitCommand.CanExecuteChanged -= OnEditElementsViewModelCommitCommandCanExecuteChanged;
                    }

                    ChangeRegistration(_editCommandSettingsViewModel, value, OnEditElementsViewModelPropertyChanged);
                    _editElementsViewModel = value;
                    NotifyPropertyChanged("EditElementsViewModel");

                    if (_editElementsViewModel != null)
                    {
                        _editElementsViewModel.CommitCommand.CanExecuteChanged += OnEditElementsViewModelCommitCommandCanExecuteChanged;
                    }
                }
            }
        }
        private EditElementsViewModel _editElementsViewModel;

        private void OnRequestCloseWindow()
        {
            // Notify the command or parent to close the window
            RequestCloseWindow?.Invoke();
        }

        public EditCommandSettingsViewModel EditCommandSettingsViewModel
        {
            get
            {
                if (_editCommandSettingsViewModel == null)
                {
                    EditCommandSettingsViewModel = new EditCommandSettingsViewModel(CommandSettings);
                }
                return _editCommandSettingsViewModel;
            }
            private set
            {
                if (_editCommandSettingsViewModel != value)
                {
                    _editCommandSettingsViewModel = value;
                    NotifyPropertyChanged("EditCommandSettingsViewModel");
                }
            }
        }
        private EditCommandSettingsViewModel _editCommandSettingsViewModel;
        public List<Element> Elements
        {
            get { return _elements; }
            set
            {
                if (_elements != value)
                {
                    _elements = value;
                    NotifyPropertyChanged("Elements");
                    UpdateCommands(SetViewModeCommand);
                    EditElementsViewModel.Elements = value;
                    SetViewModeCommand.Execute("EditElements");
                }
            }
        }
        private List<Element> _elements;

        public IConduitCalculator ConduitUpdater
        {
            get { return _conduitUpdater; }
            private set
            {
                if (_conduitUpdater != value)
                {
                    _conduitUpdater = value;
                    NotifyPropertyChanged("ConduitUpdater");
                }
            }
        }
        private IConduitCalculator _conduitUpdater;

        public string ViewMode
        {
            get { return _viewMode; }
            private set
            {
                if (_viewMode != value)
                {
                    _viewMode = value;
                    NotifyPropertyChanged("ViewMode");
                }
            }
        }
        private string _viewMode = "ManageSettings";

        private static Dictionary<string, ViewModeParts> ViewModes = new Dictionary<string, ViewModeParts>()
        {
            { "EditElements", new ViewModeParts() 
                { 
                    CanGoToViewMode = vm => vm.Elements != null && vm.Elements.Count > 0,
                    AssociatedTitle = "Conduit Calculator"
                }
            },
            { "ManageSettings", new ViewModeParts()
                {
                    CanGoToViewMode = vm => true,
                    AssociatedTitle = "Conduit Calculator Settings"
                }
            }
        };

        public ViewModelIoc ViewModelIoc { get; private set; }
        #endregion

        #region Commands
        #region Set View Mode
        private bool SetViewModeCanExecute(object param)
        {
            string mode = (param ?? "").ToString();
            return ViewModes.ContainsKey(mode) && ViewModes[mode].CanGoToViewMode(this);
        }

        private void SetViewModeExecuted(object param)
        {
            if (SetViewModeCanExecute(param))
            {
                string mode = param.ToString();

                ViewMode = mode;
                Title = ViewModes[mode].AssociatedTitle;
            }
        }

        private RelayCommand _setViewModeCommand;
        public RelayCommand SetViewModeCommand
        {
            get
            {
                if (_setViewModeCommand == null)
                {
                    _setViewModeCommand = new RelayCommand(SetViewModeCanExecute, SetViewModeExecuted);
                }
                return _setViewModeCommand;
            }
        }

        public event Action RequestCloseWindow;

        #endregion

        #region Save Command Settings
        private bool SaveCommandSettingsCanExecute(object param)
        {
            return true;
        }

        private void SaveCommandSettingsExecuted(object param)
        {
            if (SaveCommandSettingsCanExecute(param))
            {
                if (CommandSettings.AssociatedFilename != null)
                {
                    Helpers.SaveXmlToFile<CommandSettings>(CommandSettings, CommandSettings.AssociatedFilename);
                }
                else
                {
                    SaveCommandSettingsAsCommand.Execute(param);
                }
            }
        }

        private RelayCommand _saveCommandSettingsCommand;
        public RelayCommand SaveCommandSettingsCommand
        {
            get
            {
                if (_saveCommandSettingsCommand == null)
                {
                    _saveCommandSettingsCommand = new RelayCommand(SaveCommandSettingsCanExecute, SaveCommandSettingsExecuted);
                }
                return _saveCommandSettingsCommand;
            }
        }
        #endregion

        #region Save Command Settings As
        private bool SaveCommandSettingsAsCanExecute(object param)
        {
            return true;
        }

        private void SaveCommandSettingsAsExecuted(object param)
        {
            if (SaveCommandSettingsAsCanExecute(param))
            {
                string newFileName = ViewModelIoc.GetSaveFileName(new FileSearchOptions()
                {
                    AddExtension = true,
                    DefaultExtension = ".xml",
                    Filter = "XML documents (.xml)|*.xml",
                    OverwritePrompt = true,
                    Title = "Save Command Settings",
                    FileName = CommandSettings.AssociatedFilename
                });

                if (newFileName != null)
                {
                    Helpers.SaveXmlToFile<CommandSettings>(CommandSettings, newFileName);
                    CommandSettings.AssociatedFilename = newFileName;
                }
            }
        }

        private RelayCommand _saveCommandSettingsAsCommand;
        public RelayCommand SaveCommandSettingsAsCommand
        {
            get
            {
                if (_saveCommandSettingsAsCommand == null)
                {
                    _saveCommandSettingsAsCommand = new RelayCommand(SaveCommandSettingsAsCanExecute, SaveCommandSettingsAsExecuted);
                }
                return _saveCommandSettingsAsCommand;
            }
        }
        #endregion

        #region Load Command Settings
        private bool LoadCommandSettingsCanExecute(object param)
        {
            return true;
        }

        private void LoadCommandSettingsExecuted(object param)
        {
            if (LoadCommandSettingsCanExecute(param))
            {
                CommandSettings cs = ViewModelIoc.LoadSettings();
                if (cs != null)
                {
                    if (cs.FileLoadException != null)
                    {
                        CommandSettings.FileLoadException = cs.FileLoadException;
                        ViewModelIoc.AlertUser(CommandSettings.FileLoadException.Message, "An Error Occurred When Loading Command Settings");
                    }
                    else
                    {
                        CommandSettings = cs;
                    }
                }
            }
        }

        private RelayCommand _loadCommandSettingsCommand;
        public RelayCommand LoadCommandSettingsCommand
        {
            get
            {
                if (_loadCommandSettingsCommand == null)
                {
                    _loadCommandSettingsCommand = new RelayCommand(LoadCommandSettingsCanExecute, LoadCommandSettingsExecuted);
                }
                return _loadCommandSettingsCommand;
            }
        }
        #endregion

        #region Done
        private bool DoneCanExecute(object param)
        {
            return EditElementsViewModel.CommitCommand.CanExecute(param);
        }

        private void DoneExecuted(object param)
        {
            if (DoneCanExecute(param))
            {
                EditElementsViewModel.CommitCommand.Execute(null);
                EditElementsViewModel.SetMarkCommand.Execute(null);
                ViewModelIoc.OnDone();
                EditElementsViewModel.SetMark(null);

            }
        }

        private RelayCommand _doneCommand;
        public RelayCommand DoneCommand
        {
            get
            {
                if (_doneCommand == null)
                {
                    _doneCommand = new RelayCommand(DoneCanExecute, DoneExecuted);
                }
                return _doneCommand;
            }
        }

        #endregion

        #region Cancel
        private bool CancelCanExecute(object param)
        {
            return true;
        }

        private void CancelExecuted(object param)
        {
            if (CancelCanExecute(param))
            {
                ViewModelIoc.OnDone();
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(CancelCanExecute, CancelExecuted);
                }
                return _cancelCommand;
            }
        }
        #endregion
        #endregion

        #region Constructors
        public EditMasterViewModel(CommandSettings commandSettings, IConduitCalculator conduitUpdater, ViewModelIoc viewModelIoc, UIDocument uiDoc)
            : base()
        {
            ConduitUpdater = conduitUpdater;
            ViewModelIoc = viewModelIoc;
            _uiDoc_master = uiDoc;
            CommandSettings = commandSettings;
        }

        #endregion

        #region Methods

        #endregion

        #region Event Handlers
        private void OnEditElementsViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentCableSchedule")
            {
                EditCommandSettingsViewModel.SelectedCableGroup = (sender as EditElementsViewModel).CurrentCableSchedule;
            }
        }

        private void OnEditElementsViewModelCommitCommandCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateCommands(DoneCommand);
        }
        #endregion
    }
}
