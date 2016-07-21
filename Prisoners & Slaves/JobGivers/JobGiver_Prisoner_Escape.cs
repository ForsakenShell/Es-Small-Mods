using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class JobGiver_Prisoner_Escape : JobGiver_PrisonerEscape
    {

        private const int MaxRegionsToCheckWhenEscapingThroughOpenDoors = 25;

        protected override Job TryGiveJob( Pawn pawn )
        {
            IntVec3 spot;
            if(
                ( !this.ShouldStartEscaping( pawn ) )||
                ( !RCellFinder.TryFindBestExitSpot( pawn, out spot, TraverseMode.ByPawn ) )
            )
            {
                return null;
            }
            if( !pawn.guest.released )
            {
                Messages.Message(
                    Data.Strings.MessagePrisonerEscaping.Translate( pawn.NameStringShort ),
                    (TargetInfo) pawn,
                    MessageSound.SeriousAlert
                );
            }
            var job = new Job( JobDefOf.Goto, spot );
            job.exitMapOnArrival = true;
            return job;
        }

        private bool ShouldStartEscaping( Pawn pawn )
        {
            if(
                ( !pawn.guest.IsPrisoner )||
                ( pawn.guest.HostFaction != Faction.OfPlayer )||
                ( pawn.guest.PrisonerIsSecure )
            )
            {
                return false;
            }
            // Compare pawn mood to visibility to colonists and cameras
            var breakOdds = pawn.EscapeProbability();
            if( pawn.needs.mood.CurLevel > breakOdds )
            {   // Pawn is happy enough not to chance it
                return false;
            }
            var room = pawn.GetRoom();
            if( room.TouchesMapEdge )
            {   // Pawn is outside
                return true;
            }
            // Try to find an exit path
            var root = room.Regions[ 0 ];
            var exitFound = false;
            RegionTraverser.BreadthFirstTraverse(
                root,
                (from,to) =>
            {
                if( to.portal == null )
                {
                    return true;
                }
                return (
                    ( to.portal.FreePassage )||
                    ( to.portal.PawnCanOpen( pawn) )
                );
            },
                (reg) =>
            {
                if( !reg.Room.TouchesMapEdge )
                {
                    return false;
                }
                exitFound = true;
                return true;
            },
                MaxRegionsToCheckWhenEscapingThroughOpenDoors
            );
            return exitFound;
        }

    }

}
