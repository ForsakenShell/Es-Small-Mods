using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class CompSecurityStation : ThingComp
    {
        
        private List<Building_RoomMarker>       currentMarkers;

        public                                  CompSecurityStation()
        {
            currentMarkers = new List<Building_RoomMarker>();
        }

        public CompProperties_SecurityStation   Props
        {
            get
            {
                return this.props as CompProperties_SecurityStation;
            }
        }

        public override void                    PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.LookList( ref currentMarkers, "currentMarkers", LookMode.MapReference, null );
        }

        public override void                    PostSpawnSetup()
        {
            if( currentMarkers == null )
            {
                currentMarkers = new List<Building_RoomMarker>();
            }
            base.PostSpawnSetup();
        }

        public void                             FinishMonitoring()
        {
            UpdateMonitoredMarkers( Find.TickManager.TicksGame, false );
            currentMarkers.Clear();
            Data.DirtyOverlays();
        }

        private void                            UpdateMonitoredMarkers( int tick, bool currentlyMonitored )
        {
            if( currentMarkers.NullOrEmpty() )
            {
                return;
            }
            foreach( var marker in currentMarkers )
            {
                bool foundHostile = false;
                foreach( var cell in marker.compRemote.ObservableCells )
                {
                    foreach( var thing in cell.GetThingList().Where( t => t.HostileTo( Faction.OfPlayer ) ) )
                    {
                        foundHostile = true;
                        break;
                    }
                    if( foundHostile )
                    {
                        break;
                    }
                }
                if( foundHostile )
                {   // Keep this camera at the top of the list
                    marker.LastMonitoredTick = 0;
                    marker.MonitorUntilTick = tick + Data.MonitorRemoteChangeTicks;
                    // Add room to list of rooms with hostiles in them
                    Monitor.hostilesInRooms.AddUnique( marker.GetRoom() );
                }
                else
                {
                    marker.LastMonitoredTick = tick;
                }
                marker.CurrentlyMonitored = currentlyMonitored;
            }
        }

        public void                             UpdateMonitor()
        {
            if( !parent.IsHashIntervalTick( Data.MonitorRemoteCheckTicks ) )
            {
                return;
            }
            var markers = Data.AllRoomMarkersOfColony().Where( marker => marker.AllowRemoteMonitoring ).ToList();
            if( markers.NullOrEmpty() )
            {   // No markers at all, do nothing
                return;
            }
            bool dirtyOverlays = false;
            var currentGameTick = Find.TickManager.TicksGame;
            if( !currentMarkers.NullOrEmpty() )
            {   // Check current markers and remove any which have been monitored long enough (or despawned or destroyed)
                for( int index = currentMarkers.Count - 1; index >= 0; --index )
                {
                    var marker = currentMarkers[ index ];
                    if(
                        ( !marker.Spawned )||
                        ( marker.Destroyed )||
                        ( currentGameTick >= marker.MonitorUntilTick )
                    )
                    {
                        if( !marker.Destroyed )
                        {
                            marker.LastMonitoredTick = currentGameTick;
                            marker.CurrentlyMonitored = false;
                        }
                        currentMarkers.Remove( marker );
                        dirtyOverlays = true;
                    }
                }
            }
            // Sort markers so the markers which haven't been viewed recently are at the top
            markers.Sort( (x,y)=>
            {
                return x.LastMonitoredTick < y.LastMonitoredTick ? -1 : 1;
            } );
            var markerCount = currentMarkers.Count;
            if( markerCount < Props.MaxCamerasAtOnce )
            {
                if( markers.Count > markerCount )
                {
                    foreach( var marker in markers )
                    {
                        if(
                            ( !currentMarkers.Contains( marker ) )&&
                            ( !marker.CurrentlyMonitored )
                        )
                        {
                            currentMarkers.Add( marker );
                            marker.CurrentlyMonitored = true;
                            marker.MonitorUntilTick = currentGameTick + Data.MonitorRemoteChangeTicks;
                            markerCount++;
                            dirtyOverlays = true;
                        }
                        if( markerCount >= Props.MaxCamerasAtOnce )
                        {
                            break;
                        }
                    }
                }
            }
            // Now update all the markers
            UpdateMonitoredMarkers( currentGameTick, true );
            // Update minimap overlays
            if( dirtyOverlays )
            {
                Data.DirtyOverlays();
            }
        }

    }

}
