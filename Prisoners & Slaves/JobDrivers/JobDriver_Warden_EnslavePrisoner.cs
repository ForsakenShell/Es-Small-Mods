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
    
    public class JobDriver_Warden_EnslavePrisoner : JobDriver
    {
        
        private const TargetIndex PrisonerInd = TargetIndex.A;
        private const TargetIndex CollarInd = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden( PrisonerInd );
            this.FailOnDestroyedNullOrForbidden( CollarInd );
            this.FailOnBurningImmobile( CollarInd );
            this.FailOnAggroMentalState( PrisonerInd );
            yield return Toils_Reserve.Reserve( PrisonerInd, 1 );
            yield return Toils_Reserve.Reserve( CollarInd, 1 );
            yield return Toils_Goto.GotoThing( CollarInd, PathEndMode.ClosestTouch );
            yield return Toils_Haul.StartCarryThing( CollarInd );
            yield return Verse.AI.Toils_General.PutCarriedThingInInventory();
            yield return Toils_Goto.GotoThing( PrisonerInd, PathEndMode.ClosestTouch );
            yield return Toils_Haul.StartCarryThing( PrisonerInd );
            yield return Toils_Prisoner.Enslave( PrisonerInd, CollarInd );
            yield return Toils_General.DropCarriedPawnRightHere();
            yield return Toils_Reserve.Release( PrisonerInd );
        }

    }

}
