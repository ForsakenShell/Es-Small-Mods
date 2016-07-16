using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public static class Pawn_Extensions
    {

        public static bool IsValidHumanlike( this Pawn pawn )
        {
            if(
                ( pawn == null )||
                ( pawn.Dead )||
                ( pawn.Destroyed )||
                ( !pawn.RaceProps.Humanlike )
            )
            {
                return false;
            }
            return pawn.Spawned;
        }

        public static bool IsSlave( this Pawn pawn )
        {
            if( !pawn.IsValidHumanlike() )
            {
                return false;
            }
            if( pawn.WornCollar() == null )
            {
                return false;
            }
            if( pawn.story.traits.GetTrait( Data.TraitDefOf.Enslaved ) == null )
            {
                return false;
            }
            if( pawn.kindDef != Data.PawnKindDefOf.Slave )
            {
                return false;
            }
            return true;
        }

        public static bool IsSlaveOfColony( this Pawn pawn )
        {
            if( !pawn.IsPrisonerOfColony )
            {
                return false;
            }
            if( !pawn.IsSlave() )
            {
                return false;
            }
            return true;
        }

        public static bool IsGuestOfColony( this Pawn pawn)
        {
            if( pawn.IsPrisonerOfColony )
            {   // Prisoners (and slaves by extension) are not "guests"
                return false;
            }
            return (
                ( pawn.Faction != Faction.OfPlayer )&&
                ( !pawn.HostileTo( Faction.OfPlayer ) )
            );
        }

        public static Apparel WornCollar( this Pawn pawn )
        {
            if(
                ( pawn == null ) ||
                ( pawn.apparel == null )
            )
            {
                return null;
            }
            var wornApparel = pawn.apparel.wornApparel();
            if( wornApparel == null )
            {
                return null;
            }
            for( int index = 0; index < wornApparel.Count; ++index )
            {
                if( wornApparel[ index ].IsSlaveCollar() )
                {
                    return wornApparel[ index ];
                }
            }
            return null;
        }

        public static Apparel WornRestraints( this Pawn pawn, BodyPartGroupDef bodyPartGroupDef )
        {
            if(
                ( pawn == null ) ||
                ( pawn.apparel == null )
            )
            {
                return null;
            }
            var wornApparel = pawn.apparel.wornApparel();
            if( wornApparel == null )
            {
                return null;
            }
            for( int index = 0; index < wornApparel.Count; ++index )
            {
                if(
                    ( wornApparel[ index ].IsRestraints() )&&
                    ( wornApparel[ index ].def.apparel.bodyPartGroups.Contains( bodyPartGroupDef ) )
                )
                {
                    return wornApparel[ index ];
                }
            }
            return null;
        }

        public static float EscapeProbability( this Pawn pawn )
        {
            float rangePercent = 999f;

            #region Check Colonists

            var colonists = Find.MapPawns.FreeColonists;
            if( colonists.Count() > 0 )
            {   // Check all colonists
                foreach( var colonist in colonists )
                {
                    if(
                        ( pawn != colonist )&&
                        ( Data.LineOfSight( colonist.Position, pawn.Position ) )
                    )
                    {   // Colonist can see them
                        // Check distance
                        var attackVerb = colonist.TryGetAttackVerb();
                        if( attackVerb != null )
                        {   // Get use attack range for colonists
                            var rangeExtreme = attackVerb.verbProps.range * Data.MaxRangeMultiplierForChecks;
                            var rangeMin = attackVerb.verbProps.range * Data.MinRangeMultiplierForChecks;
                            var dist = ( colonist.Position - pawn.Position ).LengthHorizontal;
                            if( dist < rangeMin )
                            {   // Colonist is too close, don't attempt to escape
                                return 0.0f;
                            }
                            else if( dist < rangeExtreme )
                            {   // This colonist is closer than any others (so far)
                                dist -= rangeMin;
                                var colonistRange = dist / ( rangeExtreme - rangeMin );
                                if( colonistRange < rangePercent )
                                {
                                    rangePercent = colonistRange;
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Check Remotely Observable Room Markers (Security Cameras)

            var markers = Data.AllRoomMarkersOfColony().Where( marker => marker.AllowRemoteMonitoring );

            if( markers.Count() > 0 )
            {   // Check for remote observable markers
                foreach( var marker in markers )
                {
                    // Calculate range
                    var offset = marker.Position - pawn.Position;
                    var dist = offset.LengthHorizontal;
                    var range = dist / ( marker.compRemote.Props.observationRange * Data.MaxRangeMultiplierForChecks );
                    if( range < rangePercent )
                    {
                        rangePercent = range;
                    }
                }
            }

            #endregion

            // Return proximity distance (further is higher odds of running)
            return Mathf.Min( 1.0f, rangePercent );
        }

        public static bool ThinksTheyCanBeSeenByColony( this Pawn pawn )
        {
            var colonists = Find.MapPawns.FreeColonists.ToList();
            var markers = Data.AllRoomMarkersOfColony().Where( marker => marker.AllowRemoteMonitoring ).ToList();
            return ThinksTheyCanBeSeenByColony( pawn, colonists, markers );
        }

        public static bool ThinksTheyCanBeSeenByColony( this Pawn pawn, List<Pawn> colonists, List<Building_RoomMarker> markers )
        {
            if( !colonists.NullOrEmpty() )
            {   // Check colonists
                foreach( var colonist in colonists )
                {
                    if(
                        ( pawn != colonist )&&
                        ( Data.LineOfSight( colonist.Position, pawn.Position ) )
                    )
                    {   // Thinks colonist can see them
                        return true;
                    }
                }
            }
            if( !markers.NullOrEmpty() )
            {   // Check for remote observable markers
                foreach( var marker in markers )
                {
                    if( Data.LineOfSight( marker.Position, pawn.Position, true ) )
                    {   // Thinks marker can see them
                        return true;
                    }
                }
            }
            return false;
        }

    }

}
