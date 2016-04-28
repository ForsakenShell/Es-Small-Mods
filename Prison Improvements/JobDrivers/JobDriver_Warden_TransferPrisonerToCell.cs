using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonImprovements
{

    public class JobDriver_Warden_TransferPrisonerToCell : JobDriver_HaulToCell
    {
        
        private const TargetIndex PrisonerInd = TargetIndex.A;
        private const TargetIndex HaulToInd = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden( PrisonerInd );
            this.FailOnBurningImmobile( HaulToInd );
            if( !pawn.CurJob.GetTarget( PrisonerInd ).Thing.IsForbidden( pawn ) )
            {
                this.FailOnForbidden( PrisonerInd );
            }
            this.AddEndCondition( () =>
            {
                // Cancel out if the prisoner enters the target room on their own
                var prisonerRoom = pawn.jobs.curJob.GetTarget( PrisonerInd ).Thing.Position.GetRoom();
                var targetRoom = pawn.jobs.curJob.GetTarget( HaulToInd ).Cell.GetRoom();
                return prisonerRoom == targetRoom ? JobCondition.Succeeded : JobCondition.Ongoing;
            } );
            yield return Toils_Reserve.Reserve( HaulToInd, 1 );
            yield return Toils_Reserve.Reserve( PrisonerInd, 1 );
            var toilGoto = Toils_Goto.GotoThing( PrisonerInd, PathEndMode.ClosestTouch )
                                     .FailOn( () =>
            {
                Job job = pawn.jobs.curJob;
                if( job.haulMode == HaulMode.ToCellStorage )
                {
                    Thing prisoner = job.GetTarget( PrisonerInd ).Thing;
                    if( !pawn.jobs.curJob.GetTarget( HaulToInd ).Cell.IsValidStorageFor( prisoner ) )
                    {
                        return true;
                    }
                }
                return false;
            } );
            yield return toilGoto;
            yield return Toils_Haul.StartCarryThing( PrisonerInd );
            var carryToCell = Toils_Haul.CarryHauledThingToCell( HaulToInd );
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell( HaulToInd, carryToCell, true );
            yield return Toils_Prisoner.NoLongerNeedsHauling( PrisonerInd );
            yield return Toils_Reserve.Release( HaulToInd );
            yield return Toils_Reserve.Release( PrisonerInd );
        }

    }

}
