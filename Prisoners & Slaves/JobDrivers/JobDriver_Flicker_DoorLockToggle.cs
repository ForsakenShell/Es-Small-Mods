using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class JobDriver_Flicker_DoorLockToggle : JobDriver
    {

        private const TargetIndex DoorInd = TargetIndex.A;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var door = this.CurJob.GetTarget( DoorInd ).Thing as Building_RestrictedDoor;
            var compLock = door.TryGetComp<CompLockable>();
            this.FailOnDespawnedOrNull( DoorInd );
            this.FailOnBurningImmobile( DoorInd );
            yield return Toils_Reserve.Reserve( DoorInd, 1 );
            yield return Toils_Goto.GotoThing( DoorInd, PathEndMode.Touch );
            yield return Toils_General.DoorLockToggle( DoorInd, pawn, compLock );
            yield return Toils_Reserve.Release( DoorInd );
        }

    }

}
