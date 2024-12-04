using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Idibri.RevitPlugin.Common;
using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class CableSchedule : ModelBase
    {
        #region Properties
        public ObservableCollection<Cable> Cables
        {
            get { return _cables; }
            set
            {
                if (_cables != value)
                {
                    ChangeRegistration(_cables, value, OnCableCollectionChanged);
                    if (_cables != null) { Deintegrate(_cables); }
                    _cables = value;
                    if (_cables != null) { Integrate(_cables); }
                    NotifyPropertyChanged("Cables");
                }
            }
        }
        private ObservableCollection<Cable> _cables = null;

        private Dictionary<string, Dictionary<string, Cable>> Index
        {
            get
            {
                if (_index == null)
                {
                    _index = new Dictionary<string, Dictionary<string, Cable>>();
                }
                return _index;
            }
        }
        private Dictionary<string, Dictionary<string, Cable>> _index;

        public IEnumerable<string> CableGroups { get { return Index.Keys.OrderBy(s => s); } }
        #endregion

        #region Constructors
        public CableSchedule()
            : base()
        { }

        public CableSchedule(IEnumerable<Cable> cables)
            : this()
        {
            Cables = new ObservableCollection<Cable>(cables);
        }
        #endregion

        #region Methods
        public bool ContainsCable(string groupName, string cableName)
        {
            return groupName != null && cableName != null && Index.ContainsKey(groupName) && Index[groupName].ContainsKey(cableName);
        }

        public string FirstUniqueName(string groupName, string cableName)
        {
            if (groupName == null) { throw new ArgumentNullException("Group Name cannot be null."); }
            if (cableName == null) { throw new ArgumentNullException("Cable Name cannot be null."); }

            if (!Index.ContainsKey(groupName))
            {
                return cableName;
            }

            int iteration = 1;
            string tryName = cableName;
            while (ContainsCable(groupName, tryName))
            {
                tryName = cableName + iteration;
                ++iteration;
            }
            return tryName;
        }

        public Cable GetCable(string groupName, string cableName)
        {
            return Index[groupName][cableName];
        }

        public void AddCable(Cable cable)
        {
            if (ContainsCable(cable.GroupName, cable.Name)) { throw new CommandExceptionBase("A cable with that group/name has already been added."); }
            Cables.Add(cable);
        }

        public void RemoveCable(Cable cable)
        {
            Cables.Remove(cable);
        }

        private void AddCableToIndex(Cable cable)
        {
            if (!Index.ContainsKey(cable.GroupName))
            {
                Index[cable.GroupName] = new Dictionary<string, Cable>();
                NotifyPropertyChanged("CableGroups");
            }
            Index[cable.GroupName][cable.Name] = cable;
        }

        private void RemoveCableFromIndex(Cable cable)
        {
            RemoveCableFromIndex(cable.GroupName, cable.Name);
        }

        private void RemoveCableFromIndex(string groupName, string name)
        {
            Index[groupName].Remove(name);
            if (Index[groupName].Count == 0)
            {
                Index.Remove(groupName);
                NotifyPropertyChanged("CableGroups");
            }
        }

        private void UpdateIndex(Cable cable, ExtendedPropertyChangedEventArgs e)
        {
            string oldVal = (string)e.OldValue;

            // If another cable with that name exists, we need to iterate the
            // given cable's name to avoid a collision.
            if (ContainsCable(cable.GroupName, cable.Name))
            {
                cable.PropertyChanged -= OnCablePropertyChanged;
                cable.Name = FirstUniqueName(cable.GroupName, cable.Name);
                cable.PropertyChanged += OnCablePropertyChanged;
            }

            if (e.PropertyName == "Name")
            {
                RemoveCableFromIndex(cable.GroupName, oldVal);
                AddCableToIndex(cable);
            }
            else if (e.PropertyName == "GroupName")
            {
                RemoveCableFromIndex(oldVal, cable.Name);
                AddCableToIndex(cable);
            }
        }
        #endregion

        #region [De]Integrate
        private void Integrate(Cable cable)
        {
            cable.PropertyChanged += OnCablePropertyChanged;
            AddCableToIndex(cable);
        }

        private void Deintegrate(Cable cable)
        {
            cable.PropertyChanged -= OnCablePropertyChanged;
            RemoveCableFromIndex(cable);
        }

        private void Integrate(IEnumerable<Cable> cables)
        {
            foreach (Cable cable in cables)
            {
                Integrate(cable);
            }
        }

        private void Deintegrate(IEnumerable<Cable> cables)
        {
            foreach (Cable cable in cables)
            {
                Deintegrate(cable);
            }
        }
        #endregion

        #region Event Handlers
        private void OnCableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                Deintegrate(e.OldItems.Cast<Cable>());
            }
            if (e.NewItems != null)
            {
                Integrate(e.NewItems.Cast<Cable>());
            }
        }

        private void OnCablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" || e.PropertyName == "GroupName")
            {
                UpdateIndex((Cable)sender, (ExtendedPropertyChangedEventArgs)e);
            }
        }
        #endregion
    }
}
