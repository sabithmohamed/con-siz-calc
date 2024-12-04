using System;
using System.Xml.Serialization;
using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class Cable : ModelBase
    {
        #region Properties
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                if (_groupName != value)
                {
                    ExtendedPropertyChangedEventArgs e = new ExtendedPropertyChangedEventArgs("GroupName", _groupName, value);
                    _groupName = value;
                    NotifyPropertyChanged(e);
                }
            }
        }
        private string _groupName;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    ExtendedPropertyChangedEventArgs e = new ExtendedPropertyChangedEventArgs("Name", _name, value);
                    _name = value;
                    NotifyPropertyChanged(e);
                }
            }
        }
        private string _name;

        public string Manufacturer
        {
            get { return _manufacturer; }
            set
            {
                if (_manufacturer != value)
                {
                    _manufacturer = value;
                    NotifyPropertyChanged("Manufacturer");
                }
            }
        }
        private string _manufacturer;

        public string StandardPartNumber
        {
            get { return _standardPartNumber; }
            set
            {
                if (_standardPartNumber != value)
                {
                    _standardPartNumber = value;
                    NotifyPropertyChanged("StandardPartNumber");
                }
            }
        }
        private string _standardPartNumber;

        public string UnderGroundWetPartNumber
        {
            get { return _underGroundWetPartNumber; }
            set
            {
                if (_underGroundWetPartNumber != value)
                {
                    _underGroundWetPartNumber = value;
                    NotifyPropertyChanged("UnderGroundWetPartNumber");
                }
            }
        }
        private string _underGroundWetPartNumber;

        public string PlenumPartNumber
        {
            get { return _plenumPartNumber; }
            set
            {
                if (_plenumPartNumber != value)
                {
                    _plenumPartNumber = value;
                    NotifyPropertyChanged("PlenumPartNumber");
                }
            }
        }
        private string _plenumPartNumber;

        public string Application
        {
            get { return _application; }
            set
            {
                if (_application != value)
                {
                    _application = value;
                    NotifyPropertyChanged("Application");
                }
            }
        }
        private string _application;

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }
        private string _description;

        public double? NominalOutsideDiameterIn
        {
            get { return _nominalOutsideDiameterIn; }
            set
            {
                if (_nominalOutsideDiameterIn != value)
                {
                    _nominalOutsideDiameterIn = value;
                    NotifyPropertyChanged("NominalOutsideDiameterIn");
                    AreaIn = value.HasValue ? Math.Pow(value.Value, 2.0) * 0.7855 : (double?)null;
                }
            }
        }
        private double? _nominalOutsideDiameterIn;

        public double? NominalOutsideDiameterMm
        {
            get { return _nominalOutsideDiameterMm; }
            set
            {
                if (_nominalOutsideDiameterMm != value)
                {
                    _nominalOutsideDiameterMm = value;
                    NotifyPropertyChanged("NominalOutsideDiameterMm");
                }
            }
        }
        private double? _nominalOutsideDiameterMm;

        [XmlIgnore]
        public double? AreaIn
        {
            get { return _areaIn; }
            private set
            {
                if (_areaIn != value)
                {
                    _areaIn = value;
                    NotifyPropertyChanged("AreaIn");
                }
            }
        }
        private double? _areaIn;

        public string SignalGroup
        {
            get { return _signalGroup; }
            set
            {
                if (_signalGroup != value)
                {
                    _signalGroup = value;
                    NotifyPropertyChanged("SignalGroup");
                }
            }
        }
        private string _signalGroup;

        public decimal? CostPerFoot
        {
            get { return _costPerFoot; }
            set
            {
                if (_costPerFoot != value)
                {
                    _costPerFoot = value;
                    NotifyPropertyChanged("CostPerFoot");
                }
            }
        }
        private decimal? _costPerFoot;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    NotifyPropertyChanged("IsActive");
                }
            }
        }
        private bool _isActive;
        #endregion

        #region Constructors
        public Cable()
            : base()
        {
            IsActive = true;
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return string.Format("{0}-{1}", GroupName, Name);
        }
        #endregion
    }
}
