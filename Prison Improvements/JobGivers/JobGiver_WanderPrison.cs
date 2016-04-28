using System;
using System.Runtime.CompilerServices;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonImprovements
{

    public class JobGiver_WanderPrison : JobGiver_Wander
    {

        private static Func<Pawn, IntVec3, bool> ValidWanderSpotFor;

        public JobGiver_WanderPrison()
        {
            this.wanderRadius = 7f;
            this.ticksBetweenWandersRange = new IntRange( 125, 200 );
            this.locomotionUrgency = LocomotionUrgency.Amble;
            if( JobGiver_WanderPrison.ValidWanderSpotFor == null )
            {
                JobGiver_WanderPrison.ValidWanderSpotFor = new Func<Pawn, IntVec3, bool>((pawn, loc) =>
                {
                    Room room = loc.GetRoom();
                    if( room == null )
                    {
                        return false;
                    }
                    return room.isPrisonCell;
                } );
            }
            this.wanderDestValidator = JobGiver_WanderPrison.ValidWanderSpotFor;
        }

        protected override IntVec3 GetWanderRoot( Pawn pawn )
        {
            return pawn.Position;
        }

    }

}
