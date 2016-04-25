using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace PrisonImprovements
{

	public class Apparel_SlaveCollar : Apparel
	{

		public override void Tick()
		{
			base.Tick();
			Enforce();
		}

		public override void TickRare()
		{
			base.TickRare();
			Enforce();
		}

		public void Enforce()
		{
			var slave = this.wearer;
			if( slave == null )
			{
				return;
			}
            var compSlave = slave.TryGetComp<CompSlave>();
            if( compSlave == null )
            {
                Log.Error( "Slave " + slave.Name + " is missing CompSlave!" );
                return;
            }
            if( compSlave.originalFaction == null )
            {
                if( slave.Faction == Faction.OfColony )
                {
                    compSlave.originalFaction = Find.FactionManager.FirstFactionOfDef( FactionDefOf.Spacer );
                }
                else
                {
                    compSlave.originalFaction = slave.Faction;
                }
            }
            if( compSlave.originalPawnKind == null )
            {
                if(
                    ( slave.kindDef == PawnKindDefOf.Colonist )||
                    ( slave.kindDef == PawnKindDefOf.Slave )
                )
                {
                    var kindsForFaction = DefDatabase<PawnKindDef>.AllDefsListForReading.Where( def => (
                        ( def.defaultFactionType == slave.Faction.def )&&
                        ( def.backstoryCategory == slave.Faction.def.backstoryCategory )
                    ) ).ToList();
                    compSlave.originalPawnKind = kindsForFaction.RandomElement();
                }
                else
                {
                    compSlave.originalPawnKind = slave.kindDef;
                }
            }
            slave.kindDef = PawnKindDefOf.Slave;
            if( slave.guest == null )
			{
				Log.Warning( "Slave " + slave.Name + " is missing Pawn_GuestTracker!" );
				slave.guest = new Pawn_GuestTracker( this.wearer );
				slave.guest.Init();
			}
            if( slave.outfits == null )
            {
                Log.Warning( "Slave " + slave.Name + " is missing Pawn_OutfitTracker!" );
            }
            else if( slave.outfits.forcedHandler == null )
            {
                Log.Warning( "Slave " + slave.Name + " is missing OutfitForcedHandler!" );
            }
            else
            {
			    slave.outfits.forcedHandler.SetForced( this, true );
            }
            if( slave.story == null )
            {
                Log.Warning( "Slave " + slave.Name + " is missing Pawn_StoryTracker!" );
            }
            else if( slave.story.traits == null )
            {
                Log.Warning( "Slave " + slave.Name + " is missing TraitSet!" );
            }
            else if( !slave.story.traits.HasTrait( Data.EnslavedTraitDef ) )
			{
				slave.story.traits.GainTrait( new Trait( Data.EnslavedTraitDef ) );
			}
            if( slave.needs == null )
            {
                Log.Warning( "Slave " + slave.Name + " is missing Pawn_NeedsTracker!" );
            }
            else if( slave.needs.mood == null )
            {
                Log.Warning( "Slave " + slave.Name + " is missing Need_Mood!" );
            }
            else if( slave.needs.mood.thoughts == null )
            {
                Log.Warning( "Slave " + slave.Name + " is missing ThoughtHandler!" );
            }
            else
            {
                slave.needs.mood.thoughts.TryGainThought( Data.EnslavedThoughtDef );
            }
		}

	}

}
