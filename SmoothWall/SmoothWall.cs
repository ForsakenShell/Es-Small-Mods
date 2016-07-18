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

	public static class SmoothWall
	{

		public static readonly DesignationDef		designationDef				= DefDatabase<DesignationDef>.GetNamed( "SmoothWall_Designation", true );
		public static readonly JobDef				jobDef						= DefDatabase<JobDef>.GetNamed( "SmoothWall_Job", true );
        public static readonly ResearchProjectDef   ResearchStoneCutting        = DefDatabase<ResearchProjectDef>.GetNamed( "Stonecutting", true );
        //public static readonly ResearchProjectDef   ResearchPneumaticPicks      = DefDatabase<ResearchProjectDef>.GetNamed( "PneumaticPicks", true );
		public static readonly SoundDef				soundDef					= DefDatabase<SoundDef>.GetNamed( "Recipe_Sculpt", true );
		public static readonly ThingDef				thingDef					= DefDatabase<ThingDef>.GetNamed( "SmoothWall", true );

		public static readonly Texture2D			icon						= ContentFinder<Texture2D>.Get( "UI/Designators/SmoothWall", true );

		public static readonly string				label						= "SmoothWall_Label".Translate();
		public static readonly string				description					= "SmoothWall_Description".Translate();

	}

}

