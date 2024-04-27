using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ParameterScannerPlugin
{
    public partial class MainWindow : Window
    {
        public UIDocument uiDoc { get; }
        public Autodesk.Revit.DB.Document doc { get; }
        public MainWindow(UIDocument UiDoc)
        {
            uiDoc = UiDoc;
            doc = UiDoc.Document;
            InitializeComponent();
        }

        private void IsolateParameterInView(object sender, RoutedEventArgs e)
        {
            IList<ElementId> elementIds = GetElementIdsByParameterNameAndValue();
            using (Transaction trans = new Transaction(doc, "Isolate Elements"))
            {
                trans.Start();
                uiDoc.ActiveView.IsolateElementsTemporary(elementIds);
                trans.Commit();
            }
        }

        private void SelectParameter(object sender, RoutedEventArgs e)
        {
            IList<ElementId> elementIds = GetElementIdsByParameterNameAndValue();
            uiDoc.Selection.SetElementIds(elementIds);
        }

        private void EnableOrDisableButtons(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Boolean enableButtons = parameterNameField.Text != "";
            IsolateInView.IsEnabled = enableButtons;
            Select.IsEnabled = enableButtons;        
        }

        private IList<ElementId> GetElementIdsByParameterNameAndValue()
        {
            IList<ElementId> elementIds = new List<ElementId>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            IList<Element> allElements = collector.WhereElementIsNotElementType().ToElements();

            Int32 parameterFound = 0;

            foreach (Element element in allElements)
            {
                Parameter parameter = element.LookupParameter(parameterNameField.Text);

                if (parameter != null)
                {
                    parameterFound++;

                    Boolean valueMatches = false;

                    StorageType storageType = parameter.StorageType;

                    Boolean parameterValueFieldIsNumeric = double.TryParse(parameterValueField.Text, out _);

                    if(!parameter.HasValue && parameterValueField.Text == "")
                    {
                        valueMatches = true;
                    }
                    else if (storageType == StorageType.String && !parameterValueFieldIsNumeric)
                    {
                        valueMatches = StringParameterMatchesFilter(parameter);
                    }   
                    else if (storageType == StorageType.Integer && parameterValueFieldIsNumeric && ParameterValueIsInteger())
                    {
                        valueMatches = IntegerParameterMatchesFilter(parameter);
                    }
                    else if (storageType == StorageType.Double && parameterValueFieldIsNumeric)
                    {
                        valueMatches = DoubleParamaterMatchesFilter(parameter);
                    }

                    if (valueMatches)
                    {
                        elementIds.Add(element.Id);
                    }
                }
            }

            Int32 elementsCount = elementIds.Count;

            if(parameterFound == 0)
            {
                TaskDialog.Show("Warning", "Parameter name not found!");
            }
            else
            {
                TaskDialog.Show("Warning", $"{elementsCount} elements found!");
            }
            if (elementsCount == 0) { Focus(); } else { Close(); };
            return elementIds;
        }

        private Boolean StringParameterMatchesFilter(Parameter parameter)
        {            
            return parameterValueField.Text == parameter.AsString();            
        }

        private Boolean IntegerParameterMatchesFilter(Parameter parameter)
        {
            Int32 parameterValue = parameter.AsInteger();

            Int32 integerParameterValueField = Convert.ToInt32(parameterValueField.Text);

            return integerParameterValueField == parameterValue;
        }

        private Boolean DoubleParamaterMatchesFilter(Parameter parameter)
        {
            ForgeTypeId displayUnitTypeId = parameter.GetUnitTypeId();

            Double parameterValue = parameter.AsDouble();

            Double convertedParameterValue = UnitUtils.ConvertFromInternalUnits(parameterValue, displayUnitTypeId);

            Int32 decimalParameterValueField = GetParameterValueFieldDecimalNumbers();

            Int32 parameterValueFieldPrecision = decimalParameterValueField.ToString().Length;

            Double roundedParameterValue = Math.Round(convertedParameterValue, parameterValueFieldPrecision);

            return roundedParameterValue == DoubleParamaterValueField();
        } 
        
        private Boolean ParameterValueIsInteger()
        {
            return DoubleParamaterValueField() % 1 == 0;
        }

        private Double DoubleParamaterValueField()
        {
            return Double.Parse(parameterValueField.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private Int32 GetParameterValueFieldDecimalNumbers()
        {
            if (ParameterValueIsInteger())
            {
                return 0;
            }
            else
            {
                return new System.Version(parameterValueField.Text).Minor;
            }
        }
    }
}
