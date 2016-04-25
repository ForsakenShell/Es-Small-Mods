using System;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonImprovements
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
                    var compSlave = prisoner.TryGetComp<CompSlave>();
					if( compSlave == null )
					{
						return;
					}
					compSlave.haulTarget = null;
				}
				catch( Exception e )
				{
					Log.Error( "ESM - Prison Improvements :: Could not clear prisoner haul flag!\n\t" + e.Message );
				}
			} );
			return toil;
		}

		public static Toil Enslave( TargetIndex PrisonerInd, TargetIndex CollarInd )
		{
			var toil = new Toil();
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.initAction = new Action( () =>
			{
				try
				{
					var prisoner = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
					var collar = toil.actor.CurJob.GetTarget( CollarInd ).Thing as Apparel;
                    var compSlave = prisoner.TryGetComp<CompSlave>();
					if(
						( prisoner == null ) ||
						( collar == null ) ||
						( compSlave == null )
					)
					{
						throw new Exception(
							string.Format(
								"prisoner = {0}\n\tcollar = {1}\n\tcompSlaver = {2}",
								prisoner == null ? "null" : prisoner.ThingID,
								collar == null ? "null" : collar.ThingID,
								compSlave == null ? "null" : "valid" ) );
					}
                    if( prisoner.apparel == null )
                    {
                        throw new Exception( "Prisoner " + prisoner.Name + " is missing Pawn_ApparelTracker!" );
                    }
                    // Get the prisoners original faction and pawk kind
                    compSlave.originalFaction = prisoner.Faction;
                    compSlave.originalPawnKind = prisoner.kindDef;
                    // Assign the prisoner faction and pawn kind
                    prisoner.SetFaction( Faction.OfColony );
                    prisoner.kindDef = PawnKindDefOf.Slave;
                    // Now put the collar on the prisoner
					prisoner.apparel.Wear( collar, true );
                    if( prisoner.guest == null )
                    {
                        throw new Exception( "Prisoner " + prisoner.Name + " is missing Pawn_GuestTracker!" );
                    }
					prisoner.guest.interactionMode = PrisonerInteractionMode.NoInteraction;
				}
				catch( Exception e )
				{
					Log.Error( "ESM - Prison Improvements :: Could not enslave prisoner!\n\t" + e.Message );
				}
			} );
			return toil;
		}

		public static Toil FreeSlave( TargetIndex PrisonerInd, TargetIndex CollarInd )
		{
			var toil = new Toil();
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 100;
			toil.AddFinishAction( new Action( () =>
			{
				try
				{
					var slave = toil.actor.CurJob.GetTarget( PrisonerInd ).Thing as Pawn;
					var collar = toil.actor.CurJob.GetTarget( CollarInd ).Thing as Apparel;
                    var compSlave = slave.TryGetComp<CompSlave>();
					if(
						( slave == null ) ||
						( collar == null ) ||
						( compSlave == null )
					)
					{
						throw new Exception(
							string.Format(
								"slave = {0}\n\tcollar = {1}\n\tcompSlaver = {2}",
								slave == null ? "null" : slave.ThingID,
								collar == null ? "null" : collar.ThingID,
								compSlave == null ? "null" : "valid" ) );
					}
                    if( slave.outfits != null )
                    {
                        slave.outfits.forcedHandler.SetForced( collar, false );
                    }
					slave.apparel.wornApparel().Remove( collar );
					collar.wearer = (Pawn) null;
					Thing resultingThing = (Thing) null;
					bool flag = GenThing.TryDropAndSetForbidden( (Thing) collar, slave.Position, ThingPlaceMode.Near, out resultingThing, false );
					slave.Drawer.renderer.graphics.ResolveApparelGraphics();
					if( slave.ownership != null )
					{
						slave.ownership.UnclaimAll();
					}
					var faction = compSlave.originalFaction;
					if(
						( faction == null )||
						( faction == Faction.OfColony )
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
                    compSlave.wasSlave = true;
                    compSlave.freeSlave = false;
                    slave.guest.released = true;
                    slave.guest.interactionMode = PrisonerInteractionMode.NoInteraction;
					// Remove enslaved trait
					slave.RemoveTrait( Data.EnslavedTraitDef );
                    // Adjust thoughts
                    if(
                        ( slave.needs != null )&&
                        ( slave.needs.mood != null )&&
                        ( slave.needs.mood.thoughts != null )
                    )
                    {
                        slave.needs.mood.thoughts.RemoveThoughtsOfDef( Data.EnslavedThoughtDef );
                        slave.needs.mood.thoughts.TryGainThought( Data.FreedThoughtDef );
                    }
                    // Restore faction
                    slave.SetFaction( faction, null );
                    // Restore PawnKindDef
                    slave.kindDef = compSlave.originalPawnKind;
					// Find an exit spot
					IntVec3 spot;
					if( !ExitUtility.TryFindClosestExitSpot( slave, out spot, TraverseMode.ByPawn ) )
					{
						Log.Warning( "Tried to make slave " + slave.Name + " leave but couldn't find an exit spot" );
						return;
					}
					// Stop any other jobs
					slave.jobs.StopAll( true );
					Job newJob = new Job( JobDefOf.Goto, spot );
					newJob.exitMapOnArrival = true;
					// Assign new exit job
					slave.jobs.StartJob( newJob, JobCondition.None, (ThinkNode) null, false, true, (ThinkTreeDef) null );
				}
				catch( Exception e )
				{

					Log.Error( "ESM - Prison Improvements :: Could not free slave!\n\t" + e.Message );
				}
			} ) );
			return toil;
		}

	}

}
