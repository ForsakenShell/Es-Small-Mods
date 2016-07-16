using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public static class ThingWithComps_Extensions
    {

        public static List<CompRestricted>          CompsRestricted( this ThingWithComps thing )
        {
            if( thing.AllComps == null )
            {
                return null;
            }
            var list = new List<CompRestricted>();
            foreach( var comp in thing.AllComps )
            {
                var compRestricted = comp as CompRestricted;
                if( compRestricted != null )
                {
                    list.Add( compRestricted );
                }
            }
            return list;
        }

    }

}