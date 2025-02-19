using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Idibri.RevitPlugin.ConduitSizeCalculator
{
    [Transaction(TransactionMode.Manual)]
    public class FillParams : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            

            var eles = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).ToList();
            Dictionary<string, string> electricalParameters = new Dictionary<string, string>
            {
                { "Size1", "3/4\"" },
                { "Destination1", "Cond 1" },
                { "Fill1", "1A1" },
                { "Conduit Type1", "EMT" },
                { "Cable Destination 1", "Cab 1" },
                { "Size2", "3/4\"" },
                { "Destination2", "Cond 2" },
                { "Fill2", "2A1" },
                { "Conduit Type2", "EMT" },
                { "Cable Destination 2", "Cab 2" },
                { "Size3", "3/4\"" },
                { "Destination3", "Cond 3" },
                { "Fill3", "3A1" },
                { "Conduit Type3", "EMT" },
                { "Cable Destination 3", "Cab 3" },
                { "Size4", "3/4\"" },
                { "Destination4", "Cond 4" },
                { "Fill4", "4A1" },
                { "Conduit Type4", "EMT" },
                { "Cable Destination 4", "Cab 4" },
                { "Size5", "3/4\"" },
                { "Destination5", "Cond 5" },
                { "Fill5", "5A1" },
                { "Conduit Type5", "EMT" },
                { "Cable Destination 5", "Cab 5" },
                { "Size6", "3/4\"" },
                { "Destination6", "Cond 6" },
                { "Fill6", "6A1" },
                { "Conduit Type6", "EMT" },
                { "Cable Destination 6", "Cab 6" },
            };

            int success = 0;
            int fail = 0;

            var t = new Transaction(doc, "Reset conduit params");
            t.Start();
            foreach (Element ele in eles)
            {
                try
                {
                    foreach (KeyValuePair<string,string> paramtext in electricalParameters)
                    {
                        try
                        {
                            Parameter param = ele.LookupParameter(paramtext.Key);
                            param.Set(paramtext.Value);
                        }
                        catch
                        {
                            Element typ = doc.GetElement(ele.GetTypeId());
                            Parameter param = typ.LookupParameter(paramtext.Key);
                            param.Set(paramtext.Value);
                        }
                    }
                    success++;
                }
                catch
                {
                    fail++;
                }
            }
            t.Commit();

            string msg = $"Fill parameters:\n\nSuccess  : {success} element(s)\nFailed     : {fail} element(s)";
            TaskDialog.Show("Error", msg);
            return Result.Succeeded;
        }
    }
}
