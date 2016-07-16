using System.Collections.Generic;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    internal static class _ThoughtUtility
    {
        
        internal static void _GiveThoughtsForPawnExecuted( Pawn victim, PawnExecutionKind kind )
        {
            if( !victim.RaceProps.Humanlike )
            {
                return;
            }
            int forcedStage = 0;
            switch( kind )
            {
            case PawnExecutionKind.GenericBrutal:
                forcedStage = 1;
                break;
            case PawnExecutionKind.GenericHumane:
                forcedStage = 0;
                break;
            case PawnExecutionKind.OrganHarvesting:
                forcedStage = 2;
                break;
            }
            ThoughtDef empathicDef = null;
            ThoughtDef psychopathDef = null;
            if( victim.IsColonist )
            {
                empathicDef     = Data.ThoughtDefOf.KnowColonistExecutedEmpathic;
                psychopathDef   = Data.ThoughtDefOf.KnowColonistExecutedPsychopath;
            }
            else
            {
                empathicDef     = Data.ThoughtDefOf.KnowGuestExecutedEmpathic;
                psychopathDef   = Data.ThoughtDefOf.KnowGuestExecutedPsychopath;
            }
            foreach( var pawn in Find.MapPawns.FreeColonistsAndPrisoners )
            {
                pawn.needs.mood.thoughts.memories.TryGainMemoryThought( ThoughtMaker.MakeThought( empathicDef, forcedStage ) );
                pawn.needs.mood.thoughts.memories.TryGainMemoryThought( ThoughtMaker.MakeThought( psychopathDef, forcedStage ) );
            }
        }

        internal static void _GiveThoughtsForPawnOrganHarvested( Pawn victim )
        {
            if( !victim.RaceProps.Humanlike )
            {
                return;
            }
            ThoughtDef empathicDef = null;
            ThoughtDef psychopathDef = null;
            if( victim.IsColonist )
            {
                empathicDef     = Data.ThoughtDefOf.KnowColonistOrganHarvestedEmpathic;
                psychopathDef   = Data.ThoughtDefOf.KnowColonistOrganHarvestedPsychopath;
            }
            else if( victim.HostFaction == Faction.OfPlayer )
            {
                empathicDef     = Data.ThoughtDefOf.KnowGuestOrganHarvestedEmpathic;
                psychopathDef   = Data.ThoughtDefOf.KnowGuestOrganHarvestedPsychopath;
            }
            foreach( var pawn in Find.MapPawns.FreeColonistsAndPrisoners )
            {
                if( pawn == victim )
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemoryThought( ThoughtDefOf.MyOrganHarvested );
                }
                else
                {
                    if( empathicDef != null )
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemoryThought( empathicDef );
                    }
                    if( psychopathDef != null )
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemoryThought( psychopathDef );
                    }
                }
            }
        }

    }

}
