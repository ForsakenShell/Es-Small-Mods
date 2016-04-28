using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonImprovements
{

    public class Building_PrisonDoor : Building_Door
    {

        public override bool PawnCanOpen( Pawn p )
        {
            if( // Only allow prisoners to pass if all neighbouring rooms are prison cells
                ( p.IsPrisonerOfColony )&&
                ( this.Position.GetRegion().NonPortalNeighbors.All( region => region.Room.isPrisonCell ) )
            )
            {
                return true;
            }
            return GenAI.MachinesLike( this.Faction, p );
        }

        public override void Tick ()
        {
            this.GetRoom().isPrisonCell = true;
            base.Tick();
        }

        public override void DeSpawn ()
        {
            this.GetRoom().isPrisonCell = false;
            base.DeSpawn();
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            foreach( var region in this.Position.GetRegion().NonPortalNeighbors )
            {
                region.Room.DrawFieldEdges();
            }
        }

    }

}
