using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using RimWorld;
using Verse;

namespace esm
{
	public class Building_Hopper : Building, IStoreSettingsParent, ISlotGroupParent
	{
		public SlotGroup slotGroup;
		public StorageSettings settings;
		public StorageSettings baseSettings;
		private IEnumerable<IntVec3> cachedOccupiedCells;

		private CompHopper			CompHopper
		{
			get
			{
				return this.GetComp<CompHopper>();
			}
		}

		public bool StorageTabVisible
		{
			get
			{
				return true;
			}
		}

		public SlotGroup GetSlotGroup()
		{
			return this.slotGroup;
		}

		public virtual void Notify_ReceivedThing(Thing newItem)
		{
		}

		public virtual void Notify_LostThing(Thing newItem)
		{
		}

		public virtual IEnumerable<IntVec3> AllSlotCells()
		{
			if( cachedOccupiedCells == null )
			{
				cachedOccupiedCells = this.OccupiedRect().Cells;
			}
			return cachedOccupiedCells;
		}

		public List<IntVec3> AllSlotCellsList()
		{
			return this.AllSlotCells().ToList();
		}

		public StorageSettings GetStoreSettings()
		{
			return this.settings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return this.baseSettings;
		}

		public string SlotYielderLabel()
		{
			return this.LabelCap;
		}

		public override void PostMake()
		{
			base.PostMake();
			this.baseSettings = new StorageSettings();
			this.settings = new StorageSettings((IStoreSettingsParent) this);
			if (this.def.building.defaultStorageSettings == null)
				return;
			this.settings.CopyFrom(this.def.building.defaultStorageSettings);
		}

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			this.slotGroup = new SlotGroup((ISlotGroupParent) this);
			this.cachedOccupiedCells = this.OccupiedRect().Cells;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.LookDeep<StorageSettings>(ref settings, "settings", new Object[1]{ this } );
			Scribe_Deep.LookDeep<StorageSettings>(ref baseSettings, "baseSettings", null );
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (this.slotGroup != null)
				this.slotGroup.Notify_ParentDestroying();
			base.Destroy(mode);
		}

	}
}
