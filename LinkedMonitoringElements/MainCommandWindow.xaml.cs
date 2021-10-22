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
using static LinkedMonitoringElements.CommonLibrary;
using Point = Autodesk.Revit.DB.Point;
using Transform = Autodesk.Revit.DB.Transform;

namespace LinkedMonitoringElements
{
    /// <summary>
    /// Логика взаимодействия для MainCommandWindow.xaml
    /// </summary>
    public partial class MainCommandWindow : Window
    {
        public ExternalCommandData _CommandData;
        public ButtonApplyClick_ExternalEventHandler ButtonApplyClick_ExternalEventHandler;
        public ExternalEvent ButtonApplyClick_ExternalEvent;

        public MainCommandWindow()
        {
            InitializeComponent();
            ButtonApplyClick_ExternalEventHandler = new ButtonApplyClick_ExternalEventHandler();
            ButtonApplyClick_ExternalEvent = ExternalEvent.Create(ButtonApplyClick_ExternalEventHandler);
            ButtonApplyClick_ExternalEventHandler._WindowMain = this;
            ButtonApplyClick_ExternalEventHandler._CommandData = _CommandData;
            checkBox.IsChecked = true;
            comboBoxRVTLink.SelectionChanged += ComboBoxRVTLink_SelectionChanged;
        }

        private void ComboBoxRVTLink_SelectionChanged(object sender, EventArgs e)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;
            Document document = _CommandData.Application.ActiveUIDocument.Document;
            Document linkedDocument = ((RVTLinkViewModel)comboBox.SelectedItem).Document;
            //var uio = GetMonitoringFamilyInstances(document, linkedDocument);
            RevitLinkInstance revitLinkInstance = ((RVTLinkViewModel)comboBox.SelectedItem).RevitLinkInstance;
            Collection<FamilyInstanceViewModel> collection = GetCollectionForSource(GetMonitoringFamilyInstances(document, revitLinkInstance), revitLinkInstance);
            listViewFamilyInstances.ItemsSource = collection;
            ButtonApplyClick_ExternalEventHandler._InstanceViewModels = collection;
        }

        private void buttonCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonApplyClick(object sender, RoutedEventArgs e)
        {
            var familyInstanceViewModel = (FamilyInstanceViewModel)listViewFamilyInstances.SelectedItem;
            var familyInstanceViewModels = listViewFamilyInstances.ItemsSource;
            if (!checkBox.IsChecked.Value) // галки нет
            {
                //MessageBox.Show("01");
                if (familyInstanceViewModel != null)
                {
                    ButtonApplyClick_ExternalEventHandler._WindowMain = this;
                    ButtonApplyClick_ExternalEventHandler._CommandData = _CommandData;
                    ButtonApplyClick_ExternalEventHandler._InstanceViewModel = familyInstanceViewModel;
                    ButtonApplyClick_ExternalEvent.Raise();
                }

            }
            else if (checkBox.IsChecked.Value) // галка есть
            {
                //MessageBox.Show("02");
                if (familyInstanceViewModels != null)
                {
                    ButtonApplyClick_ExternalEventHandler._WindowMain = this;
                    ButtonApplyClick_ExternalEventHandler._CommandData = _CommandData;
                    ButtonApplyClick_ExternalEventHandler._InstanceViewModel = familyInstanceViewModel;
                    ButtonApplyClick_ExternalEvent.Raise();
                }
            }
            else // что-то третье из двух
            {
                //MessageBox.Show("03");
            }

        }

