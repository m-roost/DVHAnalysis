using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHAnalysis
{
    public class StructurePointCloud
    {
        private const double DesiredPointDistance = 0.1;
        private const int MaximumNumberOfPoints = 90000;
        private const int MinimumNumberOfPoints = 5000;

        public StructurePointCloud(Structure structure, PlanningItem planningItem)
        {
            _structure = structure;
            _planningItem = planningItem;

            if (planningItem is PlanSum)
            {
                var planSum = (PlanSum)planningItem;

                // Get the corresponding structure from the plan sum's structure set
                _structure = planSum.StructureSet.Structures
                    .SingleOrDefault(s => s.Id == structure.Id);

                if (_structure == null)
                {
                    throw new InvalidOperationException(
                        $"The structure \"{structure.Id}\" is not in the structure set " +
                        $"of the plan sum \"{planSum.Id}\".");
                }
            }

            // Used to get the registration
            _registrations = planningItem.GetCourse().Patient.Registrations;

            int nPointsToGenerate = GetNumberOfPointsToGenerate(_structure.Volume);
            _segmentProfiles = GenerateSegmentProfiles(_structure, nPointsToGenerate);

            Count = _segmentProfiles.Sum(profile => profile.Count(p => p.Value));
        }

        // The sum of all points inside every profile
        public int Count { get; }

        public double VoxelVolume =>
            _structure.Volume / GetNumberOfPointsToGenerate(_structure.Volume);

        private int GetNumberOfPointsToGenerate(double structureVolume)
        {
            int n = (int)Math.Floor(structureVolume/GetVolume(DesiredPointDistance));
            return Truncate(n, MinimumNumberOfPoints, MaximumNumberOfPoints);
        }

        private double GetVolume(double distance)
        {
            return Math.Pow(distance, 3.0);
        }

        private int Truncate(int i, int min, int max)
        {
            return (i > max) ? max : (i < min) ? min : i;
        }

        private SegmentProfile[] GenerateSegmentProfiles(Structure structure, int n)
        {
            // Multiply by 10 because the bounding box below is in mm (not cm)
            double d = GetPointDistance(structure.Volume, n) * 10;

            var bounds = structure.MeshGeometry.Bounds;

            // Get the center of each starting voxel
            var x0 = bounds.X + d / 2.0;
            var y0 = bounds.Y + d / 2.0;
            var z0 = bounds.Z + d / 2.0;

            // Get the upper bound of each voxel dimension
            var xMax = bounds.X + bounds.SizeX;
            var yMax = bounds.Y + bounds.SizeY;
            var zMax = bounds.Z + bounds.SizeZ;

            var xSize = (int)((xMax - x0) / d);

            var segmentProfiles = new List<SegmentProfile>();

            for (double z = z0; z < zMax; z += d)
            {
                for (double y = y0; y < yMax; y += d)
                {
                    BitArray bitArray = new BitArray(xSize + 1);

                    VVector start = new VVector(x0, y, z);
                    VVector stop = new VVector(x0 + xSize * d, y, z);

                    SegmentProfile profile = structure.GetSegmentProfile(start, stop, bitArray);
                    segmentProfiles.Add(profile);
                }
            }

            return segmentProfiles.ToArray();
        }

        private double GetPointDistance(double volume, int n)
        {
            return Math.Pow(volume/n, 1.0/3);
        }

        public SegmentProfile[] GetSegmentProfilesOn(PlanSetup plan)
        {
            // The points do not need any transformations if the plan data sets are the same.
            // Addition: No transformation needed if FORs are the same
            if (_planningItem.GetStructureSet().UID == plan.StructureSet.UID ||
                _planningItem.GetStructureSet().Image.FOR == plan.StructureSet.Image.FOR)
            {
                return _segmentProfiles;
            }

            var reg = GetRegistration(_planningItem.GetStructureSet(), plan.StructureSet);

            List<SegmentProfile> list = new List<SegmentProfile>();
            foreach (var profile in _segmentProfiles)
            {
                VVector start = reg.SourceFOR == _planningItem.GetStructureSet().Image.FOR
                    ? reg.TransformPoint(profile[0].Position)
                    : reg.InverseTransformPoint(profile[0].Position);
                VVector stop = reg.SourceFOR == _planningItem.GetStructureSet().Image.FOR
                    ? reg.TransformPoint(profile[profile.Count - 1].Position)
                    : reg.InverseTransformPoint(profile[profile.Count - 1].Position);
                VVector step = (stop - start) / (profile.Count - 1);
                BitArray bitArray = new BitArray(profile.Select(p => p.Value).ToArray());
                list.Add(new SegmentProfile(start, step, bitArray));
            }
            return list.ToArray();
        }

        private Registration GetRegistration(StructureSet ss1, StructureSet ss2)
        {
            try
            {
                var ss1FOR = ss1.Image.FOR;
                var ss2FOR = ss2.Image.FOR;

                return _registrations.First(r =>
                    (r.SourceFOR == ss1FOR && r.RegisteredFOR == ss2FOR) ||
                    (r.SourceFOR == ss2FOR && r.RegisteredFOR == ss1FOR));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to obtain the registration " +
                    $"between structure sets {ss1.Id} and {ss2.Id}. " +
                    $"The registration cannot be found or " +
                    $"the image FOR of either structure set cannot be obtained.", ex);
            }
        }

        private readonly PlanningItem _planningItem;
        private readonly Structure _structure;
        private readonly IEnumerable<Registration> _registrations;
        private readonly SegmentProfile[] _segmentProfiles;
    }
}
