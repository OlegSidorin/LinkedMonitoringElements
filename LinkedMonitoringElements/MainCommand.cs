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
using static LinkedMonitoringElements.CM;

namespace LinkedMonitoringElements
{
    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    class MainCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            MainCommandWindow mainCommandWindow = new MainCommandWindow();
            
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
            Collection<FamilyInstanceViewModel> familyInstancesSource = new Collection<FamilyInstanceViewModel>();
            List<FamilyInstance> familyInstances = GetMonitoringFamilyInstances(doc);
            foreach (var fi in familyInstances)
            {
                var fivm = new FamilyInstanceViewModel()
                {
                    NameInstance = fi.Name,
                    NameFamily = fi.Symbol.FamilyName
                };
                bool instanceIsIn = false;
                foreach (var item in familyInstancesSource)
                {
                    string name = item.NameInstance;
                    if (name == fivm.NameInstance)
                    {
                        instanceIsIn = true;
                    }
                }
                if (!instanceIsIn)
                    familyInstancesSource.Add(fivm);
            }
            mainCommandWindow.listViewFamilyInstances.ItemsSource = familyInstancesSource;

            //ElementId LinkedElementId = GetElementId_OfMonitoredElement(linkedDocument, familyName, xyz);
            //FamilyInstance familyInstance_Link = (FamilyInstance)linkedDocument.GetElement(LinkedElementId);
            mainCommandWindow._CommandData = commandData;
            mainCommandWindow.Show();
            return Result.Succeeded;
        }
    }

}
