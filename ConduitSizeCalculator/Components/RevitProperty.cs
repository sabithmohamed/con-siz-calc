using System;
using Autodesk.Revit.DB;

namespace ConduitSizeCalculator
{
    public class RevitProperty
    {
        #region Properties
        public string Name { get; private set; }
        public Guid Id { get; private set; }
        public bool IsTypeProperty { get; private set; }
        public BuiltInParameter? BuiltInParam { get; private set; }

        static bool IsTypePropertyDefault = false;
        #endregion

        #region Constructors
        public RevitProperty(string name, Guid id)
            : this(name, id, IsTypePropertyDefault)
        { }

        public RevitProperty(string name, Guid id, bool isTypeProperty)
            : this(name, isTypeProperty)
        {
            Id = id;
        }

        public RevitProperty(string name, BuiltInParameter builtInParam)
            : this(name, builtInParam, IsTypePropertyDefault)
        { }

        public RevitProperty(string name, BuiltInParameter builtInParam, bool isTypeProperty)
            : this(name, isTypeProperty)
        {
            BuiltInParam = builtInParam;
        }

        private RevitProperty(string name, bool isTypeProperty)
        {
            Name = name;
            IsTypeProperty = isTypeProperty;
            BuiltInParam = null;
        }
        #endregion

        public Parameter GetParameter(Element element)
        {
            return BuiltInParam.HasValue ? element.get_Parameter(BuiltInParam.Value) : element.get_Parameter(Id);
        }

        private Element GetElement(Element element)
        {
#if REVIT2016
            return IsTypeProperty ? element.Document.GetElement(element.GetTypeId()) : element;
#elif REVIT2014
            return IsTypeProperty ? element.Document.GetElement(element.GetTypeId()) : element;
#elif REVIT2012
            return IsTypeProperty ? element.Document.get_Element(element.GetTypeId()) : element;
#endif
        }

        private MissingParameterException GetMissingParameterException(Element element)
        {
            string elementName = element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();
            elementName = string.IsNullOrEmpty(elementName) ? "NO MARK" : elementName;
            return new MissingParameterException(string.Format("Element {0}: {1} is missing required parameter {2}", element.Id.IntegerValue, elementName, Name));
        }

        public string GetString(Element element)
        {
            Parameter param = GetParameter(GetElement(element));
            if (param == null) { throw GetMissingParameterException(element); }
            return param.AsString();
        }

        public void SetString(Element element, string value)
        {
            Parameter param = GetParameter(GetElement(element));
            if (param == null) { throw GetMissingParameterException(element); }
            param.Set(value);
        }

        public int GetInt(Element element)
        {
            Parameter param = GetParameter(GetElement(element));
            if (param == null) { throw GetMissingParameterException(element); }
            return param.AsInteger();
        }

        public void SetInt(Element element, int value)
        {
            Parameter param = GetParameter(GetElement(element));
            if (param == null) { throw GetMissingParameterException(element); }
            param.Set(value);
        }

        public double GetDouble(Element element)
        {
            Parameter param = GetParameter(GetElement(element));
            if (param == null) { throw GetMissingParameterException(element); }
            return param.AsDouble();
        }

        public void SetDouble(Element element, double value)
        {
            Parameter param = GetParameter(GetElement(element));
            if (param == null) { throw GetMissingParameterException(element); }
            param.Set(value);
        }

        public string GetValueString(Element element)
        {
            Parameter param = GetParameter(GetElement(element));
            if (param == null) { throw GetMissingParameterException(element); }
            return param.AsValueString();
        }

        public void SetValueString(Element element, string value)
        {
            Parameter param = GetParameter(GetElement(element));
            if (param == null) { throw GetMissingParameterException(element); }
            param.SetValueString(value);
        }
    }
}
