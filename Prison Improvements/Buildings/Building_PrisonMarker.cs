using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonImprovements
{

    public class Building_PrisonMarker : Building
    {

        private static int markerIndex = 1;
        public string markerName = "";

        private CompPowerTrader compPower;

        public virtual bool IsActive
        {
            get
            {
                return (
                    ( compPower == null )||
                    ( compPower.PowerOn )
                );
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            markerName = "PI_Default_Camera_Name".Translate( markerIndex );
            markerIndex++;
            compPower = this.TryGetComp<CompPowerTrader>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<string>( ref this.markerName, "markerName", "", false );
            Scribe_Values.LookValue<int>( ref markerIndex, "markerIndex", 1, false );
        }

        public override void Tick()
        {
            base.Tick();
            if( !this.IsHashIntervalTick( 30 ) )
            {
                return;
            }
            UpdateRoomState();
        }

        public override void TickRare()
        {
            base.TickRare();
            UpdateRoomState();
        }

        public void UpdateRoomState()
        {
            var room = this.GetRoom();
            if(
                ( room == null )||
                ( room.TouchesMapEdge )||
                ( room.ContainedBeds.Any( bed => !bed.ForPrisoners ) )
            )
            {
                room.isPrisonCell = false;
            }
            else
            {
                if( IsActive )
                {
                    room.isPrisonCell = true;
                }
                else
                {
                    room.isPrisonCell = false;
                }
            }
        }

        public override void DeSpawn()
        {
            var room = this.GetRoom();
            if( room != null )
            {
                room.RoomChanged();
            }
            base.DeSpawn();
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            var room = this.GetRoom();
            if(
                ( room == null )||
                ( !room.isPrisonCell )
            )
            {
                return;
            }
            room.DrawFieldEdges();
        }

        public override void DrawGUIOverlay()
        {
            if(
                ( Find.CameraMap.CurrentZoom != CameraZoomRange.Closest )||
                ( this.markerName.Trim().Equals("") )
            )
            {
                return;
            }
            GenWorldUI.DrawThingLabel( this, this.markerName, Color.white );
        }

    }

}
