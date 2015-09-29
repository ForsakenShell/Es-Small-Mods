using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

using CommunityCoreLibrary;

namespace esm
{

	public static class HopperHelper
	{

		/// <summary>
		/// Determines if the specified thingDef is a hopper.
		/// </summary>
		/// <returns><c>true</c> if the specified thingDef is a hopper; otherwise, <c>false</c>.</returns>
		/// <param name="thingDef">ThingDef to test</param>
		public static bool				IsHopper( this ThingDef thingDef )
		{
			return
				( thingDef.thingClass == typeof( Building_Hopper ) )&&
				( thingDef.HasComp( typeof( CompHopper ) ) );
		}


		/// <summary>
		/// Determines if the specified thingDef is a hopper user.
		/// </summary>
		/// <returns><c>true</c> if the specified thingDef is a hopper user; otherwise, <c>false</c>.</returns>
		/// <param name="thingDef">ThingDef to test</param>
		public static bool				IsHopperUser( this ThingDef thingDef )
		{
			return
				( thingDef.building != null )&&
				( thingDef.building.wantsHopperAdjacent )&&
				( thingDef.HasComp( typeof( CompHopperUser ) ) );
		}

	}

}
