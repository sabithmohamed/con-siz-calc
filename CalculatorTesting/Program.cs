using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Idibri.RevitPlugin.ConduitSizeCalculator;
using Idibri.RevitPlugin.ConduitSizeCalculator.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace CalculatorTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            UpdateFiles();
            //TestFilling();
            //TestImperialLength();
            //TestSizing();
        }

        #region Imperial Length Testing
        static void TestImperialLength()
        {
            //List<string> testStrings = new List<string>()
            //{
            //    "1'",
            //    "6\"",
            //    "2/3\"",
            //    "1' 6\"",
            //    "2'6\"",
            //    "3 1/4\"",
            //    "8' 9 1/8\"",
            //    "1.5\"",
            //    ".75\"",
            //    "1' .25\""
            //};

            //int[] denominators = { 1, 2, 3, 4, 8, 16, 32 };

            //foreach (string testString in testStrings)
            //{
            //    ImperialLength imperialLength = ImperialLength.TryParse(testString);
            //    if (imperialLength == null)
            //    {
            //        Debug.WriteLine("{0} doesn't match.", testString);
            //    }
            //    else
            //    {
            //        Debug.WriteLine("{0} => {1}", testString, imperialLength.ToString(denominators, 0.01));
            //    }
            //}

            //ImperialLength l0 = new ImperialLength(1, 6);
            //ImperialLength l1 = new ImperialLength(1, 5);
            //ImperialLength l2 = new ImperialLength(0, 8);

            //Debug.WriteLine("{0} ct {1} = {2}", l0, l1, l0.CompareTo(l1));
            //Debug.WriteLine("{0} ct {1} = {2}", l1, l0, l1.CompareTo(l0));
            //Debug.WriteLine("{0} ct {1} = {2}", l0, l2, l0.CompareTo(l2));
            //Debug.WriteLine("{0} ct {1} = {2}", l2, l0, l2.CompareTo(l0));
            //Debug.WriteLine("{0} ct {1} = {2}", l1, l2, l1.CompareTo(l2));
            //Debug.WriteLine("{0} ct {1} = {2}", l2, l1, l2.CompareTo(l1));
            //Debug.WriteLine("{0} ct {1} = {2}", l0, l0, l0.CompareTo(l0));
            //Debug.WriteLine("{0} ct {1} = {2}", l1, l1, l1.CompareTo(l1));
            //Debug.WriteLine("{0} ct {1} = {2}", l2, l2, l2.CompareTo(l2));

            //Debug.WriteLine(ImperialLength.DoubleToFraction(13.0/16, new int[] { 1, 2, 4, 8, 16, 32 }, 0.01));
        }
        #endregion

        #region Size Testing
        static void TestSizing()
        {
            List<string> testStrings = new List<string>()
            {
                "1\" PVC",
                "1'3\" EMT",
                "18'5\""
            };

            foreach (string testString in testStrings)
            {
                ConduitFillRepresentation cr = ConduitFillRepresentation.TryParse(testString);

                if (cr != null)
                {
                    Debug.WriteLine("Size: {0}; Type: {1}", cr.Diameter, cr.ConduitType ?? "Unspecified");
                }
                else
                {
                    Debug.WriteLine("Invalid sizing: {0}", testString);
                }   
            }
        }
        #endregion

        #region Fill Testing
        static void TestFilling()
        {
            UpdateSelectedElementsConduitsCommand cmd = new UpdateSelectedElementsConduitsCommand();
            
            List<string> testStrings = new List<string>()
            {
                "9E12,12E",
                "38U",
                "28A12",
                "15A10",
                "2E12",
                "8U,1Y",
                "2F,2H,1N",
                "2D12,1N",
                "4F,5H,8U",
                "8U"
            };

            CalculateConduitParameter param = new CalculateConduitParameter()
            {
                DefaultConduitType = "EMT",
                CableSchedule = "AV",
                MaxCableAreaPercent = 0.30
            };

            foreach (string testString in testStrings)
            {
                OutputInfo(testString, param);
            }
        }

        static void OutputInfo(string cableBundleDefinition, CalculateConduitParameter param)
        {
            //CableBundle cc = ConduitManagerStatic.GetCableBundle(param.WorksetName, cableBundleDefinition);
            //Conduit cd = ConduitManagerStatic.ConduitSchedule.GetConduit(param.ConduitType, cc, param.MaxCableAreaPercent);
            //Debug.WriteLine("Area: {1:#,##0.0000}; Min Conduit Area: {2:#,##0.0000}; Conduit Size: {3}; Cables: {0}",
            //    cableBundleDefinition,
            //    cc.CableArea,
            //    cc.GetMinimumConduitArea(param.MaxCableAreaPercent),
            //    cd == null ? "Too Big" : cd.Name);
            throw new NotImplementedException();
        }

        static void OutputInfo(UpdateSelectedElementsConduitsCommand cmd, string cableBundleDefinition, CalculateConduitParameter param)
        {
            //Conduit conduit = ConduitManagerStatic.GetConduit(cableBundleDefinition, param);
            //Debug.WriteLine("Conduit: {0}; Cable Definition: {1}", conduit == null ? "Too big" : conduit.Name, cableBundleDefinition);
            throw new NotImplementedException();
        }
        #endregion

        #region XML File
        static void UpdateFiles()
        {
            string directory = @"Y:\Dropbox\Idibri\Revit Conduit Addin\Schedules\";
            List<Tuple<string, string, string>> files = new List<Tuple<string, string, string>>()
            {
                new Tuple<string, string, string>("Idibri Conduits - Imperial.csv", "Idibri Cables.csv", "IdibriImperial.xml"),
                new Tuple<string, string, string>("Idibri Conduits - Metric.csv", "Idibri Cables.csv", "IdibriMetric.xml"),
                new Tuple<string, string, string>("All Conduits - Imperial.csv", null, "Imperial.xml"),
                new Tuple<string, string, string>("All Conduits - Metric.csv", null, "Metric.xml")
            };

            foreach (var t in files)
            {
                CommandSettings commandSettings = new CommandSettings()
                {
                    CableSchedule = t.Item2==null?new CableSchedule() : new CableSchedule(CsvToList(directory + t.Item2, 1, CableLineConverter)),
                    ConduitSchedule = new ConduitSchedule(CsvToList(directory + t.Item1, 1, ConduitLineConverter)),
                    DefaultMaxCableAreaPercent = 0.30,
                    WorksetToCableScheduleMap = null,
                    DefaultConduitType = "EMT"
                };
                //using (StreamWriter sw = new StreamWriter(directory + "UpdateSelectedElementConduitsCommandSettings.xml", false))
                using (StreamWriter sw = new StreamWriter(directory + t.Item3, false))
                {
                    (new XmlSerializer(commandSettings.GetType())).Serialize(sw, commandSettings);
                }
            }
        }

        static List<T1> CsvToList<T1>(string fromFilename, int skipLines, Func<string[], T1> lineConverter)
        {
            List<T1> list = new List<T1>();

            using (StreamReader sr = new StreamReader(fromFilename))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (skipLines-- > 0) { continue; }
                    string[] fields = CsvSplit(line, ',', '"');
                    list.Add(lineConverter(fields));
                }
            }

            return list;
        }

        static Conduit ConduitLineConverter(string[] fields)
        {
            return new Conduit(
                fields[0],
                fields[1],
                double.Parse(fields[2]),
                double.Parse(fields[3]),
                double.Parse(fields[4]),
                double.Parse(fields[5]),
                double.Parse(fields[6]),
                double.Parse(fields[7]));
        }

        static Cable CableLineConverter(string[] fields)
        {
            double od_in = 0;
            double? od_in_nullable = null;
            double od_mm = 0;
            double? od_mm_nullable = null;
            decimal cost = 0;
            decimal? cost_nullable = null;

            if (double.TryParse(fields[8].Trim(), out od_in)) { od_in_nullable = od_in; }
            if (double.TryParse(fields[9].Trim(), out od_mm)) { od_mm_nullable = od_mm; }
            if (decimal.TryParse(fields[11].Trim(), out cost)) { cost_nullable = cost; }
            
            return new Cable()
            {
                GroupName = fields[0].Trim(),
                Name = fields[1].Trim(),
                Manufacturer = fields[2].Trim(),
                StandardPartNumber = fields[3].Trim(),
                UnderGroundWetPartNumber = fields[4].Trim(),
                PlenumPartNumber = fields[5].Trim(),
                Application = fields[6].Trim(),
                Description = fields[7].Trim(),
                NominalOutsideDiameterIn = od_in_nullable,
                NominalOutsideDiameterMm = od_mm_nullable,
                SignalGroup = fields[10].Trim(),
                CostPerFoot = cost
            };
        }

        static string[] CsvSplit(string line, char fieldTerminator, char fieldIndicator)
        {
            string[] split = line.Split(fieldTerminator);
            string fieldIndicatorReplace = new string(fieldIndicator, 2);
            string fieldIndicatorReplacement = new string(fieldIndicator, 1);
            List<string> final = new List<string>();
            bool append = false;

            foreach (string s in split)
            {
                if (append)
                {
                    string field = s;
                    Match m = Regex.Match(field, "\\" + fieldIndicator + "+$");
                    if (m.Success && m.Groups[0].Length % 2 == 1) // If the field ends with an odd number of indicators
                    {
                        append = false;
                        field = field.Substring(0, field.Length - 1);
                    }
                    final[final.Count - 1] += field.Replace(fieldIndicatorReplace, fieldIndicatorReplacement);
                }
                else
                {
                    string field = null;

                    if (s.Length != 0)
                    {
                        if (s[0] == fieldIndicator)
                        {
                            if (s[s.Length - 1] == fieldIndicator)
                            {
                                // Quoted field.
                                field = s.Substring(1, s.Length - 2);
                            }
                            else
                            {
                                // Start quoted field.
                                field = s.Substring(1);
                                append = true;
                            }

                            field = field.Replace(fieldIndicatorReplace, fieldIndicatorReplacement);
                        }
                        else
                        {
                            field = s;
                        }
                    }
                    else
                    {
                        field = "";
                    }

                    final.Add(field);
                }
            }

            return final.ToArray();
        }
        #endregion
    }
}
