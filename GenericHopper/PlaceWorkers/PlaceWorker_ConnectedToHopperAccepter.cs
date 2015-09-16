using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace esm
{

	public class PlaceWorker_ConnectedToHopperAccepter : PlaceWorker
	{
		
		public PlaceWorker_ConnectedToHopperAccepter()
		{
		}

		public override AcceptanceReport AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
		{
			if( ( loc + rot.FacingCell ).FindHopperUser() != null )
			{
				return ( AcceptanceReport )true;
			}
			return ( AcceptanceReport )( "Must connect to a building that needs a hopper." );
		}
	}
}
