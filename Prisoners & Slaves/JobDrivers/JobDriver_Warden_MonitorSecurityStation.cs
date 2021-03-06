using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class JobDriver_Warden_MonitorSecurityStation : JobDriver
    {
        
        private const TargetIndex StationInd = TargetIndex.A;
        private const TargetIndex WardenInd = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden( StationInd );
            this.FailOnBurningImmobile( StationInd );
            yield return Toils_Reserve.Reserve( StationInd, 1 );
            yield return Toils_Goto.GotoThing( StationInd, PathEndMode.InteractionCell );
            yield return Toils_SecurityStation.MonitorStation( StationInd, WardenInd );
        }

    }

}
