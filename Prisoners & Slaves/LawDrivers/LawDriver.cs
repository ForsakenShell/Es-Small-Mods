using System;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public abstract class LawDriver : IExposable
    {

        public float                daysToImprisonFor;
        public bool                 arrestColonists;
        public bool                 arrestGuests;
        public bool                 arrestSlaves;

        public LawDef               lawDef;

        protected                   LawDriver( LawDef lawDef )
        {
            this.lawDef             = lawDef;
            this.daysToImprisonFor  = lawDef.daysToImprisonFor;
            this.arrestColonists    = lawDef.arrestColonists;
            this.arrestGuests       = lawDef.arrestGuests;
            this.arrestSlaves       = lawDef.arrestSlaves;
        }

        public void                 ExposeData()
        {
            Scribe_Values.LookValue( ref daysToImprisonFor  , "daysToImprisonFor"   , lawDef.daysToImprisonFor  , false );
            Scribe_Values.LookValue( ref arrestColonists    , "arrestColonists"     , lawDef.arrestColonists    , false );
            Scribe_Values.LookValue( ref arrestGuests       , "arrestGuests"        , lawDef.arrestGuests       , false );
            Scribe_Values.LookValue( ref arrestSlaves       , "arrestSlaves"        , lawDef.arrestSlaves       , false );
        }

        public bool                 CanArrestFor( Pawn pawn )
        {
            if( pawn.IsSlaveOfColony() )
            {
                return arrestSlaves;
            }
            if( pawn.IsGuestOfColony() )
            {
                return arrestGuests;
            }
            if( pawn.IsColonist )
            {
                return arrestColonists;
            }
            return false;
        }

        // Workers need to implement this method:
        public abstract bool        LawBroken( Pawn pawn );

    }

}
