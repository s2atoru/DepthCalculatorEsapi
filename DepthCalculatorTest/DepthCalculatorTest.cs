using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using Juntendo.MedPhys.Esapi.DepthCalculator;

namespace Juntendo.MedPhys.DepthCalculatorTest
{
    class DepthCalculatorTest
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                using (var app = VMS.TPS.Common.Model.API.Application.CreateApplication("SysAdmin", "SysAdmin"))
                {
                    Execute(app);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }
        static void Execute(VMS.TPS.Common.Model.API.Application app)
        {
            
            var patientId = "RandoPhantom";
            var courseId = "C1";
            var planId = "Plan2";

            var PatientId = patientId;

            var selectedPatient = app.OpenPatientById(patientId);

            var CourseId = courseId;
            var PlanId = planId;

            var selectedCourse = Helpers.GetCourse(selectedPatient, CourseId);
            var selectedPlanSetup = Helpers.GetPlanSetup(selectedCourse, PlanId);

            var depthCalculatorViewModel = new DepthCalculatorViewModel(selectedPlanSetup);

            var depthCalculatorView = new DepthCalculatorView(depthCalculatorViewModel);
            depthCalculatorView.ShowDialog();
        }

    }
}
