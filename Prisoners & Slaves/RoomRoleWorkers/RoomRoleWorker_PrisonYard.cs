using System.Collections.Generic;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class RoomRoleWorker_PrisonYard : RoomRoleWorker
    {

        public override float GetScore( Room room )
        {
            if( !room.PsychologicallyOutdoors )
            {
                return 0.0f;
            }
            int num1 = 0;
            List<Thing> allContainedThings = room.AllContainedThings;
            for( int index = 0; index < allContainedThings.Count; ++index )
            {
                var thing = allContainedThings[ index ];
                if( thing is Building_Bed )
                {
                    return 0.0f;
                }
                var marker = thing as Building_RoomMarker;
                if(
                    ( marker != null )&&
                    ( marker.AllowPrisoners )&&
                    ( marker.IsActive )
                )
                {
                    num1++;
                }
            }
            if( num1 == 0 )
            {
                return 0.0f;
            }
            var score = num1 * 10000.0f + room.OpenRoofCount * 100.0f;
            return score;
        }

    }

}
