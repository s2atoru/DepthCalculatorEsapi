using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using System.ComponentModel;

namespace Juntendo.MedPhys.Esapi.DepthCalculator
{

    public class PointDepth
    {
        public string BeamId { get; set; }
        public double DepthValue { get; set; } = 0.0;
        public double EffectiveDepthValue { get; set; } = 0.0;
        public double DoseValue { get; set; } = 0.0;
    }

    public class DepthCalculatorViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// x coordinate of a calculation point
        /// </summary>
        private double xCoordinate = 0.0;
        public double XCoordinate
        {
            set
            {
                if (xCoordinate != value)
                {
                    xCoordinate = value;
                    OnPropertyChanged("XCoordinate");
                }
            }
            get
            {
                return xCoordinate;
            }
        }
        /// <summary>
        /// y coordinate of a calculation point
        /// </summary>
        private double yCoordinate = 0.0;
        public double YCoordinate
        {
            set
            {
                if (yCoordinate != value)
                {
                    yCoordinate = value;
                    OnPropertyChanged("YCoordinate");
                }
            }
            get
            {
                return yCoordinate;
            }
        }
        /// <summary>
        /// z coordinate of a calculation point
        /// </summary>
        private double zCoordinate = 0.0;
        public double ZCoordinate
        {
            set
            {
                if (zCoordinate != value)
                {
                    zCoordinate = value;
                    OnPropertyChanged("ZCoordinate");
                }
            }
            get
            {
                return zCoordinate;
            }
        }
        /// <summary>
        /// PlanSetup in Eclipse
        /// </summary>
        private PlanSetup planSetup;

        public ObservableCollection<string> ReferencePointNames { get; set; } = new ObservableCollection<string>(); 

        public ObservableCollection<PointDepth> PointDepths { get; set; } = new ObservableCollection<PointDepth>();

        public string SelectedReferencePointName { get; set; }

        public DepthCalculatorViewModel(PlanSetup planSetup)
        {
            this.planSetup = planSetup;

            ReferencePointNames.Add("");

            foreach (var fieldReferencePoint in planSetup.Beams.First().FieldReferencePoints)
            {
                if (Double.IsNaN(fieldReferencePoint.RefPointLocation.x))
                {
                    continue;
                }
                ReferencePointNames.Add(fieldReferencePoint.ReferencePoint.Id);
            }

            return;

        }

        public void ObtainPointDepthAndDose()
        {
            var beams = planSetup.Beams;

            var beam0 = beams.First();
            var query0 = from f in beam0.FieldReferencePoints
                        where f.ReferencePoint.Id == SelectedReferencePointName
                        select f;
            var fieldReferencePoint0 = query0.Single();

            VVector referencePointDCSInMm = fieldReferencePoint0.RefPointLocation;
            VVector referencePointUCS = planSetup.StructureSet.Image.DicomToUser(referencePointDCSInMm, planSetup)/10.0;

            XCoordinate = referencePointUCS.x;
            YCoordinate = referencePointUCS.y;
            ZCoordinate = referencePointUCS.z;

            PointDepths.Clear();
            foreach (var beam in beams)
            {
                var query = from f in beam.FieldReferencePoints
                            where f.ReferencePoint.Id == SelectedReferencePointName
                            select f;
                var fieldReferencePoint = query.Single();

                VVector targetPointDCSInMm = new VVector(fieldReferencePoint.RefPointLocation.x,
                    fieldReferencePoint.RefPointLocation.y, fieldReferencePoint.RefPointLocation.z);
                double depth = DepthCalculator.DepthInBodyDCSInMm(targetPointDCSInMm, beam, planSetup);

                PointDepths.Add(new PointDepth { BeamId = beam.Id, DepthValue = depth,
                    EffectiveDepthValue = fieldReferencePoint.EffectiveDepth/10, DoseValue = fieldReferencePoint.FieldDose.Dose });

            }

            return;
        }

        public void CalculateDepth()
        {
            var beams = planSetup.Beams;

            VVector targetPoint = new VVector(XCoordinate, YCoordinate, ZCoordinate);

            PointDepths.Clear();
            foreach (var beam in beams)
            {
                double depth = DepthCalculator.DepthInBody(targetPoint, beam, planSetup);
                PointDepths.Add(new PointDepth { BeamId = beam.Id, DepthValue = depth });
            }

            return;

        }
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
