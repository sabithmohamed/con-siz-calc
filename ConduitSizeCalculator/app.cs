using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConduitSizeCalculator
{
    internal class app : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Failed;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string assembname = Assembly.GetExecutingAssembly().Location;
            string imgpath = System.IO.Path.GetDirectoryName(assembname);

            string tabname = "ConCalc";
            application.CreateRibbonTab(tabname);

            return Result.Succeeded;
        }
    }
}
