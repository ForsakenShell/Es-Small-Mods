using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public class ThoughtWorker_Enslaved : ThoughtWorker
    {

        protected override ThoughtState CurrentStateInternal( Pawn p )
        {
            return p.IsSlaveOfColony();
        }

    }

}
