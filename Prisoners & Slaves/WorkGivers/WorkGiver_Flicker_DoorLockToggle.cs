using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace PrisonersAndSlaves
{
    
    public class WorkGiver_Flicker_DoorLockToggle : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal( Pawn Pawn )
        {   // Find all doors which want to be locked or unlocked and their cool-down has expired
            var doors = Find.ListerBuildings.allBuildingsColonist.Where( (t) =>
            {
                var door = t as Building_RestrictedDoor;
                if( door == null )
                {   // Not an appropriate door
                    return false;
                }
                var compLock = door.TryGetComp<CompLockable>();
                if( compLock == null )
                {   // Door is missing comp
                    return false;
                }
                if(
                    ( !compLock.IssueLockToggleJob )||
                    ( Find.TickManager.TicksGame < compLock.changeStateAfterTick )
                )
                {   // Door doesn't want lock toggle or cool down hasn't expired
                    return false;
                }
                if( !Pawn.CanReserveAndReach( door, PathEndMode.Touch, Pawn.NormalMaxDanger(), 1 ) )
                {   // Pawn can't reach door
                    return false;
                }
                // This door want's it's lock status changed
                return true;
            } );
            foreach( var door in doors )
            {
                yield return door;
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override Job JobOnThing( Pawn pawn, Thing t )
        {
            return new Job( Data.JobDefOf.DoorLockToggle, t );
        }

    }

}
