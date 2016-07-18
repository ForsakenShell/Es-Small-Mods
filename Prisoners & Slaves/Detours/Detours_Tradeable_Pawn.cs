using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    internal class _Tradeable_Pawn : Tradeable_Pawn
    {

        internal void _ResolveTrade()
        {
            if( this.ActionToDo == TradeAction.PlayerSells )
            {
                List<Pawn> list = Enumerable.Cast<Pawn>( ( (IEnumerable<Thing>) this.thingsColony ).Take<Thing>( -this.countToDrop ) ).ToList<Pawn>();
                for( int index = 0; index < list.Count; ++index )
                {
                    Pawn pawn1 = list[ index ];
                    pawn1.PreTraded( TradeAction.PlayerSells, TradeSession.playerNegotiator, TradeSession.trader );
                    TradeSession.trader.AddToStock( (Thing) pawn1 );
                    if( pawn1.RaceProps.Humanlike )
                    {
                        foreach( Pawn pawn2 in Find.MapPawns.FreeColonistsAndPrisoners )
                        {   // Add memories to empathic and psychopaths
                            pawn2.needs.mood.thoughts.memories.TryGainMemoryThought( Data.ThoughtDefOf.KnowPrisonerSoldEmpathic );
                            pawn2.needs.mood.thoughts.memories.TryGainMemoryThought( Data.ThoughtDefOf.KnowPrisonerSoldPsychopath );
                        }
                    }
                }
            }
            else if( this.ActionToDo == TradeAction.PlayerBuys )
            {
                List<Pawn> list = ( (IEnumerable) Enumerable.Take<Thing>( this.thingsTrader, this.countToDrop ) ).Cast<Pawn>().ToList<Pawn>();
                for( int index = 0; index < list.Count; ++index )
                {
                    Pawn pawn = list[ index ];
                    TradeSession.trader.GiveSoldThingToBuyer( pawn, pawn );
                    pawn.PreTraded( TradeAction.PlayerBuys, TradeSession.playerNegotiator, TradeSession.trader );
                }
            }
        }

    }

}
