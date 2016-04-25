using RimWorld;
using Verse;

namespace PrisonImprovements
{

    public class ThoughtWorker_Enslaved : ThoughtWorker
    {

        protected override ThoughtState CurrentStateInternal( Pawn p )
        {
            if(
                ( p.kindDef != PawnKindDefOf.Slave )||
                ( !p.story.traits.HasTrait( Data.EnslavedTraitDef ) )
            )
            {
                return ThoughtState.Inactive;
            }
            return ThoughtState.ActiveDefault;
        }

    }

}
