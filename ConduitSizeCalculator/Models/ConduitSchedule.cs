using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class ConduitSchedule : ModelBase
    {
        #region Properties
        public ObservableCollection<Conduit> Conduits
        {
            get { return _conduits; }
            set
            {
                if (_conduits != value)
                {
                    ChangeRegistration(_conduits, value, OnConduitCollectionChanged);
                    if (_conduits != null) { Deintegrate(_conduits); }
                    _conduits = value;
                    if (_conduits != null) { Integrate(_conduits); }
                }
            }
        }
        private ObservableCollection<Conduit> _conduits = null;

        [XmlIgnore]
        private Dictionary<string, List<Conduit>> ConduitMapping
        {
            get
            {
                if (_conduitMapping == null)
                {
                    _conduitMapping = new Dictionary<string, List<Conduit>>();
                }
                return _conduitMapping;
            }
        }
        private Dictionary<string, List<Conduit>> _conduitMapping = null;

        private Dictionary<string, Dictionary<string, Conduit>> Index
        {
            get
            {
                if (_index == null)
                {
                    _index = new Dictionary<string, Dictionary<string, Conduit>>();
                }
                return _index;
            }
        }
        private Dictionary<string, Dictionary<string, Conduit>> _index = null;

        [XmlIgnore]
        public IEnumerable<string> ConduitTypes { get { return ConduitMapping.Keys.OrderBy(s => s); } }
        #endregion

        #region Constructors
        public ConduitSchedule()
            : base()
        { }

        public ConduitSchedule(IEnumerable<Conduit> conduits)
            : this()
        {
            Conduits = new ObservableCollection<Conduit>(conduits);
        }
        #endregion

        #region Methods
        public bool ContainsConduitsOfType(string conduitType)
        {
            return conduitType != null && ConduitMapping.ContainsKey(conduitType);
        }

        public bool ContainsConduit(string conduitType, string name)
        {
            return ContainsConduitsOfType(conduitType) && name != null && Index[conduitType].ContainsKey(name);
        }

        public Conduit GetConduit(string conduitType, CableBundle forCables, double maximumCableAreaPercentage, bool activeOnly)
        {
            if (!ConduitMapping.ContainsKey(conduitType))
            {
                throw new UnknownConduitTypeException(string.Format("No conduits of type '{0}'", conduitType));
            }

            double minConduitArea = forCables.GetMinimumConduitArea(maximumCableAreaPercentage);
            double conduitDiameter = Conduit.DiameterForArea(minConduitArea);
            Conduit compare = new Conduit() { InsideDiameterIn = conduitDiameter };
            int index = ConduitMapping[conduitType].BinarySearch(compare, new ConduitInnerDiameterComparer());

            if (index < 0)
            {
                index = ~index;
            }

            if (index == ConduitMapping[conduitType].Count)
            {
                return null;
            }

            if (activeOnly)
            {
                while (!ConduitMapping[conduitType][index].IsActive)
                {
                    ++index;
                    if (ConduitMapping[conduitType].Count == index)
                    {
                        return null;
                    }
                }
            }

            return ConduitMapping[conduitType][index];
        }

        public Conduit GetConduit(string conduitType, string name)
        {
            return Index[conduitType][name];
        }

        public void AddConduit(Conduit conduit)
        {
            Conduits.Add(conduit);
        }

        public void RemoveConduit(Conduit conduit)
        {
            Conduits.Remove(conduit);
        }

        public IList<Conduit> GetConduitsOfType(string type)
        {
            if (type != null && ConduitMapping.ContainsKey(type))
            {
                return ConduitMapping[type].Where(c => c.IsActive).OrderBy(c => c.InsideDiameterIn).ToList();
            }
            return null;
        }

        private void AddConduitToMap(Conduit conduit, IComparer<Conduit> comparer)
        {
            if (!ConduitMapping.ContainsKey(conduit.Type))
            {
                ConduitMapping[conduit.Type] = new List<Conduit>();
                NotifyPropertyChanged("ConduitTypes");
            }
            int index = ConduitMapping[conduit.Type].BinarySearch(conduit, comparer);

            if (index < 0)
            {
                index = ~index;
            }

            ConduitMapping[conduit.Type].Insert(index, conduit);
        }

        private void RemoveConduitFromMap(Conduit conduit, IComparer<Conduit> comparer)
        {
            if (ConduitMapping[conduit.Type].Count == 1)
            {
                ConduitMapping.Remove(conduit.Type);
                NotifyPropertyChanged("ConduitTypes");
            }
            else
            {
                int index = ConduitMapping[conduit.Type].BinarySearch(conduit, new ConduitInnerDiameterComparer());
                ConduitMapping[conduit.Type].RemoveAt(index);
            }
        }

        private void SortConduits(string conduitType, IComparer<Conduit> comparer)
        {
            ConduitMapping[conduitType].Sort(comparer);
        }

        private void AddConduitToIndex(Conduit conduit)
        {
            if (!Index.ContainsKey(conduit.Type))
            {
                Index[conduit.Type] = new Dictionary<string, Conduit>();
            }
            Index[conduit.Type][conduit.Name] = conduit;
        }

        private void RemoveConduitFromIndex(Conduit conduit)
        {
            RemoveConduitFromIndex(conduit.Type, conduit.Name);
        }

        private void RemoveConduitFromIndex(string conduitType, string name)
        {
            if (Index[conduitType].Count == 1)
            {
                Index.Remove(conduitType);
            }
            else
            {
                Index[conduitType].Remove(name);
            }
        }

        private void UpdateIndex(Conduit conduit, ExtendedPropertyChangedEventArgs e)
        {
            string oldVal = (string)e.OldValue;

            switch (e.PropertyName)
            {
                case "Name":
                    RemoveConduitFromIndex(conduit.Type, oldVal);
                    AddConduitToIndex(conduit);
                    break;
                case "Type":
                    RemoveConduitFromIndex(oldVal, conduit.Name);
                    AddConduitToIndex(conduit);
                    break;
            }
        }
        #endregion

        #region [De]Integrate
        private void Integrate(Conduit conduit, IComparer<Conduit> comparer)
        {
            conduit.PropertyChanged += OnConduitPropertyChanged;
            AddConduitToMap(conduit, comparer);
            AddConduitToIndex(conduit);
        }

        private void Deintegrate(Conduit conduit, IComparer<Conduit> comparer)
        {
            conduit.PropertyChanged -= OnConduitPropertyChanged;
            RemoveConduitFromMap(conduit, comparer);
            RemoveConduitFromIndex(conduit);
        }

        private void Integrate(IEnumerable<Conduit> conduits)
        {
            IComparer<Conduit> comparer = new ConduitInnerDiameterComparer();

            foreach (Conduit conduit in conduits)
            {
                Integrate(conduit, comparer);
            }
        }

        private void Deintegrate(IEnumerable<Conduit> conduits)
        {
            IComparer<Conduit> comparer = new ConduitInnerDiameterComparer();

            foreach (Conduit conduit in conduits)
            {
                Deintegrate(conduit, comparer);
            }
        }
        #endregion

        #region Event Handlers
        private void OnConduitCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IComparer<Conduit> comparer = new ConduitInnerDiameterComparer();

            if (e.OldItems != null)
            {
                Deintegrate(e.OldItems.Cast<Conduit>());
            }
            if (e.NewItems != null)
            {
                Integrate(e.NewItems.Cast<Conduit>());
            }
        }

        private void OnConduitPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "InsideDiameterIn":
                    SortConduits((sender as Conduit).Type, new ConduitInnerDiameterComparer());
                    break;
                case "Type":
                case "Name":
                    UpdateIndex((Conduit)sender, (ExtendedPropertyChangedEventArgs)e);
                    break;
            }
        }
        #endregion

        class ConduitInnerDiameterComparer : IComparer<Conduit>
        {
            public int Compare(Conduit l, Conduit r)
            {
                if (l.InsideDiameterIn < r.InsideDiameterIn) return -1;
                if (l.InsideDiameterIn > r.InsideDiameterIn) return 1;
                return 0;
            }
        }
    }
}
