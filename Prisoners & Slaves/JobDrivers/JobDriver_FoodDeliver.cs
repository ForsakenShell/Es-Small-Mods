using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class JobDriver_FoodDeliver : JobDriver
    {

        internal const TargetIndex FoodInd = TargetIndex.A;
        internal const TargetIndex DelivereeInd = TargetIndex.B;
        internal const TargetIndex DeliverToInd = TargetIndex.C;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var foodThing = this.TargetThing( FoodInd );
            var deliveree = (Pawn) this.TargetThing( DelivereeInd );
            var dropCell = this.TargetCell( DeliverToInd );

            yield return Toils_Reserve.Reserve( DelivereeInd, 1 );

            if( foodThing is Building )
            {
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.InteractionCell ).FailOnForbidden( FoodInd );

                if( foodThing is Building_NutrientPasteDispenser )
                {
                    yield return Toils_Ingest.TakeMealFromDispenser( FoodInd, this.pawn );
                }
                else if( foodThing is Building_AutomatedFactory )
                {
                    yield return Toils_FoodSynthesizer.TakeMealFromSynthesizer( FoodInd, this.pawn );
                }
                else // Unknown building
                {
                    throw new Exception( "Food target for JobDriver_FoodDeliver is a building but not Building_NutrientPasteDispenser or Building_AutomatedFactory!" );
                }
            }
            else if(
                ( this.pawn.inventory != null )&&
                ( this.pawn.inventory.Contains( foodThing ) )
            )
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier( this.pawn, FoodInd );
            }
            else
            {
                yield return Toils_Reserve.Reserve( FoodInd, 1 );
                yield return Toils_Goto.GotoThing( FoodInd, PathEndMode.ClosestTouch );
                yield return Toils_Ingest.PickupIngestible( FoodInd, deliveree );
            }

            var pathToTarget = new Toil();
            pathToTarget.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            pathToTarget.initAction = new Action( () =>
                {
                    var pawn = this.pawn;
                    var job = pawn.jobs.curJob;
                    pawn.pather.StartPath( job.targetC, PathEndMode.OnCell );
                }
            );
            pathToTarget.FailOnDestroyedNullOrForbidden( DelivereeInd );
            pathToTarget.AddFailCondition( () =>
            {
                if( deliveree.Downed )
                {
                    return false;
                }
                if( deliveree.IsPrisonerOfColony )
                {
                    return !deliveree.guest.ShouldBeBroughtFood;
                }
                var compPrisoner = deliveree.TryGetComp<CompPrisoner>();
                if( compPrisoner != null )
                {
                    return !compPrisoner.wasArrested;
                }
                return false;
            } );
            yield return pathToTarget;

            var dropFoodAtTarget = new Toil();
            dropFoodAtTarget.initAction = new Action( () =>
                {
                    Thing resultingThing;
                    this.pawn.carrier.TryDropCarriedThing( dropCell, ThingPlaceMode.Direct, out resultingThing );
                }
            );
            dropFoodAtTarget.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return dropFoodAtTarget;
        }

    }

}
