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
	
	public class Designator_SmoothWall : Designator
	{

		public Designator_SmoothWall()
		{
			this.defaultLabel = SmoothWall.label;
			this.defaultDesc = SmoothWall.description;
			this.icon = SmoothWall.icon;
			this.useMouseIcon = true;
			this.soundDragSustain = SoundDefOf.DesignateDragStandard;
			this.soundDragChanged = SoundDefOf.DesignateDragStandardChanged;
			this.soundSucceeded = SoundDefOf.DesignateSmoothFloor;
			this.hotKey = KeyBindingDef.Named( "SmoothWall" );
			this.tutorHighlightTag = "SmoothFloor";
		}

		public override bool Visible
		{
			get
			{
				return SmoothWall.ResearchStoneCutting.IsFinished;
			}
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}
		
		public override AcceptanceReport CanDesignateCell( IntVec3 c )
		{
			if( !c.InBounds() )
				return "OutOfBounds".Translate();
			if( c.Fogged() )
				return "CannotPlaceInUndiscovered".Translate();
			if( Find.DesignationManager.DesignationAt( c, SmoothWall.designationDef ) != null )
				return "TerrainBeingSmoothed".Translate();
			// Must be mineable
			Thing mineable = MineUtility.MineableInCell( c );
			if( mineable == null )
			{
				return "MessageMustDesignateMineable".Translate();
			}
			// Must have associated stone blocks
			string blocksDef = "Blocks" + mineable.def.defName;
			ThingDef stoneBlocks = DefDatabase<ThingDef>.GetNamed( blocksDef, false );
			if( stoneBlocks == null )
			{
				return "MessageMustDesignateMineable".Translate();
			}
			return AcceptanceReport.WasAccepted;
		}

		public override AcceptanceReport CanDesignateThing( Thing t )
		{
			if( !SmoothWall.ResearchStoneCutting.IsFinished )
			{
				return (AcceptanceReport) false;
			}
			if( !t.def.mineable )
			{
				return (AcceptanceReport) false;
			}
			// Must have associated stone blocks
			string blocksDef = "Blocks" + t.def.defName;
			ThingDef stoneBlocks = DefDatabase<ThingDef>.GetNamed( blocksDef, false );
			if( stoneBlocks == null )
			{
				return (AcceptanceReport) false;
			}
			if( Find.DesignationManager.DesignationAt( t.Position, SmoothWall.designationDef ) != null )
			{
				return AcceptanceReport.WasRejected;
			}
			return (AcceptanceReport) true;
		}

		public override void DesignateSingleCell( IntVec3 c )
		{
			Thing mineable = MineUtility.MineableInCell( c );
			// Must be mineable
			if( mineable != null )
			{
				// Must have associated stone blocks
				string blocksDef = "Blocks" + mineable.def.defName;
				ThingDef stoneBlocks = DefDatabase<ThingDef>.GetNamed( blocksDef, false );
				if( stoneBlocks != null )
				{
					if( Find.DesignationManager.DesignationAt( c, SmoothWall.designationDef ) == null )
					{
						Find.DesignationManager.AddDesignation( new Designation( c, SmoothWall.designationDef ) );
					}
				}
			}
		}

		public override void DesignateThing( Thing t )
		{
			this.DesignateSingleCell( t.Position );
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}
	}
}
