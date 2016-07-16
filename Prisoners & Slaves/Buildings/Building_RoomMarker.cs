using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonersAndSlaves
{

    public class Building_RoomMarker : Building
    {

        public string                           markerName = string.Empty;

        private bool                            _allowPrisoners;
        private bool                            _allowSlaves;

        private bool                            _queueUpdate;

        private CompPowerTrader                 compPower;
        public CompRemotelyObservable           compRemote;

        public int                              LastMonitoredTick;
        public int                              MonitorUntilTick;

        private static List<IntVec3>            fields;

        private bool                            _currentlyMonitored = false;

        public bool                             AllowPrisoners
        {
            get
            {
                return _allowPrisoners;
            }
            set
            {
                if( _allowPrisoners != value )
                {
                    _allowPrisoners = value;
                    QueueRoomUpdate();
                }
            }
        }

        public bool                             AllowSlaves
        {
            get
            {
                return _allowSlaves;
            }
            set
            {
                if( _allowSlaves != value )
                {
                    _allowSlaves = value;
                    QueueRoomUpdate();
                }
            }
        }

        public virtual bool                     IsActive
        {
            get
            {
                return (
                    ( compPower == null )||
                    ( compPower.PowerOn )
                );
            }
        }

        public string                           BaseNameKey
        {
            get
            {
                if( compPower != null )
                {
                    return Data.Strings.RoomMarkerCameraName;
                }
                return Data.Strings.RoomMarkerSignName;
            }
        }

        public bool                             AllowRemoteMonitoring
        {
            get
            {
                if( compRemote == null )
                {
                    return false;
                }
                return( IsActive );
            }
        }

        public bool                             CurrentlyMonitored
        {
            get
            {
                if( !AllowRemoteMonitoring )
                {
                    return false;
                }
                return _currentlyMonitored;
            }
            set
            {
                if( !AllowRemoteMonitoring )
                {
                    _currentlyMonitored = false;
                    return;
                }
                _currentlyMonitored = value;
            }
        }

        public override void                    SpawnSetup()
        {
            base.SpawnSetup();
            compPower = this.TryGetComp<CompPowerTrader>();
            compRemote = this.TryGetComp<CompRemotelyObservable>();
            fields = new List<IntVec3>();
            if( markerName == string.Empty )
            {   // Only assign if it's empty, so loading a named marker won't reset it's name
                markerName = BaseNameKey.Translate( Monitor.GetNextMarkerIndex() );
            }
            QueueRoomUpdate();
        }

        public override void                    ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue( ref markerName, "markerName", string.Empty, false );
            Scribe_Values.LookValue( ref _allowPrisoners, "allowPrisoners", false, false );
            Scribe_Values.LookValue( ref _allowSlaves, "allowSlaves", false, false );
            Scribe_Values.LookValue( ref LastMonitoredTick, "LastMonitoredTick", 0, false );
            Scribe_Values.LookValue( ref _currentlyMonitored, "currentlyMonitored", false, false );
            Scribe_Values.LookValue( ref MonitorUntilTick, "MonitorUntilTick", 0, false );
        }

        public override void                    DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            if( this.GetRoom() == null )
            {
                return;
            }
            this.GetRoom().DrawFieldEdges();
        }

        public override void                    DrawGUIOverlay()
        {
            var drawLabel = this.markerName.Trim();
            if(
                ( Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest )||
                ( drawLabel.Equals( "" ) )
            )
            {
                return;
            }
            if( !AllowPrisoners )
            {
                var room = this.Position.GetRoom();
                if(
                    ( room != null )&&
                    ( room.Owners.Count() > 0 )
                )
                {
                    var markerOwners = GetOwnerNames();
                    if( !markerOwners.Equals( "" ) )
                    {
                        drawLabel += "\n";
                        drawLabel += markerOwners;
                    }
                }
            }
            GenWorldUI.DrawThingLabel( this, drawLabel, Color.white );
        }

        protected override void                 ReceiveCompSignal( string signal )
        {
            base.ReceiveCompSignal( signal );
            if( compPower == null )
            {
                return;
            }
            if(
                ( signal == CompPowerTrader.PowerTurnedOffSignal )||
                ( signal == CompPowerTrader.PowerTurnedOnSignal )
            )
            {
                QueueRoomUpdate();
            }
        }

        public override void Tick()
        {
            base.Tick();
            if(
                ( _queueUpdate )&&
                ( this.IsHashIntervalTick( Data.QueueUpdateTicks ) )
            )
            {
                UpdateRoomStatus();
            }
        }

        public override void TickRare()
        {
            base.Tick();
            if( _queueUpdate )
            {
                UpdateRoomStatus();
            }
        }

        public void                             QueueRoomUpdate()
        {
            _queueUpdate = true;
        }

        private void                            UpdateRoomStatus()
        {
            _queueUpdate = false;
            if( this.GetRoom() != null )
            {
                this.GetRoom().RoomChanged();
            }
        }

        public string                           GetOwnerNames()
        {
            var markerOwner = string.Empty;
            var room = this.Position.GetRoom();
            if(
                ( room != null )&&
                ( room.Owners.Count() > 0 )
            )
            {
                markerOwner = "(";
                for( int i = 0; i < room.Owners.Count(); ++i )
                {
                    if( i > 0 )
                    {
                        markerOwner += ", ";
                    }
                    markerOwner += room.Owners.ElementAt( i ).NameStringShort;
                }
                markerOwner += ")";
            }
            return markerOwner;
        }

    }

}
