using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PrisonersAndSlaves
{
    
    public class JobDriver_Warden_ReleasePrisoner : JobDriver
    {

        private const TargetIndex PrisonerInd = TargetIndex.A;
        private const TargetIndex DoorsInd = TargetIndex.B;
        private const TargetIndex ReleaseCellInd = TargetIndex.C;

        private Pawn Prisoner
        {
            get
            {
                return this.TargetThing( PrisonerInd ) as Pawn;
            }
        }

        private CompPrisoner Comp
        {
            get
            {
                return Prisoner.TryGetComp<CompPrisoner>();
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull( PrisonerInd );
            this.FailOnBurningImmobile( PrisonerInd );
            this.FailOnDowned( PrisonerInd );
            this.FailOn( (Func<bool>)(() =>
            {
                if( Prisoner.IsPrisonerOfColony )
                {
                    return( Prisoner.guest.interactionMode != PrisonerInteractionMode.Release );
                }
                return( !Comp.wasArrested );
            } ) );
            this.globalFinishActions.Add( () =>
            {
                if( Find.Map.reservationManager.ReservedBy( Prisoner, this.pawn ) )
                {
                    Find.Map.reservationManager.Release( Prisoner, this.pawn );
                }
            } );

            yield return Toils_Reserve.Reserve( PrisonerInd, 1 );
            yield return Toils_Goto.GotoThing( PrisonerInd, PathEndMode.ClosestTouch )
                                   .FailOn( () => !this.pawn.CanReach( Prisoner, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn ) )
                                   .FailOnAggroMentalState( PrisonerInd );
            yield return Toils_Haul.StartCarryThing( PrisonerInd );

            var dropToil = Toils_General.DropCarriedPawnRightHere();
            yield return Toils_Jump.JumpIf( dropToil, (Func<bool>)(() =>
            {
                return( !Prisoner.IsPrisonerOfColony );
            } ) );

            yield return Toils_Goto.GotoCell( ReleaseCellInd, PathEndMode.OnCell );
            yield return dropToil;
            yield return Toils_Reserve.Release( PrisonerInd );

            var unarrestToil = Toils_Prisoner.UnArrestPawn( PrisonerInd );
            unarrestToil.AddEndCondition( CheckForDoorQueue );
            yield return unarrestToil;

            var extractToil = Toils_JobTransforms.ExtractNextTargetFromQueue( DoorsInd );
            var gotoDoorToil = Toils_Goto.GotoThing( DoorsInd, PathEndMode.Touch );
            var doorUnLockToil = Toils_General.DoorUnLock( DoorsInd );
            var doorPreToil = Toils_General.PrepareDoorInteraction( DoorsInd, doorUnLockToil );
            var jumpToil = Toils_Jump.JumpIf( extractToil, () =>
            {
                return( !IsQueueEmpty( DoorsInd ) );
            } );

            yield return extractToil;
            yield return gotoDoorToil;
            yield return doorPreToil;
            yield return doorUnLockToil;
            yield return jumpToil;
        }

        private JobCondition    CheckForDoorQueue()
        {
            if( IsQueueEmpty( DoorsInd ) )
            {
                return JobCondition.Succeeded;
            }
            return JobCondition.Ongoing;
        }

        private bool            IsQueueEmpty( TargetIndex ind )
        {
            return( this.CurJob.GetTargetQueue( ind ).NullOrEmpty() );
        }

    }

}
