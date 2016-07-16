using System;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class LawDef : Def
    {

        public float                daysToImprisonFor = -1;
        public bool                 arrestColonists = true;
        public bool                 arrestGuests = true;
        public bool                 arrestSlaves = false;
        public bool                 allowLethalForceToSubdue = false;
        public bool                 takeHomeByDefault = false;

        public Type                 lawDriver;

        [Unsaved]
        public LawDriver            lawWorker;

    }

}
