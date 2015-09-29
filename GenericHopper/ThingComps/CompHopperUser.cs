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
		public StorageSettings				ResourceSettings;

        private List<IntVec3>				cachedAdjCellsCardinal;
        private List<IntVec3>				AdjCellsCardinalInBounds
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

        public ThingFilter					Resources
        {
            get { return ((CompProperties_HopperUser)props).resources; }
        }

        public override void				PostSpawnSetup()
        {
            base.PostSpawnSetup();
			if( ResourceSettings == null )
			{
				ResourceSettings = new StorageSettings();
			}
			if( Resources != null )
			{
				Resources.ResolveReferences();
			}
            FindAndProgramHoppers();
        }

		public override void				PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.LookDeep<StorageSettings>(ref ResourceSettings, "baseSettings", null );
		}

		public override void				PostDeSpawn()
		{
			base.PostDeSpawn();

			// Scan for hoppers and deprogram each one
			var hoppers = FindHoppers();
			if (!hoppers.NullOrEmpty())
			{
				foreach( var hopper in hoppers )
				{
					hopper.DeprogramHopper();
				}
			}
		}
		
        public void							FindAndProgramHoppers()
        {
			var iHopperUser = parent as IHopperUser;
			if(
				( Resources == null )&&
				(
					( iHopperUser == null )||
					( iHopperUser.ResourceFilter == null )
				)
			)
			{
				// Does not contain xml resource filter
				// (properly) implement IHopperUser
				return;
			}

			// Set priority
			ResourceSettings.Priority = StoragePriority.Important;

			// Create a new filter
			ResourceSettings.filter = new ThingFilter();

			// Set the filter from the hopper user
			if( iHopperUser != null )
			{
				// Copy a filter from a building implementing IHopperUser
				ResourceSettings.filter.CopyFrom( iHopperUser.ResourceFilter );
			}
			else
			{
				// Copy filters from xml and resolve the references
				ResourceSettings.filter.CopyFrom( Resources );
				ResourceSettings.filter.ResolveReferences();
			}

			// Now scan for hoppers and program each one
            var hoppers = FindHoppers();
            if (!hoppers.NullOrEmpty())
            {
                foreach (var hopper in hoppers)
                {
                    hopper.ProgramHopper( ResourceSettings );
                }
            }
        }

        public List<CompHopper>				FindHoppers()
        {
            // Find hoppers for building
            var hoppers = new List<CompHopper>();
            var occupiedCells = parent.OccupiedRect();
            foreach (var cell in AdjCellsCardinalInBounds)
            {
				var hopper = FindHopper( cell );
                if (
                    ( hopper != null )&&
                    ( occupiedCells.Cells.Contains( hopper.Building.Position + hopper.Building.Rotation.FacingCell ) )
                )
                {
                    // Hopper is adjacent and rotated correctly
                    hoppers.Add(hopper);
                }
            }
            // Return list of hoppers connected to this building
            return hoppers;
        }

		public CompHopper					FindHopper( IntVec3 cell )
		{
			// Find hopper in cell
			var thingList = cell.GetThingList();
			foreach( var thing in thingList )
			{
				var thingDef = GenSpawn.BuiltDefOf( thing.def ) as ThingDef;
				if(
					( thingDef != null )&&
					( thingDef.IsHopper() )
				)
				{
					// This thing is a hopper
					return thing.TryGetComp< CompHopper >();
				}
			}
			// No hopper found
			return null;
		}

		public CompHopper					FindBestHopperForResources()
		{
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return null;
			}
			var bestHopper = (CompHopper)null;
			var bestResource = (Thing)null;
			foreach( var hopper in hoppers )
			{
				// Find best in hopper
				var hopperResources = hopper.GetAllResources( ResourceSettings.filter );
				foreach( var resource in hopperResources )
				{
					if( resource != null )
					{
						if(
							( bestHopper == null )||
							( resource.stackCount > bestResource.stackCount )
						)
						{
							// First resource or this hopper holds more
							bestHopper = hopper;
							bestResource = resource;
						}
					}
				}
			}
			// Return the best hopper
			return bestHopper;
		}

		public bool							RemoveResourceFromHoppers( ThingDef resourceDef, int resourceCount )
		{
			if( !EnoughResourceInHoppers( resourceDef, resourceCount ) )
			{
				return false;
			}
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return false;
			}

			foreach( var hopper in hoppers )
			{
				var resource = hopper.GetResource( resourceDef );
				if( resource!= null )
				{
					if( resource.stackCount >= resourceCount )
					{
						resource.SplitOff( resourceCount );
						return true;
					}
					else
					{
						resourceCount -= resource.stackCount;
						resource.SplitOff( resource.stackCount );
					}
				}
			}
			// Should always be true...
			return ( resourceCount <= 0 );
		}

		public bool							RemoveResourcesFromHoppers( int resourceCount )
		{
			if( !EnoughResourcesInHoppers( resourceCount ) )
			{
				return false;
			}
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return false;
			}

			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( ResourceSettings.filter );
				if( !resources.NullOrEmpty() )
				{
					foreach( var resource in resources )
					{
						if( resource.stackCount >= resourceCount )
						{
							resource.SplitOff( resourceCount );
							return true;
						}
						else
						{
							resourceCount -= resource.stackCount;
							resource.SplitOff( resource.stackCount );
						}
					}
				}
			}
			// Should always be true...
			return ( resourceCount <= 0 );
		}

		public bool							EnoughResourceInHoppers( ThingDef resourceDef, int resourceCount )
		{
			return ( CountResourceInHoppers( resourceDef ) >= resourceCount );
		}

		public bool							EnoughResourcesInHoppers( int resourceCount )
		{
			return ( CountResourcesInHoppers() >= resourceCount );
		}

		public int							CountResourceInHoppers( ThingDef resourceDef )
		{
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return 0;
			}

			int availableResources = 0;
			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( resourceDef );
				if( !resources.NullOrEmpty() )
				{
					foreach( var resource in resources )
					{
						availableResources += resource.stackCount;
					}
				}
			}
			return availableResources;
		}

		public int							CountResourcesInHoppers()
		{
			var hoppers = FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return 0;
			}

			int availableResources = 0;
			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( ResourceSettings.filter );
				if( !resources.NullOrEmpty() )
				{
					foreach( var resource in resources )
					{
						availableResources += resource.stackCount;
					}
				}
			}
			return availableResources;
		}

    }
}
