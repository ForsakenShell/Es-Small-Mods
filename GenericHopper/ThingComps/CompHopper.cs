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

	public class CompHopper : ThingComp
	{

		private bool						wasProgrammed;

		public Building_Storage				StorageBuilding
		{
			get
			{
				return parent as Building_Storage;
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
				( StorageBuilding == null )
			)
			{
				// Already programmed or not a valid storage building
				return;
			}
			var hopperUser = FindHopperUser();
			var hopperSettings = StorageBuilding.GetStoreSettings();
			if(
				( hopperUser == null )||
				( hopperSettings == null )
			)
			{
				// No user or no storage settings
				return;
			}
			var compHopperUser = hopperUser.TryGetComp<CompHopperUser>();
			if(
				( compHopperUser == null )||
				( compHopperUser.ResourceDefs.NullOrEmpty() )
			)
			{
				// Hopper user does not implement programmable IHopperUser
				return;
			}

			// Disable everything first
			hopperSettings.filter.SetDisallowAll();

			// Set priority
			hopperSettings.Priority = StoragePriority.Important;

			// Add all acceptable defs
			foreach( var resourceDef in compHopperUser.ResourceDefs )
			{
				hopperSettings.filter.SetAllow( resourceDef, true );
			}

			// Set the programming flag
			WasProgrammed = true;
		}

		public Thing						FindHopperUser()
		{
			return ( parent.Position + parent.Rotation.FacingCell ).FindHopperUser();
		}

		public Thing						GetResource( List< ThingDef > acceptableResources )
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

		public List< Thing >				GetAllResources( List< ThingDef > acceptableResources )
		{
			var things = parent.Position.GetThingList().Where( t => (
				( acceptableResources.Contains( t.def ) )
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
