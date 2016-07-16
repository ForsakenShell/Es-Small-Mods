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

    public class JobDriver_Warden_UnrestrainPawn : JobDriver
    {

        private const TargetIndex PawnInd = TargetIndex.A;
        private const TargetIndex RestaintInd = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden( PawnInd );
            this.FailOnBurningImmobile( PawnInd );
            this.FailOnDowned( PawnInd );
            this.FailOnAggroMentalState( PawnInd );
            yield return Toils_Reserve.Reserve( PawnInd, 1 );
            yield return Toils_Goto.GotoThing( PawnInd, PathEndMode.ClosestTouch );
            yield return Toils_Haul.StartCarryThing( PawnInd );
            yield return Toils_Prisoner.Unrestrain( PawnInd, RestaintInd );
            yield return Toils_General.DropCarriedPawnRightHere();
            yield return Toils_Reserve.Release( PawnInd );
        }

    }

}
