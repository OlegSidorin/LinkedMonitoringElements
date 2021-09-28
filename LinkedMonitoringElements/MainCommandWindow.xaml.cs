using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static LinkedMonitoringElements.CM;

namespace LinkedMonitoringElements
{
    /// <summary>
    /// Логика взаимодействия для MainCommandWindow.xaml
    /// </summary>
    public partial class MainCommandWindow : Window
    {
        public ExternalCommandData _CommandData;
        public ButtonArrowLeftClick_ExternalEventHandler ButtonArrowLeftClick_ExternalEventHandler;
        public ExternalEvent ButtonArrowLeftClick_ExternalEvent;

        public MainCommandWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            ButtonArrowLeftClick_ExternalEventHandler = new ButtonArrowLeftClick_ExternalEventHandler();
            ButtonArrowLeftClick_ExternalEvent = ExternalEvent.Create(ButtonArrowLeftClick_ExternalEventHandler);
            
        }

        private void buttonCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonLeftClick(object sender, RoutedEventArgs e)
        {
            ButtonArrowLeftClick_ExternalEventHandler._WindowMain = this;
            ButtonArrowLeftClick_ExternalEventHandler._CommandData = _CommandData;
            ButtonArrowLeftClick_ExternalEvent.Raise();
        }
    }
    public class ButtonArrowLeftClick_ExternalEventHandler : IExternalEventHandler
    {
        public MainCommandWindow _WindowMain;
        public ExternalCommandData _CommandData;
        public void Execute(UIApplication app)
        {

            return;
        }

        public string GetName()
        {
            return "External Left Event";
        }
    }
    class CM
    {
        public static void SetParameterInFamilyInstanceAsDouble(Document doc, FamilyInstance familyInstance, string parameterName, double valueDouble)
        {
            Parameter parameter = familyInstance.LookupParameter(parameterName);
            using (Transaction t = new Transaction(doc,"apply"))
            {
                t.Start();
                parameter.Set(valueDouble);
                t.Commit();
            }
        }
        public static void SetParameterInFamilyInstanceAsString(Document doc, FamilyInstance familyInstance, string parameterName, string valueString)
        {
            Parameter parameter = familyInstance.LookupParameter(parameterName);
            using (Transaction t = new Transaction(doc, "apply"))
            {
                t.Start();
                parameter.Set(valueString);
                t.Commit();
            }
        }
        public static double GetParameterInLinkedFamilyInstanceAsDouble(FamilyInstance linkedFamilyInstance, string parameterName)
        {
            Parameter parameter = linkedFamilyInstance.LookupParameter(parameterName);
            return parameter.AsDouble();
        }
        public static string GetParameterInLinkedFamilyInstanceAsString(FamilyInstance linkedFamilyInstance, string parameterName)
        {
            Parameter parameter = linkedFamilyInstance.LookupParameter(parameterName);
            return parameter.AsString();
        }
        public static ElementId GetElementId_OfMonitoredElement(Document linkedDoc, string FamilyName, XYZ xyz)
        {
            IList<Element> elements = new FilteredElementCollector(linkedDoc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().ToElements();
            ElementId elementId = null;
            List<FamilyInstance> fiList = new List<FamilyInstance>();
            foreach (var element in elements)
            {
                var fi = element as FamilyInstance;
                if (element.Name == FamilyName)
                {
                    fiList.Add(fi);
                }
            }
            foreach (var fi in fiList)
            {
                LocationPoint lp = fi.Location as LocationPoint;
                var point = lp.Point;
                if (point.X == xyz.X)
                {
                    if (point.Y == xyz.Y)
                    {
                        if (point.Z == xyz.Z)
                        {
                            elementId = fi.Id;
                        }
                    }
                }
            }
            return elementId;
        }
        public static  FamilyInstance SelectElement(UIDocument uidoc)
        {
            Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            Element element = uidoc.Document.GetElement(reference);
            return (FamilyInstance)element;
        }
        public static ObservableCollection<ParameterInFamily> GetCollectionFromFamilyInstance(FamilyInstance familyInstance)
        {
            var collection = new ObservableCollection<ParameterInFamily>();
            ParameterMap parametersMap = familyInstance.ParametersMap;
            foreach (Parameter p in parametersMap)
            {
                string pname = p.Definition.Name;
                if (pname.Contains("ADSK"))
                {
                    if (p.StorageType == StorageType.Double)
                    {
                        ParameterInFamily pif = new ParameterInFamily()
                        {
                            Name = p.Definition.Name,
                            Value = p.AsValueString(),
                            Type = "Double"
                        };
                        collection.Add(pif);
                    }
                    else if (p.StorageType == StorageType.String)
                    {
                        ParameterInFamily pif = new ParameterInFamily()
                        {
                            Name = p.Definition.Name,
                            Value = p.AsString(),
                            Type = "String"
                        };
                        collection.Add(pif);
                    }
                    else if (p.StorageType == StorageType.ElementId)
                    {
                        ParameterInFamily pif = new ParameterInFamily()
                        {
                            Name = p.Definition.Name,
                            Value = p.AsValueString(),
                            Type = "ElementId"
                        };
                        collection.Add(pif);
                    }
                }
            }
            return collection;
        }
        public static List<FamilyInstance> GetMonitoringFamilyInstances(Document doc)
        {
            var fi = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().Cast<FamilyInstance>().Where(x => x.IsMonitoringLinkElement()).ToList();
            return fi;
        }

    }
}
