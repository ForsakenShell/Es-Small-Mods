using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class CompForbiddable : RimWorld.CompForbiddable
    {
        
        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            var toggle = new Command_Toggle();
            toggle.hotKey = KeyBindingDefOf.CommandItemForbid;
            toggle.icon = TexCommand.Forbidden;
            toggle.isActive = () => { return this.Forbidden; };
            toggle.toggleAction = () =>
            {
                this.Forbidden = !this.Forbidden;
                ConceptDatabase.KnowledgeDemonstrated( ConceptDefOf.Forbidding, KnowledgeAmount.SpecificInteraction );
                var door = this.parent as Building_RestrictedDoor;
                if( door != null )
                {
                    door.QueueDoorStatusUpdate( true );
                }
            };
            toggle.defaultDesc = this.Forbidden
                ? "CommandForbiddenDesc".Translate()
                : "CommandNotForbiddenDesc".Translate();
            toggle.tutorHighlightTag = "ToggleForbidden";
            yield return toggle;
        }

    }

}