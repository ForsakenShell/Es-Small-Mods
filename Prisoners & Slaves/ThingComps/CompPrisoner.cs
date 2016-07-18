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

        // For wardens to release from prison (LawDef)
        public LawDef                   lawBroken;
        public bool                     wasArrested;
        public int                      releaseAfterTick;

        #region Properties

        public bool ShouldBeTransfered
        {
            get
            {
                // Does it have a haul target and is the prisoner not in the same room?
                return(
                    ( haulTarget != null )&&
                    ( parent.Position.GetRoom() != haulTarget.GetRoom() )
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
            Scribe_Values.LookValue( ref freeSlave, "freeSlave", false, false );
            Scribe_Values.LookValue( ref wasSlave, "wasSlave", false, false );
            Scribe_Values.LookValue( ref wasArrested, "wasArrested", false, false );
            Scribe_Values.LookValue( ref releaseAfterTick, "releaseAfterTick", 0, false );

            // Scribe references and lists last
            Scribe_References.LookReference( ref haulTarget, "haulTarget" );
            Scribe_References.LookReference( ref originalFaction, "originalFaction" );
            Scribe_Defs.LookDef( ref originalPawnKind, "originalPawnKind" );
            Scribe_Defs.LookDef( ref lawBroken, "lawBroken" );
        }

        public void CheckStatus()
        {
            var prisoner = parent as Pawn;
            if( prisoner == null )
            {
                return;
            }
            // Check hand cuffs
            var handCuffs = prisoner.WornRestraints( Data.BodyPartGroupDefOf.Hands );
            if( handCuffs != null )
            {   // Enforce cuffs
                ApplyApparelEffects( prisoner, handCuffs );
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
                ApplyApparelEffects( prisoner, legShackles );
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
                ApplyApparelEffects( prisoner, collar );
            }
        }

        public void ForceApparelOnPawn( Pawn pawn, Apparel apparel )
        {
            pawn.apparel.Wear( apparel, true );
            // Try to lock the apparel
            var compLock = apparel.TryGetComp<CompLockable>();
            if( compLock != null )
            {
                compLock.ChangeLockState( true );
            }
            ApplyApparelEffects( pawn, apparel );
        }

        public void RemoveApparelFromPawn( Pawn pawn, Apparel apparel, IntVec3 dropCell )
        {
            // Try to unlock the apparel
            var compLock = apparel.TryGetComp<CompLockable>();
            if( compLock != null )
            {
                compLock.ChangeLockState( false );
            }
            Apparel result = null;
            pawn.apparel.TryDrop( apparel, out result, dropCell, false );
            if( apparel.IsRestraints() )
            {
                RemoveRestraintsEffects( pawn, apparel );
            }
        }

        public void ApplyApparelEffects( Pawn pawn, Apparel apparel )
        {
            if(
                ( pawn.outfits != null )&&
                ( pawn.outfits.forcedHandler != null )
            )
            {
                pawn.outfits.forcedHandler.SetForced( apparel, true );
            }
            pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
            if( apparel.IsSlaveCollar() )
            {
                ApplyCollarEffects( pawn, apparel );
            }
            if( apparel.IsRestraints() )
            {
                ApplyRestraintsEffects( pawn, apparel );
            }
        }

        public void ApplyHediffDefOn( Pawn pawn, HediffDef hediffDef, BodyPartRecord bodyPartRecord )
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

        public void ApplyRestraintsEffects( Pawn pawn, Apparel restraints )
        {
            foreach( var bodyPartGroup in restraints.def.apparel.bodyPartGroups )
            {
                if( bodyPartGroup == Data.BodyPartGroupDefOf.Hands )
                {
                    ApplyHediffDefOn( pawn, Data.HediffDefOf.HandCuffed, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.LeftHand ) );
                    ApplyHediffDefOn( pawn, Data.HediffDefOf.HandCuffed, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.RightHand ) );
                }
                if( bodyPartGroup == BodyPartGroupDefOf.Legs )
                {
                    ApplyHediffDefOn( pawn, Data.HediffDefOf.LegShackled, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.LeftLeg ) );
                    ApplyHediffDefOn( pawn, Data.HediffDefOf.LegShackled, pawn.health.hediffSet.GetBodyPartRecord( BodyPartDefOf.RightLeg ) );
                }
            }
        }

        public void ApplyCollarEffects( Pawn pawn, Apparel collar )
        {
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
        }

        public void RemoveRestraintsEffects( Pawn pawn, Apparel restraints )
        {
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
