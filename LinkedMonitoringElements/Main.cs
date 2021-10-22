using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using System;
using Autodesk.Revit.DB.Events;
using System.Collections.ObjectModel;

namespace LinkedMonitoringElements
{
    [Transaction(TransactionMode.Manual), Regeneration(RegenerationOption.Manual)]
    class Main : IExternalApplication
    {
        public static string DllLocation { get; set; }
        public static string DllFolderLocation { get; set; }
        public static string UserFolder { get; set; }
        public static string TabName { get; set; } = "Надстройки";
        public static string PanelName { get; set; } = "Мониторинг";

        public Result OnStartup(UIControlledApplication application)
        {
            var techPanel = application.CreateRibbonPanel(PanelName);
            DllLocation = Assembly.GetExecutingAssembly().Location;
            DllFolderLocation = Path.GetDirectoryName(DllLocation);
            UserFolder = @"C:\Users\" + Environment.UserName;
            var MBtnData = new PushButtonData("MBtnData", "Передать\nзначения параметров", DllLocation, "LinkedMonitoringElements.MainCommand")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(DllLocation) + "\\res\\sockets.png", UriKind.Absolute)),
                //ToolTipImage = PngImageSource("BatchAddingParameters.res.bap-icon.png"),
                ToolTip = "Позволяет передать значения параметров по экземпляру из связного докумета"
            };
            var TechBtn = techPanel.AddItem(MBtnData) as PushButton;
            TechBtn.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(DllLocation) + "\\res\\sockets.png", UriKind.Absolute));
            //TechBtn.LargeImage = PngImageSource("BatchAddingParameters.res.bap-icon.png");

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
    public class ParameterInFamily
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public ParameterInFamily()
        {

        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class FamilyInstanceViewModel
    {
        public string NameInstance { get; set; }
        public string NameFamily { get; set; }
        public string Count { get; set; }
        public RevitLinkInstance RevitLinkInstance { get; set; }
        public FamilyInstanceViewModel()
        {

        }
    }
    public class RVTLinkViewModel
    {
        public string Name { get; set; }
        public RevitLinkInstance RevitLinkInstance { get; set; }
        public Document Document { get; set; }
        public RVTLinkViewModel()
        {

        }
    }

}
