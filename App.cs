using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ParameterScannerPlugin
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            String assemblyLocation = Assembly.GetExecutingAssembly().Location;

            String tabName = "Parameters";
            application.CreateRibbonTab(tabName);

            String panelName = "Paramenters Panel";
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            String buttonName = "mainButton";
            String buttonText = "Parameter Scanner";
            String className = "ParameterScannerPlugin.ActiveViewManager";
            PushButtonData buttonData = new PushButtonData(buttonName, buttonText, assemblyLocation, className);

            String imgPath = System.IO.Path.GetDirectoryName(assemblyLocation);
            Uri imgUri = new Uri(imgPath + @"\icons8-parameter-32.png");
            buttonData.LargeImage = new BitmapImage(imgUri);

            panel.AddItem(buttonData);

            buttonData.ToolTip = "Scan model elements for specific parameter values.";

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Failed;
        }
    }
}
