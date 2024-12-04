using System.Collections.Generic;
using System.Text.RegularExpressions;
using Idibri.RevitPlugin.Common.Infrastructure;
using Idibri.RevitPlugin.Common;

namespace Idibri.RevitPlugin.ConduitSizeCalculator.Models
{
    public class ConduitFillRepresentation : ModelBase
    {
        #region Properties
        public string ConduitType { get; set; }
        public ImperialLength Diameter { get; set; }

        private static Regex ParseRegex = new Regex(@"^(.*?)(?:\s(.*))?$");
        #endregion

        #region Constructors
        public ConduitFillRepresentation(string conduitType, ImperialLength diameter)
            : base()
        {
            ConduitType = conduitType;
            Diameter = diameter;
        }

        public static ConduitFillRepresentation TryParse(string representation)
        {
            Match match = ParseRegex.Match(representation ?? "");
            if (!match.Success) { return null; }
            ImperialLength il = ImperialLength.TryParse(match.Groups[1].Value);
            if (il == null) { return null; }
            return new ConduitFillRepresentation(match.Groups[2].Success ? match.Groups[2].Value : null, il);
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return string.Format("{0} {1}", Diameter, ConduitType);
        }

        public string ToString(IEnumerable<int> denominators, double closeness)
        {
            return string.Format("{0} {1}", Diameter.ToString(denominators, closeness), ConduitType);
        }
        #endregion
    }
}
