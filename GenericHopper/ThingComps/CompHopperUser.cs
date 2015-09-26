using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace esm
{
    public class CompHopperUser : ThingComp
    {
        private List<IntVec3> cachedAdjCellsCardinal;
        private List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                if (this.cachedAdjCellsCardinal == null)
                {
                    this.cachedAdjCellsCardinal = GenAdj.CellsAdjacentCardinal(this.parent).Where(c => c.InBounds()).ToList();
                }
                return this.cachedAdjCellsCardinal;
            }
        }

        public List<ThingDef> ResourceDefs
        {
            get { return ((CompProperties_HopperUser)props).resourceDefs; }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            FindAndProgramHoppers();
        }

        public void FindAndProgramHoppers()
        {
            var hoppers = FindHoppers(this.parent);
            if (!hoppers.NullOrEmpty())
            {
                foreach (var hopper in hoppers)
                {
                    hopper.ProgramHopper();
                }
            }
        }

        /// <summary>
        /// Finds hoppers connected to this Comp's parent building
        /// </summary>
        /// <returns>list of connected hoppers.</returns>
        /// <param name="building">Building</param>
        public List<CompHopper> FindHoppers(ThingWithComps building)
        {
            // Find hoppers for building
            var hoppers = new List<CompHopper>();
            var occupiedCells = building.OccupiedRect();
            foreach (var cell in AdjCellsCardinalInBounds)
            {
                var hopper = cell.FindHopper();
                if (
                    (hopper != null) &&
                    (occupiedCells.Cells.Contains(hopper.StorageBuilding.Position + hopper.StorageBuilding.Rotation.FacingCell))
                )
                {
                    // Hopper is adjacent and rotated correctly
                    hoppers.Add(hopper);
                }
            }
            // Return list of hoppers connected to this building
            return hoppers;
        }
    }
}
