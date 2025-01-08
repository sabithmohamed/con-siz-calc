using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Idibri.RevitPlugin.ConduitSizeCalculator.Models;
using Microsoft.Win32;
using Idibri.RevitPlugin.Common;

namespace Idibri.RevitPlugin.ConduitSizeCalculator
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UpdateSelectedElementsConduitsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            DocumentHelper documentHelper = new DocumentHelper(uiDoc);
            CommandSettings commandSettings = documentHelper.LoadSettings();
            ConduitCalculator calculator = new ConduitCalculator(documentHelper);

            List<Element> useElements = new List<Element>();

            List<Element> selectedElements = uiDoc.Selection.GetElementIds().Select(eid => uiDoc.Document.GetElement(eid)).ToList();

            foreach (Element element in selectedElements)
            {
                if (calculator.IsUsableElement(element))
                {
                    useElements.Add(element);
                }
            }

            if (useElements.Count == 0)
            {
                UserInteraction.AlertUserWithMessageBox("No elements selected", "No Elements");
            }
            else
            {
                try
                {
                    documentHelper.UpdateElements(useElements, calculator, commandSettings);
                }
                catch (CommandExceptionBase ex)
                {
                    UserInteraction.AlertUserWithMessageBox(ex.Message, "An Error Occurred");
                    return Result.Failed;
                }
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UpdateAllElementsConduitsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            DocumentHelper documentHelper = new DocumentHelper(uiDoc);
            CommandSettings commandSettings = documentHelper.LoadSettings();
            ConduitCalculator calculator = new ConduitCalculator(documentHelper);

            List<Element> useElements = new List<Element>();
            FilteredElementCollector collector = new FilteredElementCollector(uiDoc.Document);
            collector.WherePasses(new ElementClassFilter(typeof(FamilyInstance)));

            foreach (Element element in collector)
            {
                if (calculator.IsUsableElement(element))
                {
                    useElements.Add(element);
                }
            }

            if (useElements.Count == 0)
            {
                UserInteraction.AlertUserWithMessageBox("No elements found to update", "No Elements");
            }
            else
            {
                try
                {
                    documentHelper.UpdateElements(useElements, calculator, commandSettings);
                    UserInteraction.AlertUserWithMessageBox(string.Format("Updated {0} Elements.", useElements.Count), "Elements Updated");
                }
                catch (CommandExceptionBase ex)
                {
                    UserInteraction.AlertUserWithMessageBox(ex.Message, "An Error Occurred");
                    return Result.Failed;
                }
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ManageConduitsCommand : IExternalCommand
    {
        static bool AllowHeterogenousWorksetSelection = true;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            DocumentHelper documentHelper = new DocumentHelper(uiDoc);
            CommandSettings commandSettings = documentHelper.LoadSettings();
            ConduitCalculator calculator = new ConduitCalculator(documentHelper);
            
            List<Element> useElements = new List<Element>();
            string firstWorkset = null;
            bool isHeterogenousWorksetSelection = false;

            List<Element> selectedElements = uiDoc.Selection.GetElementIds().Select(eid => uiDoc.Document.GetElement(eid)).ToList();

            foreach (Element element in selectedElements)
            {
                if (calculator.IsUsableElement(element))
                {
                    string workset = documentHelper.GetWorkset(element).Name;
                    if (firstWorkset == null)
                    {
                        firstWorkset = workset;
                    }
                    else
                    {
                        if (workset != firstWorkset)
                        {
                            isHeterogenousWorksetSelection = true;
                            if (!AllowHeterogenousWorksetSelection) { break; }
                        }
                    }
                    useElements.Add(element);
                }
            }

            if (isHeterogenousWorksetSelection && !AllowHeterogenousWorksetSelection)
            {
                MessageBox.Show("The elements you have selected do not all belong to the same workset. Please select all elements from the same workset.", "Heterogenous Workset Selection");
            }
            else
            {
                try
                {
                    Window window = null;

                    ViewModels.ViewModelIoc viewModelIoc = new ViewModels.ViewModelIoc()
                    {
                        OnDone = () => window.Close(),
                        GetSaveFileName = UserInteraction.GetSaveFileName,
                        AlertUser = UserInteraction.AlertUserWithMessageBox,
                        LoadSettings = () =>
                        {
                            string filename = UserInteraction.GetOpenFileName(new FileSearchOptions()
                            {
                                Filter = "XML documents (.xml)|*.xml",
                                Title = "Open Command Settings",
                                FileName = commandSettings.AssociatedFilename
                            });

                            if (filename != null)
                            {
                                return documentHelper.LoadSettings(filename);
                            }
                            return null;
                        }
                    };
  
                    ViewModels.EditMasterViewModel masterViewModel = new ViewModels.EditMasterViewModel(commandSettings, calculator, viewModelIoc, uiDoc)
                    {
                        Elements = useElements,
                        _uiDoc_master = uiDoc,
                    };

                    window = new Window()
                    {
                        Content = new Views.EditMasterView(),
                        DataContext = masterViewModel,
                        ResizeMode = ResizeMode.NoResize,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    window.SetBinding(Window.TitleProperty, "Title");
                    window.KeyUp += (s, e) => { if (e.Key == Key.Escape) { window.Close(); } };

                    using (Transaction transaction = new Transaction(uiDoc.Document))
                    {
                        transaction.Start("Conduit Size Calc");
                        window.ShowDialog();

                        if (documentHelper.CanAssociateCommandSettingsWithDocument && masterViewModel.CommandSettings.AssociatedFilename != null)
                        {
                            documentHelper.SaveSettingsPath(masterViewModel.CommandSettings.AssociatedFilename);
                        }
                        transaction.Commit();
                    }
                }
                catch (CommandExceptionBase ex)
                {
                    UserInteraction.AlertUserWithMessageBox(ex.Message, "An Error Occurred");
                    return Result.Failed;
                }
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ConduitReportingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            DocumentHelper documentHelper = new DocumentHelper(uiDoc);
            CommandSettings commandSettings = documentHelper.LoadSettings();
            ConduitCalculator calculator = new ConduitCalculator(documentHelper);

            Func<IEnumerable<Element>> GetElements = () =>
            {
                List<Element> useElements = new List<Element>();
                FilteredElementCollector collector = new FilteredElementCollector(uiDoc.Document);
                collector.WherePasses(new ElementClassFilter(typeof(FamilyInstance)));

                foreach (Element element in collector)
                {
                    if (calculator.IsUsableElement(element))
                    {
                        useElements.Add(element);
                    }
                }

                return useElements;
            };

            ViewModels.ReportingMasterViewModel reportingMasterViewModel;
            try
            {
                reportingMasterViewModel = new ViewModels.ReportingMasterViewModel(documentHelper, commandSettings, GetElements);
            }
            catch (CommandExceptionBase ex)
            {
                MessageBox.Show(ex.Message, "An Error Occurred");
                return Result.Failed;
            }

            Window window = null;

            window = new Window()
            {
                Content = new Views.ReportingMasterView(),
                DataContext = reportingMasterViewModel,
                ResizeMode = ResizeMode.CanResize,
                SizeToContent = SizeToContent.Width,
                Height = 400,
                Title = "Conduit Reporting",
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.KeyUp += (s, e) => { if (e.Key == Key.Escape) { window.Close(); } };

            window.Show();

            return Result.Succeeded;
        }
    }

    public class DocumentHelper
    {
        #region Properties
        static readonly RevitProperty CommandSettingsPathProperty = new RevitProperty("Project Conduit Calculator Settings Path", new Guid("84d0cb2b-ce62-4194-8212-410c06399733"));

        public UIDocument UIDocument { get; private set; }
        public Document Document { get { return UIDocument.Document; } }
        private Parameter CommandSettingsPathParameter { get { return CommandSettingsPathProperty.GetParameter(Document.ProjectInformation); } }
        public string CommandSettingsPath { get { return CommandSettingsPathParameter.AsString() ?? null; } }
        public bool CanAssociateCommandSettingsWithDocument { get { return CommandSettingsPathParameter != null; } }
        #endregion

        #region Constructors
        public DocumentHelper(UIDocument document)
        {
            UIDocument = document;
        }
        #endregion

        #region Methods
        public void SaveSettingsPath(string path)
        {
            CommandSettingsPathParameter.Set(path);
        }

        public CommandSettings LoadSettings()
        {
            return LoadSettings(CommandSettingsPath);
        }

        public CommandSettings LoadSettings(string filename)
        {
            return StaticCommandSettings.GetSettings(filename).MergeWorksets(GetWorksets().Select(ws => ws.Name));
        }

        public IList<Workset> GetWorksets()
        {
            return GetWorksets(WorksetKind.UserWorkset);
        }

        public IList<Workset> GetWorksets(WorksetKind worksetKind)
        {
            FilteredWorksetCollector collector = new FilteredWorksetCollector(Document);
            collector.OfKind(worksetKind);
            return collector.ToWorksets();
        }

        public Workset GetWorkset(Element element)
        {
            return Document.GetWorksetTable().GetWorkset(Document.GetWorksetId(element.Id));
        }

        public Workset GetWorkset(string worksetName)
        {
            foreach (Workset workset in GetWorksets())
            {
                if (workset.Name == worksetName)
                {
                    return workset;
                }
            }
            return null;
        }

        public void UpdateElements(IEnumerable<Element> elements, ConduitCalculator calculator, CommandSettings commandSettings)
        {
            using (Transaction transaction = new Transaction(Document))
            {
                transaction.Start("Conduit Size Calc");
                foreach (Element element in elements)
                {
                    Workset wks = GetWorkset(element);
                    WorksetCableSchedulePair wcsp = commandSettings.WorksetToCableScheduleMap.Pairs.FirstOrDefault(p => p.WorksetName == wks.Name);

                    if (wcsp != null)
                    {
                        CalculateConduitParameter param = new CalculateConduitParameter()
                        {
                            CableSchedule = wcsp.CableSchedule,
                            DefaultConduitType = commandSettings.DefaultConduitType,
                            MaxCableAreaPercent = commandSettings.DefaultMaxCableAreaPercent
                        };
                        calculator.UpdateConduits(element, commandSettings, param);
                    }
                }
                transaction.Commit();
            }
        }

        public void HighlightElement(Element element)
        {
            UIDocument.ShowElements(element.Id);
            //UIDocument.Selection.Elements.Clear();
            //UIDocument.Selection.Elements.Add(element);
            UIDocument.Selection.SetElementIds(new List<ElementId>() { element.Id });
        }
        #endregion
    };

    public class FileSearchOptions
    {
        public bool AddExtension { get; set; }
        public string DefaultExtension { get; set; }
        public string FileName { get; set; }
        public string Filter { get; set; }
        public bool OverwritePrompt { get; set; }
        public string Title { get; set; }
    }

    public class UserInteraction
    {
        public static string GetSaveFileName(FileSearchOptions fso)
        {
            string initialDirectory = fso.FileName != null ? System.IO.Path.GetDirectoryName(fso.FileName) : null;
            string filename = fso.FileName != null ? System.IO.Path.GetFileName(fso.FileName) : null;

            SaveFileDialog sfd = new SaveFileDialog()
            {
                AddExtension = fso.AddExtension,
                DefaultExt = fso.DefaultExtension,
                FileName = filename,
                Filter = fso.Filter,
                InitialDirectory = initialDirectory,
                OverwritePrompt = fso.OverwritePrompt,
                Title = fso.Title
            };

            bool? result = sfd.ShowDialog();

            if (result == true)
            {
                return sfd.FileName;
            }
            return null;
        }

        public static string GetOpenFileName(FileSearchOptions fso)
        {
            string initialDirectory = fso.FileName != null ? System.IO.Path.GetDirectoryName(fso.FileName) : null;
            string filename = fso.FileName != null ? System.IO.Path.GetFileName(fso.FileName) : null;

            OpenFileDialog ofd = new OpenFileDialog()
            {
                FileName = filename,
                Filter = fso.Filter,
                InitialDirectory = initialDirectory,
                Title = fso.Title
            };

            bool? result = ofd.ShowDialog();

            if (result == true)
            {
                return ofd.FileName;
            }
            return null;
        }

        public static void AlertUserWithMessageBox(string message, string title)
        {
            MessageBox.Show(message, title);
        }
    }

    public class StaticCommandSettings
    {
        private static object LoadLock = new object();

        private static CommandSettings _settings;
        public static CommandSettings Settings
        {
            get { return _settings; }
            private set
            {
                if (_settings != value)
                {
                    _settings = value;
                }
            }
        }

        public static CommandSettings GetSettings()
        {
            return GetSettings(null);
        }

        public static CommandSettings GetSettings(string externalPath)
        {
            if (Settings == null || externalPath != null)
            {
                FileInfo fileInfo = new FileInfo(externalPath);

                if (Settings == null || fileInfo.Exists && (externalPath != Settings.AssociatedFilename || fileInfo.LastWriteTime != Settings.AssociatedFileChangeTime))
                {
                    LoadSettings(fileInfo);
                }
            }

            if (string.IsNullOrEmpty(Settings.DefaultConduitType))
            {
                // There will always be a default, or an error will be thrown.
                Settings.DefaultConduitType = Settings.ConduitSchedule.ConduitTypes.First();
            }

            return Settings;
        }

        private static void LoadSettings(FileInfo fileInfo)
        {
            lock (LoadLock)
            {
                if (fileInfo.Exists)
                {
                    try
                    {
                        Settings = Helpers.LoadFromXmlFile<CommandSettings>(fileInfo.FullName);
                        Settings.AssociatedFilename = fileInfo.FullName;
                        Settings.AssociatedFileChangeTime = fileInfo.LastWriteTime;
                        Settings.IsFromResource = false;
                        Settings.FileLoadException = null;
                    }
                    catch (Exception ex)
                    {
                        Settings = Helpers.LoadFromXmlResource<CommandSettings>(Assembly.GetExecutingAssembly(), ResourcePath);
                        Settings.AssociatedFilename = null;
                        Settings.IsFromResource = true;
                        Settings.FileLoadException = new Exception(string.Format("An error occurred when loading the settings at {0}: {1}", fileInfo.FullName, ex.Message), ex);
                    }
                }
                else
                {
                    Settings = Helpers.LoadFromXmlResource<CommandSettings>(Assembly.GetExecutingAssembly(), ResourcePath);
                    Settings.AssociatedFilename = null;
                    Settings.IsFromResource = true;
                    Settings.FileLoadException = null;
                }
            }
        }

        private static readonly string ResourcePath = "ConduitSizeCalculator.Resources.CommandSettings.xml";
    }

    public class ConduitParameters
    {
        public RevitProperty Mark { get; set; }
        public RevitProperty Size { get; private set; }
        public RevitProperty Fill { get; private set; }
        public RevitProperty ConduitType { get; private set; }
        public RevitProperty Destination { get; private set; }
        public RevitProperty CableDestination { get; private set; }
        public bool AutoCalculate { get; private set; }

        public ConduitParameters(RevitProperty size, RevitProperty fill, RevitProperty conduitType, RevitProperty destination, RevitProperty cableDestination, bool autoCalculate, RevitProperty mark = null)
        {
            Size = size;
            Fill = fill;
            ConduitType = conduitType;
            Destination = destination;
            CableDestination = cableDestination;
            AutoCalculate = autoCalculate;
            Mark = mark;
        }
    }

    public class JunctionBoxConduits
    {
        static readonly RevitProperty Mark = new RevitProperty("Mark", new Guid("81DB8C34-F0F1-41FE-B736-E091D5D8DD2C"));

        static readonly RevitProperty Size1 = new RevitProperty("Size1", new Guid("6c3bc498-1ef6-4cfd-9c18-0722fab4c93e"));
        static readonly RevitProperty Size2 = new RevitProperty("Size2", new Guid("f518bcd9-d84b-426a-84bc-34e134528681"));
        static readonly RevitProperty Size3 = new RevitProperty("Size3", new Guid("9d7ab982-063c-4137-b7ef-73536ea75c84"));
        static readonly RevitProperty Size4 = new RevitProperty("Size4", new Guid("f7654c19-6d96-414a-968e-98353e36639f"));
        static readonly RevitProperty Size5 = new RevitProperty("Size5", new Guid("01eb6b4e-10b7-4aa4-9a48-76a7cb78f90a"));
        static readonly RevitProperty Size6 = new RevitProperty("Size6", new Guid("51c9b74c-3afa-4f75-8bcb-e1ee6c1b467a"));

        static readonly RevitProperty Fill1 = new RevitProperty("Fill1", new Guid("58bdfa34-6421-431f-aec3-76778f184bd4"));
        static readonly RevitProperty Fill2 = new RevitProperty("Fill2", new Guid("2e5a4514-5d53-467a-b507-f5f243f9ffdd"));
        static readonly RevitProperty Fill3 = new RevitProperty("Fill3", new Guid("b91a6c00-b78e-4727-be2a-734f9f982635"));
        static readonly RevitProperty Fill4 = new RevitProperty("Fill4", new Guid("2c181937-626f-462c-b93e-93d465d8ffd6"));
        static readonly RevitProperty Fill5 = new RevitProperty("Fill5", new Guid("c01c055d-3484-44d5-8e0f-0f2b3dac7c14"));
        static readonly RevitProperty Fill6 = new RevitProperty("Fill6", new Guid("afedb458-e158-4629-a339-251e85884525"));

        static readonly RevitProperty ConduitType1 = new RevitProperty("Conduit Type1", new Guid("025010e9-0146-4fde-83db-5508e421a199"));
        static readonly RevitProperty ConduitType2 = new RevitProperty("Conduit Type2", new Guid("101ae441-5d3e-4e6e-b4da-57782de94c31"));
        static readonly RevitProperty ConduitType3 = new RevitProperty("Conduit Type3", new Guid("fe10c2b4-67a1-49d8-8a66-c61de317e37b"));
        static readonly RevitProperty ConduitType4 = new RevitProperty("Conduit Type4", new Guid("0e3cadc6-48c0-4cdf-9197-45d50998eb3c"));
        static readonly RevitProperty ConduitType5 = new RevitProperty("Conduit Type5", new Guid("7a8a2b8f-bf6c-4b06-a964-f64df3f1f59f"));
        static readonly RevitProperty ConduitType6 = new RevitProperty("Conduit Type6", new Guid("07b9fab7-5654-4e69-b44b-83d52cecbe76"));

        static readonly RevitProperty Destination1 = new RevitProperty("Destination1", new Guid("f395aae7-823d-4cf6-bd05-4628564bfa83"));
        static readonly RevitProperty Destination2 = new RevitProperty("Destination2", new Guid("2fd0d09c-c834-4268-bfb3-f9088bad7515"));
        static readonly RevitProperty Destination3 = new RevitProperty("Destination3", new Guid("71633baf-6bea-4ae7-a927-18c131a063f1"));
        static readonly RevitProperty Destination4 = new RevitProperty("Destination4", new Guid("873bd505-b34c-4780-8b83-b0953b4e85c7"));
        static readonly RevitProperty Destination5 = new RevitProperty("Destination5", new Guid("4f382b12-a203-4942-9f44-32151a1725f7"));
        static readonly RevitProperty Destination6 = new RevitProperty("Destination6", new Guid("4ff0fd82-11c2-4081-91d1-a5a009581135"));

        static readonly RevitProperty CableDestination1 = new RevitProperty("Cable Destination 1", new Guid("ace90422-871b-4351-8e0f-638eeb5c08b4"));
        static readonly RevitProperty CableDestination2 = new RevitProperty("Cable Destination 2", new Guid("781ca782-cca8-448e-bb1a-4ec392993bce"));
        static readonly RevitProperty CableDestination3 = new RevitProperty("Cable Destination 3", new Guid("2eaaf659-837f-41ef-8f0c-0447318f2ef1"));
        static readonly RevitProperty CableDestination4 = new RevitProperty("Cable Destination 4", new Guid("a2f4e7b8-8ccd-4ad2-a7e9-138f4f6b08b3"));
        static readonly RevitProperty CableDestination5 = new RevitProperty("Cable Destination 5", new Guid("87adeae0-1cdc-455b-9d47-b6b2d6a4edae"));
        static readonly RevitProperty CableDestination6 = new RevitProperty("Cable Destination 6", new Guid("2648cdfd-82ee-47d1-b64a-4c2f170329e3"));

        public static readonly ConduitParameters Conduit1 = new ConduitParameters(Size1, Fill1, ConduitType1, Destination1, CableDestination1, true, Mark);
        public static readonly ConduitParameters Conduit2 = new ConduitParameters(Size2, Fill2, ConduitType2, Destination2, CableDestination2, true, Mark);
        public static readonly ConduitParameters Conduit3 = new ConduitParameters(Size3, Fill3, ConduitType3, Destination3, CableDestination3, true, Mark);
        public static readonly ConduitParameters Conduit4 = new ConduitParameters(Size4, Fill4, ConduitType4, Destination4, CableDestination4, true, Mark);
        public static readonly ConduitParameters Conduit5 = new ConduitParameters(Size5, Fill5, ConduitType5, Destination5, CableDestination5, true, Mark);
        public static readonly ConduitParameters Conduit6 = new ConduitParameters(Size6, Fill6, ConduitType6, Destination6, CableDestination6, true, Mark);

        public static IEnumerable<ConduitParameters> ConduitParameters
        {
            get
            {
                yield return Conduit1;
                yield return Conduit2;
                yield return Conduit3;
                yield return Conduit4;
                yield return Conduit5;
                yield return Conduit6;
            }
        }
    }

    public class JunctionBoxParameters
    {
        public static readonly RevitProperty BackBoxInstalledBy = new RevitProperty("Back Box Installed By", new Guid("9970f3e6-29dc-48bb-9f04-70aea5c867d4"));
        public static readonly RevitProperty BackBoxProvidedBy = new RevitProperty("Back Box Provided By", new Guid("8a32d8a2-2f25-4c49-9333-d364915b85cf"));

        public static readonly RevitProperty DeviceInstalledBy = new RevitProperty("Device Installed By", new Guid("d43b0223-30e8-445a-b96b-335b80ee54ce"));
        public static readonly RevitProperty DeviceProvidedBy = new RevitProperty("Device Provided By", new Guid("e187597f-89ad-47c4-87bd-d5ae3e698319"));

        public static readonly RevitProperty PanelInstalledBy = new RevitProperty("Panel Installed By", new Guid("013fac66-3b6a-48ac-9aae-25ddfde9bb31"));
        public static readonly RevitProperty PanelProvidedBy = new RevitProperty("Panel Provided By", new Guid("ddff7f41-c9c1-41e5-999c-291e75070fa1"));

        public static readonly RevitProperty CustomPanel = new RevitProperty("Custom Panel", new Guid("5268f224-4fa6-4aa8-9880-3160cbc7d258"));
        public static readonly RevitProperty FlushMount = new RevitProperty("Flush Mount", new Guid("7b660f2f-9ae4-4406-a2ae-8aee4fa16621"));

        public static readonly RevitProperty NemaType = new RevitProperty("NEMA Type", new Guid("329d522b-8eba-4c22-8d60-918a52932d03"));

        public static readonly RevitProperty Notes = new RevitProperty("Notes", new Guid("22700de1-a113-43c1-a0f7-57f0346f9164"));

        public static readonly RevitProperty Depth = new RevitProperty("Depth", new Guid("464db563-f483-43db-b409-62f49cd7a35b"), true, true);
        public static readonly RevitProperty Height = new RevitProperty("Height", new Guid("3946b890-b3f7-46f8-9ad3-b89ee87f5fc4"), true, true);
        public static readonly RevitProperty Width = new RevitProperty("Width", new Guid("595c71ca-3064-44c1-a226-3fd479cd6593"), true, true);
    }


}
