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

	public interface IHopperUser
	{
		// This property tells is the list of things to program the hopper with
		List< ThingDef >				ResourceDefs
		{
			get;
		}

		// This method finds and programs all attached hoppers
		// This should be called once in SpawnSetup()
		// Copy-pasta the method below
		void							FindAndProgramHoppers();

		/*
		public void						FindAndProgramHoppers()
		{
			var hoppers = this.FindHoppers();
			if( !hoppers.NullOrEmpty() )
			{
				foreach( var hopper in hoppers )
				{
					hopper.ProgramHopper();
				}
			}
		}
		*/

	}

}

