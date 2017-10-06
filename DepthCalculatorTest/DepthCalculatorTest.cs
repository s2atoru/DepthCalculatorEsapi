using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using Juntendo.MedPhys.Esapi.DepthCalculator;
using Juntendo.MedPhys.CoordinateTransform;

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
            string message =
                "Current user is " + app.CurrentUser.Id + "\n\n" +
                "The number of patients in the database is " +
                app.PatientSummaries.Count();
            Console.WriteLine(message);

            //Mat src = new Mat(@"D:\Varian\Transfer\lena.jpg", ImreadModes.GrayScale);   // OpenCvSharp 3.x

            //Mat dst = new Mat();

            //Cv2.Canny(src, dst, 50, 200);
            //using (new OpenCvSharp.Window("src image", src))
            //using (new OpenCvSharp.Window("dst image", dst))
            //{
            //    Cv2.WaitKey();
            //}

            var patientId = "RandoPhantom";
            var courseId = "C1";
            var planId = "Plan2";

            var PatientId = patientId;

            var selectedPatient = app.OpenPatientById(patientId);

            // Requires a research license
            //Console.WriteLine("CanModifyData: {0}", selectedPatient.CanModifyData());
            //selectedPatient.BeginModifications();

            var CourseId = courseId;
            var PlanId = planId;

            var selectedCourse = Helpers.GetCourse(selectedPatient, CourseId);
            var selectedPlanSetup = Helpers.GetPlanSetup(selectedCourse, PlanId);


            var depthCalculatorViewModel = new DepthCalculatorViewModel(selectedPlanSetup);

            var depthCalculatorView = new DepthCalculatorView(depthCalculatorViewModel);
            depthCalculatorView.ShowDialog();


            var doseEsapi = selectedPlanSetup.Dose;
            var imageEsapi = selectedPlanSetup.StructureSet.Image;

            var query = from s in selectedPlanSetup.StructureSet.Structures where s.Id == "BODY" select s;
            if (query.Count() != 1) throw new InvalidOperationException("No BODY in StructureSet");

            var body = query.First();

            var contour1 = body.GetContoursOnImagePlane(0);
            var contour2 = body.GetContoursOnImagePlane(122);

            int len1 = contour1.Count();
            int len2 = contour2.Count();

            var beam = selectedPlanSetup.Beams.ElementAt(1);
            ControlPoint controlPoint0 = beam.ControlPoints.First();
            double gantryAngle = controlPoint0.GantryAngle;
            double collimatorAngle = controlPoint0.CollimatorAngle;
            double couchAngle = controlPoint0.PatientSupportAngle;
            double[] isocenter = { beam.IsocenterPosition.x, beam.IsocenterPosition.y, beam.IsocenterPosition.z };
            BeamGeometry beamGeometry = new BeamGeometry(gantryAngle, collimatorAngle, couchAngle, isocenter);
            var sourceCoordinate = beamGeometry.SourcePosition;

            //VVector testPoint = new VVector(6.5, 1.4, -1.7);
            VVector testPoint = new VVector(9.5, -8.5, 0.0);
            VVector testPointInMm = 10.0 * testPoint;
            VVector testPointInDCS = selectedPlanSetup.StructureSet.Image.UserToDicom(testPointInMm, selectedPlanSetup);

            var start = new VVector(sourceCoordinate[0], sourceCoordinate[1], sourceCoordinate[2]);
            //var stop = new VVector(isocenter[0], isocenter[1], isocenter[2]);

            var stop = testPointInMm;
            var length = (start - stop).Length;
            BitArray segmentProfile = new BitArray(1001);
            body.GetSegmentProfile(start, stop, segmentProfile);

            int sum = 0;
            foreach (bool p in segmentProfile)
            {
                if (p == true)
                {
                    sum += 1;
                }
            }

            double depthInBody = DepthCalculator.DepthInBody(testPoint, beam, selectedPlanSetup);

            // Require a research lisence
            //var externalPlanSetup = (ExternalPlanSetup)selectedPlanSetup;
            //externalPlanSetup.CalculateDose();
            //app.SaveModifications();

            var folderPath = @"C:\Users\Admin\Desktop\IscFluence";

            // For Non-clinical Eclipse
            var computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
            if (computerName == "ECM516NC")
            {
                folderPath = @"C:\Users\Admin\Desktop\IscFluence";
            }

            Console.WriteLine($"First Name: {selectedPatient.FirstName:s}");
            Console.WriteLine($"Last Name: {selectedPatient.LastName:s}");
            Console.WriteLine($"Course ID: {CourseId:s}");
            Console.WriteLine($"Plan ID: {PlanId:s}");

            Console.ReadLine();
        }

    }
}
