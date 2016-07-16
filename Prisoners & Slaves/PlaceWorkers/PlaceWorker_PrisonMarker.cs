using System.Linq;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class PlaceWorker_PrisonMarker : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            var room = loc.GetRoom();
            if( room.ContainedBeds.Any( bed => !bed.ForPrisoners ) )
            {
                return (AcceptanceReport) Data.Strings.NoColonistBeds.Translate();
            }
            return (AcceptanceReport) true;
        }

    }

}
