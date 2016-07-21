using System.Collections.Generic;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public class RoomRoleWorker_SlaveWorkArea : RoomRoleWorker
    {

        public override float GetScore( Room room )
        {
            int num1 = 0;
            List<Thing> allContainedThings = room.AllContainedThings;
            for( int index = 0; index < allContainedThings.Count; ++index )
            {
                var thing = allContainedThings[ index ];
                var marker = thing as Building_RoomMarker;
                if(
                    ( marker != null )&&
                    ( marker.AllowSlaves )&&
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
            var score = num1;
            return score;
        }

    }

}
