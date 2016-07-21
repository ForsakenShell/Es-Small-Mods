using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    internal static class _Room
    {

        private static Color                    PrisonFieldColor       = new Color( 1.0f, 0.7f, 0.2f );
        private static Color                    SlaveFieldColor        = new Color( 1.0f, 1.0f, 0.2f );
        private static Color                    CustomFieldColor       = new Color( 1.0f, 0.5f, 1.0f );
        private static Color                    NonPrisonFieldColor    = new Color( 0.3f, 1.0f, 0.3f );

        internal static void                    _RoomChanged( this Room room )
        {
            ProfilerThreadCheck.BeginSample( "RoomChanged" );
            room.CachedCellCountSet( -1 );
            room.CachedOpenRoofCountSet( -1 );
            if( Current.ProgramState == ProgramState.MapPlaying )
            {
                ProfilerThreadCheck.BeginSample( "RoofGenerationRequest" );
                AutoBuildRoofZoneSetter.TryGenerateRoofFor( room );
                ProfilerThreadCheck.EndSample();
            }
            room.isPrisonCell = false;
            var allContainedThings = room.AllContainedThings;
            if( !room.TouchesMapEdge )
            {
                for( int index = 0; index < allContainedThings.Count; ++index )
                {
                    var bed = allContainedThings[ index ] as Building_Bed;
                    if(
                        ( bed != null )&&
                        ( bed.ForPrisoners )
                    )
                    {
                        room.isPrisonCell = true;
                        break;
                    }
                }
            }
            var markers = room.ContainedMarkers( allContainedThings );
            if( !markers.NullOrEmpty() )
            {
                foreach( var marker in markers )
                {
                    if(
                        ( marker.IsActive )&&
                        ( marker.AllowPrisoners )&&
                        ( !room.TouchesMapEdge )
                    )
                    {
                        room.isPrisonCell = true;
                        break;
                    }
                }
            }
            if( Current.ProgramState == ProgramState.MapPlaying )
            {
                if( room.isPrisonCell )
                {
                    foreach( var bed in room.ContainedBeds )
                    {
                        bed.ForPrisoners = true;
                    }
                }
            }
            room.lastChangeTick = Find.TickManager.TicksGame;
            room.TempTracker.RoomChanged();

            room.StatsAndRoleDirtySet( true );
            room.Notify_BedTypeChanged();

            FacilitiesUtility.NotifyFacilitiesAboutChangedLOSBlockers( room.Regions );

            if( Current.ProgramState != ProgramState.Entry )
            {
                foreach( var door in room.Portals() )
                {   // Inform doors that this room has changed
                    var restrictedDoor = door as Building_RestrictedDoor;
                    if( restrictedDoor != null )
                    {
                        //restrictedDoor.QueueDoorStatusUpdate( true );
                        restrictedDoor.ClearCache( true );
                    }
                    //door.BroadcastCompSignal( Data.Signal.InternalRecache );
                }
            }
            ProfilerThreadCheck.EndSample();
        }

        internal static void _DrawFieldEdges( this Room room )
        {
            if(
                ( room.RegionCount >= 20 )||
                ( room.TouchesMapEdge )
            )
            {
                return;
            }
            //Color color = room.isPrisonCell ? PrisonFieldColor :
            //                  room.IsSlaveWorkArea() ? SlaveFieldColor :
            //                  NonPrisonFieldColor;
            Color color = NonPrisonFieldColor;
            if( room.isPrisonCell )
            {
                color = PrisonFieldColor;
            }
            else if( room.Role == Data.RoomRoleDefOf.SlaveWorkArea )
            {
                color = SlaveFieldColor;
            }
            else
            {
                var markers = room.ContainedMarkers();
                if(
                    ( !markers.NullOrEmpty() )&&
                    ( markers.Any( marker => marker.IsActive ) )
                )
                {
                    color = CustomFieldColor;
                }
            }
            color.a = Pulser.PulseBrightness( 1f, 0.6f );
            GenDraw.DrawFieldEdges( room.Cells.ToList(), color );
        }

    }

}