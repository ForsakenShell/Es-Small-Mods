using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class WorkGiver_Warden_UnrestrainPawn : WorkGiver_Warden
    {

        public override Job JobOnThing( Pawn pawn, Thing t )
        {
            var warden = pawn;
            var prisoner = t as Pawn;
            if( prisoner == null )
            {
                return null;
            }
            var compPrisoner = prisoner.TryGetComp<CompPrisoner>();
            if(
                ( compPrisoner == null )||
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
            // Get the restraints to remove
            Thing restraints = null;
            if(
                ( restraints == null )&&
                ( !compPrisoner.ShouldBeCuffed )
            )
            {
                restraints = prisoner.WornRestraints( Data.BodyPartGroupDefOf.Hands );
            }
            if(
                ( restraints == null )&&
                ( !compPrisoner.ShouldBeShackled )
            )
            {
                restraints = prisoner.WornRestraints( BodyPartGroupDefOf.Legs );
            }
            if( restraints == null )
            {   // Slave isn't wearing any restraints, wut?
                return null;
            }
            var job = new Job( Data.JobDefOf.UnrestainPawn, prisoner, restraints );
            job.maxNumToCarry = 1;
            return job;
        }

    }

}
