using System;
using System.Collections.Generic;
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

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Juntendo.MedPhys.Esapi.DepthCalculator
{
    /// <summary>
    /// Interaction logic for DepthCalculatorView.xaml
    /// </summary>

    public partial class DepthCalculatorView : Window
    {

        private DepthCalculatorViewModel depthCalculatorViewModel;

        public DepthCalculatorView(DepthCalculatorViewModel depthCalculatorViewModel)
        {
            InitializeComponent();
            this.DataContext = depthCalculatorViewModel;
            this.depthCalculatorViewModel = depthCalculatorViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(depthCalculatorViewModel.SelectedReferencePointName))
            {
                depthCalculatorViewModel.CalculateDepth();
                return;
            }
            else
            {
                depthCalculatorViewModel.ObtainPointDepthAndDose();
                return;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            depthCalculatorViewModel.SelectedReferencePointName = (string)this.referencePointName.SelectedValue;
            return;
        }
    }
}
