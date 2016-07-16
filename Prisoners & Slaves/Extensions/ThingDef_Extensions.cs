using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public static class ThingDef_Extensions
    {

        public static bool IsSlaveCollar( this ThingDef def )
        {
            return def.HasComp( typeof( CompSlaveCollar ) );
        }

        public static bool IsRestraints( this ThingDef def )
        {
            return def.HasComp( typeof( CompRestraints ) );
        }

    }

}