using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public class Alert_NeedRestraints : Alert_Medium
    {

        public override string FullLabel
        {
            get
            {
                return Data.Strings.AlertNeedRestraintsLabel.Translate();
            }
        }

        public override string FullExplanation
        {
            get
            {
                return Data.Strings.AlertNeedRestraintsExplaination.Translate();
            }
        }

        public override AlertReport Report
        {
            get
            {
                var pawns = Find.MapPawns.AllPawns;
                if( pawns.Count() > 0 )
                {
                    var restraints = Data.AllRestraintsOfColony( Data.BodyPartGroupDefOf.Hands );
                    if( !restraints.NullOrEmpty() )
                    {
                        foreach( var restraint in restraints )
                        {
                            if( !restraint.IsForbidden( Faction.OfPlayer ) )
                            {
                                return AlertReport.Inactive;
                            }
                        }
                    }
                    foreach( Pawn pawn in pawns )
                    {
                        var compPrisoner = pawn.TryGetComp<CompPrisoner>();
                        if(
                            ( compPrisoner != null )&&
                            ( compPrisoner.ShouldBeCuffed )&&
                            ( pawn.WornRestraints( Data.BodyPartGroupDefOf.Hands ) == null )
                        )
                        {
                            return pawn;
                        }
                    }
                }
                return AlertReport.Inactive;
            }
        }

    }

}
