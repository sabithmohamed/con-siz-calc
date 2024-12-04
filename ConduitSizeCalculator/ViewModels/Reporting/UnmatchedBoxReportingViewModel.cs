using System;
using System.Collections.Generic;
using System.Linq;
using Idibri.RevitPlugin.Common.Infrastructure;
using Idibri.RevitPlugin.Common;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.ViewModels
{
    public class UnmatchedBoxReportingViewModel : ViewModelBase
    {
        public class OffendingJunctionBox : ModelBase
        {
            public JunctionBox JunctionBox { get; private set; }
            public List<ConduitDefinition> UnmatchedConduits { get; private set; }
            public bool IsDuplicate { get; private set; }
            public UnmatchedBoxReportingViewModel Owner { get; private set; }

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

            public OffendingJunctionBox(UnmatchedBoxReportingViewModel owner, JunctionBox junctionBox, bool isDuplicate)
            {
                Owner = owner;
                JunctionBox = junctionBox;
                IsDuplicate = isDuplicate;
                UnmatchedConduits = new List<ConduitDefinition>();
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

        public class IndexMember
        {
            public JunctionBox JunctionBox { get; private set; }
            public int DuplicateCount { get; set; }
            public bool IsDuplicate { get { return DuplicateCount != 0; } }

            public IndexMember(JunctionBox junctionBox)
            {
                JunctionBox = junctionBox;
                DuplicateCount = 0;
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

        public Exception ViewModelException
        {
            get { return _viewModelException; }
            set
            {
                if (_viewModelException != value)
                {
                    _viewModelException = value;
                    NotifyPropertyChanged("ViewModelException");
                }
            }
        }
        private Exception _viewModelException;

        public List<JunctionBox> JunctionBoxes
        {
            get { return _junctionBoxes; }
            set
            {
                if (_junctionBoxes != value)
                {
                    _junctionBoxes = value;
                    NotifyPropertyChanged("JunctionBoxes");

                    try
                    {
                        Recalculate();
                    }
                    catch (CommandExceptionBase ex)
                    {
                        ViewModelException = ex;
                    }
                }
            }
        }
        private List<JunctionBox> _junctionBoxes;

        public Dictionary<string, IndexMember> JunctionBoxIndex
        {
            get { return _junctionBoxIndex; }
            set
            {
                if (_junctionBoxIndex != value)
                {
                    _junctionBoxIndex = value;
                    NotifyPropertyChanged("JunctionBoxIndex");
                }
            }
        }
        private Dictionary<string, IndexMember> _junctionBoxIndex;

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
        public UnmatchedBoxReportingViewModel(DocumentHelper documentHelper)
            : base()
        {
            DocumentHelper = documentHelper;
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
            JunctionBoxIndex = new Dictionary<string, IndexMember>();

            if (JunctionBoxes != null)
            {
                foreach (JunctionBox junctionBox in JunctionBoxes)
                {
                    if (JunctionBoxIndex.ContainsKey(junctionBox.Mark))
                    {
                        JunctionBoxIndex[junctionBox.Mark].DuplicateCount += 1;
                    }
                    JunctionBoxIndex[junctionBox.Mark] = new IndexMember(junctionBox);
                }

                OffendingJunctionBoxes = new List<OffendingJunctionBox>(JunctionBoxes.Select(jb => GetOffendingJunctionBox(jb)).Where(jb => jb.UnmatchedConduits.Count != 0));

                Dictionary<string, List<OffendingJunctionBox>> groups = new Dictionary<string, List<OffendingJunctionBox>>();
                foreach (OffendingJunctionBox jb in OffendingJunctionBoxes)
                {
                    string worksetName = DocumentHelper.GetWorkset(jb.JunctionBox.Element).Name;
                    if (!groups.ContainsKey(worksetName))
                    {
                        groups[worksetName] = new List<OffendingJunctionBox>();
                    }
                    groups[worksetName].Add(jb);
                }
                JunctionBoxGroups = groups.Keys.OrderBy(k => k).Select(k => new JunctionBoxGroup(k, groups[k])).ToList();
            }
            else
            {
                OffendingJunctionBoxes = null;
                JunctionBoxGroups = null;
            }
        }

        private OffendingJunctionBox GetOffendingJunctionBox(JunctionBox junctionBox)
        {
            OffendingJunctionBox offendingJunctionBox = new OffendingJunctionBox(this, junctionBox, JunctionBoxIndex[junctionBox.Mark].IsDuplicate);

            foreach (ConduitDefinition cd in junctionBox.Conduits)
            {
                if (cd.Conduit == null) { continue; }
                if (cd.Destination == null || !JunctionBoxIndex.ContainsKey(cd.Destination))
                {
                    offendingJunctionBox.UnmatchedConduits.Add(cd);
                }
            }

            return offendingJunctionBox;
        }
        #endregion
    }
}
