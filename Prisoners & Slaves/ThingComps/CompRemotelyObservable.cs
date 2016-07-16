using System;
using System.Collections.Generic;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public class CompRemotelyObservable : ThingComp
    {

        private const int                           OBSERVABLE_UPDATE_TICKS = 250;

        private List<IntVec3>                       cachedObservableCells;
        private List<IntVec3>                       cachedVisibleRangeCells;
        private int                                 nextObservableUpdate;

        private CompPowerTrader                     compPower;

        public List<IntVec3>                        ObservableCells
        {
            get
            {
                if(
                    ( cachedObservableCells == null )||
                    ( Find.TickManager.TicksGame > nextObservableUpdate )
                )
                {
                    cachedObservableCells = new List<IntVec3>();
                    foreach( var cell in VisualRangeCells )
                    {
                        if( Data.LineOfSight( this.parent.Position, cell, true ) )
                        {
                            var edifice = cell.GetEdifice();
                            var addCell = (
                                ( edifice == null )||
                                ( !edifice.def.blockLight )
                            );
                            if( addCell )
                            {
                                cachedObservableCells.Add( cell );
                            }
                        }
                    }
                    nextObservableUpdate = Find.TickManager.TicksGame + OBSERVABLE_UPDATE_TICKS;
                }
                return cachedObservableCells;
            }
        }

        public List<IntVec3>                        VisualRangeCells
        {
            get
            {
                if( cachedVisibleRangeCells == null )
                {
                    cachedVisibleRangeCells = new List<IntVec3>();
                    float angleMin = 0f;
                    float angleMax = 360f;
                    if( Props.observationAngle < 360.0f )
                    {
                        angleMin = this.parent.Rotation.AsAngle - Props.observationAngle / 2;
                        angleMax = this.parent.Rotation.AsAngle + Props.observationAngle / 2;
                        if( angleMin < 0.0f )
                        {
                            angleMin += 360.0f;
                        }
                        if( angleMax > 360.0f )
                        {
                            angleMax -= 360.0f;
                        }
                    }
                    //Log.Message( string.Format( "{0} Angles: '{1}' to '{2}'", this.parent.Position.ToString(), angleMin, angleMax ) );
                    foreach( var cell in GenRadial.RadialCellsAround( this.parent.Position, Props.observationRange, false ) )
                    {
                        bool addCell = Mathf.Approximately( Props.observationAngle, 360.0f );
                        if( Props.observationAngle < 360.0f )
                        {
                            var offset = cell - this.parent.Position;
                            var offsetAngle = offset.AngleFlat;
                            //Log.Message( string.Format( "{0} Angle to Camera: '{1}'", cell.ToString(), offsetAngle ) );
                            if(
                                (
                                    ( angleMin < angleMax )&&
                                    (
                                        ( offsetAngle >= angleMin )&&
                                        ( offsetAngle <= angleMax )
                                    )
                                )||
                                (
                                    ( angleMin > angleMax )&&
                                    (
                                        ( offsetAngle >= angleMin )||
                                        ( offsetAngle <= angleMax )
                                    )
                                )
                            )
                            {
                                addCell = true;
                            }
                        }
                        if( addCell )
                        {
                            cachedVisibleRangeCells.Add( cell );
                        }
                    }
                }
                return cachedVisibleRangeCells;
            }
        }

        public CompProperties_RemotelyObservable    Props
        {
            get
            {
                return this.props as CompProperties_RemotelyObservable;
            }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            compPower = this.parent.TryGetComp<CompPowerTrader>();
            // Invalidate observability on spawn
            cachedObservableCells = null;
            cachedVisibleRangeCells = null;
            nextObservableUpdate = 0;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            if(
                ( compPower != null )&&
                ( !compPower.PowerOn )
            )
            {
                return;
            }
            GenDraw.DrawFieldEdges( ObservableCells );
        }

    }

}
