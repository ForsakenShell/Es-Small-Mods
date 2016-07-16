using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class WorkGiver_Warden_MonitorSecurityStation : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal( Pawn Pawn )
        {   // Find all free security stations this pawn can reach and reserve
            var stations = Find.ListerBuildings.allBuildingsColonist.Where( t => (
                ( t.TryGetComp<CompSecurityStation>() != null )&&
                ( Pawn.CanReserveAndReach( t, PathEndMode.InteractionCell, Pawn.NormalMaxDanger(), 1 ) )
            ) );
            foreach( var station in stations )
            {
                yield return station;
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.InteractionCell;
            }
        }

        public override Job JobOnThing( Pawn pawn, Thing t )
        {
            if( !pawn.CanReserveAndReach( t, PathEndMode.InteractionCell, pawn.NormalMaxDanger(), 1 ) )
            {
                return null;
            }
            var compPower = t.TryGetComp<CompPowerTrader>();
            if(
                ( compPower != null ) &&
                ( !compPower.PowerOn )
            )
            {   // Power is off on thing
                return null;
            }
            if( !Data.AllRoomMarkersOfColony().Any( marker => marker.AllowRemoteMonitoring ) )
            {   // No markers which can be remotely monitored
                return null;
            }
            return new Job( Data.JobDefOf.MonitorSecurityStation, t, pawn );
        }

    }

}
