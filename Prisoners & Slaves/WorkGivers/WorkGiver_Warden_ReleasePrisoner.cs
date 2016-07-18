using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace PrisonersAndSlaves
{
    
    public class WorkGiver_Warden_ReleasePrisoner : WorkGiver_Scanner
    {
        
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup( ThingRequestGroup.Pawn );
            }
        }

        public override Job JobOnThing( Pawn pawn, Thing t )
        {
            var otherPawn = t as Pawn;
            if( otherPawn == null )
            {   // Thing isn't a pawn
                return null;
            }

            var compPrisoner = otherPawn.TryGetComp<CompPrisoner>();
            if( compPrisoner == null )
            {   // Pawn is missing comp
                Log.ErrorOnce( string.Format( "{0} is missing CompPrisoner!", otherPawn.LabelShort ), ( 0x0BAD0000 | ( otherPawn.GetHashCode() & 0x0000FFFF ) ) );
                return null;
            }

            if( !ShouldTakeCareOfPrisoner( pawn, otherPawn, compPrisoner ) )
            {   // Not a prisoner or not ready to be released
                return (Job) null;
            }

            IntVec3 releaseCell = IntVec3.Invalid;
            if(
                ( !otherPawn.IsColonist )&&
                ( !RCellFinder.TryFindPrisonerReleaseCell( otherPawn, pawn, out releaseCell ) )
            )
            {   // Can't find a release spot for non-colonists
                return null;
            }

            Log.Message( string.Format( "{0} wants to release {1}", pawn.NameStringShort, otherPawn.NameStringShort ) );

            List<Building_Door> doors = null;

            if(
                ( compPrisoner.lawBroken.takeHomeByDefault )&&
                ( otherPawn.IsColonist )
            )
            {   // Take the offender home (only if they are a colonist)
                Log.Message( string.Format( "{0} wants to release {1} from home", pawn.NameStringShort, otherPawn.NameStringShort ) );
                var bedFor = otherPawn.ownership.OwnedBed;
                doors = bedFor.GetRoom().Portals();
            }
            var releaseJob = new Job( JobDefOf.ReleasePrisoner, otherPawn, null, releaseCell );
            if( !doors.NullOrEmpty() )
            {
                releaseJob.targetQueueB = new List<TargetInfo>();
                releaseJob.numToBringList = new List<int>();
                foreach( var door in doors )
                {
                    if( door is Building_RestrictedDoor )
                    {
                        var compLock = door.TryGetComp<CompLockable>();
                        if( compLock != null )
                        {
                            releaseJob.targetQueueB.Add( door );
                            releaseJob.numToBringList.Add( 1 );
                        }
                    }
                }
            }
            releaseJob.maxNumToCarry = 1;
            return releaseJob;
        }

        protected bool ShouldTakeCareOfPrisoner( Pawn warden, Pawn prisoner, CompPrisoner comp )
        {
            if( prisoner == null )
            {
                return false;
            }
            if(
                ( prisoner.InAggroMentalState )||
                ( warden.IsForbidden( prisoner ) )||
                ( !warden.CanReserveAndReach( prisoner, PathEndMode.OnCell, warden.NormalMaxDanger(), 1 ) )||
                ( prisoner.Downed )||
                ( !prisoner.Awake() )
            )
            {
                return false;
            }
            if(
                ( prisoner.IsPrisonerOfColony )&&
                ( prisoner.guest.PrisonerIsSecure )&&
                ( prisoner.holder == null )
            )
            {
                return( prisoner.guest.interactionMode == PrisonerInteractionMode.Release );
            }
            return(
                ( comp.wasArrested )&&
                ( Find.TickManager.TicksGame > comp.releaseAfterTick )
            );
        }
    }

}
