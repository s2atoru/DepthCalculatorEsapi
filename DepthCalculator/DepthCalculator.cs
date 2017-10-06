using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using Juntendo.MedPhys.CoordinateTransform;

namespace Juntendo.MedPhys.Esapi.DepthCalculator
{
    public class DepthCalculator
    {
        public static double DepthInBody(VVector targetPoint, Beam beam, PlanSetup planSetup)
        {

            // Get Body structure from StructureSet
            var query = from s in planSetup.StructureSet.Structures where s.Id == "BODY" select s;
            if (query.Count() != 1) throw new InvalidOperationException("No BODY in StructureSet");
            var body = query.First();

            ControlPoint controlPoint0 = beam.ControlPoints.First();
            double gantryAngle = controlPoint0.GantryAngle;
            double collimatorAngle = controlPoint0.CollimatorAngle;
            double couchAngle = controlPoint0.PatientSupportAngle;
            double[] isocenter = { beam.IsocenterPosition.x, beam.IsocenterPosition.y, beam.IsocenterPosition.z };
            BeamGeometry beamGeometry = new BeamGeometry(gantryAngle, collimatorAngle, couchAngle, isocenter);
            var sourceCoordinate = beamGeometry.SourcePosition;
            var start = new VVector(sourceCoordinate[0], sourceCoordinate[1], sourceCoordinate[2]);

            //cm to mm
            VVector targetPointInMm = 10.0 * targetPoint;
            // Planning coordinate to DICOM coordinate
            var stop = planSetup.StructureSet.Image.UserToDicom(targetPointInMm, planSetup);

            double length = (start - stop).Length;
            int numberOfSegment = 1001;

            BitArray segmentProfile = new BitArray(numberOfSegment);

            body.GetSegmentProfile(start, stop, segmentProfile);

            int sum = 0;
            foreach (bool p in segmentProfile)
            {
                if (p == true)
                {
                    sum += 1;
                }
            }

            // divide by 10 to convert mm to cm
            return ((double)sum) / ((double)(numberOfSegment - 1)) * length / 10.0;

        }

        public static double DepthInBodyDCSInMm(VVector targetPointDCSInMm, Beam beam, PlanSetup planSetup)
        {
            VVector targetPointUCSInMm = planSetup.StructureSet.Image.DicomToUser(targetPointDCSInMm, planSetup);

            return DepthInBody(targetPointUCSInMm / 10.0, beam, planSetup);
        }
    }
}
