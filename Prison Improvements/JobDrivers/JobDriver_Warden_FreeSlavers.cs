using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonImprovements
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
			this.FailOn( () =>
			{
				var slave = (Pawn) this.GetActor().CurJob.GetTarget( SlaveInd ).Thing;
				return slave.guest.interactionMode != (PrisonerInteractionMode) Data.PIM_FreeSlave;
			} );
			this.FailOnDowned( SlaveInd );
			this.FailOnAggroMentalState( SlaveInd );
			yield return Toils_Reserve.Reserve( SlaveInd, 1 );
			yield return Toils_Goto.GotoThing( SlaveInd, PathEndMode.ClosestTouch );
			yield return Toils_Haul.StartCarryThing( SlaveInd );
			var gotoToil = Toils_Goto.GotoCell( ReleaseCellInd, PathEndMode.ClosestTouch );
			yield return gotoToil;
			yield return Toils_Haul.PlaceHauledThingInCell( ReleaseCellInd, gotoToil, false );
			yield return Toils_Reserve.Release( SlaveInd );
			yield return Toils_Prisoner.FreeSlave( SlaveInd, CollarInd );
		}

	}

}
