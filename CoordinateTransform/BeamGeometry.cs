using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Juntendo.MedPhys.CoordinateTransform
{
    /// <summary>
    /// Class for Beam geometry
    /// </summary>
    /// <remarks>
    /// Scales are in mm and angle in degree
    /// Linac scale: IEC 61217 (Varian 1217)
    /// Patient coordinate: DICOM default
    /// Linac scale used in Juntendo University Hospital is Varian IEC 601-2-1
    /// (The sense of Couch rotation is reversed.)
    /// </remarks>
    public class BeamGeometry
    {
        // Gantry angle in degree
        public double GantryAngle { get; set; }

        // Collimator angle in degree
        public double CollimatorAngle { get; set; }

        // Couch angle in degree
        public double CouchAngle { get; set; }

        // Isocneter coordinate in patient coordinate system in mm
        public double[] Isocenter = new double[3] { 0.0, 0.0, 0.0 };

        // Source position in the patient coordinate system
        public double[] SourcePosition = new double[3] { 0.0, 0.0, 0.0 };

        /// <summary>
        /// Constructor for BeamGeometry
        /// </summary>
        /// <param name="gantryAngle"> Gantry angle in degree </param>
        /// <param name="collimatorAngle"> Collimator angle in degree </param>
        /// <param name="couchAngle"> Couch angle in radian </param>
        /// <param name="isocenter"> Isocenter coordinate in the patient coordinate system in mm </param>
        public BeamGeometry(double gantryAngle, double collimatorAngle,
            double couchAngle, double[] isocenter)
        {
            this.GantryAngle = gantryAngle;
            this.CollimatorAngle = collimatorAngle;
            this.CouchAngle = couchAngle;
            for (int i = 0; i < 3; i++)
            {
                this.Isocenter[i] = isocenter[i];
            }

            CoordinateTransform3D.SourceCoordinateInPatientCoordinate(
                SourcePosition, isocenter, gantryAngle, collimatorAngle, couchAngle, 1000);

        }

        /// <summary>
        /// Transform Patient coordinate to Unit Coordinate
        /// </summary>
        /// <param name="patientCoordinate"> Patient coordinate to transform </param>
        /// <param name="unitCoordinate"> Transfomred Unit coordinate </param>
        public void PCStoUCS(double[] patientCoordinate, double[] unitCoordinate)
        {
            CoordinateTransform3D.PatientToUnitCoordinate(Isocenter,
            GantryAngle, CollimatorAngle, CouchAngle,
            patientCoordinate, unitCoordinate);

            return;
        }

        public double[] ProjectedPointAtIsocenterPlaneInUCS(double[] pointPCS, double SAD = 1000)
        {
            double[] sourceUCS = new double[3] { 0.0, -SAD, 0.0 };
            double[] pointUCS = new double[3] { 0.0, 0.0, 0.0 };
            CoordinateTransform3D.PatientToUnitCoordinate(Isocenter,
            GantryAngle, CollimatorAngle, CouchAngle,
            pointPCS, pointUCS);

            double[] sourceToPointUCS = new double[3] { pointUCS[0] - sourceUCS[0], pointUCS[1] - sourceUCS[1], pointUCS[2] - sourceUCS[2] };

            if (sourceToPointUCS[1] < Double.Epsilon)
            {
                throw new InvalidOperationException("pointPCS is on or above the source plane");    
            }

            double sf = SAD / Math.Abs(sourceToPointUCS[1]);
            double[] sourceToPointAtIcpUCS = new double[3] { sf * sourceToPointUCS[0], sf * sourceToPointUCS[1], sf * sourceToPointUCS[2] };

            double[] projectedPointAtIcpUCS = new double[3] {
                sourceUCS[0]+sourceToPointAtIcpUCS[0],
                sourceUCS[1]+sourceToPointAtIcpUCS[1],
                sourceUCS[2]+sourceToPointAtIcpUCS[2] };

            return projectedPointAtIcpUCS;
        }
    }
}
