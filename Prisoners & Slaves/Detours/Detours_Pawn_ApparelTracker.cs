using System.Reflection;

using CommunityCoreLibrary;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    internal class _Pawn_ApparelTracker : Pawn_ApparelTracker
	{

        internal static MethodInfo      _ApparelChanged;

        private void ApparelChanged()
        {
            if( _ApparelChanged == null )
            {
                _ApparelChanged = typeof( Pawn_ApparelTracker ).GetMethod( "ApparelChanged", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            _ApparelChanged.Invoke( this, null );
        }

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
            var compLock = ap.TryGetComp<CompLockable>();
            if( compLock != null )
            {
                if(
                    ( !ap.wearer.health.Dead )&&
                    ( compLock.Locked )
                )
                {
    				// Can only take off slave collars of the dead
                    Verse.Log.Message( string.Format( "{0} tried to remove apparel {1} with CompLockable which is locked but they aren't dead.", this.pawn.LabelShort, ap.Label ) );
    				return false;
                }
            }
			wornApparel.Remove( ap );
			ap.wearer = (Pawn) null;
			Thing resultingThing = (Thing) null;
			bool flag = GenThing.TryDropAndSetForbidden( (Thing) ap, pos, ThingPlaceMode.Near, out resultingThing, forbid );
			resultingAp = resultingThing as Apparel;
            ApparelChanged();
			if(
				( flag )&&
				( this.pawn.outfits != null )
			)
			{
				this.pawn.outfits.forcedHandler.SetForced( ap, false );
			}
			return flag;
		}

	}

}