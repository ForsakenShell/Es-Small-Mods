using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;
using RimWorld;

namespace esm
{
    public class PlaceWorker_HopperUserMagnifier : PlaceWorker
    {
        /// <summary>
        /// Draws a target highlight on Hopper user
        /// </summary>
        /// <param name="def"></param>
        /// <param name="center"></param>
        /// <param name="rot"></param>
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            Thing hopperUser = CompHopper.FindHopperUser( center + rot.FacingCell );
            if ( (hopperUser != null) && !hopperUser.OccupiedRect().Cells.Contains( center ) )
            {                
                GenDraw.DrawTargetHighlight( hopperUser );
            }
        }
    }
}
