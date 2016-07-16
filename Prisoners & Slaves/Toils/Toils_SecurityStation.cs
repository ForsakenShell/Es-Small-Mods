using System;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{
    
    public static class Toils_SecurityStation
    {
        
        public static Toil MonitorStation( TargetIndex StationInd, TargetIndex WardenInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = Data.MonitorRemoteJobTicks;
            toil.WithProgressBarToilDelay( WardenInd );
            toil.AddFinishAction( () =>
            {
                var stationThing = toil.actor.CurJob.GetTarget( StationInd ).Thing;
                if( stationThing == null )
                {
                    throw new Exception( "Target thing is null for Toils_SecurityStation.MonitorStation!" );
                }
                var stationComp = stationThing.TryGetComp<CompSecurityStation>();
                if( stationComp == null )
                {
                    throw new Exception( "Target thing is missing CompSecurityStation for Toils_SecurityStation.MonitorStation!" );
                }
                stationComp.FinishMonitoring();
                Find.Reservations.Release( toil.actor.jobs.curJob.GetTarget( StationInd ), toil.actor );
            } );
            toil.tickAction = () =>
            {
                var stationThing = toil.actor.CurJob.GetTarget( StationInd ).Thing;
                if( stationThing == null )
                {
                    throw new Exception( "Target thing is null for Toils_SecurityStation.MonitorStation!" );
                }
                var stationComp = stationThing.TryGetComp<CompSecurityStation>();
                if( stationComp == null )
                {
                    throw new Exception( "Target thing is missing CompSecurityStation for Toils_SecurityStation.MonitorStation!" );
                }
                stationComp.UpdateMonitor();
            };
            return toil;
        }

    }

}
