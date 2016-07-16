using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CommunityCoreLibrary;
using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public static class Room_Extensions
    {

        internal static List<Building_RoomMarker> ContainedMarkers( this Room room, List<Thing> checkList = null )
        {
            if( checkList == null )
            {
                checkList = room.AllContainedThings;
            }
            if( checkList.NullOrEmpty() )
            {
                return null;
            }
            var allContainedMarkers = new List<Building_RoomMarker>();
            foreach( var thing in checkList )
            {
                if( thing is Building_RoomMarker )
                {
                    allContainedMarkers.Add( (Building_RoomMarker)thing );
                }
            }
            return allContainedMarkers;
        }

        internal static bool                IsSlaveWorkArea( this Room room )
        {
            if( room.CellCount == 1 )
            {
                var door = room.Cells.ElementAt( 0 ).GetRegion().portal;
                if( door != null )
                {
                    var comp = door.TryGetComp<CompRestrictedDoor>();
                    if( comp != null )
                    {
                        return comp.AllowSlaves;
                    }
                    return false;
                }
            }
            if( room.ContainedMarkers().NullOrEmpty() )
            {
                return false;
            }
            return room.ContainedMarkers().Any( marker => (
                ( marker.IsActive )&&
                ( marker.AllowSlaves )
            ) );
        }

    }

}