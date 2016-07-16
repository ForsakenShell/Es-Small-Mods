using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class WorkGiver_Warden_RestrainPawn : WorkGiver_Warden
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
            // Find restraints of the appropriate type
            Thing restraint = null;
            if(
                ( restraint == null )&&
                ( compPrisoner.ShouldBeCuffed )&&
                ( prisoner.WornRestraints( Data.BodyPartGroupDefOf.Hands ) == null )
            )
            {
                restraint = FindRestraints( warden, Data.BodyPartGroupDefOf.Hands );
            }
            if(
                ( restraint == null )&&
                ( compPrisoner.ShouldBeShackled )&&
                ( prisoner.WornRestraints( BodyPartGroupDefOf.Legs ) == null )
            )
            {
                restraint = FindRestraints( warden, BodyPartGroupDefOf.Legs );
            }
            if( restraint == null )
            {   // Not carrying a restraint and no restraints available
                return null;
            }
            var job = new Job( Data.JobDefOf.RestainPawn, prisoner, restraint );
            job.maxNumToCarry = 1;
            return job;
        }

        private Thing FindRestraints( Pawn warden, BodyPartGroupDef bodyPartGroupDef )
        {
            // Is the warden carrying a restraint already?
            var restraint = warden.inventory.container.FirstOrDefault( thing => (
                ( thing.IsRestraints() )&&
                ( thing.def.apparel.bodyPartGroups.Contains( bodyPartGroupDef ) )
            ) );
            if( restraint == null )
            {   // Nope, find one in the colony
                var restraints = Data.AllRestraintsOfColony( bodyPartGroupDef );
                if( restraints == null )
                {
                    return null;
                }
                restraint = restraints.Find( thing => (
                    ( !thing.IsForbidden( Faction.OfPlayer ) )&&
                    ( warden.CanReserveAndReach(
                        thing,
                        PathEndMode.ClosestTouch,
                        warden.NormalMaxDanger(),
                        1 )
                    )
                ) );
            }
            return restraint;
        }

    }

}
