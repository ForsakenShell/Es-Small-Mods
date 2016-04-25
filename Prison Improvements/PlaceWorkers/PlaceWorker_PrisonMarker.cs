using System.Linq;

using RimWorld;
using Verse;

namespace PrisonImprovements
{
    
    public class PlaceWorker_PrisonMarker : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            var room = loc.GetRoom();
            if(
                ( room == null )||
                ( room.TouchesMapEdge )
            )
            {
                return (AcceptanceReport) "PI_MustBeInRoom".Translate();
            }
            if( room.ContainedBeds.Any( bed => !bed.ForPrisoners ) )
            {
                return (AcceptanceReport) "PI_NoColonistBeds".Translate();
            }
            return (AcceptanceReport) true;
        }

    }

}
