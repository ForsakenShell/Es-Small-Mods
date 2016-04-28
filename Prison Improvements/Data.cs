using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace PrisonImprovements
{

	internal static class Data
	{

        #region Constants

		public static JobDef EnslavePrisonerJobDef      = DefDatabase<JobDef>.GetNamed( "EnslavePrisoner", true );
		public static JobDef FreeSlaveJobDef            = DefDatabase<JobDef>.GetNamed( "FreeSlave", true );
		public static JobDef TransferPrisonerJobDef     = DefDatabase<JobDef>.GetNamed( "TransferPrisoner", true );

        public static PawnKindDef SlavePawnKindDef      = DefDatabase<PawnKindDef>.GetNamed( "Slave", true );

        // If we don't use a specific def, we can add more collar types
        //public static ThingDef SlaveCollarThingDef      = DefDatabase<ThingDef>.GetNamed( "SlaveCollar", true );

        public static ThoughtDef EnslavedThoughtDef     = DefDatabase<ThoughtDef>.GetNamed( "Enslaved", true );
        public static ThoughtDef FreedThoughtDef        = DefDatabase<ThoughtDef>.GetNamed( "FreedSlave", true );

        public static TraitDef EnslavedTraitDef         = DefDatabase<TraitDef>.GetNamed( "Enslaved", true );

		public const byte PIM_EnslavePrisoner           = 127;
		public const byte PIM_FreeSlave                 = 128;

        public const string SlaveCollarApparalTag       = "Slave";

        #endregion

        #region Static Vars

        public static bool BoughtSlavesAsColonists      = false;

        #endregion

        #region Static Methods

        public static void UpdateData()
        {
            // For some reason, this requires a restart.
            if( BoughtSlavesAsColonists )
            {
                if( SlavePawnKindDef.apparelTags.Contains( SlaveCollarApparalTag ) )
                {
                    SlavePawnKindDef.apparelTags.Remove( SlaveCollarApparalTag );
                }
                SlavePawnKindDef.apparelAllowHeadwearChance = 0;
            }
            else
            {
                if( !SlavePawnKindDef.apparelTags.Contains( SlaveCollarApparalTag ) )
                {
                    SlavePawnKindDef.apparelTags.Add( SlaveCollarApparalTag );
                }
                SlavePawnKindDef.apparelAllowHeadwearChance = 100;
            }
            SlavePawnKindDef.ResolveReferences();
            //SlaveCollarThingDef.ResolveReferences();
        }

		public static Apparel_SlaveCollar WornCollar( this Pawn pawn )
		{
			if(
				( pawn == null ) ||
				( pawn.apparel == null )
			)
			{
				return null;
			}
			var wornApparel = pawn.apparel.wornApparel();
			if( wornApparel == null )
			{
				return null;
			}
			for( int index = 0; index < wornApparel.Count; ++index )
			{
				if( wornApparel[ index ] is Apparel_SlaveCollar )
				{
					return (Apparel_SlaveCollar) wornApparel[ index ];
				}
			}
			return null;
		}

		public static Trait GetTrait( this Pawn pawn, TraitDef traitDef )
		{
			foreach( var trait in pawn.story.traits.allTraits )
			{
				if( trait.def == traitDef )
				{
					return trait;
				}
			}
			return null;
		}

		public static void RemoveTrait( this Pawn pawn, TraitDef traitDef )
		{
			var trait = pawn.GetTrait( traitDef );
			if( trait == null )
			{
				return;
			}
			pawn.story.traits.allTraits.Remove( trait );
			if( pawn.workSettings == null )
			{
				return;
			}
			pawn.workSettings.Notify_GainedTrait();
		}

        public static List<Thing> AllSlaveCollarsOfColony()
        {
            return Find.ListerThings.AllThings.Where( thing => (
                ( thing is Apparel_SlaveCollar )&&
                ( thing.Faction == Faction.OfColony )
            ) ).ToList();
        }

        #endregion

	}

}
