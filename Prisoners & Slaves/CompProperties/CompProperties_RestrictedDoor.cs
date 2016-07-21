using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public class CompProperties_RestrictedDoor : CompProperties
    {

        public bool DefaultAllowPrisoners = false;
        public bool DefaultAllowSlaves = false;
        public bool DefaultAllowGuests = true;
        public bool DefaultAllowColonists = true;
        public bool DefaultAllowAnimals = true;

        public CompProperties_RestrictedDoor()
        {
            this.compClass = typeof( CompRestrictedDoor );
        }

    }

}