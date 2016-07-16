using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

	public class WorkGiver_Warden_FreeSlave : WorkGiver_Warden
	{

		public override Job JobOnThing( Pawn pawn, Thing t )
		{
			var warden = pawn;
			var slave = t as Pawn;
			if(
				( slave == null )||
				( slave.guest == null )||
				( slave.guest.interactionMode != Data.PIM.FreeSlave )||
				( slave.Downed )||
				( !slave.Awake() )||
				( !warden.CanReserveAndReach(
					slave,
					PathEndMode.ClosestTouch,
					warden.NormalMaxDanger(),
					1 )
				)
			)
			{
				return null;
			}
			var collar = slave.WornCollar();
			if( collar == null )
			{   // Slave isn't wearing a collar, wut?
				return null;
			}
			IntVec3 result;
			if( !RCellFinder.TryFindPrisonerReleaseCell( slave, warden, out result ) )
			{
				return null;
			}
			var job = new Job( Data.JobDefOf.FreeSlave, slave, collar, result );
			job.maxNumToCarry = 1;
			return job;
		}

	}

}
