using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class CompApparel : ThingComp
    {
        
        #region Properties

        public CompProperties_Apparel   Props
        {
            get
            {
                return this.props as CompProperties_Apparel;
            }
        }

        #endregion

    }

}
