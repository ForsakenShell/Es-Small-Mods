using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CommunityCoreLibrary;

using RimWorld;
using Verse;

namespace PrisonImprovements
{

	public class PrisonInjector : SpecialInjector
	{

		public override bool Inject()
		{
			var thingsToReplaceITabOn = DefDatabase<ThingDef>.AllDefs.Where( thingDef => (
				( thingDef.inspectorTabs != null ) &&
				( thingDef.inspectorTabs.Contains( typeof( RimWorld.ITab_Pawn_Prisoner ) ) )
			) ).ToList();

			foreach( var thingDef in thingsToReplaceITabOn )
			{
				thingDef.inspectorTabs.Remove( typeof( RimWorld.ITab_Pawn_Prisoner ) );
                thingDef.inspectorTabs.Add( typeof( ITab_Pawn_Prisoner ) );
                thingDef.inspectorTabs.Add( typeof( ITab_Pawn_Slave ) );

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
                    thingDef.inspectorTabsResolved.Add( ITabManager.GetSharedInstance( typeof( ITab_Pawn_Slave ) ) );
				}

				if( thingDef.comps == null )
				{
					thingDef.comps = new List<CompProperties>();
				}
				if( !thingDef.HasComp( typeof( CompSlave ) ) )
				{
					var compProperties = new CompProperties();
					compProperties.compClass = typeof( CompSlave );
					thingDef.comps.Add( compProperties );
				}
			}

			// Allow prisoners to have a joy need
			var RW_Pawn_NeedsTracker_ShouldHaveNeed =
				typeof( Pawn_NeedsTracker ).GetMethod( "ShouldHaveNeed", BindingFlags.Instance | BindingFlags.NonPublic );
			var PI_Pawn_NeedsTracker_ShouldHaveNeed =
				typeof( _Pawn_NeedsTracker ).GetMethod( "_ShouldHaveNeed", BindingFlags.Static | BindingFlags.NonPublic );
			if( !Detours.TryDetourFromTo( RW_Pawn_NeedsTracker_ShouldHaveNeed, PI_Pawn_NeedsTracker_ShouldHaveNeed ) )
			{
				return false;
			}

            // Disallow removing the collar
            var RW_Pawn_ApparelTracker_TryDrop = typeof( Pawn_ApparelTracker ).GetMethods().First<MethodInfo>((arg) => (
                ( arg.Name == "TryDrop" )&&
                ( arg.GetParameters().Count() == 4 )
            ) );
            var PI_Pawn_ApparelTracker_TryDrop =
                typeof( _Pawn_ApparelTracker ).GetMethod( "_TryDrop", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RW_Pawn_ApparelTracker_TryDrop, PI_Pawn_ApparelTracker_TryDrop ) )
            {
                return false;
            }

			return true;
		}

	}

}
