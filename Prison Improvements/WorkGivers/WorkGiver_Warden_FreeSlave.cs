using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonImprovements
{

	public class WorkGiver_Warden_FreeSlave : WorkGiver_Warden
	{

		public override Job JobOnThing( Pawn pawn, Thing t )
		{
			var warden = pawn;
			var slave = t as Pawn;
			if(
				( slave == null ) ||
				( slave.guest == null ) ||
				( slave.guest.interactionMode != (PrisonerInteractionMode) Data.PIM_FreeSlave ) ||
				( slave.Downed ) ||
				( !slave.Awake() ) ||
				( !warden.CanReserveAndReach(
					slave,
					PathEndMode.ClosestTouch,
					warden.NormalMaxDanger(),
					1 )
				)
			)
			{
				return (Job) null;
			}
			var collar = slave.WornCollar();
			if( collar == null )
			{
				return (Job) null;
			}
			IntVec3 result;
			if( !RCellFinder.TryFindPrisonerReleaseCell( slave, warden, out result ) )
			{
				return (Job) null;
			}
			Job job = new Job( Data.FreeSlaveJobDef, slave, collar, result );
			job.maxNumToCarry = 1;
			return job;
		}

	}

}
