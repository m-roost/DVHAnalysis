using System.Xml.Serialization;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    [XmlInclude(typeof(StandardDVHModel))]
    [XmlInclude(typeof(LQBioDoseDVHModel))]
    [XmlInclude(typeof(LQLBioDoseDVHModel))]
    public abstract class DVHModel
    {
        private const double DefaultBinSize = 0.1;  // Gy

        public virtual DoseValuePresentation DoseType { get; set; }
        public virtual VolumePresentation VolumeType { get; set; }
        public virtual double BinSize { get; set; }

        private DVHCache DVHCache { get; set; }

        public DVHModel()
        {
            BinSize = DefaultBinSize;
            DVHCache = DVHCache.DefaultCache;
        }

        public virtual DVH Calculate(EclipseData eclipseData)
        {
            if (DVHCache.IsDVHCached(eclipseData, this))
            {
                return DVHCache.GetCachedDVH(eclipseData, this);
            }
            else
            {
                DVH dvh = CalculateBase(eclipseData);
                DVHCache.CacheDVH(eclipseData, this, dvh);
                return dvh;
            }
        }

        protected abstract DVH CalculateBase(EclipseData eclipseData);
    }
}
