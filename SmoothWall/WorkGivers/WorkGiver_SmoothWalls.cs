using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

using CommunityCoreLibrary;

namespace esm
{
	public class WorkGiver_SmoothWall : WorkGiver_Scanner
	{

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal( Pawn pawn )
		{

			List<IntVec3> possibleTargets = new List<IntVec3>();
			foreach( Designation curDesignation in Find.DesignationManager.DesignationsOfDef( SmoothWall.designationDef ) )
			{
				IntVec3 curCell  = curDesignation.target.Cell;
				if( ( curCell.InBounds() )&&( pawn.CanReserveAndReach( curDesignation.target, PathEndMode.Touch, Danger.None ) ) )
				{
					possibleTargets.Add( curCell );
				}
			}
			return possibleTargets.AsEnumerable<IntVec3>();
		}

		public override bool ShouldSkip( Pawn pawn )
		{
			return Find.DesignationManager.DesignationsOfDef( SmoothWall.designationDef ).Count<Designation>() == 0;
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
		{
            return ( pawn.Faction == Faction.OfPlayer )&&( Find.DesignationManager.DesignationAt( c, SmoothWall.designationDef ) != null )&&( pawn.CanReserveAndReach( c, PathEndMode.Touch, Danger.None ) );
		}

		public override Job JobOnCell( Pawn pawn, IntVec3 cell )
		{
			return new Job( SmoothWall.jobDef, new TargetInfo( cell ) );
		}
	}
}