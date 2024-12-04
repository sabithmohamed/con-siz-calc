using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Idibri.RevitPlugin.ConduitSizeCalculator.Models;
using Idibri.RevitPlugin.Common;

namespace Idibri.RevitPlugin.ConduitSizeCalculator
{
    public class CalculateConduitParameter
    {
        public string CableSchedule { get; set; }
        public string DefaultConduitType { get; set; }
        public double MaxCableAreaPercent { get; set; }
    }

    public class ConduitCalculationResult
    {
        public CableBundle CableBundle { get; set; }
        public Conduit Conduit { get; set; }
        public string Description { get; set; }
    }

    public interface IConduitCalculator
    {
        DocumentHelper DocumentHelper { get; }
        ConduitCalculationResult GetNewConduitSize(CommandSettings commandSettings, string cableBundleDefinition, CalculateConduitParameter param, string conduitType, string currentConduitName);
        void UpdateConduit(Element element, CommandSettings commandSettings, ConduitParameters conduitPropertyPair, CalculateConduitParameter param);
        bool IsUsableElement(Element element);
    }

    public class ConduitCalculator : IConduitCalculator
    {
        #region Properties
        public DocumentHelper DocumentHelper { get; set; }

        private static readonly Regex CableTypeRegex = new Regex(@"^(\d*)([A-Za-z][A-Za-z0-9]*)$");

        private static int[] ImperialLengthOutputDenominators = new int[] { 1, 2, 3, 4, 6, 8, 16 };
        private const double ImperialLengthFractionCloseness = 0.01;

        public static HashSet<string> NonCalculableCableDefinitionStrings
        {
            get
            {
                if (_nonCalculableCableDefinitionStrings == null)
                {
                    _nonCalculableCableDefinitionStrings = new HashSet<string>() { "TBD", "FUTURE", "EMPTY", "" };
                }
                return _nonCalculableCableDefinitionStrings;
            }
        }
        private static HashSet<string> _nonCalculableCableDefinitionStrings = null;
        #endregion

        #region Constructor
        public ConduitCalculator(DocumentHelper documentHelper)
        {
            DocumentHelper = documentHelper;
        }
        #endregion

        #region Methods
        public void UpdateConduits(Element element, CommandSettings commandSettings, CalculateConduitParameter param)
        {
            foreach (ConduitParameters cpp in JunctionBoxConduits.ConduitParameters)
            {
                if (cpp.AutoCalculate)
                {
                    UpdateConduit(element, commandSettings, cpp, param);
                }
            }
        }

        public void UpdateConduit(Element element, CommandSettings commandSettings, ConduitParameters conduitParams, CalculateConduitParameter param)
        {
            string cableDefinition = (conduitParams.Fill.GetString(element) ?? "").Trim();
            string conduitType = (conduitParams.ConduitType.GetString(element) ?? "").Trim();
            string newConduitSize = null;

            if (conduitType.Length == 0)
            {
                conduitType = param.DefaultConduitType;
                conduitParams.ConduitType.SetString(element, conduitType);
            }

            if (cableDefinition.Length != 0)
            {
                string currentConduitName = conduitParams.Size.GetString(element);
                ConduitCalculationResult result = GetNewConduitSize(commandSettings, cableDefinition, param, conduitType, currentConduitName);
                newConduitSize = result.Description;
            }

            conduitParams.Size.SetString(element, newConduitSize);
        }

