using System;
using System.Collections.Generic;
using System.Linq;
using Idibri.RevitPlugin.Common.Infrastructure;
using Idibri.RevitPlugin.ConduitSizeCalculator.Models;
using Idibri.RevitPlugin.Common;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.ViewModels
{
    public class ExcessiveConduitAreaReportingViewModel : ViewModelBase
    {
        public class Node : ModelBase
        {
            #region Properties
            const double Buffer = 5.0 / 8.0;

            public JunctionBox JunctionBox { get; private set; }
            public List<Edge> Edges { get; private set; }
            public double RequiredConduitAreaIn { get { return GetRequiredConduitAreaIn(); } }
            public double AvailableConduitAreaIn { get { return JunctionBox.DepthIn * JunctionBox.WidthIn; } }
            public bool IsUndersized { get { return RequiredConduitAreaIn > AvailableConduitAreaIn; } }
            public bool MarkIsDuplicate { get; set; }
            public ExcessiveConduitAreaReportingViewModel Owner { get; private set; }

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
            #endregion

            #region Constructors
            public Node(ExcessiveConduitAreaReportingViewModel owner, JunctionBox junctionBox)
            {
                Owner = owner;
                JunctionBox = junctionBox;
                Edges = new List<Edge>();
                MarkIsDuplicate = false;
            }
            #endregion

            #region Commands
            #region Select Element
            private bool SelectElementCanExecute(object param)
            {
                return true;
            }

            private void SelectElementExecuted(object param)
            {
                if (SelectElementCanExecute(param))
                {
                    Owner.SelectElementCommand.Execute(JunctionBox.Element);
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
            public void AddEdge(Edge edge)
            {
                Edges.Add(edge);
            }

            private double GetRequiredConduitAreaIn()
            {
                if (Edges.Count == 0) { return 0; }
                return Edges.Sum(e => Math.Pow(e.Conduit.OutsideDiameterIn + Buffer, 2.0));
            }
            #endregion
        }

        public class Edge
        {
            #region Properties
            public Node Source { get; private set; }
            public Node Destination { get; private set; }
            public Conduit Conduit { get; private set; }
            public ExcessiveConduitAreaReportingViewModel Owner { get; private set; }
            #endregion

            #region Constructors
            public Edge(ExcessiveConduitAreaReportingViewModel owner, Node source, Node destination, Conduit conduit, bool addEdgeToNodes)
            {
                Owner = owner;
                Source = source;
                Destination = destination;
                Conduit = conduit;

                if (addEdgeToNodes)
                {
                    Source.AddEdge(this);
                    // If the destination is a duplicate, we don't know which node to assign
                    // the edge to, so we won't add it to any of them.
                    if (Destination != null && !Destination.MarkIsDuplicate)
                    {
                        Destination.AddEdge(this);
                    }
                }
            }
            #endregion

            #region Commands
            #region Select Source
            private bool SelectSourceCanExecute(object param)
            {
                return true;
            }

            private void SelectSourceExecuted(object param)
            {
                if (SelectSourceCanExecute(param))
                {
                    Owner.SelectElementCommand.Execute(Source);
                }
            }

            private RelayCommand _selectSourceCommand;
            public RelayCommand SelectSourceCommand
            {
                get
                {
                    if (_selectSourceCommand == null)
                    {
                        _selectSourceCommand = new RelayCommand(SelectSourceCanExecute, SelectSourceExecuted);
                    }
                    return _selectSourceCommand;
                }
            }
            #endregion

            #region Select Destination
            private bool SelectDestinationCanExecute(object param)
            {
                return Destination != null;
            }

            private void SelectDestinationExecuted(object param)
            {
                if (SelectDestinationCanExecute(param))
                {
                    Owner.SelectElementCommand.Execute(Destination);
                }
            }

            private RelayCommand _selectDestinationCommand;
            public RelayCommand SelectDestinationCommand
            {
                get
                {
                    if (_selectDestinationCommand == null)
                    {
                        _selectDestinationCommand = new RelayCommand(SelectDestinationCanExecute, SelectDestinationExecuted);
                    }
                    return _selectDestinationCommand;
                }
            }
            #endregion
            #endregion
        }

        public class IndexMember
        {
            public Node Node { get; private set; }
            public int DuplicateCount { get; set; }
            public bool IsDuplicate { get { return DuplicateCount != 0; } }

            public IndexMember(Node node)
            {
                Node = node;
                DuplicateCount = 0;
            }
        }

        public class NodeGroup : ModelBase
        {
            public string Name { get; private set; }
            public List<Node> Nodes { get; private set; }

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

            public NodeGroup(string name, IEnumerable<Node> nodes)
            {
                Name = name;
                Nodes = new List<Node>(nodes);
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

        public List<Node> Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes != value)
                {
                    _nodes = value;
                    NotifyPropertyChanged("Nodes");
                }
            }
        }
        private List<Node> _nodes;

        public List<NodeGroup> NodeGroups
        {
            get { return _nodeGroups; }
            set
            {
                if (_nodeGroups != value)
                {
                    _nodeGroups = value;
                    NotifyPropertyChanged("NodeGroups");
                }
            }
        }
        private List<NodeGroup> _nodeGroups;
        #endregion

        #region Constructors
        public ExcessiveConduitAreaReportingViewModel(DocumentHelper documentHelper)
        {
            DocumentHelper = documentHelper;
        }
        #endregion

        #region Commands
        #region Select Element
        private bool SelectElementCanExecute(object param)
        {
            return param is Node;
        }

        private void SelectElementExecuted(object param)
        {
            if (SelectElementCanExecute(param))
            {
                Node node = param as Node;
                DocumentHelper.HighlightElement(node.JunctionBox.Element);
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
            List<Node> nodes = new List<Node>();

            if (JunctionBoxes != null)
            {
                foreach (JunctionBox junctionBox in JunctionBoxes)
                {
                    Node node = new Node(this, junctionBox);
                    nodes.Add(node);

                    if (JunctionBoxIndex.ContainsKey(junctionBox.Mark))
                    {
                        JunctionBoxIndex[junctionBox.Mark].DuplicateCount += 1;
                        JunctionBoxIndex[junctionBox.Mark].Node.MarkIsDuplicate = true; // This is only necessary the first time a duplicate is found.
                        node.MarkIsDuplicate = true;
                    }

                    JunctionBoxIndex[junctionBox.Mark] = new IndexMember(node);
                }
            }

            foreach (Node node in nodes)
            {
                foreach (ConduitDefinition cd in node.JunctionBox.Conduits)
                {
                    if (cd.Conduit != null)
                    {
                        Node destination = cd.Destination != null && JunctionBoxIndex.ContainsKey(cd.Destination) ? JunctionBoxIndex[cd.Destination].Node : null;
                        Edge edge = new Edge(this, node, destination, cd.Conduit, true);
                    }
                }
            }

            Nodes = nodes;

            Dictionary<string, List<Node>> groups = new Dictionary<string, List<Node>>();
            foreach (Node node in Nodes.Where(n => n.IsUndersized))
            {
                string worksetName = DocumentHelper.GetWorkset(node.JunctionBox.Element).Name;
                if (!groups.ContainsKey(worksetName))
                {
                    groups[worksetName] = new List<Node>();
                }
                groups[worksetName].Add(node);
            }
            NodeGroups = groups.Keys.OrderBy(k => k).Select(k => new NodeGroup(k, groups[k])).ToList();

            #endregion
        }
    }
}
