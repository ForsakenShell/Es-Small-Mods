using System;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;

namespace PrisonImprovements
{

    internal static class _Pawn_NeedsTracker
    {

        internal static FieldInfo       _pawn;

        internal static Pawn            pawn( this Pawn_NeedsTracker obj )
        {
            if( _pawn == null )
            {
                _pawn = typeof( Pawn_NeedsTracker ).GetField( "pawn", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (Pawn) _pawn.GetValue( obj );
        }

        internal static bool _ShouldHaveNeed( this Pawn_NeedsTracker obj, NeedDef nd )
        {
            var pawn = obj.pawn();
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
                ( this.pawn.HostFaction != null )
            )
            {
                return false;
            }
            */
            if(
                ( !nd.colonistAndPrisonersOnly )||
                ( pawn.Faction != null )&&
                ( pawn.Faction.def == FactionDefOf.Colony )
            )
            {
                return true;
            }
            if( pawn.HostFaction != null )
            {
                return pawn.HostFaction == Faction.OfColony;
            }
            return false;
        }

    }

}