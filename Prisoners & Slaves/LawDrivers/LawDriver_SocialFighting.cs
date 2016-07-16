using System;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class LawDriver_SocialFighting : LawDriver
    {

        public LawDriver_SocialFighting( LawDef lawDef ) : base( lawDef )
        {
        }

        public override bool LawBroken( Pawn pawn )
        {
            if( !pawn.InMentalState )
            {   // Other pawn isn't in a mental state
                return false;
            }
            var socialFight = pawn.MentalState as MentalState_SocialFight;
            if( socialFight == null )
            {   // Mental state isn't a social fight
                return false;
            }
            // Return pawn is instigator
            return socialFight.Instigator == pawn;
        }

    }

}
