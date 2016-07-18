using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

using CommunityCoreLibrary;

namespace esm
{

    [StaticConstructorOnStartup]
	public static class MineVein
	{

		public static readonly DesignationDef		designationDef				= DefDatabase<DesignationDef>.GetNamed( "MineVein", true );

        public static readonly Texture2D			icon;

		public static readonly string				label						= "MineVein_Label".Translate();
		public static readonly string				description					= "MineVein_Description".Translate();

        static MineVein()
        {
            icon = ContentFinder<Texture2D>.Get( "UI/Designators/MineVein", true );
        }

	}

}
