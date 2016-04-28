using RimWorld;
using Verse;

namespace PrisonImprovements
{
    
    public class CompSlave : ThingComp
    {

        // For wardens to haul to a marker
        public Thing                    haulTarget = null;

        // For wardens to free slaves
        public Faction                  originalFaction = null;
        public PawnKindDef              originalPawnKind = null;
        public bool                     freeSlave = false;
        public bool                     wasSlave = false;

        public bool ShouldBeTransfered
        {
            get
            {
                return haulTarget != null;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            var pawn = this.parent as Pawn;
            if( pawn == null )
            {
                return;
            }
            var collar = pawn.WornCollar();
            if( collar == null )
            {
                return;
            }
            collar.TickRare();
        }

        public override void CompTick()
        {
            base.CompTick();
            var pawn = this.parent as Pawn;
            if( pawn == null )
            {
                return;
            }
            var collar = pawn.WornCollar();
            if( collar == null )
            {
                return;
            }
            collar.Tick();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue<bool>( ref this.freeSlave, "freeSlave", false, false );
            Scribe_Values.LookValue<bool>( ref this.wasSlave, "wasSlave", false, false );
            Scribe_References.LookReference<Thing>( ref this.haulTarget, "haulTarget" );
            Scribe_References.LookReference<Faction>( ref this.originalFaction, "originalFaction" );
            Scribe_Defs.LookDef<PawnKindDef>( ref this.originalPawnKind, "originalPawnKind" );
        }

    }

}
