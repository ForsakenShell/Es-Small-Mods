using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

using Verse;
using Verse.AI;

namespace PrisonImprovements
{

	public class WorkGiver_Warden_EnslavePrisoner : WorkGiver_Warden
	{

		public override Job JobOnThing( Pawn pawn, Thing t )
		{
			if( !this.ShouldTakeCareOfPrisoner( pawn, t ) )
			{
				return (Job) null;
			}
			var warden = pawn;
			var prisoner = t as Pawn;
			if(
				( prisoner.guest.interactionMode != (PrisonerInteractionMode) Data.PIM_EnslavePrisoner ) ||
				( !warden.CanReserveAndReach(
					prisoner,
					PathEndMode.ClosestTouch,
					warden.NormalMaxDanger(),
					1 )
				)
			)
			{
				return (Job) null;
			}
            var collars = Data.AllSlaveCollarsOfColony();
			if( collars == null )
			{
				return (Job) null;
			}
			var collar = collars.Find( thing => (
                ( !thing.IsForbidden( Faction.OfColony ) )&&
                ( warden.CanReserveAndReach(
                     thing,
                     PathEndMode.ClosestTouch,
                     warden.NormalMaxDanger(),
                     1 )
                )
            ) );
			if( collar == null )
			{
				return (Job) null;
			}
			Job job = new Job( Data.EnslavePrisonerJobDef, prisoner, collar, prisoner.Position );
			job.maxNumToCarry = 1;
			return job;
		}

	}

}
