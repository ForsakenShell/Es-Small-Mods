using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class CompPrisoner : ThingComp
    {

        // For wardens to put restraints on prisoners and slaves
        public bool                     ShouldBeCuffed;
        public bool                     ShouldBeShackled;

        // For wardens to haul to a marker
        public Thing                    haulTarget = null;

        // For wardens to free slaves
        public Faction                  originalFaction = null;
        public PawnKindDef              originalPawnKind = null;
        public bool                     freeSlave = false;
        public bool                     wasSlave = false;

        #region Properties

        public bool ShouldBeTransfered
        {
            get
            {
                // Does it have a haul target and is the prisoner not in the same room?
                return(
                    ( haulTarget != null )&&
                    ( this.parent.Position.GetRoom() != haulTarget.GetRoom() )
                );
            }
        }

        #endregion

        public override void CompTickRare()
        {
            base.CompTickRare();
            CheckStatus();
        }

        public override void CompTick()
        {
            base.CompTick();
            CheckStatus();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue( ref ShouldBeCuffed, "ShouldBeCuffed", false, false );
            Scribe_Values.LookValue( ref ShouldBeShackled, "ShouldBeShackled", false, false );
            Scribe_Values.LookValue( ref this.freeSlave, "freeSlave", false, false );
            Scribe_Values.LookValue( ref this.wasSlave, "wasSlave", false, false );
            Scribe_References.LookReference( ref this.haulTarget, "haulTarget" );
            Scribe_References.LookReference( ref this.originalFaction, "originalFaction" );
            Scribe_Defs.LookDef( ref this.originalPawnKind, "originalPawnKind" );
        }

        public void CheckStatus()
        {
            var prisoner = this.parent as Pawn;
            if( prisoner == null )
            {
                return;
            }
            // Check hand cuffs
            var handCuffs = prisoner.WornRestraints( Data.BodyPartGroupDefOf.Hands );
            if( handCuffs != null )
            {   // Enforce cuffs
                EnforceRestraints( prisoner, handCuffs );
            }
            else
            {   // Make sure hediffs are removed
                RemoveHediffDefOn( prisoner, Data.HediffDefOf.HandCuffed, prisoner.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.LeftHand ) );
                RemoveHediffDefOn( prisoner, Data.HediffDefOf.HandCuffed, prisoner.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.RightHand ) );
            }
            // Check leg shackles
            var legShackles = prisoner.WornRestraints( BodyPartGroupDefOf.Legs );
            if( legShackles != null )
            {   // Enforce shackles
                EnforceRestraints( prisoner, legShackles );
            }
            else
            {   // Make sure hediffs are removed
                RemoveHediffDefOn( prisoner, Data.HediffDefOf.LegShackled, prisoner.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.LeftLeg ) );
                RemoveHediffDefOn( prisoner, Data.HediffDefOf.LegShackled, prisoner.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.RightLeg ) );
            }
            // Check collars
            var collar = prisoner.WornCollar();
            if( collar != null )
            {
                EnforceCollar( prisoner, collar );
            }
        }

        public void EnforceApparel( Pawn pawn, Apparel apparel )
        {
            if(
                ( pawn.outfits != null )&&
                ( pawn.outfits.forcedHandler != null )
            )
            {
                pawn.outfits.forcedHandler.SetForced( apparel, true );
            }
            pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
        }

        public void EnforceHediffDefOn( Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPartRecord )
        {
            if(
                ( pawn == null )||
                ( hediffDef == null )||
                ( bodyPartRecord == null )
            )
            {
                return;
            }
            if( pawn.health.hediffSet.HasHediff( hediffDef, bodyPartRecord ) )
            {
                return;
            }
            pawn.health.AddHediff( hediffDef, bodyPartRecord );
        }

        public void EnforceRestraints( Pawn pawn, Apparel restraints )
        {
            EnforceApparel( pawn, restraints );
            foreach( var bodyPartGroup in restraints.def.apparel.bodyPartGroups )
            {
                if( bodyPartGroup == Data.BodyPartGroupDefOf.Hands )
                {
                    EnforceHediffDefOn( pawn, Data.HediffDefOf.HandCuffed, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.LeftHand ) );
                    EnforceHediffDefOn( pawn, Data.HediffDefOf.HandCuffed, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.RightHand ) );
                }
                if( bodyPartGroup == BodyPartGroupDefOf.Legs )
                {
                    EnforceHediffDefOn( pawn, Data.HediffDefOf.LegShackled, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.LeftLeg ) );
                    EnforceHediffDefOn( pawn, Data.HediffDefOf.LegShackled, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.RightLeg ) );
                }
            }
        }

        public void EnforceCollar( Pawn pawn, Apparel collar )
        {
            EnforceApparel( pawn, collar );
            if( originalFaction == null )
            {
                if( pawn.Faction == Faction.OfPlayer )
                {
                    originalFaction = Find.FactionManager.FirstFactionOfDef( FactionDefOf.Spacer );
                }
                else
                {
                    originalFaction = pawn.Faction;
                }
            }
            if( originalPawnKind == null )
            {
                originalPawnKind = pawn.kindDef;
            }
            pawn.kindDef = Data.PawnKindDefOf.Slave;
            if(
                ( pawn.story != null )&&
                ( pawn.story.traits != null )&&
                ( !pawn.story.traits.HasTrait( Data.TraitDefOf.Enslaved ) )
            )
            {
                pawn.story.traits.GainTrait( new Trait( Data.TraitDefOf.Enslaved ) );
            }
            if(
                ( pawn.needs != null )&&
                ( pawn.needs.mood != null )&&
                ( pawn.needs.mood.thoughts != null )
            )
            {
                pawn.needs.mood.thoughts.memories.TryGainMemoryThought( Data.ThoughtDefOf.Enslaved );
            }
        }

        public void RemoveApparel( Pawn pawn, Apparel apparel, IntVec3 dropCell )
        {
            if( pawn.outfits != null )
            {
                pawn.outfits.forcedHandler.SetForced( apparel, false );
            }
            pawn.apparel.wornApparel().Remove( apparel );
            apparel.wearer = null;
            Thing resultingThing = null;
            bool flag = GenThing.TryDropAndSetForbidden( (Thing) apparel, dropCell, ThingPlaceMode.Near, out resultingThing, false );
            pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
        }

        public void RemoveRestraints( Pawn pawn, Apparel restraints, IntVec3 dropCell )
        {
            RemoveApparel( pawn, restraints, dropCell );
            foreach( var bodyPartGroup in restraints.def.apparel.bodyPartGroups )
            {
                if( bodyPartGroup == Data.BodyPartGroupDefOf.Hands )
                {
                    RemoveHediffDefOn( pawn, Data.HediffDefOf.HandCuffed, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.LeftHand ) );
                    RemoveHediffDefOn( pawn, Data.HediffDefOf.HandCuffed, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.RightHand ) );
                }
                if( bodyPartGroup == BodyPartGroupDefOf.Legs )
                {
                    RemoveHediffDefOn( pawn, Data.HediffDefOf.LegShackled, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.LeftLeg ) );
                    RemoveHediffDefOn( pawn, Data.HediffDefOf.LegShackled, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.RightLeg ) );
                }
            }
        }

        public void RemoveSlaveCollar( Pawn pawn, Apparel collar, IntVec3 dropCell )
        {
            RemoveApparel( pawn, collar, dropCell );
        }

        public void RemoveHediffDefOn( Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPartRecord )
        {
            if(
                ( pawn == null )||
                ( hediffDef == null )||
                ( bodyPartRecord == null )
            )
            {
                return;
            }
            var hediff = pawn.health.hediffSet.hediffs.FirstOrDefault( diff => (
                ( diff.Part == bodyPartRecord )&&
                ( diff.def == hediffDef )
            ) );
            if( hediff == null )
            {
                return;
            }
            pawn.health.hediffSet.hediffs.Remove( hediff );
        }

    }

}
