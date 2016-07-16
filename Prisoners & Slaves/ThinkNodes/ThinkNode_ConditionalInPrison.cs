using System;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public class ThinkNode_ConditionalInPrison : ThinkNode_Conditional
    {

        /*
        public ThinkNode_ConditionalInPrison() : base()
        {
        }
        */

        protected override bool Satisfied( Pawn pawn )
        {
            return (
                ( pawn != null )&&
                ( pawn.GetRoom() != null )&&
                ( pawn.GetRoom().isPrisonCell )
            );
        }

    }

}
