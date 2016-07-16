using System;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public static class Toils_General
    {

        public static Toil DoorLockToggle( TargetIndex DoorInd, Pawn pawn, CompLockable compLock )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = compLock.LockToggleTime( pawn );
            toil.WithProgressBarToilDelay( DoorInd );
            toil.AddFinishAction( () =>
            {
                compLock.ChangeLockState( compLock.setLockState );
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

    }

}
