using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

	public class WorkGiver_Warden_EnslavePrisoner : WorkGiver_Warden
	{

		public override Job JobOnThing( Pawn pawn, Thing t )
		{
			if( !this.ShouldTakeCareOfPrisoner( pawn, t ) )
			{
				return null;
			}
			var warden = pawn;
			var prisoner = t as Pawn;
            if( prisoner == null )
            {
                return null;
            }
			if(
                ( prisoner.guest.interactionMode != Data.PIM.EnslavePrisoner )||
				( !warden.CanReserveAndReach(
					prisoner,
					PathEndMode.ClosestTouch,
					warden.NormalMaxDanger(),
					1 )
				)
			)
			{
				return null;
			}
            // Is the warden carrying a slave collar already?
            var collar = warden.inventory.container.FirstOrDefault( thing => thing.IsSlaveCollar() );
            if( collar == null )
            {   // Nope, find one in the colony
                var collars = Data.AllSlaveCollarsOfColony();
    			if( collars == null )
    			{
    				return null;
    			}
    			collar = collars.Find( thing => (
                    ( !thing.IsForbidden( Faction.OfPlayer ) )&&
                    ( warden.CanReserveAndReach(
                         thing,
                         PathEndMode.ClosestTouch,
                         warden.NormalMaxDanger(),
                         1 )
                    )
                ) );
            }
			if( collar == null )
			{   // Not carrying a collar and no collars available
				return null;
			}
            var job = new Job( Data.JobDefOf.EnslavePrisoner, prisoner, collar );
			job.maxNumToCarry = 1;
			return job;
		}

	}

}
