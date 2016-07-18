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
    
    public class JobDriver_Warden_ArrestPawn : JobDriver
    {

        private const TargetIndex PrisonerInd = TargetIndex.A;
        private const TargetIndex DoorsInd = TargetIndex.B;
        private const TargetIndex BedInd = TargetIndex.C;

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

        private Building_Bed Bed
        {
            get
            {
                return this.TargetThing( BedInd ) as Building_Bed;
            }
        }



        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull( PrisonerInd );
            this.FailOnDespawnedNullOrForbidden( BedInd );
            this.FailOnBurningImmobile( PrisonerInd );
            this.FailOnBurningImmobile( BedInd );
            this.globalFinishActions.Add( () =>
            {
                if( Find.Map.reservationManager.ReservedBy( Prisoner, this.pawn ) )
                {
                    Find.Map.reservationManager.Release( Prisoner, this.pawn );
                }
                if( Find.Map.reservationManager.ReservedBy( Bed, this.pawn ) )
                {
                    Find.Map.reservationManager.Release( Bed, this.pawn );
                }
            } );
            yield return Toils_Reserve.Reserve( PrisonerInd, 1 );
            yield return Toils_Reserve.Reserve( BedInd, 1 );
            yield return Toils_Goto.GotoThing( PrisonerInd, PathEndMode.ClosestTouch )
                                   .FailOn( () => !Prisoner.CanBeArrested() )
                                   .FailOn( () => !this.pawn.CanReach( Prisoner, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn ) )
                                   .FailOn( () => !this.pawn.CanReach( Bed, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn ) )
                                   .FailOnAggroMentalState( PrisonerInd );
            yield return Toils_Prisoner.CheckArrestable( PrisonerInd, BedInd );
            yield return Toils_Prisoner.ArrestPawn( PrisonerInd, BedInd );
            yield return Toils_Haul.StartCarryThing( PrisonerInd );
            yield return Toils_Bed.ClaimBedIfNonMedical( BedInd, PrisonerInd );
            yield return Toils_Goto.GotoThing( BedInd, PathEndMode.ClosestTouch );
            yield return Toils_Reserve.Release( BedInd );
            yield return Toils_General.TuckPawnIntoBed( PrisonerInd, BedInd );

            var releasePrisonerToil = Toils_Reserve.Release( PrisonerInd );
            releasePrisonerToil.AddEndCondition( CheckForDoorQueue );
            yield return releasePrisonerToil;
            
            var extractToil = Toils_JobTransforms.ExtractNextTargetFromQueue( DoorsInd );
            var gotoDoorToil = Toils_Goto.GotoThing( DoorsInd, PathEndMode.Touch );
            var doorLockToil = Toils_General.DoorLock( DoorsInd );
            var doorPreToil = Toils_General.PrepareDoorInteraction( DoorsInd, doorLockToil );
            var jumpToil = Toils_Jump.JumpIf( extractToil, () =>
            {
                return( !IsQueueEmpty( DoorsInd ) );
            } );

            yield return extractToil;
            yield return gotoDoorToil;
            yield return doorPreToil;
            yield return doorLockToil;
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

