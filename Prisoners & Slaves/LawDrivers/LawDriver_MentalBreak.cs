using System;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class LawDriver_MentalBreak : LawDriver
    {

        public LawDriver_MentalBreak( LawDef lawDef ) : base( lawDef )
        {
        }

        public override bool LawBroken( Pawn pawn )
        {
            return pawn.MentalState != null;
        }

    }

}
