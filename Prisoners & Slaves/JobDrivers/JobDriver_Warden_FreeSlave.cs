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

	public class JobDriver_Warden_FreeSlave : JobDriver
	{

		private const TargetIndex SlaveInd = TargetIndex.A;
		private const TargetIndex CollarInd = TargetIndex.B;
		private const TargetIndex ReleaseCellInd = TargetIndex.C;

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedNullOrForbidden( SlaveInd );
			this.FailOnBurningImmobile( SlaveInd );
            this.AddEndCondition( () =>
			{
				var slave = (Pawn) this.GetActor().CurJob.GetTarget( SlaveInd ).Thing;
                if( slave.guest.interactionMode == Data.PIM.FreeSlave )
                {
                    return JobCondition.Ongoing;
                }
                //Don't think this line is needed, framework should release all reservations for failed/ended jobs?
                //Find.Reservations.ReleaseAllClaimedBy( pawn );
                return JobCondition.InterruptOptional;
			} );
			this.FailOnDowned( SlaveInd );
			this.FailOnAggroMentalState( SlaveInd );
			yield return Toils_Reserve.Reserve( SlaveInd, 1 );
			yield return Toils_Goto.GotoThing( SlaveInd, PathEndMode.ClosestTouch );
			yield return Toils_Haul.StartCarryThing( SlaveInd );
			yield return Toils_Goto.GotoCell( ReleaseCellInd, PathEndMode.ClosestTouch );
            yield return Toils_Prisoner.FreeSlave( SlaveInd, CollarInd );
            yield return Toils_General.DropCarriedPawnRightHere();
			yield return Toils_Reserve.Release( SlaveInd );
            yield return Toils_Prisoner.IssueLeaveJob( SlaveInd );
		}

	}

}