        public ConduitCalculationResult GetNewConduitSize(CommandSettings commandSettings, string cableBundleDefinition, CalculateConduitParameter param, string conduitType, string currentConduitName)
        {
            ConduitCalculationResult result = new ConduitCalculationResult();

            Conduit oldConduit = commandSettings.ConduitSchedule.ContainsConduit(conduitType, currentConduitName) ? commandSettings.ConduitSchedule.GetConduit(conduitType, currentConduitName) : null;

            try
            {
                result.CableBundle = GetCableBundle(commandSettings, param.CableSchedule, cableBundleDefinition, param);
            }
            catch (BadCableDefinitionStringException)
            {
                result.Description = "INVALID CABLING";
                return result;
            }

            if (result.CableBundle == null)
            {
                result.Description = null;
                return result;
            }
            else if (result.CableBundle.IsEmpty)
            {
                if (oldConduit != null)
                {
                    result.Description = oldConduit.Name;
                    result.Conduit = oldConduit;
                }
                else
                {
                    result.Description = null;
                    result.Conduit = null;
                }

                return result;
            }
            else
            {
                conduitType = conduitType ?? param.DefaultConduitType;

                if (!commandSettings.ConduitSchedule.ContainsConduitsOfType(conduitType))
                {
                    result.Description = "BAD CONDUIT TYPE";
                    return result;
                }
                else
                {
                    Conduit proposedConduit;

                    try
                    {
                        proposedConduit = commandSettings.ConduitSchedule.GetConduit(conduitType, result.CableBundle, param.MaxCableAreaPercent, true);
                    }
                    catch (NoSizeSpecifiedException)
                    {
                        // This occurs when one or more of the cables in the
                        // bundle has no OD specified.
                        result.Description = "BY EE";
                        return result;
                    }
                    catch (BadCableDefinitionStringException)
                    {
                        // Technically this will be handled by the
                        // HasConduitsOfType check above.
                        result.Description = "BAD CONDUIT TYPE";
                        return result;
                    }

                    if (proposedConduit == null)
                    {
                        result.Description = "TOO BIG";
                        return result;
                    }
                    else
                    {
                        ImperialLength currentSize = oldConduit != null ? new ImperialLength(0, oldConduit.TradeSizeIn) : null;
                        ImperialLength newSize = new ImperialLength(0, proposedConduit.TradeSizeIn);

                        if (currentSize == null || currentSize.CompareTo(newSize) < 0 || commandSettings.ShrinkOversizedConduits)
                        {
                            result.Conduit = proposedConduit;
                        }
                        else
                        {
                            result.Conduit = oldConduit;
                        }

                        result.Description = result.Conduit.Name;
                        return result;
                    }
                }
            }
        }

        public CableBundle GetCableBundle(CommandSettings commandSettings, string worksetName, string cableBundleDefinition, CalculateConduitParameter param)
        {
            cableBundleDefinition = (cableBundleDefinition ?? "").Trim();

            if (cableBundleDefinition.Length == 0)
            {
                return null;
            }

            if (NonCalculableCableDefinitionStrings.Contains(cableBundleDefinition.ToUpper()))
            {
                return new CableBundle() { IsEmpty = true };
            }

            CableBundle cableCollection = new CableBundle();

            foreach (string originalCableDefinition in cableBundleDefinition.Split(','))
            {
                string cableDefinition = originalCableDefinition.Trim();

                if (cableDefinition.Length == 0) { continue; }

                Match match = CableTypeRegex.Match(cableDefinition);

                if (!match.Success)
                {
                    throw new BadCableDefinitionStringException(string.Format("Bad Cable Definition Part '{0}' in '{1}'", cableDefinition, cableBundleDefinition));
                }

                int quantity = match.Groups[1].Value.Length == 0 ? 1 : int.Parse(match.Groups[1].Value);
                string cableName = match.Groups[2].Value;

                if (!commandSettings.CableSchedule.ContainsCable(worksetName, cableName))
                {
                    throw new BadCableDefinitionStringException(string.Format("Unknown Cable Name '{0}' in '{1}'", cableName, cableBundleDefinition));
                }

                Cable cable = commandSettings.CableSchedule.GetCable(worksetName, cableName);

                if (!cable.IsActive)
                {
                    throw new BadCableDefinitionStringException(string.Format("Inactive Cable: '{0}'", cable.Name));
                }

                cableCollection.Add(new CableBundleMember(cable, quantity));
            }

            return cableCollection;
        }

        public bool IsUsableElement(Element element)
        {
            return element.get_Parameter(JunctionBoxConduits.Conduit1.Size.Id) != null;
        }
        #endregion
    }
}
