using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ParameterScannerPlugin
{
    [Transaction(TransactionMode.Manual)]
    public class ActiveViewManager : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                String assemblyLocation = Assembly.GetExecutingAssembly().Location;
                String imgPath = System.IO.Path.GetDirectoryName(assemblyLocation);
                Uri imgUri = new Uri(imgPath + @"\icons8-parameter-32.png");

                BitmapImage bitmapImage = new BitmapImage(imgUri);

                MainWindow window = new MainWindow(uidoc);
                
                window.iconImg.Source = bitmapImage;

                window.ShowDialog();

                return Result.Succeeded;
            }

            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }

        }
    }
}