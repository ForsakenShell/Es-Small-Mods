using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using Verse;
using Verse.AI;

namespace PrisonImprovements
{
    
    public class JobDriver_Warden_EnslavePrisoner : JobDriver
    {
        
        private const TargetIndex PrisonerInd = TargetIndex.A;
        private const TargetIndex CollarInd = TargetIndex.B;
        private const TargetIndex CollarCellInd = TargetIndex.C;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden( PrisonerInd );
            this.FailOnDestroyedNullOrForbidden( CollarInd );
            this.FailOnBurningImmobile( CollarInd );
            yield return Toils_Reserve.Reserve( PrisonerInd, 1 );
            yield return Toils_Reserve.Reserve( CollarInd, 1 );
            yield return Toils_Goto.GotoThing( CollarInd, PathEndMode.ClosestTouch );
            yield return Toils_Haul.StartCarryThing( CollarInd );
            var gotoToil = Toils_Goto.GotoThing( PrisonerInd, PathEndMode.ClosestTouch );
            yield return gotoToil;
            yield return Toils_Haul.PlaceHauledThingInCell( CollarCellInd, gotoToil, false );
            yield return Toils_Reserve.Release( CollarInd );
            yield return Toils_Prisoner.Enslave( PrisonerInd, CollarInd );
            yield return Toils_Reserve.Release( PrisonerInd );
        }

    }

}
