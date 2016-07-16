using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public class CompProperties_RestrictedDoor : CompProperties
    {

        public bool DefaultAllowForPrisoners = false;
        public bool DefaultAllowForSlaves = false;
        public bool DefaultAllowForGuests = true;

        public CompProperties_RestrictedDoor()
        {
            this.compClass = typeof( CompRestrictedDoor );
        }

    }

}