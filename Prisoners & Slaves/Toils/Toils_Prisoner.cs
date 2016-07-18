using System;
using System.Reflection;

using CommunityCoreLibrary;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PrisonersAndSlaves
{

    public static class Toils_Prisoner
    {

        public static Toil NoLongerNeedsHauling( TargetIndex PrisonerInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = new Action( () =>
            {
                try
                {
                    var prisoner = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
                    var compPrisoner = prisoner.TryGetComp<CompPrisoner>();
                    if( compPrisoner == null )
                    {
                        return;
                    }
                    compPrisoner.haulTarget = null;
                }
                catch( Exception e )
                {
                    Log.Error( "Prisoners & Slaves :: Could not clear prisoner haul flag!\n\t" + e.Message );
                }
            } );
            return toil;
        }

        public static Toil Enslave( TargetIndex PrisonerInd, TargetIndex CollarInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 250;
            toil.WithProgressBarToilDelay( PrisonerInd );
            toil.AddFinishAction( () =>
            {
                try
                {
                    var prisoner = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
                    var collar = toil.actor.CurJob.GetTarget( CollarInd ).Thing as Apparel;
                    var compPrisoner = prisoner.TryGetComp<CompPrisoner>();
                    if(
                        ( prisoner == null ) ||
                        ( collar == null ) ||
                        ( compPrisoner == null )
                    )
                    {
                        throw new Exception(
                            string.Format(
                                "prisoner = {0}\n\tcollar = {1}\n\tcompPrisoner = {2}",
                                prisoner == null ? "null" : prisoner.ThingID,
                                collar == null ? "null" : collar.ThingID,
                                compPrisoner == null ? "null" : "valid" ) );
                    }
                    if( prisoner.apparel == null )
                    {
                        throw new Exception( "Prisoner " + prisoner.Name + " is missing Pawn_ApparelTracker!" );
                    }
                    // Get the prisoners original faction and pawk kind
                    compPrisoner.originalFaction = prisoner.Faction;
                    compPrisoner.originalPawnKind = prisoner.kindDef;
                    // Assign the prisoner faction and pawn kind then update guest status
                    prisoner.SetFaction( Faction.OfPlayer );
                    prisoner.kindDef = Data.PawnKindDefOf.Slave;
                    prisoner.guest.SetGuestStatus( Faction.OfPlayer, true );
                    // Now put the collar on the prisoner
                    toil.actor.inventory.container.Remove( collar );
                    compPrisoner.ForceApparelOnPawn( prisoner, collar );
                    // Update the interaction mode
                    prisoner.guest.interactionMode = PrisonerInteractionMode.NoInteraction;
                    // Update doors
                    Data.UpdateAllDoors();
                }
                catch( Exception e )
                {
                    Log.Error( "Prisoners & Slaves :: Could not enslave prisoner!\n\t" + e.Message );
                }
            } );
            return toil;
        }

        public static Toil FreeSlave( TargetIndex PrisonerInd, TargetIndex CollarInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 250;
            toil.WithProgressBarToilDelay( PrisonerInd );
            toil.AddFinishAction( () =>
            {
                try
                {
                    var slave = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
                    var collar = toil.actor.CurJob.GetTarget( CollarInd ).Thing as Apparel;
                    var compPrisoner = slave.TryGetComp<CompPrisoner>();
                    if(
                        ( slave == null ) ||
                        ( collar == null ) ||
                        ( compPrisoner == null )
                    )
                    {
                        throw new Exception(
                            string.Format(
                                "slave = {0}\n\tcollar = {1}\n\tcompPrisoner = {2}",
                                slave == null ? "null" : slave.ThingID,
                                collar == null ? "null" : collar.ThingID,
                                compPrisoner == null ? "null" : "valid" ) );
                    }
                    compPrisoner.RemoveApparelFromPawn( slave, collar, toil.actor.Position );
                    if( slave.ownership != null )
                    {
                        slave.ownership.UnclaimAll();
                    }
                    var faction = compPrisoner.originalFaction;
                    if(
                        ( faction == null ) ||
                        ( faction == Faction.OfPlayer )
                    )
                    {   // Unknown faction or originally from the colony
                        // Pick outlander faction
                        faction = Find.FactionManager.FirstFactionOfDef( FactionDefOf.Outlander );
                        if( faction == null )
                        {   // No outlander faction, pick a random non-colony faction
                            Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction( out faction, false );
                        }
                    }
                    // Update control flags
                    compPrisoner.wasSlave = true;
                    compPrisoner.freeSlave = false;
                    slave.guest.released = true;
                    slave.guest.interactionMode = PrisonerInteractionMode.NoInteraction;
                    // Remove enslaved trait
                    slave.story.traits.RemoveTrait( Data.TraitDefOf.Enslaved );
                    if( slave.workSettings != null )
                    {
                        slave.workSettings.Notify_GainedTrait();
                    }
                    // Adjust thoughts
                    if(
                        ( slave.needs != null ) &&
                        ( slave.needs.mood != null ) &&
                        ( slave.needs.mood.thoughts != null )
                    )
                    {
                        slave.needs.mood.thoughts.memories.TryGainMemoryThought( Data.ThoughtDefOf.Freed );
                    }
                    // Restore faction
                    slave.SetFaction( faction, null );
                    // Restore PawnKindDef
                    slave.kindDef = compPrisoner.originalPawnKind;
                    // Update doors
                    Data.UpdateAllDoors();
                }
                catch( Exception e )
                {
                    Log.Error( "Prisoners & Slaves :: Could not free slave!\n\t" + e.Message );
                }
            } );
            return toil;
        }

        public static Toil IssueLeaveJob( TargetIndex PrisonerInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.AddFinishAction( () =>
            {
                try
                {
                    var slave = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
                    // Find an exit spot
                    IntVec3 spot;
                    if( !RCellFinder.TryFindBestExitSpot( slave, out spot, TraverseMode.ByPawn ) )
                    {
                        Log.Warning( string.Format( "Tried to make pawn {0} leave but couldn't find an exit spot", slave.NameStringShort ) );
                        return;
                    }
                    // Stop any other jobs
                    slave.jobs.StopAll( true );
                    var newJob = new Job( JobDefOf.Goto, spot );
                    newJob.exitMapOnArrival = true;
                    // Assign new exit job
                    slave.jobs.StartJob( newJob, JobCondition.None, (ThinkNode) null, false, true, (ThinkTreeDef) null );
                }
                catch( Exception e )
                {
                    Log.Error( "Prisoners & Slaves :: Could not issue leave job to pawn!\n\t" + e.Message );
                }
            } );
            return toil;
        }

        public static Toil Restrain( TargetIndex PrisonerInd, TargetIndex RestraintInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 250;
            toil.WithProgressBarToilDelay( PrisonerInd );
            toil.AddFinishAction( () =>
            {
                try
                {
                    var prisoner = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
                    var restraint = toil.actor.CurJob.GetTarget( RestraintInd ).Thing as Apparel;
                    var compPrisoner = prisoner.TryGetComp<CompPrisoner>();
                    if(
                        ( prisoner == null ) ||
                        ( restraint == null ) ||
                        ( compPrisoner == null )
                    )
                    {
                        throw new Exception(
                            string.Format(
                                "prisoner = {0}\n\trestraint = {1}\n\tcompPrisoner = {2}",
                                prisoner == null ? "null" : prisoner.ThingID,
                                restraint == null ? "null" : restraint.ThingID,
                                compPrisoner == null ? "null" : "valid" ) );
                    }
                    if( prisoner.apparel == null )
                    {
                        throw new Exception( "Prisoner " + prisoner.Name + " is missing Pawn_ApparelTracker!" );
                    }
                    // Put the restraint on the prisoner
                    toil.actor.inventory.container.Remove( restraint );
                    compPrisoner.ForceApparelOnPawn( prisoner, restraint );
                }
                catch( Exception e )
                {
                    Log.Error( "Prisoners & Slaves :: Could not restrain pawn!\n\t" + e.Message );
                }
            } );
            return toil;
        }

        public static Toil Unrestrain( TargetIndex PrisonerInd, TargetIndex RestraintInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 250;
            toil.WithProgressBarToilDelay( PrisonerInd );
            toil.AddFinishAction( () =>
            {
                try
                {
                    var slave = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
                    var restraint = toil.actor.CurJob.GetTarget( RestraintInd ).Thing as Apparel;
                    var compPrisoner = slave.TryGetComp<CompPrisoner>();
                    if(
                        ( slave == null ) ||
                        ( restraint == null ) ||
                        ( compPrisoner == null )
                    )
                    {
                        throw new Exception(
                            string.Format(
                                "slave = {0}\n\trestraint = {1}\n\tcompPrisoner = {2}",
                                slave == null ? "null" : slave.ThingID,
                                restraint == null ? "null" : restraint.ThingID,
                                compPrisoner == null ? "null" : "valid" ) );
                    }
                    compPrisoner.RemoveApparelFromPawn( slave, restraint, toil.actor.Position );
                }
                catch( Exception e )
                {
                    Log.Error( "Prisoners & Slaves :: Could not unrestrain pawn!\n\t" + e.Message );
                }
            } );
            return toil;
        }

        public static Toil CheckArrestable( TargetIndex PrisonerInd, TargetIndex BedInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = () =>
            {
                var prisoner = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
                var bed = toil.actor.CurJob.GetTarget( BedInd ).Thing as Building_Bed;
                if( !bed.ForPrisoners )
                {
                    return;
                }
                var lord = prisoner.GetLord();
                if( lord != null )
                {
                    lord.Notify_PawnAttemptArrested( prisoner );
                }
                GenClamor.DoClamor( prisoner, 10f, ClamorType.Harm );
                if( prisoner.CheckAcceptArrest( toil.actor ) )
                {
                    return;
                }
                Log.Message( "Toils_Prisoner.CheckArrestable returning JobCondition.Incompleteable!" );
                toil.actor.jobs.EndCurrentJob( JobCondition.Incompletable );
            };
            return toil;
        }

        public static Toil ArrestPawn( TargetIndex PrisonerInd, TargetIndex BedInd )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.initAction = () =>
            {
                var prisoner = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
                var bed = toil.actor.CurJob.GetTarget( BedInd ).Thing as Building_Bed;
                var compPrisoner = prisoner.TryGetComp<CompPrisoner>();
                if( bed.ForPrisoners )
                {
                    if( prisoner.guest.released )
                    {
                        prisoner.guest.released = false;
                        prisoner.guest.interactionMode = PrisonerInteractionMode.NoInteraction;
                    }
                    if( !prisoner.IsPrisonerOfColony )
                    {
                        if( prisoner.Faction != null )
                        {
                            prisoner.Faction.Notify_MemberCaptured( prisoner, toil.actor.Faction );
                        }
                        prisoner.guest.SetGuestStatus( Faction.OfPlayer, true );
                        if( prisoner.guest.IsPrisoner )
                        {
                            TaleRecorder.RecordTale( TaleDefOf.Captured, toil.actor, prisoner );
                            toil.actor.records.Increment( RecordDefOf.PeopleCaptured );
                        }
                    }
                }
                else if(
                    ( prisoner.Faction != Faction.OfPlayer )&&
                    ( prisoner.HostFaction != Faction.OfPlayer )&&
                    ( prisoner.guest != null )
                )
                {
                    prisoner.guest.SetGuestStatus( Faction.OfPlayer, false );
                }
                if( prisoner.playerSettings == null )
                {
                    prisoner.playerSettings = new Pawn_PlayerSettings( prisoner );
                }
                // Set that the pawn was arrested and how long it should be held for
                compPrisoner.wasArrested = true;
                compPrisoner.releaseAfterTick = Find.TickManager.TicksGame + (int)( compPrisoner.lawBroken.daysToImprisonFor * GenDate.TicksPerDay );
            };
            return toil;
        }

    }

}
