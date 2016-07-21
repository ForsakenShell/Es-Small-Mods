using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CommunityCoreLibrary;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

	public class MainInjector : SpecialInjector
	{

        private void InjectOverridesIntoPawns()
        {
            var thingDefsToInjectInto = DefDatabase<ThingDef>.AllDefs.Where( thingDef => (
                ( thingDef.inspectorTabs != null ) &&
                ( thingDef.inspectorTabs.Contains( typeof( RimWorld.ITab_Pawn_Prisoner ) ) )
            ) ).ToList();

            foreach( var thingDef in thingDefsToInjectInto )
            {
                thingDef.inspectorTabs.Remove( typeof( RimWorld.ITab_Pawn_Prisoner ) );
                thingDef.inspectorTabs.Add( typeof( ITab_Pawn_Prisoner ) );

                if( thingDef.inspectorTabsResolved != null )
                {
                    foreach( var iTab in thingDef.inspectorTabsResolved )
                    {
                        if( iTab is RimWorld.ITab_Pawn_Prisoner )
                        {
                            thingDef.inspectorTabsResolved.Remove( iTab );
                            break;
                        }
                    }
                    thingDef.inspectorTabsResolved.Add( ITabManager.GetSharedInstance( typeof( ITab_Pawn_Prisoner ) ) );
                }

                if( thingDef.comps == null )
                {
                    thingDef.comps = new List<CompProperties>();
                }
                if( !thingDef.HasComp( typeof( CompPrisoner ) ) )
                {
                    var compProperties = new CompProperties();
                    compProperties.compClass = typeof( CompPrisoner );
                    thingDef.comps.Add( compProperties );
                }
            }
        }

        private void InjectOverridesIntoDoors()
        {
            var thingDefsToInjectInfo = DefDatabase<ThingDef>.AllDefs.Where( thingDef => (
                ( thingDef.thingClass == typeof( Building_Door ) )
            ) ).ToList();

            foreach( var thingDef in thingDefsToInjectInfo )
            {
                thingDef.thingClass = typeof( Building_RestrictedDoor );
                if( thingDef.inspectorTabs == null )
                {
                    thingDef.inspectorTabs = new List<Type>();
                }
                if( thingDef.inspectorTabsResolved == null )
                {
                    thingDef.inspectorTabsResolved = new List<ITab>();
                }
                if( thingDef.comps == null )
                {
                    thingDef.comps = new List<CompProperties>();
                }

                if( !thingDef.inspectorTabs.Contains( typeof( ITab_Restricted ) ) )
                {
                    thingDef.inspectorTabs.Add( typeof( ITab_Restricted ) );
                    thingDef.inspectorTabsResolved.Add( ITabManager.GetSharedInstance( typeof( ITab_Restricted ) ) );
                }

                thingDef.comps.Add( new CompProperties_RestrictedDoor() );
                thingDef.comps.Add( new CompProperties_Ownable() );
                thingDef.comps.Add( new CompProperties_Lockable() );

                thingDef.drawGUIOverlay = true;
            }
        }

		public override bool Inject()
		{
            // Replace ITab and add ThingComp to pawns
            InjectOverridesIntoPawns();

            // Replace thingClass and add ITab and ThingComp to doors
            InjectOverridesIntoDoors();

            // Change social fighting driver
            MentalStateDefOf.SocialFighting.stateClass = typeof( MentalState_SocialFight );

            // Make rooms Building_RoomMarker aware
            var RW_Room_RoomChanged =
                typeof( Room ).GetMethod( "RoomChanged", BindingFlags.Instance | BindingFlags.Public );
            var PaS_Room_RoomChanged =
                typeof( _Room ).GetMethod( "_RoomChanged", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_Room_RoomChanged, PaS_Room_RoomChanged ) )
            {
                return false;
            }

            // Make rooms Building_RoomMarker aware
            var RW_Room_DrawFieldEdges =
                typeof( Room ).GetMethod( "DrawFieldEdges", BindingFlags.Instance | BindingFlags.Public );
            var PaS_Room_DrawFieldEdges =
                typeof( _Room ).GetMethod( "_DrawFieldEdges", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_Room_DrawFieldEdges, PaS_Room_DrawFieldEdges ) )
            {
                return false;
            }

            // Make reachability cache clearing tell Building_RestrictedDoor to clear its cache
            var RW_Reachability_ClearCache =
                typeof( Reachability ).GetMethod( "ClearCache", BindingFlags.Static | BindingFlags.Public );
            var PaS_Reachability_ClearCache =
                typeof( _Reachability ).GetMethod( "_ClearCache", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_Reachability_ClearCache, PaS_Reachability_ClearCache ) )
            {
                return false;
            }

			// Allow prisoners to have a joy need
			var RW_Pawn_NeedsTracker_ShouldHaveNeed =
				typeof( Pawn_NeedsTracker ).GetMethod( "ShouldHaveNeed", BindingFlags.Instance | BindingFlags.NonPublic );
			var PaS_Pawn_NeedsTracker_ShouldHaveNeed =
				typeof( _Pawn_NeedsTracker ).GetMethod( "_ShouldHaveNeed", BindingFlags.Instance | BindingFlags.NonPublic );
			if( !Detours.TryDetourFromTo( RW_Pawn_NeedsTracker_ShouldHaveNeed, PaS_Pawn_NeedsTracker_ShouldHaveNeed ) )
			{
				return false;
			}

			// Disallow removing the collar
            var RW_Pawn_ApparelTracker_TryDrop = typeof( Pawn_ApparelTracker ).GetMethods().First<MethodInfo>( ( methodInfo ) => (
				 ( methodInfo.Name == "TryDrop" )&&
				 ( methodInfo.GetParameters().Count() == 4 )
			 ) );
			var PaS_Pawn_ApparelTracker_TryDrop =
				typeof( _Pawn_ApparelTracker ).GetMethod( "_TryDrop", BindingFlags.Instance | BindingFlags.NonPublic );
			if( !Detours.TryDetourFromTo( RW_Pawn_ApparelTracker_TryDrop, PaS_Pawn_ApparelTracker_TryDrop ) )
			{
				return false;
			}

            // Change display of restraints on pawns
            var RW_Pawn_GetInspectString =
                typeof( Pawn ).GetMethod( "GetInspectString", BindingFlags.Instance | BindingFlags.Public );
            var PaS_Pawn_GetInspectString =
                typeof( _Pawn ).GetMethod( "_GetInspectString", BindingFlags.Instance | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_Pawn_GetInspectString, PaS_Pawn_GetInspectString ) )
            {
                return false;
            }

            // Change adding slavery memories to pawns
            var RW_Tradeable_Pawn_ResolveTrade =
                typeof( Tradeable_Pawn ).GetMethod( "ResolveTrade", BindingFlags.Instance | BindingFlags.Public );
            var PaS_Tradeable_Pawn_ResolveTrade =
                typeof( _Tradeable_Pawn ).GetMethod( "_ResolveTrade", BindingFlags.Instance | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_Tradeable_Pawn_ResolveTrade, PaS_Tradeable_Pawn_ResolveTrade ) )
            {
                return false;
            }

            // Change adding execution memories to pawns
            var RW_ThoughtUtility_GiveThoughtsForPawnExecuted =
                typeof( ThoughtUtility ).GetMethod( "GiveThoughtsForPawnExecuted", BindingFlags.Static | BindingFlags.Public );
            var PaS_ThoughtUtility_GiveThoughtsForPawnExecuted =
                typeof( _ThoughtUtility ).GetMethod( "_GiveThoughtsForPawnExecuted", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_ThoughtUtility_GiveThoughtsForPawnExecuted, PaS_ThoughtUtility_GiveThoughtsForPawnExecuted ) )
            {
                return false;
            }

            // Change adding organ harvesting memories to pawns
            var RW_ThoughtUtility_GiveThoughtsForPawnOrganHarvested =
                typeof( ThoughtUtility ).GetMethod( "GiveThoughtsForPawnOrganHarvested", BindingFlags.Static | BindingFlags.Public );
            var PaS_ThoughtUtility_GiveThoughtsForPawnOrganHarvested =
                typeof( _ThoughtUtility ).GetMethod( "_GiveThoughtsForPawnOrganHarvested", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_ThoughtUtility_GiveThoughtsForPawnOrganHarvested, PaS_ThoughtUtility_GiveThoughtsForPawnOrganHarvested ) )
            {
                return false;
            }

            // Change starting social fight so instigator is known
            var RW_Pawn_InteractionsTracker_StartSocialFight =
                typeof( Pawn_InteractionsTracker ).GetMethod( "StartSocialFight", BindingFlags.Instance | BindingFlags.Public );
            var PaS_Pawn_InteractionsTracker_StartSocialFight =
                typeof( _Pawn_InteractionsTracker ).GetMethod( "_StartSocialFight", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_Pawn_InteractionsTracker_StartSocialFight, PaS_Pawn_InteractionsTracker_StartSocialFight ) )
            {
                return false;
            }

            // Change warden ShouldTakeCareOfPrisoner to be compPrisoner aware
            var RW_WorkGiver_Warden_ShouldTakeCareOfPrisoner =
                typeof( WorkGiver_Warden ).GetMethod( "ShouldTakeCareOfPrisoner", BindingFlags.Instance | BindingFlags.NonPublic );
            var PaS_WorkGiver_Warden_ShouldTakeCareOfPrisoner =
                typeof( _WorkGiver_Warden ).GetMethod( "_ShouldTakeCareOfPrisoner", BindingFlags.Instance | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_WorkGiver_Warden_ShouldTakeCareOfPrisoner, PaS_WorkGiver_Warden_ShouldTakeCareOfPrisoner ) )
            {
                return false;
            }

            // Change warden feed utility to be compPrisoner aware
            var RW_WardenFeedUtility_ShouldBeFed =
                typeof( WardenFeedUtility ).GetMethod( "ShouldBeFed", BindingFlags.Static | BindingFlags.Public );
            var PaS_WardenFeedUtility_ShouldBeFed =
                typeof( _WardenFeedUtility ).GetMethod( "_ShouldBeFed", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_WardenFeedUtility_ShouldBeFed, PaS_WardenFeedUtility_ShouldBeFed ) )
            {
                return false;
            }

			return true;
		}

	}

}
