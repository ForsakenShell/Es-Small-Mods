using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

using CommunityCoreLibrary;

namespace esm
{

	public class SpecialInjector_MineVein : SpecialInjector
	{

		public override void Inject()
		{
			// Set this even though it won't show the Cancel designator
			// due to how Tynan has coded the Cancel designator.
			MineVein.designationDef.designateCancelable = true;
			// Add Smooth Wall to reverse designations
			ReverseDesignatorDatabase_Extensions.Add( (Designator) new Designator_MineVein() );
		}

	}

}
