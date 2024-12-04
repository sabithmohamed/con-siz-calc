using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class WorksetToCableScheduleMap : ModelBase
    {
        #region Properties
        public ObservableCollection<WorksetCableSchedulePair> Pairs
        {
            get { return _pairs; }
            set
            {
                if (_pairs != value)
                {
                    ChangeRegistration(_pairs, value, OnPairCollectionChanged);
                    _pairs = value;
                    IndexPairs();
                }
            }
        }
        private ObservableCollection<WorksetCableSchedulePair> _pairs;

        private Dictionary<string, string> Index
        {
            get
            {
                if (_index == null)
                {
                    IndexPairs();
                }
                return _index;
            }
        }
        private Dictionary<string, string> _index = null;
        #endregion

        #region Constructors
        public WorksetToCableScheduleMap()
            : base()
        { }
        #endregion

        #region Methods
        public bool MapsWorkset(string worksetName)
        {
            return Index.ContainsKey(worksetName);
        }

        public string CableScheduleForWorksetId(string worksetName)
        {
            return Index[worksetName];
        }

        public void Merge(IEnumerable<string> worksetNames)
        {
            IEnumerable<WorksetCableSchedulePair> unionWith = (IEnumerable<WorksetCableSchedulePair>)Pairs ?? new List<WorksetCableSchedulePair>();

            Pairs = new ObservableCollection<WorksetCableSchedulePair>(worksetNames
                    .Distinct()
                    .Where(wsn => !Index.ContainsKey(wsn))
                    .Select(wsn => new WorksetCableSchedulePair() { WorksetName = wsn, CableSchedule = null })
                    .Union(unionWith)
                    .OrderBy(p => p.WorksetName));
        }

        public void Initialize(IEnumerable<string> worksetNames)
        {
            Pairs = new ObservableCollection<WorksetCableSchedulePair>(worksetNames
                .Distinct()
                .OrderBy(s => s)
                .Select(wsn => new WorksetCableSchedulePair()
                {
                    WorksetName = wsn,
                    CableSchedule = null
                }));

        }

        private void IndexPairs()
        {
            _index = new Dictionary<string, string>();
            if (Pairs != null)
            {
                foreach (WorksetCableSchedulePair pair in Pairs)
                {
                    _index[pair.WorksetName] = pair.CableSchedule;
                }
            }
        }
        #endregion

        #region Event Handlers
        private void OnPairCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (object o in e.OldItems)
                {
                    Index.Remove(((WorksetCableSchedulePair)o).WorksetName);
                }
            }
            if (e.NewItems != null)
            {
                foreach (object o in e.NewItems)
                {
                    WorksetCableSchedulePair p = (WorksetCableSchedulePair)o;
                    Index[p.WorksetName] = p.CableSchedule;
                }
            }
        }
        #endregion
    }

    public class WorksetCableSchedulePair : ModelBase
    {
        public string WorksetName
        {
            get { return _worksetName; }
            set
            {
                if (_worksetName != value)
                {
                    _worksetName = value;
                    NotifyPropertyChanged("WorksetName");
                }
            }
        }
        private string _worksetName;

        public string CableSchedule
        {
            get { return _cableSchedule; }
            set
            {
                if (_cableSchedule != value)
                {
                    _cableSchedule = value;
                    NotifyPropertyChanged("CableSchedule");
                }
            }
        }
        private string _cableSchedule;
    }
}
