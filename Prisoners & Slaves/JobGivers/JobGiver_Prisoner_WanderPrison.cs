using System;
using System.Runtime.CompilerServices;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class JobGiver_Prisoner_WanderPrison : JobGiver_Wander
    {

        private static Func<Pawn, IntVec3, bool> ValidWanderSpotFor;

        public JobGiver_Prisoner_WanderPrison()
        {
            this.wanderRadius = 7f;
            this.ticksBetweenWandersRange = new IntRange( 125, 200 );
            this.locomotionUrgency = LocomotionUrgency.Amble;
            if( ValidWanderSpotFor == null )
            {
                ValidWanderSpotFor = new Func<Pawn, IntVec3, bool>((pawn, loc) =>
                {
                    Room room = loc.GetRoom();
                    if( room == null )
                    {
                        return false;
                    }
                    return room.isPrisonCell;
                } );
            }
            this.wanderDestValidator = ValidWanderSpotFor;
        }

        protected override IntVec3 GetWanderRoot( Pawn pawn )
        {
            return pawn.Position;
        }

    }

}
