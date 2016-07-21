using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace PrisonersAndSlaves
{
    
    internal class _WorkGiver_Warden : WorkGiver_Warden
    {
        
        internal bool _ShouldTakeCareOfPrisoner( Pawn warden, Thing thing )
        {
            var compPrisoner = warden.TryGetComp<CompPrisoner>();
            if(
                ( compPrisoner != null )&&
                ( compPrisoner.wasArrested )
            )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - warden has been arrested themselves!", warden.LabelShort ) );
                return false;
            }
            var prisoner = thing as Pawn;
            if( prisoner == null )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - is not a Pawn", thing.ThingID ) );
                return false;
            }
            if( prisoner.InAggroMentalState )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - InAggroMentalState", prisoner.LabelShort ) );
                return false;
            }
            if( warden.IsForbidden( prisoner ) )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - IsForbidden( {1} )", prisoner.LabelShort, warden.LabelShort ) );
                return false;
            }
            if( !warden.CanReserveAndReach( prisoner, PathEndMode.OnCell, warden.NormalMaxDanger(), 1 ) )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - !CanReserveAndReach( {1} )", prisoner.LabelShort, warden.LabelShort ) );
                return false;
            }
            if( prisoner.Downed )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - Downed", prisoner.LabelShort ) );
                return false;
            }
            if( !prisoner.Awake() )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - !Awake", prisoner.LabelShort ) );
                return false;
            }
            if( prisoner.holder != null )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - holder != null", prisoner.LabelShort ) );
                return false;
            }
            if(
                ( prisoner.IsPrisonerOfColony )&&
                ( prisoner.guest.PrisonerIsSecure )
            )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - PrisonerIsSecure", prisoner.LabelShort ) );
                return true;
            }
            compPrisoner = prisoner.TryGetComp<CompPrisoner>();
            if(
                ( compPrisoner != null )&&
                ( compPrisoner.wasArrested )
            )
            {
                //Log.Message( string.Format( "\t_ShouldTakeCareOfPrisoner - {0} - wasArrested", prisoner.LabelShort ) );
                return true;
            }
            return false;
        }

    }

}
