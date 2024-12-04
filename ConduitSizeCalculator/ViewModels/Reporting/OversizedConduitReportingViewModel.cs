using System.Collections.Generic;
using System.Linq;
using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.ViewModels
{
    public class OversizedConduitsReportingViewModel : ViewModelBase
    {
        public class OffendingJunctionBox : ModelBase
        {
            public JunctionBox JunctionBox { get; private set; }
            public List<ConduitDefinition> OversizedConduits { get; private set; }
            public OversizedConduitsReportingViewModel Owner { get; private set; }
            public bool IsExpanded
            {
                get { return _isExpanded; }
                set
                {
                    if (_isExpanded != value)
                    {
                        _isExpanded = value;
                        NotifyPropertyChanged("IsExpanded");
                    }
                }
            }
            private bool _isExpanded = true;

            private RelayCommand _selectElementCommand;
            public RelayCommand SelectElementCommand
            {
                get
                {
                    if (_selectElementCommand == null)
                    {
                        _selectElementCommand = new RelayCommand(o => true, o => Owner.SelectElementExecuted(this));
                    }
                    return _selectElementCommand;
                }
            }

            public OffendingJunctionBox(OversizedConduitsReportingViewModel owner, JunctionBox junctionBox)
            {
                Owner = owner;
                JunctionBox = junctionBox;
                OversizedConduits = new List<ConduitDefinition>();
            }
        }

        public class JunctionBoxGroup : ModelBase
        {
            public string Name { get; private set; }
            public List<OffendingJunctionBox> OffendingJunctionBoxes { get; private set; }

            public bool IsExpanded
            {
                get { return _isExpanded; }
                set
                {
                    if (_isExpanded != value)
                    {
                        _isExpanded = value;
                        NotifyPropertyChanged("IsExpanded");
                    }
                }
            }
            private bool _isExpanded = true;

            public JunctionBoxGroup(string name, IEnumerable<OffendingJunctionBox> offendingJunctionBoxes)
            {
                Name = name;
                OffendingJunctionBoxes = new List<OffendingJunctionBox>(offendingJunctionBoxes);
            }
        }

        #region Properties
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

        public List<JunctionBox> JunctionBoxes
        {
            get { return _junctionBoxes; }
            set
            {
                if (_junctionBoxes != value)
                {
                    _junctionBoxes = value;
                    NotifyPropertyChanged("JunctionBoxes");
                    Recalculate();
                }
            }
        }
        private List<JunctionBox> _junctionBoxes;

        public double DepthBufferIn
        {
            get { return _depthBufferIn; }
            set
            {
                if (_depthBufferIn != value)
                {
                    _depthBufferIn = value;
                    NotifyPropertyChanged("DepthBufferIn");
                    Recalculate();
                }
            }
        }
        private double _depthBufferIn;

        public List<OffendingJunctionBox> OffendingJunctionBoxes
        {
            get { return _offendingJunctionBoxes; }
            set
            {
                if (_offendingJunctionBoxes != value)
                {
                    _offendingJunctionBoxes = value;
                    NotifyPropertyChanged("OffendingJunctionBoxes");
                }
            }
        }
        private List<OffendingJunctionBox> _offendingJunctionBoxes;

        private bool AllowRecalculate = false;

        public List<JunctionBoxGroup> JunctionBoxGroups
        {
            get { return _junctionBoxGroups; }
            set
            {
                if (_junctionBoxGroups != value)
                {
                    _junctionBoxGroups = value;
                    NotifyPropertyChanged("JunctionBoxGroups");
                }
            }
        }
        private List<JunctionBoxGroup> _junctionBoxGroups;
        #endregion

        #region Constructors
        public OversizedConduitsReportingViewModel(DocumentHelper documentHelper, double depthBufferIn)
        {
            DocumentHelper = documentHelper;
            DepthBufferIn = depthBufferIn;
            AllowRecalculate = true;
        }
        #endregion

        #region Commands
        #region Select Element
        private bool SelectElementCanExecute(object param)
        {
            return param is OffendingJunctionBox;
        }

        private void SelectElementExecuted(object param)
        {
            if (SelectElementCanExecute(param))
            {
                OffendingJunctionBox offendingJunctionBox = param as OffendingJunctionBox;
                DocumentHelper.HighlightElement(offendingJunctionBox.JunctionBox.Element);
            }
        }

        private RelayCommand _selectElementCommand;
        public RelayCommand SelectElementCommand
        {
            get
            {
                if (_selectElementCommand == null)
                {
                    _selectElementCommand = new RelayCommand(SelectElementCanExecute, SelectElementExecuted);
                }
                return _selectElementCommand;
            }
        }
        #endregion
        #endregion

        #region Methods
        private void Recalculate()
        {
            if (!AllowRecalculate) { return; }

            if (JunctionBoxes == null) { OffendingJunctionBoxes = null; }

            OffendingJunctionBoxes = JunctionBoxes.Select(jb => GetOffendingJunctionBox(jb)).Where(ojb => ojb.OversizedConduits.Count != 0).ToList();

            Dictionary<string, List<OffendingJunctionBox>> groups = new Dictionary<string, List<OffendingJunctionBox>>();
            foreach (OffendingJunctionBox offendingJunctionBox in OffendingJunctionBoxes)
            {
                string worksetName = DocumentHelper.GetWorkset(offendingJunctionBox.JunctionBox.Element).Name;
                if (!groups.ContainsKey(worksetName))
                {
                    groups[worksetName] = new List<OffendingJunctionBox>();
                }
                groups[worksetName].Add(offendingJunctionBox);
            }
            JunctionBoxGroups = groups.Keys.OrderBy(k => k).Select(k => new JunctionBoxGroup(k, groups[k])).ToList();
        }

        private OffendingJunctionBox GetOffendingJunctionBox(JunctionBox junctionBox)
        {
            OffendingJunctionBox offendingJunctionBox = new OffendingJunctionBox(this, junctionBox);

            foreach (ConduitDefinition cd in junctionBox.Conduits)
            {
                if (cd.Conduit != null && cd.Conduit.OutsideDiameterIn + DepthBufferIn > junctionBox.DepthIn)
                {
                    offendingJunctionBox.OversizedConduits.Add(cd);
                }
            }

            return offendingJunctionBox;
        }
        #endregion
    }
}
