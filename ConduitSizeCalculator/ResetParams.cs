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
    public class ResetParams : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            

            var eles = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).ToList();
            List<string> electricalParameters = new List<string>
            {
            "Signal Type",
            "Size1",
            "Destination1",
            "Fill1",
            "Cable Destination 1",
            "Conduit Type1",
            "Size2",
            "Destination2",
            "Fill2",
            "Cable Destination 2",
            "Conduit Type2",
            "Size3",
            "Destination3",
            "Fill3",
            "Cable Destination 3",
            "Conduit Type3",
            "Size4",
            "Destination4",
            "Fill4",
            "Cable Destination 4",
            "Conduit Type4",
            "Size5",
            "Destination5",
            "Fill5",
            "Cable Destination 5",
            "Conduit Type5",
            "Size6",
            "Destination6",
            "Fill6",
            "Cable Destination 6",
            "Conduit Type6"
            };

            int success = 0;
            int fail = 0;

            var t = new Transaction(doc, "Reset conduit params");
            t.Start();
            foreach (Element ele in eles)
            {
                try
                {
                    foreach (string paramtext in electricalParameters)
                    {
                        try
                        {
                            Parameter param = ele.LookupParameter(paramtext);
                            param.Set("");
                        }
                        catch
                        {
                            Element typ = doc.GetElement(ele.GetTypeId());
                            Parameter param = typ.LookupParameter(paramtext);
                            param.Set("");
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

            string msg = $"Reset parameters:\n\nSuccess  : {success} element(s)\nFailed     : {fail} element(s)";
            TaskDialog.Show("Error", msg);
            return Result.Succeeded;
        }
    }
}
