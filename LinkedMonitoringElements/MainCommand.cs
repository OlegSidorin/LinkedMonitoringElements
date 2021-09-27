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
            // Get the element selection of current document.
            //Autodesk.Revit.UI.Selection.Selection selection = uidoc.Selection;
            ICollection<Autodesk.Revit.DB.ElementId> selectedIds = new Collection<ElementId>();
            IList<Reference> references = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
            foreach (var reference in references)
            {
                selectedIds.Add(reference.ElementId);
            };

            //ICollection<Autodesk.Revit.DB.ElementId> selectedIds = uidoc.Selection.GetElementIds();
            string str = "";
            XYZ xyz = new XYZ();
            string familyName = "";
            Document linkedDocument = null;
            if (selectedIds.Count > 0)
            {
                foreach (var eid in selectedIds)
                {
                    if (doc.GetElement(eid).IsMonitoringLinkElement())
                    {
                        var elem = doc.GetElement(eid) as FamilyInstance;
                        LocationPoint lp = elem.Location as LocationPoint;
                        xyz = lp.Point;
                        familyName = elem.Name;
                        IList<ElementId> eIds = elem.GetMonitoredLinkElementIds();
                        foreach (var e in eIds)
                        {
                            RevitLinkInstance revitLinkInstance = doc.GetElement(e) as RevitLinkInstance;
                            linkedDocument = revitLinkInstance.GetLinkDocument();
                        }
                        str += familyName + "\n" + linkedDocument.Title + "\n" + xyz.ToString();
                    }  
                }
            }

            //DocumentSet docSet = commandData.Application.Application.Documents;
            //foreach(Document d in docSet)
            //{
            //    str += "\n" + d.Title;
            //}

            //IList<Element> elementsRVTLinks = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements();


            //var ff = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements();

            //foreach(var t in ff)
            //{
            //    str += t.Name;
            //}
            str += "\n------";
            str += GetElementId_OfMonitoredElement(linkedDocument, familyName, xyz);
            mainCommandWindow.textBlock_FamilyName.Text = str;
            mainCommandWindow.Show();
            //MessageBox.Show(str);
            return Result.Succeeded;
        }
        ElementId GetElementId_OfMonitoredElement(Document linkedDoc, string FamilyName, XYZ xyz)
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
    }
}
