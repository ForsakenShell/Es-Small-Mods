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
        
        private const TargetIndex HaulableInd = TargetIndex.A;
        private const TargetIndex StoreCellInd = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden( HaulableInd );
            this.FailOnBurningImmobile( StoreCellInd );
            if( !pawn.CurJob.GetTarget( HaulableInd ).Thing.IsForbidden( pawn ) )
            {
                this.FailOnForbidden( HaulableInd );
            }
            yield return Toils_Reserve.Reserve( StoreCellInd, 1 );
            yield return Toils_Reserve.Reserve( HaulableInd, 1 );
            var toilGoto = Toils_Goto.GotoThing( HaulableInd, PathEndMode.ClosestTouch )
                                     .FailOn( () =>
            {
                Job job = pawn.jobs.curJob;
                if( job.haulMode == HaulMode.ToCellStorage )
                {
                    Thing prisoner = job.GetTarget( HaulableInd ).Thing;
                    if( !pawn.jobs.curJob.GetTarget( StoreCellInd ).Cell.IsValidStorageFor( prisoner ) )
                    {
                        return true;
                    }
                }
                return false;
            } );
            yield return toilGoto;
            yield return Toils_Haul.StartCarryThing( HaulableInd );
            var carryToCell = Toils_Haul.CarryHauledThingToCell( StoreCellInd );
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell( StoreCellInd, carryToCell, true );
            yield return Toils_Prisoner.NoLongerNeedsHauling( HaulableInd );
            yield return Toils_Reserve.Release( StoreCellInd );
            yield return Toils_Reserve.Release( HaulableInd );
        }

    }

}
