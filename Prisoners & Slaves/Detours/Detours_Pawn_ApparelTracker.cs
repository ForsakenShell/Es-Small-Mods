using System;

using CommunityCoreLibrary;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    internal class _Pawn_ApparelTracker : Pawn_ApparelTracker
	{

        public _Pawn_ApparelTracker( Pawn pawn ) : base( pawn )
        {   // Required for some derpy reason
        }

		internal bool _TryDrop( Apparel ap, out Apparel resultingAp, IntVec3 pos, bool forbid = true )
		{
            resultingAp = (Apparel) null;
			var wornApparel = this.wornApparel();
			if( !wornApparel.Contains( ap ) )
			{
				Verse.Log.Warning( this.pawn.LabelCap + " tried to drop apparel he didn't have: " + ap.LabelCap );
				return false;
			}
            if( ap.IsSlaveCollar() )
            {
                var compLock = ap.TryGetComp<CompLockable>();
                if( compLock != null )
                {
                    if(
                        ( !ap.wearer.health.Dead )||
                        ( compLock.Locked )
                    )
                    {
        				// Can only take off slave collars of the dead
                        Verse.Log.Message( this.pawn.LabelCap + " tried to remove slave collar but they aren't dead." );
        				return false;
                    }
                }
			}
			wornApparel.Remove( ap );
			ap.wearer = (Pawn) null;
			Thing resultingThing = (Thing) null;
			bool flag = GenThing.TryDropAndSetForbidden( (Thing) ap, pos, ThingPlaceMode.Near, out resultingThing, forbid );
			resultingAp = resultingThing as Apparel;
			this.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
			if(
				( flag ) &&
				( this.pawn.outfits != null )
			)
			{
				this.pawn.outfits.forcedHandler.SetForced( ap, false );
			}
			return flag;
		}

	}

}