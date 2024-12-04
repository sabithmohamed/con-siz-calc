using System;
using Idibri.RevitPlugin.Common.Infrastructure;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class Conduit : ModelBase
    {
        #region Properties
        public string Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    ExtendedPropertyChangedEventArgs e = new ExtendedPropertyChangedEventArgs("Type", _type, value);
                    _type = value;
                    NotifyPropertyChanged(e);
                }
            }
        }
        private string _type;

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

        public double TradeSizeIn
        {
            get { return _tradeSizeIn; }
            set
            {
                if (_tradeSizeIn != value)
                {
                    _tradeSizeIn = value;
                    NotifyPropertyChanged("TradeSizeIn");
                }
            }
        }
        private double _tradeSizeIn;

        public double TradeSizeMm
        {
            get { return _tradeSizeMm; }
            set
            {
                if (_tradeSizeMm != value)
                {
                    _tradeSizeMm = value;
                    NotifyPropertyChanged("TradeSizeMm");
                }
            }
        }
        private double _tradeSizeMm;

        public double InsideDiameterIn
        {
            get { return _insideDiameterIn; }
            set
            {
                if (_insideDiameterIn != value)
                {
                    _insideDiameterIn = value;
                    NotifyPropertyChanged("InsideDiameterIn", "InsideAreaIn");
                }
            }
        }
        private double _insideDiameterIn;

        public double InsideDiameterMm
        {
            get { return _insideDiameterMm; }
            set
            {
                if (_insideDiameterMm != value)
                {
                    _insideDiameterMm = value;
                    NotifyPropertyChanged("InsideDiameterMm");
                }
            }
        }
        private double _insideDiameterMm;

        public double OutsideDiameterIn
        {
            get { return _outsideDiameterIn; }
            set
            {
                if (_outsideDiameterIn != value)
                {
                    _outsideDiameterIn = value;
                    NotifyPropertyChanged("OutsideDiameterIn", "OutsideAreaIn");
                }
            }
        }
        private double _outsideDiameterIn;

        public double OutsideDiameterMm
        {
            get { return _outsideDiameterMm; }
            set
            {
                if (_outsideDiameterMm != value)
                {
                    _outsideDiameterMm = value;
                    NotifyPropertyChanged("OutsideDiameterMm");
                }
            }
        }
        private double _outsideDiameterMm;

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

        private const double MillimetersPerInch = 25.4;

        public double InsideAreaIn { get { return AreaForDiameter(InsideDiameterIn); } }
        public double OutsideAreaIn { get { return AreaForDiameter(OutsideDiameterIn); } }
        #endregion

        #region Constructors
        public Conduit()
            : base()
        {
            IsActive = true;
        }

        public Conduit(string type, string name, double tradeSizeIn, double insideDiameterIn, double outsideDiameterIn)
            : this(type, name, tradeSizeIn, tradeSizeIn * MillimetersPerInch, insideDiameterIn, insideDiameterIn * MillimetersPerInch, outsideDiameterIn, outsideDiameterIn * MillimetersPerInch)
        { }

        public Conduit(string type, string name, double tradeSizeIn, double tradeSizeMm, double insideDiameterIn, double insideDiameterMm, double outsideDiameterIn, double outsideDiameterMm)
            : this()
        {
            Type = type;
            Name = name;
            TradeSizeIn = tradeSizeIn;
            TradeSizeMm = tradeSizeMm;
            InsideDiameterIn = insideDiameterIn;
            InsideDiameterMm = insideDiameterMm;
            OutsideDiameterIn = outsideDiameterIn;
            OutsideDiameterMm = outsideDiameterMm;
        }
        #endregion

        #region Methods
        public static double DiameterForArea(double area)
        {
            return Math.Sqrt(area / Math.PI) * 2.0;
        }

        public static double AreaForDiameter(double diameter)
        {
            return Math.PI * Math.Pow(diameter / 2.0, 2.0);
        }

        public override string ToString()
        {
            return string.Format("{0} / {1}", Type, Name);
        }
        #endregion
    }
}
