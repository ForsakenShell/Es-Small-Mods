using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

using CommunityCoreLibrary;

namespace esm
{

	public static class HopperHelper
	{

		/// <summary>
		/// Determines if the specified thingDef is a hopper.
		/// </summary>
		/// <returns><c>true</c> if the specified thingDef is a hopper; otherwise, <c>false</c>.</returns>
		/// <param name="thingDef">ThingDef to test</param>
		public static bool				IsHopper( this ThingDef thingDef )
		{
			return
				( thingDef.thingClass == typeof( Building_Storage ) )&&
				( thingDef.HasComp( typeof( CompHopper ) ) );
		}

		/// <summary>
		/// Finds connected hopper in given cell
		/// </summary>
		/// <returns>Connected hopper in cell</returns>
		/// <param name="cell">Cell to test</param>
		public static CompHopper		FindHopper( this IntVec3 cell )
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

		//// <summary>
		/// Finds the best connected hopper for the given resources.
		/// </summary>
		/// <returns>Hopper with most resources</returns>
		/// <param name="building">Building</param>
		/// <param name="acceptableResources">Acceptable resources</param>
		public static CompHopper		FindBestHopperForResources( this Building building, List< ThingDef > acceptableResources )
		{
			var hoppers = building.FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return null;
			}
			var bestHopper = (CompHopper)null;
			var bestResource = (Thing)null;
			foreach( var hopper in hoppers )
			{
				// Find best in hopper
				var hopperResource = hopper.GetResource( acceptableResources );
				if( hopperResource != null )
				{
					if(
						( bestHopper == null )||
						( hopperResource.stackCount > bestResource.stackCount )
					)
					{
						// First resource or this hopper holds more
						bestHopper = hopper;
						bestResource = hopperResource;
					}
				}
			}
			// Return the best hopper
			return bestHopper;
		}

		/// <summary>
		/// Finds hoppers connected to this building
		/// </summary>
		/// <returns>list of connected hoppers.</returns>
		/// <param name="building">Building</param>
		public static List< CompHopper > FindHoppers( this Building building )
		{
			// Find hoppers for building
			var hoppers = new List< CompHopper >();
			var occupiedCells = building.OccupiedRect();
			var adjacentCells = GenAdj.CellsAdjacentCardinal( building );
			foreach( var cell in adjacentCells )
			{
				var hopper = cell.FindHopper();
				if(
					( hopper != null )&&
					( occupiedCells.Cells.Contains( hopper.StorageBuilding.Position + hopper.StorageBuilding.Rotation.FacingCell ) )
				)
				{
					// Hopper is adjacent and rotated correctly
					hoppers.Add( hopper );
				}
			}
			// Return list of hoppers connected to this building
			return hoppers;
		}

		/// <summary>
		/// Finds the hopper user in given cell
		/// </summary>
		/// <returns>Thing which wants a hopper in cell</returns>
		/// <param name="cell">Cell to test</param>
		public static Thing				FindHopperUser( this IntVec3 cell )
		{
			if( !cell.InBounds() )
			{
				// Out of bounds
				return null;
			}
			var thingList = cell.GetThingList();
			foreach( var thing in thingList )
			{
				var thingDef = GenSpawn.BuiltDefOf( thing.def ) as ThingDef;
				if(
					( thingDef != null )&&
					( thingDef.building != null )&&
					( thingDef.building.wantsHopperAdjacent )
				)
				{
					// This thing wants a hopper
					return thing;
				}
			}

			// Nothing found
			return null;
		}

		//// <summary>
		/// Removes a quantity of resource from hoppers.
		/// </summary>
		/// <returns><c>true</c>, if resource from hoppers was removed, <c>false</c> otherwise.</returns>
		/// <param name="building">Building.</param>
		/// <param name="resourceDef">Resource def.</param>
		/// <param name="resourceCount">Resource count.</param>
		public static bool				RemoveResourceFromHoppers( this Building building, ThingDef resourceDef, int resourceCount )
		{
			if( !building.EnoughResourceInHoppers( resourceDef, resourceCount ) )
			{
				return false;
			}
			var hoppers = building.FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return false;
			}

			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( resourceDef );
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

		/// <summary>
		/// Removes a quantity of the resources from hoppers.
		/// </summary>
		/// <returns><c>true</c>, if resources from hoppers was removed, <c>false</c> otherwise.</returns>
		/// <param name="building">Building.</param>
		/// <param name="acceptableResources">Acceptable resources.</param>
		/// <param name="resourceCount">Resource count.</param>
		public static bool				RemoveResourcesFromHoppers( this Building building, List< ThingDef > acceptableResources, int resourceCount )
		{
			if( !building.EnoughResourcesInHoppers( acceptableResources, resourceCount ) )
			{
				return false;
			}
			var hoppers = building.FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return false;
			}

			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( acceptableResources );
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

		/// <summary>
		/// Enough of the resource in hoppers.
		/// </summary>
		/// <returns><c>true</c>, if there is enough of the resource in hoppers, <c>false</c> otherwise.</returns>
		/// <param name="building">Building.</param>
		/// <param name="resourceDef">Resource def.</param>
		/// <param name="resourceCount">Resource count.</param>
		public static bool				EnoughResourceInHoppers( this Building building, ThingDef resourceDef, int resourceCount )
		{
			return ( building.CountResourceInHoppers( resourceDef ) >= resourceCount );
		}

		/// <summary>
		/// Enough of the resources in hoppers.
		/// </summary>
		/// <returns><c>true</c>, if there is enough of the resources in hoppers, <c>false</c> otherwise.</returns>
		/// <param name="building">Building.</param>
		/// <param name="acceptableResources">Acceptable resources.</param>
		/// <param name="resourceCount">Resource count.</param>
		public static bool				EnoughResourcesInHoppers( this Building building, List< ThingDef > acceptableResources, int resourceCount )
		{
			return ( building.CountResourcesInHoppers( acceptableResources ) >= resourceCount );
		}

		/// <summary>
		/// Counts the resource in hoppers.
		/// </summary>
		/// <returns>The quantity of the resource in hoppers.</returns>
		/// <param name="building">Building.</param>
		/// <param name="resourceDef">Resource def.</param>
		public static int				CountResourceInHoppers( this Building building, ThingDef resourceDef )
		{
			var hoppers = building.FindHoppers();
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

		/// <summary>
		/// Counts the resources in hoppers.
		/// </summary>
		/// <returns>The quantity of the resources in hoppers.</returns>
		/// <param name="building">Building.</param>
		/// <param name="acceptableResources">Acceptable resources.</param>
		public static int				CountResourcesInHoppers( this Building building, List< ThingDef > acceptableResources )
		{
			var hoppers = building.FindHoppers();
			if( hoppers.NullOrEmpty() )
			{
				return 0;
			}

			int availableResources = 0;
			foreach( var hopper in hoppers )
			{
				var resources = hopper.GetAllResources( acceptableResources );
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
