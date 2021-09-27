using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using System;
using Autodesk.Revit.DB.Events;

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
            var MBtnData = new PushButtonData("MBtnData", "Zzzzzz\nzzzzzz", DllLocation, "LinkedMonitoringElements.MainCommand")
            {
                ToolTipImage = new BitmapImage(new Uri(Path.GetDirectoryName(DllLocation) + "\\res\\main.png", UriKind.Absolute)),
                //ToolTipImage = PngImageSource("BatchAddingParameters.res.bap-icon.png"),
                ToolTip = "--"
            };
            var TechBtn = techPanel.AddItem(MBtnData) as PushButton;
            TechBtn.LargeImage = new BitmapImage(new Uri(Path.GetDirectoryName(DllLocation) + "\\res\\main.png", UriKind.Absolute));
            //8TechBtn.LargeImage = PngImageSource("BatchAddingParameters.res.bap-icon.png");

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
