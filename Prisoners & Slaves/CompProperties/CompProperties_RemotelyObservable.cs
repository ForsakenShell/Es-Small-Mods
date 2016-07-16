using System;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class CompProperties_RemotelyObservable : CompProperties
    {

        public float observationAngle;
        public float observationRange;

        public CompProperties_RemotelyObservable()
        {
            compClass = typeof( CompRemotelyObservable );
        }

    }

}
