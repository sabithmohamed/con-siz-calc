using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Idibri.RevitPlugin.Common.Infrastructure;
using Idibri.RevitPlugin.ConduitSizeCalculator.Models;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.ViewModels
{
    public class ConduitDefinition
    {
        public int Index { get; private set; }
        public Conduit Conduit { get; private set; }
        public string Destination { get; private set; }

        public ConduitDefinition(int index, Conduit conduit, string destination)
        {
            Index = index;
            Conduit = conduit;
            Destination = destination;
        }
    }

    public class JunctionBox : ModelBase
    {
        #region Properties
        public Element Element { get; private set; }
        public string Mark { get; private set; }

        public double DepthIn { get; private set; }
        public double HeightIn { get; private set; }
        public double WidthIn { get; private set; }

        public ConduitDefinition Conduit1 { get; private set; }
        public ConduitDefinition Conduit2 { get; private set; }
        public ConduitDefinition Conduit3 { get; private set; }
        public ConduitDefinition Conduit4 { get; private set; }

        public IEnumerable<ConduitDefinition> Conduits
        {
            get
            {
                yield return Conduit1;
                yield return Conduit2;
                yield return Conduit3;
                yield return Conduit4;
            }
        }
        #endregion

        #region Constructors
        public JunctionBox(Element element, ConduitSchedule conduitSchedule, string defaultConduitType)
        {
            Element = element;
            Mark = Element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();

            // These values are in feet, we want it in inches.
            DepthIn = JunctionBoxParameters.Depth.GetDouble(Element) * 12.0;
            HeightIn = JunctionBoxParameters.Height.GetDouble(Element) * 12.0;
            WidthIn = JunctionBoxParameters.Width.GetDouble(Element) * 12.0;

            Conduit1 = GetConduit(1, JunctionBoxConduits.Conduit1, conduitSchedule, defaultConduitType);
            Conduit2 = GetConduit(2, JunctionBoxConduits.Conduit2, conduitSchedule, defaultConduitType);
            Conduit3 = GetConduit(3, JunctionBoxConduits.Conduit3, conduitSchedule, defaultConduitType);
            Conduit4 = GetConduit(4, JunctionBoxConduits.Conduit4, conduitSchedule, defaultConduitType);
        }
        #endregion

        #region Methods
        private ConduitDefinition GetConduit(int index, ConduitParameters parameters, ConduitSchedule conduitSchedule, string defaultConduitType)
        {
            string conduitType = ReduceString(parameters.ConduitType.GetString(Element)) ?? defaultConduitType;
            string size = parameters.Size.GetString(Element);
            Conduit conduit = null;

            if (conduitSchedule.ContainsConduit(conduitType, size))
            {
                conduit = conduitSchedule.GetConduit(conduitType, size);
            }

            return new ConduitDefinition(index, conduit, ReduceString(parameters.Destination.GetString(Element)));
        }
        #endregion
    }

    public class ReportingMasterViewModel : ViewModelBase
    {
        #region Properties
        static double JunctionBoxDepthBufferIn = 0.5; //10.0 / 8.0;

        private Func<IEnumerable<Element>> GetElements { get; set; }

        public DocumentHelper DocumentHelper
        {
            get { return _documentHelper; }
            set
            {
                if (_documentHelper != value)
                {
                    _documentHelper = value;
                    NotifyPropertyChanged("DocumentHelper");
                }
            }
        }
        private DocumentHelper _documentHelper;

        public CommandSettings CommandSettings
        {
            get { return _commandSettings; }
            set
            {
                if (_commandSettings != value)
                {
                    _commandSettings = value;
                    NotifyPropertyChanged("CommandSettings");
                }
            }
        }
        private CommandSettings _commandSettings;

        public List<JunctionBox> JunctionBoxes
        {
            get { return _junctionBoxes; }
            set
            {
                if (_junctionBoxes != value)
                {
                    _junctionBoxes = value;
                    NotifyPropertyChanged("JunctionBoxes");

                    OversizedConduitsReportingViewModel.JunctionBoxes = JunctionBoxes;
                    UnmatchedBoxReportingViewModel.JunctionBoxes = JunctionBoxes;
                    ExcessiveConduitAreaReportingViewModel.JunctionBoxes = JunctionBoxes;
                }
            }
        }
        private List<JunctionBox> _junctionBoxes;

        public OversizedConduitsReportingViewModel OversizedConduitsReportingViewModel
        {
            get
            {
                if (_oversizedConduitsReportingViewModel == null)
                {
                    OversizedConduitsReportingViewModel = new OversizedConduitsReportingViewModel(DocumentHelper, JunctionBoxDepthBufferIn);
                }
                return _oversizedConduitsReportingViewModel;
            }
            private set
            {
                if (_oversizedConduitsReportingViewModel != value)
                {
                    _oversizedConduitsReportingViewModel = value;
                    NotifyPropertyChanged("OversizedConduitsReportingViewModel");
                }
            }
        }
        private OversizedConduitsReportingViewModel _oversizedConduitsReportingViewModel;

        public UnmatchedBoxReportingViewModel UnmatchedBoxReportingViewModel
        {
            get
            {
                if (_unmatchedBoxReportingViewModel == null)
                {
                    UnmatchedBoxReportingViewModel = new UnmatchedBoxReportingViewModel(DocumentHelper);
                }
                return _unmatchedBoxReportingViewModel;
            }
            private set
            {
                if (_unmatchedBoxReportingViewModel != value)
                {
                    _unmatchedBoxReportingViewModel = value;
                    NotifyPropertyChanged("UnmatchedBoxReportingViewModel");
                }
            }
        }
        private UnmatchedBoxReportingViewModel _unmatchedBoxReportingViewModel;

        public ExcessiveConduitAreaReportingViewModel ExcessiveConduitAreaReportingViewModel
        {
            get
            {
                if (_excessiveConduitAreaReportingViewModel == null)
                {
                    ExcessiveConduitAreaReportingViewModel = new ExcessiveConduitAreaReportingViewModel(DocumentHelper);
                }
                return _excessiveConduitAreaReportingViewModel;
            }
            private set
            {
                if (_excessiveConduitAreaReportingViewModel != value)
                {
                    _excessiveConduitAreaReportingViewModel = value;
                    NotifyPropertyChanged("ExcessiveConduitAreaReportingViewModel");
                }
            }
        }
        private ExcessiveConduitAreaReportingViewModel _excessiveConduitAreaReportingViewModel;
        #endregion

        #region Constructors
        public ReportingMasterViewModel(DocumentHelper documentHelper, CommandSettings commandSettings, Func<IEnumerable<Element>> getElements)
            : base()
        {
            DocumentHelper = documentHelper;
            CommandSettings = commandSettings;
            GetElements = getElements;
            Refresh();
        }
        #endregion

        #region Commands
        #region Refresh
        private bool RefreshCanExecute(object param)
        {
            return true;
        }

        private void RefreshExecuted(object param)
        {
            if (RefreshCanExecute(param))
            {
                Refresh();
            }
        }

        private RelayCommand _refreshCommand;
        public RelayCommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(RefreshCanExecute, RefreshExecuted);
                }
                return _refreshCommand;
            }
        }
        #endregion
        #endregion

        #region Methods
        private void Refresh()
        {
            JunctionBoxes = new List<JunctionBox>(GetElements().Select(element => new JunctionBox(element, CommandSettings.ConduitSchedule, CommandSettings.DefaultConduitType)).Where(jb => jb.Mark != null));
        }
        #endregion
    }
}
