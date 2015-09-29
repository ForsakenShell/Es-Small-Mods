using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

using CommunityCoreLibrary;

namespace esm
{

	public class CompHopper : ThingComp
	{

		private bool						wasProgrammed;

		public Building_Hopper				Building
		{
			get
			{
				return parent as Building_Hopper;
			}
		}

		public bool							WasProgrammed
		{
			get
			{
				return wasProgrammed;
			}
			set
			{
				wasProgrammed = value;
			}
		}

		public List<ThingDef>				ResourceDefs
		{
			get
			{
				return Building.GetStoreSettings().filter.AllowedThingDefs.ToList();
			}
		}

		public override void				PostSpawnSetup()
		{
			base.PostSpawnSetup();
			ProgramHopper();
		}

		public override void				PostExposeData()
		{
			Scribe_Values.LookValue( ref wasProgrammed, "wasProgrammed", false );
		}

		public void							ProgramHopper()
		{
			if(
				( WasProgrammed )||
				( Building == null )
			)
			{
				// Already programmed or not a valid hopper
				return;
			}
			var hopperUser = FindHopperUser();
			var hopperSettings = Building.GetStoreSettings();
			if(
				( hopperUser == null )||
				( hopperSettings == null )
			)
			{
				// No user or no storage settings
				return;
			}
			var compHopperUser = hopperUser.TryGetComp<CompHopperUser>();
			var iHopperUser = hopperUser as IHopperUser;
			if(
				( compHopperUser == null )||
				(
					( compHopperUser.Resources == null )&&
					(
						( iHopperUser == null )||
						( iHopperUser.ResourceFilter == null )
					)
				)
			)
			{
				// Hopper user does not contain xml resource
				// filter or (properly) implement IHopperUser
				return;
			}

			// Set priority
			hopperSettings.Priority = StoragePriority.Important;

			// Create a new filter
			hopperSettings.filter = new ThingFilter();

			// Set the filter from the hopper user
			if( iHopperUser != null )
			{
				// Copy a filter from a building implementing IHopperUser
				hopperSettings.filter.CopyFrom( iHopperUser.ResourceFilter );
			}
			else
			{
				// Copy filters from xml and resolve the references
				hopperSettings.filter.CopyFrom( compHopperUser.Resources );
				hopperSettings.filter.ResolveReferences();
			}

			// Set the base settings to the default settings
			Building.baseSettings.CopyFrom( hopperSettings );

			// Set the programming flag
			WasProgrammed = true;
		}

		public Thing						FindHopperUser()
		{
			return ( parent.Position + parent.Rotation.FacingCell ).FindHopperUser();
		}

		public Thing						GetResource( ThingFilter acceptableResources )
		{
			var things = GetAllResources( acceptableResources );
			if( things.NullOrEmpty() )
			{
				return null;
			}
			return things.FirstOrDefault();
		}

		public Thing						GetResource( ThingDef resourceDef )
		{
			var things = GetAllResources( resourceDef );
			if( things.NullOrEmpty() )
			{
				return null;
			}
			return things.FirstOrDefault();
		}

		public List< Thing >				GetAllResources( ThingFilter acceptableResources )
		{
			var things = parent.Position.GetThingList().Where( t => (
				( acceptableResources.Allows( t.def ) )
			) ).ToList();
			if( things.NullOrEmpty() )
			{
				return null;
			}

			things.Sort( ( Thing x, Thing y ) => ( x.stackCount > y.stackCount ) ? -1 : 1 );

			// Return sorted by quantity list of things
			return things;
		}

		public List< Thing >				GetAllResources( ThingDef resourceDef )
		{
			var things = parent.Position.GetThingList().Where( t => (
				( resourceDef == t.def )
			) ).ToList();
			if( things.NullOrEmpty() )
			{
				return null;
			}

			things.Sort( ( Thing x, Thing y ) => ( x.stackCount > y.stackCount ) ? -1 : 1 );

			// Return sorted by quantity list of things
			return things;
		}

	}

}
