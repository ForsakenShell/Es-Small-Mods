using System;
using System.Collections.Generic;
using System.Reflection;

using CommunityCoreLibrary;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    internal class _Pawn_NeedsTracker : Pawn_NeedsTracker
    {

        internal bool _ShouldHaveNeed( NeedDef nd )
        {
            var pawn = this.pawn();
            if( pawn.RaceProps.intelligence < nd.minIntelligence )
            {
                return false;
            }
            if( nd == NeedDefOf.Food )
            {
                return pawn.RaceProps.EatsFood;
            }
            if( nd == NeedDefOf.Rest )
            {
                return pawn.RaceProps.needsRest;
            }
            /*
            if(
                ( nd == NeedDefOf.Joy )&&
                ( pawn.HostFaction != null )
            )
            {
                return false;
            }
            */
            if(
                ( !nd.colonistAndPrisonersOnly )||
                ( pawn.Faction != null )&&
                ( pawn.Faction.IsPlayer )
            )
            {
                return true;
            }
            if( pawn.HostFaction != null )
            {
                return pawn.HostFaction == Faction.OfPlayer;
            }
            return false;
        }

    }

}