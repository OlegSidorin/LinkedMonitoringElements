using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using static LinkedMonitoringElements.CommonLibrary;
using System.Collections.Specialized;
using System.Linq;

namespace LinkedMonitoringElements
{
    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    class MainCommand : IExternalCommand
    {
        public MainCommandWindow MainCommandWindow { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            MainCommandWindow mainCommandWindow = new MainCommandWindow();
            mainCommandWindow._CommandData = commandData;
            //string familyName = "";
            //var reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            //var familyInstance_Target = (FamilyInstance)doc.GetElement(reference); 
            //XYZ xyz = new XYZ();
            //Document linkedDocument = null;
            //if (familyInstance_Target.IsMonitoringLinkElement())
            //{
            //    LocationPoint locationPoint = (LocationPoint)familyInstance_Target.Location;
            //    xyz = locationPoint.Point;
            //    familyName = familyInstance_Target.Name;
            //    IList<ElementId> linkElementsIds = familyInstance_Target.GetMonitoredLinkElementIds();
            //    foreach (var elementId in linkElementsIds)
            //    {
            //        RevitLinkInstance revitLinkInstance = (RevitLinkInstance)doc.GetElement(elementId);
            //        linkedDocument = revitLinkInstance.GetLinkDocument();
            //    }
            //}

            var rvtLinksList = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToList();
            
            Collection<RVTLinkViewModel> comboBoxSource = new Collection<RVTLinkViewModel>();
            foreach (RevitLinkInstance item in rvtLinksList)
            {
                var rvtLinkViewModel = new RVTLinkViewModel()
                {
                    Name = item.Name,
                    RevitLinkInstance = item,
                    Document = item.GetLinkDocument()
                };
                comboBoxSource.Add(rvtLinkViewModel);
            }

            mainCommandWindow.comboBoxRVTLink.ItemsSource = comboBoxSource;
            if (comboBoxSource.Count != 0)
            {
                mainCommandWindow.comboBoxRVTLink.SelectedIndex = 0;
                var rvtLinkViewModel = (RVTLinkViewModel)mainCommandWindow.comboBoxRVTLink.SelectedItem;
                mainCommandWindow.listViewFamilyInstances.ItemsSource = GetCollectionForSource(GetMonitoringFamilyInstances(doc, rvtLinkViewModel.RevitLinkInstance), rvtLinkViewModel.RevitLinkInstance);
            }
            mainCommandWindow.checkBox.IsChecked = true;
            

            //ElementId LinkedElementId = GetElementId_OfMonitoredElement(linkedDocument, familyName, xyz);
            //FamilyInstance familyInstance_Link = (FamilyInstance)linkedDocument.GetElement(LinkedElementId);
            
            MainCommandWindow = mainCommandWindow;
            
            mainCommandWindow.Show();
            return Result.Succeeded;
        }


    }

}
