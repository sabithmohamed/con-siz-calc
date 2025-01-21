using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Idibri.RevitPlugin.Common.Infrastructure;
using Idibri.RevitPlugin.ConduitSizeCalculator.Models;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.ViewModels
{
    public class EditElementsViewModel : ViewModelBase
    {
        #region Properties
        public CommandSettings CommandSettings
        {
            get { return _commandSettings; }
            set
            {
                if (_commandSettings != value)
                {
                    ChangeRegistration(_commandSettings, value, OnCommandSettingsPropertyChanged);
                    _commandSettings = value;
                    NotifyPropertyChanged("CommandSettings");
                    MaxCableAreaPercent = _commandSettings.DefaultMaxCableAreaPercent;
                    UpdateWorksetCableSchedulePair();
                    UpdateConduitModels();
                }
            }
        }
        private CommandSettings _commandSettings;

        public ConduitViewModel Conduit1
        {
            get
            {
                if (_conduit1 == null)
                {
                    Conduit1 = new ConduitViewModel(
                        JunctionBoxConduits.Conduit1,
                        CommandSettings.ConduitSchedule,
                        MaxCableAreaPercent,
                        CommandSettings.DefaultConduitType);
                }
                return _conduit1;
            }
            private set
            {
                if (_conduit1 != value)
                {
                    ChangeRegistration(_conduit1, value, OnConduitPropertyChanged);
                    _conduit1 = value;
                    NotifyPropertyChanged("Conduit1");
                }
            }
        }
        private ConduitViewModel _conduit1;

        public ConduitViewModel Conduit2
        {
            get
            {
                if (_conduit2 == null)
                {
                    Conduit2 = new ConduitViewModel(
                        JunctionBoxConduits.Conduit2,
                        CommandSettings.ConduitSchedule,
                        MaxCableAreaPercent,
                        CommandSettings.DefaultConduitType);
                }
                return _conduit2;
            }
            private set
            {
                if (_conduit2 != value)
                {
                    ChangeRegistration(_conduit2, value, OnConduitPropertyChanged);
                    _conduit2 = value;
                    NotifyPropertyChanged("Conduit2");
                }
            }
        }
        private ConduitViewModel _conduit2;

        public ConduitViewModel Conduit3
        {
            get
            {
                if (_conduit3 == null)
                {
                    Conduit3 = new ConduitViewModel(
                        JunctionBoxConduits.Conduit3,
                        CommandSettings.ConduitSchedule,
                        MaxCableAreaPercent,
                        CommandSettings.DefaultConduitType);
                }
                return _conduit3;
            }
            private set
            {
                if (_conduit3 != value)
                {
                    ChangeRegistration(_conduit3, value, OnConduitPropertyChanged);
                    _conduit3 = value;
                    NotifyPropertyChanged("Conduit3");
                }
            }
        }
        private ConduitViewModel _conduit3;

        public ConduitViewModel Conduit4
        {
            get
            {
                if (_conduit4 == null)
                {
                    Conduit4 = new ConduitViewModel(
                        JunctionBoxConduits.Conduit4,
                        CommandSettings.ConduitSchedule,
                        MaxCableAreaPercent,
                        CommandSettings.DefaultConduitType);
                }
                return _conduit4;
            }
            set
            {
                if (_conduit4 != value)
                {
                    ChangeRegistration(_conduit4, value, OnConduitPropertyChanged);
                    _conduit4 = value;
                    NotifyPropertyChanged("Conduit4");
                }
            }
        }
        private ConduitViewModel _conduit4;

        public ConduitViewModel Conduit5
        {
            get
            {
                if (_conduit5 == null)
                {
                    Conduit5 = new ConduitViewModel(
                        JunctionBoxConduits.Conduit5,
                        CommandSettings.ConduitSchedule,
                        MaxCableAreaPercent,
                        CommandSettings.DefaultConduitType);
                }
                return _conduit5;
            }
            set
            {
                if (_conduit5 != value)
                {
                    ChangeRegistration(_conduit5, value, OnConduitPropertyChanged);
                    _conduit5 = value;
                    NotifyPropertyChanged("Conduit5");
                }
            }
        }
        private ConduitViewModel _conduit5;

        public ConduitViewModel Conduit6
        {
            get
            {
                if (_conduit6 == null)
                {
                    Conduit6 = new ConduitViewModel(
                        JunctionBoxConduits.Conduit6,
                        CommandSettings.ConduitSchedule,
                        MaxCableAreaPercent,
                        CommandSettings.DefaultConduitType);
                }
                return _conduit6;
            }
            set
            {
                if (_conduit6 != value)
                {
                    ChangeRegistration(_conduit6, value, OnConduitPropertyChanged);
                    _conduit6 = value;
                    NotifyPropertyChanged("Conduit6");
                }
            }
        }
        private ConduitViewModel _conduit6;

        public BoxViewModel BoxViewModel
        {
            get
            {
                if (_boxViewModel == null)
                {
                    BoxViewModel = new BoxViewModel();
                }
                return _boxViewModel;
            }
            set
            {
                if (_boxViewModel != value)
                {
                    ChangeRegistration(_boxViewModel, value, OnBoxViewModelPropertyChanged);
                    _boxViewModel = value;
                    NotifyPropertyChanged("BoxViewModel");
                }
            }
        }
        private BoxViewModel _boxViewModel;

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

        public double MaxCableAreaPercent
        {
            get { return _maxCableAreaPercent; }
            set
            {
                if (_maxCableAreaPercent != value)
                {
                    _maxCableAreaPercent = value;
                    NotifyPropertyChanged("MaxCableAreaPercent");

                    foreach (ConduitViewModel cvm in ConduitModels)
                    {
                        cvm.MaxCableAreaPercent = _maxCableAreaPercent;
                    }
                }
            }
        }
        private double _maxCableAreaPercent;

        public List<Element> Elements
        {
            get { return _elements; }
            set
            {
                if (_elements != value)
                {
                    _elements = value;
                    UpdateWorksetCableSchedulePair();
                    UpdateConduitModels();
                    UpdateBoxViewModel();
                }
            }
        }
        private List<Element> _elements = null;

        public WorksetCableSchedulePair WorksetCableSchedulePair
        {
            get { return _worksetCableSchedulePair; }
            set
            {
                if (_worksetCableSchedulePair != value)
                {
                    ChangeRegistration(_worksetCableSchedulePair, value, OnWorksetCableSchedulePairPropertyChanged);
                    _worksetCableSchedulePair = value;
                    UpdateCommands(CommitCommand);
                    NotifyPropertyChanged("WorksetCableSchedulePair", "CurrentCableSchedule");
                    UpdateAllConduits();
                }
            }
        }
        private WorksetCableSchedulePair _worksetCableSchedulePair;

        public string CurrentCableSchedule { get { return WorksetCableSchedulePair == null ? null : WorksetCableSchedulePair.CableSchedule; } }

        private IEnumerable<ConduitViewModel> ConduitModels
        {
            get
            {
                yield return Conduit1;
                yield return Conduit2;
                yield return Conduit3;
                yield return Conduit4;
                yield return Conduit5;
                yield return Conduit6;
            }
        }

        private Action OnCommit { get; set; }

        private bool PerformUpdates = false;

        public UIDocument _uiDoc_edit;

        private string _markText;

        // Properties
        public string MarkText
        {
            get { return _markText; }
            set
            {
                value = ReduceString(value);
                if (_markText != value)
                {
                    _markText = value;
                    NotifyPropertyChanged("MarkText");
                    //SetMark(null);
                    Update = true;
                }
            }
        }
        private bool Update { get; set; }

        public ICommand SetMarkCommand { get; }
        public Action CloseAction { get; set; }


        #region This part is for controlling the IsEnabled property of the rows based on whether or not the Fill1,Fill2.. parameters are available in the elements
        private bool available_Fill1;

        public bool Available_Fill1
        {
            get { return available_Fill1; }
            set
            {
                if (available_Fill1 != value)
                {
                    available_Fill1 = value;
                    OnPropertyChanged(nameof(Available_Fill1)); // Notify of the change if implementing INotifyPropertyChanged
                }
            }
        }

        private bool available_Fill2;

        public bool Available_Fill2
        {
            get { return available_Fill2; }
            set
            {
                if (available_Fill2 != value)
                {
                    available_Fill2 = value;
                    OnPropertyChanged(nameof(Available_Fill2)); // Notify of the change if implementing INotifyPropertyChanged
                }
            }
        }

        private bool available_Fill3;

        public bool Available_Fill3
        {
            get { return available_Fill3; }
            set
            {
                if (available_Fill3 != value)
                {
                    available_Fill3 = value;
                    OnPropertyChanged(nameof(Available_Fill3)); // Notify of the change if implementing INotifyPropertyChanged
                }
            }
        }

        private bool available_Fill4;

        public bool Available_Fill4
        {
            get { return available_Fill4; }
            set
            {
                if (available_Fill4 != value)
                {
                    available_Fill4 = value;
                    OnPropertyChanged(nameof(Available_Fill4)); // Notify of the change if implementing INotifyPropertyChanged
                }
            }
        }

        private bool available_Fill5;

        public bool Available_Fill5
        {
            get { return available_Fill5; }
            set
            {
                if (available_Fill5 != value)
                {
                    available_Fill5 = value;
                    OnPropertyChanged(nameof(Available_Fill5)); // Notify of the change if implementing INotifyPropertyChanged
                }
            }
        }

        private bool available_Fill6;

        public bool Available_Fill6
        {
            get { return available_Fill6; }
            set
            {
                if (available_Fill6 != value)
                {
                    available_Fill6 = value;
                    OnPropertyChanged(nameof(Available_Fill6)); // Notify of the change if implementing INotifyPropertyChanged
                }
            }
        }

        #endregion This part is for controlling the IsEnabled property of the rows based on whether or not the Fill1,Fill2.. parameters are available in the elements
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        #region Commands
        #region Recalculate
        private bool RecalculateCanExecute(object param)
        {
            return true;
        }

        private void RecalculateExecuted(object param)
        {
            if (RecalculateCanExecute(param))
            {
                UpdateAllConduits();
            }
        }

        private RelayCommand _recalculateCommand;
        public RelayCommand RecalculateCommand
        {
            get
            {
                if (_recalculateCommand == null)
                {
                    _recalculateCommand = new RelayCommand(RecalculateCanExecute, RecalculateExecuted);
                }
                return _recalculateCommand;
            }
        }
        #endregion

        #region Commit
        private bool CommitCanExecute(object param)
        {
            return WorksetCableSchedulePair != null && CurrentCableSchedule != null && (ConduitModels.Any(cf => cf.Update) || BoxViewModel.UpdateAny);
        }

        private void CommitExecuted(object param)
        {
            if (CommitCanExecute(param))
            {
                UpdateElements();
                OnCommit();
            }
        }

        private RelayCommand _commitCommand;
        public RelayCommand CommitCommand
        {
            get
            {
                if (_commitCommand == null)
                {
                    _commitCommand = new RelayCommand(CommitCanExecute, CommitExecuted);
                }
                return _commitCommand;
            }
        }
        #endregion
        #endregion

        #region Constructors
        public EditElementsViewModel(CommandSettings commandSettings, IConduitCalculator conduitUpdater, Action onCommit, UIDocument uiDoc)
            : base()
        {
            ConduitUpdater = conduitUpdater;
            CommandSettings = commandSettings;
            OnCommit = onCommit ?? (Action)(() => { });
            PerformUpdates = true;
            SetMarkCommand = new RelayCommand(SetMark);
            CloseWindowCommand = new RelayCommand(ExecuteCloseWindow);
            _uiDoc_edit = uiDoc;
            LoadInitialMarkValue();
        }
        #endregion

        #region Methods
 
        private void LoadInitialMarkValue()
        {
            var element = GetSelectedElement();
            if (element.Count >0)
                MarkText = element.Count > 1?"<Multiple Selection>":element.First().LookupParameter("Mark").AsString();

            #region  This part is for controlling the IsEnabled property of the rows based on whether or not the Fill1,Fill2.. parameters are available in the elements
            if (GetParam(element.First(), "Fill1") == null)
                Available_Fill1 = false;
            else { Available_Fill1 = true; }

            if (GetParam(element.First(), "Fill2") == null)
                Available_Fill2 = false;
            else { Available_Fill2 = true; }

            if (GetParam(element.First(), "Fill3") == null)
                Available_Fill3 = false;
            else { Available_Fill3 = true; }

            if (GetParam(element.First(), "Fill4") == null)
                Available_Fill4 = false;
            else { Available_Fill4 = true; }

            if (GetParam(element.First(), "Fill5") == null)
                Available_Fill5 = false;
            else { Available_Fill5 = true; }

            if (GetParam(element.First(), "Fill6") == null)
                Available_Fill6 = false;
            else { Available_Fill6 = true; }
            #endregion
        }
        private List<Element> GetSelectedElement()
        {
            var selectedIds = _uiDoc_edit.Selection.GetElementIds();
            //return selectedIds.Count == 1 ? _uiDoc_edit.Document.GetElement(selectedIds.First()) :null;
            return selectedIds.Select(x => _uiDoc_edit.Document.GetElement(x)).ToList();
        }

        public Parameter GetParam(Element element, String param_name)
        {
            Parameter mark = element.LookupParameter(param_name);
            if (mark == null)
            
            {
                Element typ = _uiDoc_edit.Document.GetElement(element.GetTypeId());
                mark = typ.LookupParameter(param_name);
            }
            return mark == null ? null : mark;
        }
        private void SetMark(Object a)
        {
            var element = GetSelectedElement();
            foreach (var ele in element)
            {
                if (ele != null && ele.LookupParameter("Mark") != null)
                {
                    using (var transaction = new Transaction(_uiDoc_edit.Document, "Set Mark Parameter"))
                    {
                        ele.LookupParameter("Mark").Set(MarkText);
                    }
                    CloseAction?.Invoke(); // Close the view after setting the parameter
                }
                else
                {
                    MessageBox.Show("Invalid selection or 'Mark' parameter missing.");
                }
                RequestCloseWindow?.Invoke();
            }
            
        }

        public ICommand CloseWindowCommand { get; }

        // Define an event to notify the request to close the window
        public event Action RequestCloseWindow;

        private void ExecuteCloseWindow(Object a)
        {
            // Raise the event when the command is executed
            RequestCloseWindow?.Invoke();
        }

        private new string ReduceString(string value)
        {
            return value?.Trim();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateConduitModels()
        {
            foreach (ConduitViewModel conduit in ConduitModels)
            {
                conduit.SetFromElements(Elements, CommandSettings.ConduitSchedule);
            }

            if (CommandSettings.ConduitSchedule.ConduitTypes != null)
            {
                if (CommandSettings.ConduitSchedule.ConduitTypes.Count() == 0)
                {
                    foreach (ConduitViewModel conduit in ConduitModels)
                    {
                        conduit.ConduitType = null;
                    }
                }
                else
                {
                    foreach (ConduitViewModel conduit in ConduitModels)
                    {
                        if (conduit.ConduitType == null || !CommandSettings.ConduitSchedule.ConduitTypes.Contains(conduit.ConduitType))
                        {
                            conduit.ConduitType = CommandSettings.ConduitSchedule.ConduitTypes.First();
                        }
                    }
                }
            }

            foreach (ConduitViewModel conduit in ConduitModels)
            {
                conduit.UpdateSize(CommandSettings, ConduitUpdater, GetCalculateConduitParameter());
            }
        }

        private void UpdateBoxViewModel()
        {
            BoxViewModel.SetFromElements(Elements);
        }

        private void UpdateWorksetCableSchedulePair()
        {
            if (Elements == null || Elements.Count == 0 || !AllAreInSameWorkset(Elements))
            {
                WorksetCableSchedulePair = null;
            }
            else
            {
                // ASSUMPTION: The Workset that the element belongs to is registered.
                Workset wks = ConduitUpdater.DocumentHelper.GetWorkset(Elements[0]);
                WorksetCableSchedulePair = CommandSettings.WorksetToCableScheduleMap.Pairs.First(p => p.WorksetName == wks.Name);
            }
        }

        private void UpdateElements()
        {
            foreach (ConduitViewModel conduit in ConduitModels)
            {
                conduit.SetToElements(Elements, CommandSettings, GetCalculateConduitParameter());
            }
            BoxViewModel.SetToElements(Elements);
            UpdateElementWorksets();
        }

        private void UpdateElementWorksets()
        {
            Workset workset = ConduitUpdater.DocumentHelper.GetWorkset(WorksetCableSchedulePair.WorksetName);
            int worksetId = workset.Id.IntegerValue;

            if (workset != null)
            {
                foreach (Element element in Elements)
                {
                    Parameter worksetParameter = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                    if (worksetParameter.AsInteger() != worksetId)
                    {
                        worksetParameter.Set(worksetId);
                    }
                }
            }
        }

        private bool AllAreInSameWorkset(IEnumerable<Element> elements)
        {
            int? worksetId = null;
            foreach (Element element in elements)
            {
                Parameter worksetParameter = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                if (!worksetId.HasValue)
                {
                    worksetId = worksetParameter.AsInteger();
                }
                else
                {
                    if (worksetId != worksetParameter.AsInteger())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private CalculateConduitParameter GetCalculateConduitParameter()
        {
            return new CalculateConduitParameter()
            {
                MaxCableAreaPercent = MaxCableAreaPercent,
                CableSchedule = CurrentCableSchedule,
                DefaultConduitType = CommandSettings.DefaultConduitType
            };
        }

        private void UpdateAllConduits()
        {
            if (!PerformUpdates) { return; }

            CalculateConduitParameter parameter = GetCalculateConduitParameter();
            foreach (ConduitViewModel conduit in ConduitModels)
            {
                conduit.UpdateSize(CommandSettings, ConduitUpdater, parameter);
            }
        }
        #endregion

        #region Event Handlers
        private void OnConduitPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Update":
                    UpdateCommands(CommitCommand);
                    SetMark(null);

                    break;
                case "Fill":
                case "ConduitType":
                    (sender as ConduitViewModel).UpdateSize(CommandSettings, ConduitUpdater, GetCalculateConduitParameter());
                    break;
            }
        }

        private void OnWorksetCableSchedulePairPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CableSchedule")
            {
                UpdateCommands(CommitCommand);
                NotifyPropertyChanged("CurrentCableSchedule");
                UpdateAllConduits();
            }
        }

        private void OnCommandSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DefaultMaxCableAreaPercent":
                    MaxCableAreaPercent = CommandSettings.DefaultMaxCableAreaPercent;
                    break;
            }
        }

        private void OnBoxViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateCommands(CommitCommand);
        }
        #endregion
    }

    public class ConduitViewModel : ViewModelBase
    {
        #region Properties
        public string Mark
        {
            get { return _mark; }
            set
            {
                value = ReduceString(value);
                if (_mark != value)
                {
                    _mark = value;
                    NotifyPropertyChanged("Mark");
                    Update = true;
                }
            }
        }
        private string _mark;

        public string Fill
        {
            get { return _fill; }
            set
            {
                value = ReduceString(value);
                if (_fill != value)
                {
                    _fill = value;
                    NotifyPropertyChanged("Fill");
                    Update = true;
                }
            }
        }
        private string _fill;

        public string ConduitType
        {
            get { return _conduitType; }
            set
            {
                value = ReduceString(value) ?? DefaultConduitType;
                if (_conduitType != value)
                {
                    _conduitType = value;
                    NotifyPropertyChanged("ConduitType");
                    ConduitsOfType = ConduitSchedule.GetConduitsOfType(_conduitType);
                }
            }
        }
        private string _conduitType;

        public string Size
        {
            get { return _size; }
            set
            {
                value = ReduceString(value);
                if (_size != value)
                {
                    _size = value;
                    NotifyPropertyChanged("Size");
                }
            }
        }
        private string _size;

        public string Destination
        {
            get { return _destination; }
            set
            {
                if (_destination != value)
                {
                    _destination = value;
                    NotifyPropertyChanged("Destination");
                }
            }
        }
        private string _destination;
        //Test comment
        public string CableDestination
        {
            get { return _cabledestination; }
            set
            {
                if (_cabledestination != value)
                {
                    _cabledestination = value;
                    NotifyPropertyChanged("Cable Destination");
                }
            }
        }
        private string _cabledestination;



        public bool Update
        {
            get { return _update; }
            set
            {
                if (_update != value)
                {
                    _update = value;
                    NotifyPropertyChanged("Update");
                }
            }
        }
        private bool _update;

        public IList<Conduit> ConduitsOfType
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
        private IList<Conduit> _conduitsOfType;

        public Conduit SelectedConduit
        {
            get { return _selectedConduit; }
            set
            {
                if (_selectedConduit != value)
                {
                    _selectedConduit = value;
                    NotifyPropertyChanged("SelectedConduit");

                    if (_selectedConduit != null)
                    {
                        Size = _selectedConduit.Name;
                    }
                    UpdateCableAreaPercent();
                }
            }
        }
        private Conduit _selectedConduit;

        public CableBundle CableBundle
        {
            get { return _cableBundle; }
            set
            {
                if (_cableBundle != value)
                {
                    _cableBundle = value;
                    NotifyPropertyChanged("CableBundle");
                    UpdateCableAreaPercent();
                }
            }
        }
        private CableBundle _cableBundle;

        public double MaxCableAreaPercent
        {
            get { return _maxCableAreaPercent; }
            set
            {
                if (_maxCableAreaPercent != value)
                {
                    _maxCableAreaPercent = value;
                    NotifyPropertyChanged("MaxCableAreaPercent");
                    UpdateIsTooFull();
                }
            }
        }
        private double _maxCableAreaPercent;

        public double? CableAreaPercent
        {
            get { return _cableAreaPercent; }
            private set
            {
                if (_cableAreaPercent != value)
                {
                    _cableAreaPercent = value;
                    NotifyPropertyChanged("CableAreaPercent");
                    UpdateIsTooFull();
                }
            }
        }
        private double? _cableAreaPercent;

        public bool IsTooFull
        {
            get { return _isTooFull; }
            set
            {
                if (_isTooFull != value)
                {
                    _isTooFull = value;
                    NotifyPropertyChanged("IsTooFull");
                }
            }
        }
        private bool _isTooFull;

        public ConduitSchedule ConduitSchedule { get; private set; }

        public string DefaultConduitType { get; private set; }

        public System.Windows.Visibility ElementVisibility { get; private set; }
        public ConduitParameters ConduitParameters { get; private set; }
        #endregion

        #region Constructors
        public ConduitViewModel(ConduitParameters conduitPropertyPair, ConduitSchedule conduitSchedule, double maxCableAreaPercent, string defaultConduitType)
            : base()
        {
            ConduitParameters = conduitPropertyPair;
            ConduitSchedule = conduitSchedule;
            MaxCableAreaPercent = maxCableAreaPercent;
            DefaultConduitType = defaultConduitType;
            ElementVisibility = System.Windows.Visibility.Visible;
        }
        #endregion

        #region Methods
        public void Clear()
        {
            Fill = null;
            ConduitType = null;
            Size = null;
            Update = false;
            Mark = null;
        }

        public void SetFromElements(IEnumerable<Element> elements, ConduitSchedule conduitSchedule)
        {
            Clear();
            if (elements == null) { return; }

            int i = 0;

            foreach (Element element in elements)
            {
                string fill = ConduitParameters.Fill.GetString(element);
                string conduitType = ConduitParameters.ConduitType.GetString(element);
                string size = ConduitParameters.Size.GetString(element);
                string destination = ConduitParameters.Destination.GetString(element);
                string cableDestination = ConduitParameters.CableDestination.GetString(element);
                string mark = ConduitParameters.Mark.GetString(element);

                if (i++ == 0)
                {
                    Fill = fill;
                    ConduitType = conduitType;
                    Size = size;
                    Destination = destination;
                    CableDestination = cableDestination;
                    Mark = mark;
                }
                else
                {
                    if (fill != Fill) { Fill = null; }
                    if (conduitType != ConduitType) { ConduitType = null; }
                    if (size != Size) { Size = null; }
                    if (destination != Destination) { Destination = null; }
                    if (mark != Mark) { Mark = null; }
                    if (cableDestination!= CableDestination) { CableDestination = null; }

                }
            }

            if (!conduitSchedule.ContainsConduitsOfType(ConduitType))
            {
                ConduitType = null;
            }

            Update = Fill != null && Destination != null && Mark != null;
        }

        public void UpdateSize(CommandSettings commandSettings, IConduitCalculator conduitUpdater, CalculateConduitParameter parameter)
        {
            if (!ConduitParameters.AutoCalculate) { return; }

            ConduitCalculationResult result = conduitUpdater.GetNewConduitSize(commandSettings, Fill, parameter, ConduitType, Size);
            SelectedConduit = result.Conduit;
            CableBundle = result.CableBundle;
            Size = result.Description;
        }

        public void SetToElements(IEnumerable<Element> elements, CommandSettings commandSettings, CalculateConduitParameter parameter)
        {
            if (!Update) { return; }
            foreach (Element element in elements)
            {
                ConduitParameters.Fill.SetString(element, Fill);
                ConduitParameters.Size.SetString(element, Size);
                ConduitParameters.ConduitType.SetString(element, ConduitType);
                ConduitParameters.Destination.SetString(element, Destination);
                ConduitParameters.CableDestination.SetString(element, CableDestination);
                ConduitParameters.Mark.SetString(element, Mark);

            }
        }

        private void UpdateCableAreaPercent()
        {
            if (CableBundle != null && CableBundle.CableAreaIn != null && SelectedConduit != null)
            {
                CableAreaPercent = CableBundle.CableAreaIn / SelectedConduit.InsideAreaIn;
            }
            else
            {
                CableAreaPercent = null;
            }
        }

        private void UpdateIsTooFull()
        {
            IsTooFull = CableAreaPercent.HasValue && CableAreaPercent.Value > MaxCableAreaPercent;
        }
        #endregion
    }

    public class BoxViewModel : ViewModelBase
    {
        #region Properties
        public string BackBoxProvidedBy
        {
            get { return _backBoxProvidedBy; }
            set
            {
                value = ReduceString(value);
                if (_backBoxProvidedBy != value)
                {
                    _backBoxProvidedBy = value;
                    NotifyPropertyChanged("BackBoxProvidedBy");
                    UpdateBoxProperties = true;
                }
            }
        }
        private string _backBoxProvidedBy;

        public string BackBoxInstalledBy
        {
            get { return _backBoxInstalledBy; }
            set
            {
                value = ReduceString(value);
                if (_backBoxInstalledBy != value)
                {
                    _backBoxInstalledBy = value;
                    NotifyPropertyChanged("BackBoxInstalledBy");
                    UpdateBoxProperties = true;
                }
            }
        }
        private string _backBoxInstalledBy;

        public string PanelProvidedBy
        {
            get { return _panelProvidedBy; }
            set
            {
                value = ReduceString(value);
                if (_panelProvidedBy != value)
                {
                    _panelProvidedBy = value;
                    NotifyPropertyChanged("PanelProvidedBy");
                    UpdateBoxProperties = true;
                }
            }
        }
        private string _panelProvidedBy;

        public string PanelInstalledBy
        {
            get { return _panelInstalledBy; }
            set
            {
                value = ReduceString(value);
                if (_panelInstalledBy != value)
                {
                    _panelInstalledBy = value;
                    NotifyPropertyChanged("PanelInstalledBy");
                    UpdateBoxProperties = true;
                }
            }
        }
        private string _panelInstalledBy;

        public string DeviceProvidedBy
        {
            get { return _deviceProvidedBy; }
            set
            {
                value = ReduceString(value);
                if (_deviceProvidedBy != value)
                {
                    _deviceProvidedBy = value;
                    NotifyPropertyChanged("DeviceProvidedBy");
                    UpdateBoxProperties = true;
                }
            }
        }
        private string _deviceProvidedBy;

        public string DeviceInstalledBy
        {
            get { return _deviceInstalledBy; }
            set
            {
                value = ReduceString(value);
                if (_deviceInstalledBy != value)
                {
                    _deviceInstalledBy = value;
                    NotifyPropertyChanged("DeviceInstalledBy");
                    UpdateBoxProperties = true;
                }
            }
        }
        private string _deviceInstalledBy;

        public bool IsCustomPanel
        {
            get { return _isCustomPanel; }
            set
            {
                if (_isCustomPanel != value)
                {
                    _isCustomPanel = value;
                    NotifyPropertyChanged("IsCustomPanel");
                    UpdateFlags = true;
                }
            }
        }
        private bool _isCustomPanel;

        public bool IsFlushMount
        {
            get { return _isFlushMount; }
            set
            {
                if (_isFlushMount != value)
                {
                    _isFlushMount = value;
                    NotifyPropertyChanged("IsFlushMount");
                    UpdateFlags = true;
                }
            }
        }
        private bool _isFlushMount;

        public string Notes
        {
            get { return _notes; }
            set
            {
                value = ReduceString(value);
                if (_notes != value)
                {
                    _notes = value;
                    NotifyPropertyChanged("Notes");
                    UpdateNotes = true;
                }
            }
        }
        private string _notes;

        public string NemaType
        {
            get { return _nemaType; }
            set
            {
                value = ReduceString(value);
                if (_nemaType != value)
                {
                    _nemaType = value;
                    NotifyPropertyChanged("NemaType");
                    UpdateNemaType = true;
                }
            }
        }
        private string _nemaType;

        public bool UpdateBoxProperties
        {
            get { return _updateBoxProperties; }
            set
            {
                if (_updateBoxProperties != value)
                {
                    _updateBoxProperties = value;
                    NotifyPropertyChanged("UpdateBoxProperties");
                }
            }
        }
        private bool _updateBoxProperties;

        public bool UpdateNotes
        {
            get { return _updateNotesProperties; }
            set
            {
                if (_updateNotesProperties != value)
                {
                    _updateNotesProperties = value;
                    NotifyPropertyChanged("UpdateNotes");
                }
            }
        }
        private bool _updateNotesProperties;

        public bool UpdateFlags
        {
            get { return _updateFlags; }
            set
            {
                if (_updateFlags != value)
                {
                    _updateFlags = value;
                    NotifyPropertyChanged("UpdateFlags");
                }
            }
        }
        private bool _updateFlags;

        public bool UpdateNemaType
        {
            get { return _updateNemaType; }
            set
            {
                if (_updateNemaType != value)
                {
                    _updateNemaType = value;
                    NotifyPropertyChanged("UpdateNemaType");
                }
            }
        }
        private bool _updateNemaType;

        public bool UpdateAny { get { return UpdateBoxProperties || UpdateNotes || UpdateFlags || UpdateNemaType; } }
        #endregion

        #region Constructors
        public BoxViewModel()
            : base()
        { }
        #endregion

        #region Methods
        public void Clear()
        {
            BackBoxInstalledBy = null;
            BackBoxProvidedBy = null;
            PanelInstalledBy = null;
            PanelProvidedBy = null;
            DeviceInstalledBy = null;
            DeviceProvidedBy = null;
            IsCustomPanel = false;
            IsFlushMount = false;
            Notes = null;
            UpdateBoxProperties = false;
            UpdateNotes = false;
            UpdateFlags = false;
            UpdateNemaType = false;
        }

        public void SetFromElements(IEnumerable<Element> elements)
        {
            Clear();
            if (elements == null) { return; }

            int i = 0;

            foreach (Element element in elements)
            {
                string backBoxProvidedBy = JunctionBoxParameters.BackBoxProvidedBy.GetString(element);
                string backBoxInstalledBy = JunctionBoxParameters.BackBoxInstalledBy.GetString(element);
                string panelProvidedBy = JunctionBoxParameters.PanelProvidedBy.GetString(element);
                string panelInstalledBy = JunctionBoxParameters.PanelInstalledBy.GetString(element);
                string deviceProvidedBy = JunctionBoxParameters.DeviceProvidedBy.GetString(element);
                string deviceInstalledBy = JunctionBoxParameters.DeviceInstalledBy.GetString(element);
                bool isCustomPanel = JunctionBoxParameters.CustomPanel.GetInt(element) == 0 ? false : true;
                bool isFlushMount = JunctionBoxParameters.FlushMount.GetInt(element) == 0 ? false : true;
                string notes = JunctionBoxParameters.Notes.GetString(element);
                string nemaType = JunctionBoxParameters.NemaType.GetString(element);

                if (i++ == 0)
                {
                    BackBoxProvidedBy = backBoxProvidedBy;
                    BackBoxInstalledBy = backBoxInstalledBy;
                    PanelProvidedBy = panelProvidedBy;
                    PanelInstalledBy = panelInstalledBy;
                    DeviceProvidedBy = deviceProvidedBy;
                    DeviceInstalledBy = deviceInstalledBy;
                    IsCustomPanel = isCustomPanel;
                    IsFlushMount = isFlushMount;
                    Notes = notes;
                    NemaType = nemaType;
                }
                else
                {
                    if (backBoxProvidedBy != BackBoxProvidedBy) { BackBoxProvidedBy = null; }
                    if (backBoxInstalledBy != BackBoxInstalledBy) { BackBoxInstalledBy = null; }
                    if (panelProvidedBy != PanelProvidedBy) { PanelProvidedBy = null; }
                    if (panelInstalledBy != PanelInstalledBy) { PanelInstalledBy = null; }
                    if (deviceProvidedBy != DeviceProvidedBy) { DeviceProvidedBy = null; }
                    if (deviceInstalledBy != DeviceInstalledBy) { DeviceInstalledBy = null; }

                    if (isCustomPanel != IsCustomPanel) { IsCustomPanel = false; }
                    if (isFlushMount != IsFlushMount) { IsFlushMount = false; }

                    if (notes != Notes) { Notes = null; }

                    if (nemaType != NemaType) { NemaType = null; }
                }

                UpdateBoxProperties = false;
                UpdateNotes = false;
                UpdateNemaType = false;
                UpdateFlags = false;
            }
        }

        public void SetToElements(IEnumerable<Element> elements)
        {
            if (elements != null && UpdateAny)
            {
                foreach (Element element in elements)
                {
                    if (UpdateBoxProperties)
                    {
                        JunctionBoxParameters.BackBoxProvidedBy.SetString(element, BackBoxProvidedBy);
                        JunctionBoxParameters.BackBoxInstalledBy.SetString(element, BackBoxInstalledBy);
                        JunctionBoxParameters.PanelProvidedBy.SetString(element, PanelProvidedBy);
                        JunctionBoxParameters.PanelInstalledBy.SetString(element, PanelInstalledBy);
                        JunctionBoxParameters.DeviceProvidedBy.SetString(element, DeviceProvidedBy);
                        JunctionBoxParameters.DeviceInstalledBy.SetString(element, DeviceInstalledBy);
                    }

                    if (UpdateFlags)
                    {
                        JunctionBoxParameters.CustomPanel.SetInt(element, IsCustomPanel ? 1 : 0);
                        JunctionBoxParameters.FlushMount.SetInt(element, IsFlushMount ? 1 : 0);
                    }

                    if (UpdateNemaType)
                    {
                        JunctionBoxParameters.NemaType.SetString(element, NemaType);
                    }

                    if (UpdateNotes)
                    {
                        JunctionBoxParameters.Notes.SetString(element, Notes);
                    }
                }
            }
        }
        #endregion

        #region newmodel

        #endregion
    }
}