using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class CommandSettings : ModelBase
    {
        #region Properties
        [XmlIgnore]
        public string AssociatedFilename
        {
            get { return _associatedFilename; }
            set
            {
                if (_associatedFilename != value)
                {
                    _associatedFilename = value;
                    NotifyPropertyChanged("AssociatedFilename");
                }
            }
        }
        private string _associatedFilename;

        [XmlIgnore]
        public DateTime AssociatedFileChangeTime
        {
            get { return _associatedFileChangeTime; }
            set
            {
                if (_associatedFileChangeTime != value)
                {
                    _associatedFileChangeTime = value;
                    NotifyPropertyChanged("AssociatedFileChangeTime");
                }
            }
        }
        private DateTime _associatedFileChangeTime;
        [XmlIgnore]
        public Exception FileLoadException
        {
            get { return _fileLoadException; }
            set
            {
                if (_fileLoadException != value)
                {
                    _fileLoadException = value;
                    NotifyPropertyChanged("FileLoadException");
                }
            }
        }
        private Exception _fileLoadException;

        [XmlIgnore]
        public bool CanPersistForProject
        {
            get { return _canPersistForProject; }
            set
            {
                if (_canPersistForProject != value)
                {
                    _canPersistForProject = value;
                    NotifyPropertyChanged("CanPersistForProject");
                }
            }
        }
        private bool _canPersistForProject;

        [XmlIgnore]
        public bool IsFromResource
        {
            get { return _isFromResource; }
            set
            {
                if (_isFromResource != value)
                {
                    _isFromResource = value;
                    NotifyPropertyChanged("IsFromResource");
                }
            }
        }
        private bool _isFromResource;

        public CableSchedule CableSchedule
        {
            get { return _cableSchedule; }
            set
            {
                if (_cableSchedule != value)
                {
                    _cableSchedule = value;
                }
            }
        }
        private CableSchedule _cableSchedule;

        public ConduitSchedule ConduitSchedule
        {
            get { return _conduitSchedule; }
            set
            {
                if (_conduitSchedule != value)
                {
                    _conduitSchedule = value;
                }
            }
        }
        private ConduitSchedule _conduitSchedule;

        public WorksetToCableScheduleMap WorksetToCableScheduleMap
        {
            get { return _worksetToCableScheduleMap; }
            set
            {
                if (_worksetToCableScheduleMap != value)
                {
                    _worksetToCableScheduleMap = value;
                }
            }
        }
        private WorksetToCableScheduleMap _worksetToCableScheduleMap;

        public double DefaultMaxCableAreaPercent
        {
            get { return _defaultMaxCableAreaPercent; }
            set
            {
                value = Math.Abs(value);
                if (value < 0.01 || value > 1.00)
                {
                    SetError("DefaultMaxCableAreaPercent", "Value must be in the range [0.01, 1.00]");
                }
                else
                {
                    ClearError("DefaultMaxCableAreaPercent");
                    _defaultMaxCableAreaPercent = value;
                    NotifyPropertyChanged("DefaultMaxCableAreaPercent");
                }
            }
        }
        private double _defaultMaxCableAreaPercent;

        public string DefaultConduitType
        {
            get { return _defaultConduitType; }
            set
            {
                if (_defaultConduitType != value)
                {
                    _defaultConduitType = value;
                    NotifyPropertyChanged("DefaultConduitType");
                }
            }
        }
        private string _defaultConduitType;

        public bool ShrinkOversizedConduits
        {
            get { return _shrinkOversizedConduits; }
            set
            {
                if (_shrinkOversizedConduits != value)
                {
                    _shrinkOversizedConduits = value;
                    NotifyPropertyChanged("ShrinkOversizedConduits");
                }
            }
        }
        private bool _shrinkOversizedConduits = false;
        #endregion

        #region Methods
        public CommandSettings MergeWorksets(IEnumerable<string> worksetNames)
        {
            if (WorksetToCableScheduleMap == null)
            {
                WorksetToCableScheduleMap = new WorksetToCableScheduleMap();
            }
            if (worksetNames != null)
            {
                WorksetToCableScheduleMap.Merge(worksetNames);
            }
            return this;
        }
        #endregion
    }
}
