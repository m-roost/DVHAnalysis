using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    public class DVHCache
    {
        private struct CacheKey
        {
            // Used for LQ model's DT value
            private const double InvalidDT = -1.0;

            public string PatientId;
            public string CourseId;
            public string PlanId;
            public string StructureId;
            public DoseValuePresentation DoseType;
            public VolumePresentation VolumeType;
            public double BinSize;
            public double AlphaBeta;
            public double DT;

            public CacheKey(EclipseData eclipseData, DVHModel dvhModel) : this()
            {
                PatientId = eclipseData.Patient.Id;
                CourseId = eclipseData.Plan.GetCourse().Id;
                PlanId = eclipseData.Plan.Id;
                StructureId = eclipseData.Structure.Id;
                DoseType = dvhModel.DoseType;
                VolumeType = dvhModel.VolumeType;
                BinSize = dvhModel.BinSize;

                if (dvhModel is LQBioDoseDVHModel)
                {
                    AlphaBeta = (dvhModel as LQBioDoseDVHModel).AlphaBeta;
                    DT = InvalidDT;
                }

                if (dvhModel is LQLBioDoseDVHModel)
                {
                    AlphaBeta = (dvhModel as LQLBioDoseDVHModel).AlphaBeta;
                    DT = (dvhModel as LQLBioDoseDVHModel).DT;
                }
            }
        }

        private Dictionary<CacheKey, DVH> Cache { get; set; }

        // Allow clients to use a default cache globally
        private static DVHCache _defaultCache;
        public static DVHCache DefaultCache
        {
            get
            {
                if (_defaultCache == null)
                {
                    _defaultCache = new DVHCache();
                }

                return _defaultCache;
            }
        }

        public DVHCache()
        {
            Cache = new Dictionary<CacheKey, DVH>();
        }

        public bool IsDVHCached(EclipseData eclipseData, DVHModel dvhModel)
        {
            ClearCacheIfNewPatient(eclipseData.Patient.Id);
            CacheKey cacheKey = new CacheKey(eclipseData, dvhModel);
            return Cache.ContainsKey(cacheKey);
        }

        public void CacheDVH(EclipseData eclipseData, DVHModel dvhModel, DVH dvh)
        {
            ClearCacheIfNewPatient(eclipseData.Patient.Id);
            CacheKey cacheKey = new CacheKey(eclipseData, dvhModel);
            Cache[cacheKey] = dvh;
        }

        public DVH GetCachedDVH(EclipseData eclipseData, DVHModel dvhModel)
        {
            ClearCacheIfNewPatient(eclipseData.Patient.Id);
            CacheKey cacheKey = new CacheKey(eclipseData, dvhModel);
            return Cache[cacheKey];
        }

        public void Clear()
        {
            Cache?.Clear();
        }

        private void ClearCacheIfNewPatient(string patientId)
        {
            if (Cache.Any())
            {
                var cacheKey = Cache.First().Key;
                if (cacheKey.PatientId != patientId)
                    Clear();
            }
        }
    }
}
