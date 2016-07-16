using System;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class LawDriver_PublicDrunkeness : LawDriver
    {

        public LawDriver_PublicDrunkeness( LawDef lawDef ) : base( lawDef )
        {
        }

        public override bool LawBroken( Pawn pawn )
        {
            var hediffAlcohol = pawn.health.hediffSet.GetFirstHediffOfDef( HediffDefOf.Alcohol );
            if( hediffAlcohol == null )
            {   // Hasn't consumed alcohol recently
                return false;
            }
            var room = pawn.GetRoom();
            if( room != null )
            {
                foreach( var owner in room.Owners )
                {
                    if( owner == pawn )
                    {   // Pawn is in their own room, leave them be
                        return false;
                    }
                }
            }
            // Return pawn is drunk
            return hediffAlcohol.CurStageIndex >= 3;
        }

    }

}
