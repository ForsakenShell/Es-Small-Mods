using System;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public static class Toils_General
    {

        public static Toil PrepareDoorInteraction( TargetIndex DoorInd, Toil interactionToil )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = () =>
            {
                var door = toil.actor.CurJob.GetTarget( DoorInd ).Thing as Building_RestrictedDoor;
                var compLock = door.TryGetComp<CompLockable>();
                interactionToil.defaultDuration = compLock.LockToggleTime( toil.actor );
            };
            return toil;
        }

        public static Toil DoorLockToggle( TargetIndex DoorInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.WithProgressBarToilDelay( DoorInd );
            toil.AddFinishAction( () =>
            {
                var door = toil.actor.CurJob.GetTarget( DoorInd ).Thing as Building_RestrictedDoor;
                var compLock = door.TryGetComp<CompLockable>();
                compLock.ChangeLockState( compLock.setLockState );
            } );
            return toil;
        }

        public static Toil DoorLock( TargetIndex DoorInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.WithProgressBarToilDelay( DoorInd );
            toil.AddFinishAction( () =>
            {
                var door = toil.actor.CurJob.GetTarget( DoorInd ).Thing as Building_RestrictedDoor;
                var compLock = door.TryGetComp<CompLockable>();
                compLock.ChangeLockState( true );
            } );
            return toil;
        }

        public static Toil DoorUnLock( TargetIndex DoorInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.WithProgressBarToilDelay( DoorInd );
            toil.AddFinishAction( () =>
            {
                var door = toil.actor.CurJob.GetTarget( DoorInd ).Thing as Building_RestrictedDoor;
                var compLock = door.TryGetComp<CompLockable>();
                compLock.ChangeLockState( false );
            } );
            return toil;
        }

        public static Toil DropCarriedPawnRightHere()
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.AddFinishAction( () =>
            {
                var pawn = toil.actor;
                var carriedPawn = pawn.carrier.CarriedThing as Pawn;
                if( carriedPawn == null )
                {
                    throw new Exception( string.Format( "{0} tried to drop pawn but carried thing isn't a pawn!", pawn.NameStringShort ) );
                }
                var cell = pawn.Position;
                Thing resultingThing;
                if( !pawn.carrier.TryDropCarriedThing( cell, ThingPlaceMode.Direct, out resultingThing ) )
                {
                    if( !pawn.carrier.TryDropCarriedThing( cell, ThingPlaceMode.Near, out resultingThing ) )
                    {
                        Log.Error( string.Format( "{0} tried to drop {1} but no free cells for placement near {2}", pawn.NameStringShort, carriedPawn.NameStringShort, cell.ToString() ) );
                    }
                }
            } );
            return toil;
        }

        public static Toil UnclaimBed( TargetIndex PawnInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = () =>
            {
                var pawn = toil.actor.CurJob.GetTarget( PawnInd ).Thing as Pawn;
                if( pawn.ownership.OwnedBed != null )
                {
                    pawn.ownership.UnclaimBed();
                }
            };
            return toil;
        }

        public static Toil TuckPawnIntoBed( TargetIndex PawnInd, TargetIndex BedInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = () =>
            {
                var pawn = toil.actor.CurJob.GetTarget( PawnInd ).Thing as Pawn;
                var bed = toil.actor.CurJob.GetTarget( BedInd ).Thing as Building_Bed;
                Thing resultingThing;
                toil.actor.carrier.TryDropCarriedThing( bed.Position, ThingPlaceMode.Direct, out resultingThing, null );
                if(
                    ( !bed.Destroyed )&&
                    (
                        ( bed.owners.Contains( pawn ) )||
                        ( bed.Medical )&&
                        ( bed.AnyUnoccupiedSleepingSlot )||
                        ( pawn.ownership == null )
                    )
                )
                {
                    pawn.jobs.Notify_TuckedIntoBed( bed );
                    if(
                        ( pawn.RaceProps.Humanlike )&&
                        ( !pawn.IsPrisonerOfColony )
                    )
                    {
                        pawn.relations.Notify_RescuedBy( toil.actor );
                    }
                }
                if( pawn.IsPrisonerOfColony )
                {
                    ConceptDecider.TeachOpportunity( ConceptDefOf.PrisonerTab, pawn, OpportunityType.GoodToKnow );
                }
            };
            return toil;
        }


    }

}
