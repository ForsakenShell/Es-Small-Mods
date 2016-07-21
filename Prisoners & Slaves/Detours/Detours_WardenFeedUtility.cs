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
    
    internal class _WardenFeedUtility
    {

        internal static bool _ShouldBeFed( Pawn p )
        {
            if(
                ( !p.InBed() )||
                ( !p.health.PrefersMedicalRest )
            )
            {
                return false;
            }
            if( p.IsPrisonerOfColony )
            {
                return p.guest.ShouldBeBroughtFood;
            }
            var compPrisoner = p.TryGetComp<CompPrisoner>();
            if( compPrisoner != null )
            {
                return compPrisoner.wasArrested;
            }
            return false;
        }

    }

}
