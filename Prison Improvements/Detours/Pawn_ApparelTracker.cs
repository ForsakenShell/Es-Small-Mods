using System;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonImprovements
{

	internal static class _Pawn_ApparelTracker
	{

		internal static FieldInfo _wornApparel;

		internal static List<Apparel> wornApparel( this Pawn_ApparelTracker obj )
		{
			if( _wornApparel == null )
			{
				_wornApparel = typeof( Pawn_ApparelTracker ).GetField( "wornApparel", BindingFlags.Instance | BindingFlags.NonPublic );
			}
			return (List<Apparel>) _wornApparel.GetValue( obj );
		}

		internal static bool _TryDrop( this Pawn_ApparelTracker obj, Apparel ap, out Apparel resultingAp, IntVec3 pos, bool forbid = true )
		{
			var wornApparel = obj.wornApparel();
			if( !wornApparel.Contains( ap ) )
			{
				Verse.Log.Warning( obj.pawn.LabelCap + " tried to drop apparel he didn't have: " + ap.LabelCap );
				resultingAp = (Apparel) null;
				return false;
			}
            if(
                ( ap is Apparel_SlaveCollar )&&
                ( !ap.wearer.health.Dead )
            )
			{
				// Can only take off slave collars of the dead
                Verse.Log.Message( obj.pawn.LabelCap + " tried to remove slave collar but they aren't dead." );
				resultingAp = (Apparel) null;
				return false;
			}
			wornApparel.Remove( ap );
			ap.wearer = (Pawn) null;
			Thing resultingThing = (Thing) null;
			bool flag = GenThing.TryDropAndSetForbidden( (Thing) ap, pos, ThingPlaceMode.Near, out resultingThing, forbid );
			resultingAp = resultingThing as Apparel;
			obj.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
			if(
				( flag ) &&
				( obj.pawn.outfits != null )
			)
			{
				obj.pawn.outfits.forcedHandler.SetForced( ap, false );
			}
			return flag;
		}

	}

}