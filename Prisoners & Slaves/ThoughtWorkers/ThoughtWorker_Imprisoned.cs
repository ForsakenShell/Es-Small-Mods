﻿using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public class ThoughtWorker_Imprisoned : ThoughtWorker
    {

        protected override ThoughtState CurrentStateInternal( Pawn p )
        {
            return(
                ( p.IsPrisonerOfColony )&&
                ( !p.IsSlaveOfColony() )
            );
        }

    }

}
