using System;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class CompProperties_SecurityStation : CompProperties
    {

        public int                              MaxCamerasAtOnce = 0;

        public CompProperties_SecurityStation()
        {
            compClass = typeof( CompSecurityStation );
        }

    }

}
