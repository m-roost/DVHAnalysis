using System;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis.UnitTests
{
    internal class DVHModelBuilder
    {
        private readonly string _modelType;
        private DoseValuePresentation _doseType;
        private VolumePresentation _volumeType;
        private double _alphaBeta;
        private double _dt;

        public DVHModelBuilder(string modelType)
        {
            _modelType = modelType;
        }

        public DVHModelBuilder AbsoluteDose()
        {
            _doseType = DoseValuePresentation.Absolute;
            return this;
        }

        public DVHModelBuilder RelativeDose()
        {
            _doseType = DoseValuePresentation.Relative;
            return this;
        }

        public DVHModelBuilder AbsoluteVolume()
        {
            _volumeType = VolumePresentation.AbsoluteCm3;
            return this;
        }

        public DVHModelBuilder RelativeVolume()
        {
            _volumeType = VolumePresentation.Relative;
            return this;
        }

        public DVHModelBuilder WithBioParams(double alphaBeta, double dt = 0)
        {
            _alphaBeta = alphaBeta;
            _dt = dt;
            return this;
        }

        public DVHModel Build()
        {
            DVHModel dvhModel = null;

            switch (_modelType)
            {
                case "Standard":
                    dvhModel = new StandardDVHModel();
                    break;
                case "LQ":
                    dvhModel = new LQBioDoseDVHModel {AlphaBeta = _alphaBeta};
                    break;
                case "LQL":
                    dvhModel = new LQLBioDoseDVHModel {AlphaBeta = _alphaBeta, DT = _dt};
                    break;
                default:
                    throw new InvalidOperationException("Invalid model type.");
            }

            dvhModel.DoseType = _doseType;
            dvhModel.VolumeType = _volumeType;

            return dvhModel;
        }
    }
}