        private void ListViewItem_Click(object sender, MouseButtonEventArgs e)
        {
            try
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
            catch 
            {

            }
        }
    }
    public class ButtonApplyClick_ExternalEventHandler : IExternalEventHandler
    {
        public MainCommandWindow _WindowMain;
        public ExternalCommandData _CommandData;
        public FamilyInstanceViewModel _InstanceViewModel;
        public Collection<FamilyInstanceViewModel> _InstanceViewModels;
        public void Execute(UIApplication app)
        {
            Document doc = _CommandData.Application.ActiveUIDocument.Document;
            if (!_WindowMain.checkBox.IsChecked.Value) // галки нет
            {
                //MessageBox.Show("1");
                int i = 0;
                try
                {
                    i = SetParametersFromLinkedFamily(_InstanceViewModel, doc);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                }
                
                if (i == 0)
                    MessageBox.Show($"Ничего не было сделано");
                else
                    MessageBox.Show($"Значения параметров перенесено в {i} семейств");
            }
            else if (_WindowMain.checkBox.IsChecked.Value) // галка есть
            {
                //MessageBox.Show("2");
                int i = 0;
                foreach (var instanceViewModel in _InstanceViewModels)
                {
                    try
                    {
                        i += SetParametersFromLinkedFamily(instanceViewModel, doc);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.ToString());
                    }
                }
                if (i == 0)
                    MessageBox.Show($"Ничего не было сделано");
                else
                    MessageBox.Show($"Значения параметров перенесено в {i} семейств");
            }
            else // что-то третье из двух
            {
                //MessageBox.Show("3");
            }

            return;
        }

        public string GetName()
        {
            return "External Left Event";
        }
    }
    class CommonLibrary
    {
        public static string NormalizeInstanceName(string inputString)
        {
            var l = inputString.Length;
            string result = inputString;
            if (l > 2)
            {
                var last2 = inputString.Substring(l - 2);
                if (last2 == " 2")
                    result = inputString.Remove(l - 2);
            }
            return result; 
        }
        public static string NormalizeFamilyName(string inputString)
        {
            var l = inputString.Length;
            string result = inputString;
            if (l > 10)
            {
                var last2 = inputString.Substring(l - 1);
                if (last2 == "1")
                    result = inputString.Remove(l - 1);
            }
            return result;
        }
        public static XYZ Shift(RevitLinkInstance revitLinkInstance)
        {
            return revitLinkInstance.GetTotalTransform().Inverse.Origin;
        }
        public static int SetParametersFromLinkedFamily(FamilyInstanceViewModel familyInstanceViewModel, Document doc)
        {
            string nameInstance = familyInstanceViewModel.NameInstance;
            string nameFamily = familyInstanceViewModel.NameFamily;
            var familyInstances = GetMonitoredFamilyInstances(doc, nameInstance, nameFamily);
            var i = 0;
            foreach (FamilyInstance familyInstance in familyInstances)
            {
                RevitLinkInstance revitLinkInstance = (RevitLinkInstance)doc.GetElement(familyInstance.GetMonitoredLinkElementIds().First());
                Document linkedDoc = revitLinkInstance.GetLinkDocument();
                //MessageBox.Show(linkedDoc.Title);
                XYZ xyz = ((LocationPoint)familyInstance.Location).Point;
                FamilyInstance familyInstanceInLinkedDocument = (FamilyInstance)linkedDoc.GetElement(GetElementId_OfMonitoredElement(linkedDoc, nameInstance, nameFamily, xyz, Shift(revitLinkInstance)));
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
                                bool result = false;
                                if (null != parameter && !parameter.IsReadOnly)
                                {
                                    if (parameter.Definition.Name != "Марка")
                                    {
                                        StorageType parameterType = parameter.StorageType;
                                        if (StorageType.Double == parameterType)
                                        {
                                            result = parameter.Set(parValue);
                                        }
                                    }
                                    
                                }
                                
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                        }
                        if (parameterInFamily.Type == "String")
                        {
                            string parValue = GetParameterInLinkedFamilyInstanceAsString(familyInstanceInLinkedDocument, parameterInFamily.Name);
                            Parameter parameter = familyInstance.LookupParameter(parameterInFamily.Name);
                            try
                            {
                                bool result = false;
                                if (null != parameter && !parameter.IsReadOnly)
                                {
                                    if (parameter.Definition.Name != "Марка")
                                    {
                                        if (parameter.Definition.Name != "Марка")
                                        {
                                            StorageType parameterType = parameter.StorageType;
                                            if (StorageType.String == parameterType)
                                            {
                                                result = parameter.Set(parValue);
                                            }
                                        }
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString());
                            }
                        }
                    }
                    t.Commit();
                }
                i++;
            }
            return i;
        }
        public static double GetParameterInLinkedFamilyInstanceAsDouble(FamilyInstance linkedFamilyInstance, string parameterName)
        {
            double output = 0;
            Parameter parameter = linkedFamilyInstance.LookupParameter(parameterName);
            try
            {
                output = parameter.AsDouble();
            }
            catch
            { }
            return output;
        }
        public static string GetParameterInLinkedFamilyInstanceAsString(FamilyInstance linkedFamilyInstance, string parameterName)
        {
            string output = "";
            Parameter parameter = linkedFamilyInstance.LookupParameter(parameterName);
            try
            {
                output = parameter.AsString();
            }
            catch
            { }
            return output;
        }
        public static ElementId GetElementId_OfMonitoredElement(Document linkedDoc, string InstanceName, string FamilyName, XYZ xyz, XYZ shift)
        {
            IList<Element> elements = new FilteredElementCollector(linkedDoc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().ToElements();
            ElementId elementId = null;
            List<FamilyInstance> fiList = new List<FamilyInstance>();
            foreach (var element in elements)
            {
                var fi = element as FamilyInstance;
                if (fi.Name == NormalizeInstanceName(InstanceName) && fi.Symbol.FamilyName == NormalizeFamilyName(FamilyName))
                {
                    fiList.Add(fi);
                }
            }
            foreach (var fi in fiList)
            {
                LocationPoint lp = fi.Location as LocationPoint;
                var point = lp.Point;
                if (Math.Abs(point.X - (xyz.X + shift.X)) < 0.0001)
                {
                    if (Math.Abs(point.Y - (xyz.Y + shift.Y)) < 0.0001)
                    {
                        if (Math.Abs(point.Z - (xyz.Z + shift.Z)) < 0.0001)
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
                if (!p.IsReadOnly)
                {
                    string pname = p.Definition.Name;

                    if (pname != "Марка")
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
                        //else if (p.StorageType == StorageType.ElementId)
                        //{
                        //    ParameterInFamily pif = new ParameterInFamily()
                        //    {
                        //        Name = p.Definition.Name,
                        //        Value = p.AsValueString(),
                        //        Type = "ElementId"
                        //    };
                        //    collection.Add(pif);
                        //}
                    }

                }
                
            }
            return collection;
        }
        public static List<FamilyInstance> GetMonitoringFamilyInstances(Document doc)
        {
            var outputAll = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(x => x.IsMonitoringLinkElement())
                .ToList();

            return outputAll;
        }
        //public static List<FamilyInstance> GetMonitoringFamilyInstances(Document doc, Document linkDoc)
        //{
        //    var outputAll = new FilteredElementCollector(doc)
        //        .OfClass(typeof(FamilyInstance))
        //        .WhereElementIsNotElementType()
        //        .Cast<FamilyInstance>()
        //        .Where(x => x.IsMonitoringLinkElement())
        //        .ToList();

        //    var output = new List<FamilyInstance>();
        //    foreach (FamilyInstance fi in outputAll)
        //    {
        //        RevitLinkInstance rvtInstance = (RevitLinkInstance)doc.GetElement(fi.GetMonitoredLinkElementIds().First());

        //        if (rvtInstance.GetLinkDocument().Title == linkDoc.Title)
        //        {
        //            output.Add(fi);
        //        }
        //    }

        //    return output;
        //}
        public static List<FamilyInstance> GetMonitoringFamilyInstances(Document doc, RevitLinkInstance rvtLinkInstance)
        {
            var outputAll = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(x => x.IsMonitoringLinkElement())
                .ToList();

            var output = new List<FamilyInstance>();
            foreach (FamilyInstance fi in outputAll)
            {
                RevitLinkInstance rvtInstance = (RevitLinkInstance)doc.GetElement(fi.GetMonitoredLinkElementIds().First());

                if (rvtInstance.Name == rvtLinkInstance.Name)
                {
                    output.Add(fi);
                }
            }

            return output;
        }
        public static Collection<FamilyInstanceViewModel> GetCollectionForSource(List<FamilyInstance> familyInstances, RevitLinkInstance revitLinkInstance)
        {
            Collection<FamilyInstanceViewModel> familyInstancesSource = new Collection<FamilyInstanceViewModel>();
            foreach (var fi in familyInstances)
            {
                var fivm = new FamilyInstanceViewModel()
                {
                    NameInstance = fi.Name,
                    NameFamily = fi.Symbol.FamilyName,
                    RevitLinkInstance = revitLinkInstance
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
        //public static List<FamilyInstance> GetMonitoredFamilyInstances(Document doc, Document linkDoc, string nameInstance, string nameFamily)
        //{
        //    var outputList = new FilteredElementCollector(doc)
        //        .OfClass(typeof(FamilyInstance))
        //        .WhereElementIsNotElementType().Cast<FamilyInstance>()
        //        .Where(x => x.Name == nameInstance && x.Symbol.FamilyName == nameFamily)
        //        .Where(x => x.IsMonitoringLinkElement())
        //        .ToList();
        //    var output = new List<FamilyInstance>();
        //    foreach (FamilyInstance fi in outputList)
        //    {
        //        RevitLinkInstance rvtInstance = (RevitLinkInstance)doc.GetElement(fi.GetMonitoredLinkElementIds().First());
                
        //        if (rvtInstance.GetLinkDocument().Title == linkDoc.Title)
        //        {
        //            output.Add(fi);
        //        }
        //    }
        //    return output;
        //}
    }
}
