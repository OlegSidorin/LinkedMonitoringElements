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
            ButtonArrowLeftClick_ExternalEventHandler = new ButtonArrowLeftClick_ExternalEventHandler();
            ButtonArrowLeftClick_ExternalEvent = ExternalEvent.Create(ButtonArrowLeftClick_ExternalEventHandler);
            
        }

        private void buttonCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonApplyClick(object sender, RoutedEventArgs e)
        {
            var familyInstanceViewModel = (FamilyInstanceViewModel)listViewFamilyInstances.SelectedItem;
            
            if (familyInstanceViewModel != null)
            {
                ButtonArrowLeftClick_ExternalEventHandler._CommandData = _CommandData;
                ButtonArrowLeftClick_ExternalEventHandler._InstanceViewModel = familyInstanceViewModel; 
                ButtonArrowLeftClick_ExternalEvent.Raise();

            }
        }

        private void ListViewItem_Click(object sender, MouseButtonEventArgs e)
        {
            var lv = sender as ListView;
            var lvi = lv.SelectedItem as FamilyInstanceViewModel;
            string NameInstance = lvi.NameInstance;
            string NameFamily = lvi.NameFamily;
            //MessageBox.Show($"{NameFamily} + {NameInstance}");
            var familyInstance = GetMonitoredFamilyInstances(_CommandData.Application.ActiveUIDocument.Document, NameInstance, NameFamily).First();
            if (familyInstance != null)
            {
                var parametersSource = GetParametersCollectionFromFamilyInstance(familyInstance);
                listViewParameters.ItemsSource = parametersSource;
            }
        }
    }
    public class ButtonArrowLeftClick_ExternalEventHandler : IExternalEventHandler
    {
        public MainCommandWindow _WindowMain;
        public ExternalCommandData _CommandData;
        public FamilyInstanceViewModel _InstanceViewModel;
        public void Execute(UIApplication app)
        {
            Document doc = _CommandData.Application.ActiveUIDocument.Document;
            string nameInstance = _InstanceViewModel.NameInstance;
            string nameFamily = _InstanceViewModel.NameFamily;
            var familyInstances = GetMonitoredFamilyInstances(_CommandData.Application.ActiveUIDocument.Document, nameInstance, nameFamily);
            //MessageBox.Show($"{nameFamily} + {nameInstance}");
            var i = 0;
            foreach (FamilyInstance familyInstance in familyInstances)
            {
                RevitLinkInstance revitLinkInstance = (RevitLinkInstance)doc.GetElement(familyInstance.GetMonitoredLinkElementIds().First());
                Document linkedDoc = revitLinkInstance.GetLinkDocument();
                //MessageBox.Show(linkedDoc.Title);
                XYZ xyz = ((LocationPoint)familyInstance.Location).Point;
                FamilyInstance familyInstanceInLinkedDocument = (FamilyInstance)linkedDoc.GetElement(GetElementId_OfMonitoredElement(linkedDoc, nameInstance, nameFamily, xyz));
                //MessageBox.Show(familyInstanceInLinkedDocument.Id.ToString());
                Collection<ParameterInFamily> pCollection = GetParametersCollectionFromFamilyInstance(familyInstance);
                
                using (Transaction t = new Transaction(doc, "apply"))
                {
                    t.Start();
                    foreach (ParameterInFamily parameterInFamily in pCollection)
                    {
                        if (parameterInFamily.Type == "Double")
                        {
                            double parValue = GetParameterInLinkedFamilyInstanceAsDouble(familyInstanceInLinkedDocument, parameterInFamily.Name);
                            Parameter parameter = familyInstance.LookupParameter(parameterInFamily.Name);
                            try
                            {
                                parameter.Set(parValue);

                            }
                            catch { }
                        }
                        if (parameterInFamily.Type == "ElementId")
                        {
                            double parValue = GetParameterInLinkedFamilyInstanceAsDouble(familyInstanceInLinkedDocument, parameterInFamily.Name);
                            Parameter parameter = familyInstance.LookupParameter(parameterInFamily.Name);
                            try
                            {
                                parameter.Set(parValue);

                            }
                            catch { } 
                        }
                        if (parameterInFamily.Type == "String")
                        {
                            string parValue = GetParameterInLinkedFamilyInstanceAsString(familyInstanceInLinkedDocument, parameterInFamily.Name);
                            Parameter parameter = familyInstance.LookupParameter(parameterInFamily.Name);
                            try
                            {
                                parameter.Set(parValue);

                            }
                            catch { }
                        }
                        
                    }
                    t.Commit();
                }
                i++;
            }
            MessageBox.Show($"Значения параметров добавлены в {i} экземплярах семейств");
            return;
        }

        public string GetName()
        {
            return "External Left Event";
        }
    }
    class CM
    {
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
        public static ElementId GetElementId_OfMonitoredElement(Document linkedDoc, string InstanceName, string FamilyName, XYZ xyz)
        {
            IList<Element> elements = new FilteredElementCollector(linkedDoc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().ToElements();
            ElementId elementId = null;
            List<FamilyInstance> fiList = new List<FamilyInstance>();
            foreach (var element in elements)
            {
                var fi = element as FamilyInstance;
                if (fi.Name == InstanceName && fi.Symbol.FamilyName == FamilyName)
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
        public static Collection<ParameterInFamily> GetParametersCollectionFromFamilyInstance(FamilyInstance familyInstance)
        {
            var collection = new Collection<ParameterInFamily>();
            ParameterMap parametersMap = familyInstance.ParametersMap;
            foreach (Parameter p in parametersMap)
            {
                if (p.UserModifiable && p.IsShared && !p.IsReadOnly)
                {
                    string pname = p.Definition.Name;

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
        public static Collection<FamilyInstanceViewModel> GetCollectionForSource(List<FamilyInstance> familyInstances)
        {
            Collection<FamilyInstanceViewModel> familyInstancesSource = new Collection<FamilyInstanceViewModel>();
            foreach (var fi in familyInstances)
            {
                var fivm = new FamilyInstanceViewModel()
                {
                    NameInstance = fi.Name,
                    NameFamily = fi.Symbol.FamilyName,
                };
                bool instanceIsIn = false;
                foreach (var item in familyInstancesSource)
                {
                    string nameInstance = item.NameInstance;
                    string nameFamily = item.NameFamily;
                    if (nameInstance == fivm.NameInstance && nameFamily == fivm.NameFamily)
                    {
                        instanceIsIn = true;
                    }
                }
                if (!instanceIsIn)
                    familyInstancesSource.Add(fivm);
            }
            foreach (var fivm in familyInstancesSource)
            {
                int count = 0;
                foreach (var fi in familyInstances)
                {
                    if (fivm.NameInstance == fi.Name && fivm.NameFamily == fi.Symbol.FamilyName)
                        count += 1;
                }
                fivm.Count = count.ToString();
            }
            return familyInstancesSource;
        }
        public static List<FamilyInstance> GetMonitoredFamilyInstances(Document doc, string nameInstance, string nameFamily)
        {
            var outputList = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType().Cast<FamilyInstance>()
                .Where(x => x.Name == nameInstance && x.Symbol.FamilyName == nameFamily)
                .Where(x => x.IsMonitoringLinkElement())
                .ToList();
            return outputList;
        }

    }
}
