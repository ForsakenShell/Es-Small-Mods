using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions are found here
using Verse;         // RimWorld universal objects are here
//using Verse.AI;    // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound

namespace esm
{
    
	public class Alert_BuildingNeedsHopper : Alert_High
    {

        public override AlertReport Report
        {
            get
            {
				var buildings =
					Find.ListerBuildings.allBuildingsColonist
						.Where( b => (
							( b.def.building.wantsHopperAdjacent )
						) ).ToList();

                foreach( var building in buildings )
                {
					if( building.FindHoppers().NullOrEmpty() )
                    {
						this.baseExplanation = string.Format( "You have a {0} with no hopper connected to it.\n\nTo work, {0} must draw from a connected hopper filled with the appropriate resources.\n\nBuild a hopper connected to the {0}.", building.def.label );
						return AlertReport.CulpritIs( building );
                    }
                }
                return AlertReport.Inactive;
            }
        }

		public Alert_BuildingNeedsHopper()
        {
            this.baseLabel = "Need hopper";
        }
    }
}
