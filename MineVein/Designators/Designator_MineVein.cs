using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

using CommunityCoreLibrary;

namespace esm
{

	public class Designator_MineVein : Designator_Mine
	{

		public Designator_MineVein()
		{
			this.defaultLabel = MineVein.label;
			this.defaultDesc = MineVein.description;
			this.icon = MineVein.icon;
			this.useMouseIcon = true;
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.soundSucceeded = SoundDefOf.DesignateMine;
			this.hotKey = KeyBindingDefOf.Misc10;
			this.tutorHighlightTag = "DesignatorMine";
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return false;
			}
		}
		
		public override int DraggableDimensions
		{
			get
			{
				return 0;
			}
		}

		public override AcceptanceReport CanDesignateCell( IntVec3 c )
		{
			if( !c.InBounds() )
				return "OutOfBounds".Translate();
			if( c.Fogged() )
				return "CannotPlaceInUndiscovered".Translate();
			if( Find.DesignationManager.DesignationAt( c, DesignationDefOf.Mine ) != null )
				return "SpaceAlreadyOccupied".Translate();
			Thing mineable = MineUtility.MineableInCell( c );
			if( ( mineable == null )||( !mineable.def.building.isResourceRock ) )
				return "MessageMustDesignateMineable".Translate();
			return AcceptanceReport.WasAccepted;
		}

		public override void DesignateSingleCell( IntVec3 c )
		{
			if( Find.DesignationManager.DesignationAt( c, DesignationDefOf.Mine ) != null )
				return;
			Thing mineable = MineUtility.MineableInCell( c );
			if( ( mineable == null )||( !mineable.def.building.isResourceRock ) )
				return;
			Find.DesignationManager.AddDesignation( new Designation( c, DesignationDefOf.Mine ) );
			MineAdjacentCellsAt( c );
		}

		private void MineAdjacentCellsAt( IntVec3 at )
		{
			// Simple recursive function to designate all valid neighbouring cells
			//foreach( IntVec3 c in GenAdjFast.AdjacentCells8Way( ( TargetInfo )at ) )  //<-- Fails to report all cells???
			foreach( IntVec3 c in GenAdj.CellsAdjacent8Way( ( TargetInfo )at ) )
			{
				if( ( c.InBounds() )&&( Find.DesignationManager.DesignationAt( c, DesignationDefOf.Mine ) == null ) )
				{
					Thing mineable = MineUtility.MineableInCell( c );
					if( ( mineable != null )&&( mineable.def.building.isResourceRock ) )
					{
						Find.DesignationManager.AddDesignation( new Designation( c, DesignationDefOf.Mine) );
						MineAdjacentCellsAt( c );
					}
				}
			}
		}
	}
}
