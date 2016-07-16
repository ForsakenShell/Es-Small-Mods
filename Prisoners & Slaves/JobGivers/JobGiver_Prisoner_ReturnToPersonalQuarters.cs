using System;
using System.Runtime.CompilerServices;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class JobGiver_Prisoner_ReturnToPersonalQuarters : ThinkNode
    {

        public override ThinkResult TryIssueJobPackage( Pawn pawn )
        {
            if(
                ( pawn == null )||
                ( pawn.ownership == null )||
                ( pawn.ownership.OwnedBed == null )||
                ( pawn.GetRoom() == pawn.ownership.OwnedBed.GetRoom() )
            )
            {
                return ThinkResult.NoJob;
            }
            return new ThinkResult(
                new Job(
                    JobDefOf.Goto,
                    (TargetInfo) pawn.ownership.OwnedBed
                ),
                this
            );
        }

    }

}
