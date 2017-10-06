using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using Juntendo.MedPhys.Esapi.DepthCalculator;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            // TODO : Add here your code that is called when the script is launched from Eclipse

            Patient currentPatient = context.Patient;
            PlanSetup currentPlanSetup = context.PlanSetup;
            Course currentCourse = context.Course;

            var depthCalculatorViewModel = new DepthCalculatorViewModel(currentPlanSetup);
            var depthCalculatorView = new DepthCalculatorView(depthCalculatorViewModel);
            depthCalculatorView.ShowDialog();

        }
    }
}
