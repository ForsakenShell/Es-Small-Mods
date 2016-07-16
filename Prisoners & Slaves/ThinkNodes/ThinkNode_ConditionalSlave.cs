using System;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public class ThinkNode_ConditionalSlave : ThinkNode_Conditional
    {

        /*
        public ThinkNode_ConditionalSlave() : base()
        {
        }
        */

        protected override bool Satisfied( Pawn pawn )
        {
            return pawn.IsSlaveOfColony();
        }

    }

}
