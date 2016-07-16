using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public static class Thing_Extensions
    {

        public static bool IsSlaveCollar( this Thing thing )
        {
            return thing.def.IsSlaveCollar();
        }

        public static bool IsRestraints( this Thing thing )
        {
            return thing.def.IsRestraints();
        }

        public static bool CanBeSeenByColony( this Thing thing, bool checkColonists = true, bool checkCameras = true )
        {
            List<Pawn> colonists = null;
            List<Building_RoomMarker> cameras = null;
            if( checkColonists )
            {
                colonists = Find.MapPawns.FreeColonists.ToList();
            }
            if( checkCameras )
            {
                cameras = Data.AllRoomMarkersOfColony().Where( marker => marker.AllowRemoteMonitoring ).ToList();
            }
            return CanBeSeenByColony( thing, colonists, cameras );
        }

        public static bool CanBeSeenByColony( this Thing thing, List<Pawn> colonists, List<Building_RoomMarker> cameras )
        {
            if( !colonists.NullOrEmpty() )
            {   // Check colonists
                foreach( var colonist in colonists )
                {
                    if(
                        ( thing != colonist )&&
                        ( Data.LineOfSight( colonist.Position, thing.Position ) )
                    )
                    {   // Colonist can see thing
                        return true;
                    }
                }
            }
            if( !cameras.NullOrEmpty() )
            {   // Check remote observable markers currently monitored
                foreach( var marker in cameras )
                {
                    if(
                        ( marker.CurrentlyMonitored )&&
                        ( marker.compRemote.ObservableCells.Contains( thing.Position ) )
                    )
                    {   // Warden can see thing through a camera
                        return true;
                    }
                }
            }
            return false;
        }

    }

}