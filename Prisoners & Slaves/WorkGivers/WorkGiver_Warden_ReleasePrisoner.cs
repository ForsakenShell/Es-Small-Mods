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
    
    public class WorkGiver_Warden_ReleasePrisoner : WorkGiver_Warden
    {
        
        public override Job JobOnThing( Pawn pawn, Thing t )
        {
            //Log.Message( string.Format( "WorkGiver_Warden_ReleasePrisoner( {0}, {1} )", pawn.LabelShort, t.ThingID ) );
            if( !ShouldTakeCareOfPrisoner( pawn, t ) )
            {   // Not a prisoner
                return null;
            }

            var prisoner = t as Pawn;
            var compPrisoner = prisoner.TryGetComp<CompPrisoner>();
            if( compPrisoner == null )
            {   // Pawn is missing comp
                //Log.Message( string.Format( "\t{0} is missing CompPrisoner!", prisoner.LabelShort ) );
                return null;
            }

            if(
                (
                    ( prisoner.IsPrisonerOfColony )&&
                    ( prisoner.guest.interactionMode != PrisonerInteractionMode.Release )
                )||
                (
                    ( compPrisoner.releaseAfterTick > 0 )&&
                    ( Find.TickManager.TicksGame < compPrisoner.releaseAfterTick )
                )
            )
            {
                //Log.Message( string.Format( "\t{0} - IsPrisonerOfColony && interactionMode != Release OR TicksGame < releaseAfterTick", prisoner.LabelShort ) );
                return null;
            }

            IntVec3 releaseCell = IntVec3.Invalid;
            if(
                ( !prisoner.IsColonist )&&
                ( !RCellFinder.TryFindPrisonerReleaseCell( prisoner, pawn, out releaseCell ) )
            )
            {   // Can't find a release spot for non-colonists
                //Log.Message( string.Format( "\t{0} - !IsColonst && !TryFindPrisonerReleaseCell", prisoner.LabelShort ) );
                return null;
            }

            //Log.Message( string.Format( "{0} wants to release {1}", pawn.NameStringShort, prisoner.NameStringShort ) );

            List<Building_Door> doors = null;

            if(
                ( compPrisoner.lawBroken.takeHomeByDefault )&&
                ( prisoner.IsColonist )
            )
            {   // Take the offender home (only if they are a colonist)
                //Log.Message( string.Format( "{0} wants to release {1} from home", pawn.NameStringShort, prisoner.NameStringShort ) );
                var bedFor = prisoner.ownership.OwnedBed;
                doors = bedFor.GetRoom().Portals();
            }
            var releaseJob = new Job( JobDefOf.ReleasePrisoner, prisoner, null, releaseCell );
            if( !doors.NullOrEmpty() )
            {
                Log.Message( "Queueing doors to unlock" );
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

    }

}
