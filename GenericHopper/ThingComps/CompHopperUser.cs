﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace esm
{
    public class CompHopperUser : ThingComp
    {
        private StorageSettings userResourceSettings;
        private ThingFilter xmlResources;
        public ThingFilter XmlResourceFilter
        {
            get
            {
                if (xmlResources == null)
                {
                    xmlResources = ((CompProperties_HopperUser)props).resources;
                }
                return xmlResources;
            }
        }

        private List<IntVec3> cachedAdjCellsCardinal;
        private List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                if (this.cachedAdjCellsCardinal == null)
                {
                    this.cachedAdjCellsCardinal = GenAdj.CellsAdjacentCardinal(this.parent).Where(c => c.InBounds()).ToList();
                }
                return this.cachedAdjCellsCardinal;
            }
        }

        public StorageSettings ResourceSettings
        {
            get
            {
                if (userResourceSettings == null)
                {
                    var iHopperUser = parent as IHopperUser;
                    if (
                        (XmlResourceFilter == null) &&
                        (
                            (iHopperUser == null) ||
                            (iHopperUser.ResourceFilter == null)
                        )
                    )
                    {
                        // Does not contain xml resource filter
                        // or (properly) implement IHopperUser
                        Log.Message(parent.def.defName + " Improperly configured!");
                        return null;
                    }

                    // Create storage settings
                    userResourceSettings = new StorageSettings();

                    // Set priority
                    userResourceSettings.Priority = StoragePriority.Important;

                    // instanciating StorageSettings.filter
                    userResourceSettings.filter = new ThingFilter();

                    // Copy filters from the IHopperUser building
                    if ( (iHopperUser != null) && (iHopperUser.ResourceFilter != null) )
                    {
                        iHopperUser.ResourceFilter.ResolveReferences();
                        userResourceSettings.filter.CopyFrom( iHopperUser.ResourceFilter );
                    }
                    // Or filters from xml comp. properties
                    else if ( xmlResources != null )
                    {
                        xmlResources.ResolveReferences();
                        userResourceSettings.filter.CopyFrom( xmlResources );
                    }
                    else
                    {
                        return null;
                    }

                    // Disallow quality
                    userResourceSettings.filter.allowedQualitiesConfigurable = false;

                    // Block default special filters
                    userResourceSettings.filter.BlockDefaultAcceptanceFilters();
                }
                return userResourceSettings;
            }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();

            if (ResourceSettings != null)
            {
                FindAndProgramHoppers();
            }
        }

        public override void PostDeSpawn()
        {
            base.PostDeSpawn();

            // Scan for hoppers and deprogram each one
            var hoppers = FindHoppers();
            if (!hoppers.NullOrEmpty())
            {
                foreach (var hopper in hoppers)
                {
                    hopper.DeprogramHopper();
                }
            }
        }

        public void FindAndProgramHoppers()
        {
            // Now scan for hoppers and program each one
            var hoppers = FindHoppers();
            if (!hoppers.NullOrEmpty())
            {
                foreach (var hopper in hoppers)
                {
                    hopper.ProgramHopper(ResourceSettings);
                }
            }
        }

        public List<CompHopper> FindHoppers()
        {
            // Find hoppers for building
            var hoppers = new List<CompHopper>();
            var occupiedCells = parent.OccupiedRect();
            foreach (var cell in AdjCellsCardinalInBounds)
            {
                var hopper = FindHopper(cell);
                if (
                    (hopper != null) &&
                    (occupiedCells.Cells.Contains(hopper.Building.Position + hopper.Building.Rotation.FacingCell))
                )
                {
                    // Hopper is adjacent and rotated correctly
                    hoppers.Add(hopper);
                }
            }
            // Return list of hoppers connected to this building
            return hoppers;
        }

        public CompHopper FindHopper(IntVec3 cell)
        {
            // Find hopper in cell
            var thingList = cell.GetThingList();
            foreach (var thing in thingList)
            {
                var thingDef = GenSpawn.BuiltDefOf(thing.def) as ThingDef;
                if (
                    (thingDef != null) &&
                    (thingDef.IsHopper())
                )
                {
                    // This thing is a hopper
                    return thing.TryGetComp<CompHopper>();
                }
            }
            // No hopper found
            return null;
        }

        public CompHopper FindBestHopperForResources()
        {
            var hoppers = FindHoppers();
            if (hoppers.NullOrEmpty())
            {
                return null;
            }
            var bestHopper = (CompHopper)null;
            var bestResource = (Thing)null;
            foreach (var hopper in hoppers)
            {
                // Find best in hopper
                var hopperResources = hopper.GetAllResources(ResourceSettings.filter);
                foreach (var resource in hopperResources)
                {
                    if (resource != null)
                    {
                        if (
                            (bestHopper == null) ||
                            (resource.stackCount > bestResource.stackCount)
                        )
                        {
                            // First resource or this hopper holds more
                            bestHopper = hopper;
                            bestResource = resource;
                        }
                    }
                }
            }
            // Return the best hopper
            return bestHopper;
        }

        public bool RemoveResourceFromHoppers(ThingDef resourceDef, int resourceCount)
        {
            if (!EnoughResourceInHoppers(resourceDef, resourceCount))
            {
                return false;
            }
            var hoppers = FindHoppers();
            if (hoppers.NullOrEmpty())
            {
                return false;
            }

            foreach (var hopper in hoppers)
            {
                var resource = hopper.GetResource(resourceDef);
                if (resource != null)
                {
                    if (resource.stackCount >= resourceCount)
                    {
                        resource.SplitOff(resourceCount);
                        return true;
                    }
                    else
                    {
                        resourceCount -= resource.stackCount;
                        resource.SplitOff(resource.stackCount);
                    }
                }
            }
            // Should always be true...
            return (resourceCount <= 0);
        }

        public bool RemoveResourcesFromHoppers(int resourceCount)
        {
            if (!EnoughResourcesInHoppers(resourceCount))
            {
                return false;
            }
            var hoppers = FindHoppers();
            if (hoppers.NullOrEmpty())
            {
                return false;
            }

            foreach (var hopper in hoppers)
            {
                var resources = hopper.GetAllResources(ResourceSettings.filter);
                if (!resources.NullOrEmpty())
                {
                    foreach (var resource in resources)
                    {
                        if (resource.stackCount >= resourceCount)
                        {
                            resource.SplitOff(resourceCount);
                            return true;
                        }
                        else
                        {
                            resourceCount -= resource.stackCount;
                            resource.SplitOff(resource.stackCount);
                        }
                    }
                }
            }
            // Should always be true...
            return (resourceCount <= 0);
        }

        public bool EnoughResourceInHoppers(ThingDef resourceDef, int resourceCount)
        {
            return (CountResourceInHoppers(resourceDef) >= resourceCount);
        }

        public bool EnoughResourcesInHoppers(int resourceCount)
        {
            return (CountResourcesInHoppers() >= resourceCount);
        }

        public int CountResourceInHoppers(ThingDef resourceDef)
        {
            var hoppers = FindHoppers();
            if (hoppers.NullOrEmpty())
            {
                return 0;
            }

            int availableResources = 0;
            foreach (var hopper in hoppers)
            {
                var resources = hopper.GetAllResources(resourceDef);
                if (!resources.NullOrEmpty())
                {
                    foreach (var resource in resources)
                    {
                        availableResources += resource.stackCount;
                    }
                }
            }
            return availableResources;
        }

        public int CountResourcesInHoppers()
        {
            var hoppers = FindHoppers();
            if (hoppers.NullOrEmpty())
            {
                return 0;
            }

            int availableResources = 0;
            foreach (var hopper in hoppers)
            {
                var resources = hopper.GetAllResources(ResourceSettings.filter);
                if (!resources.NullOrEmpty())
                {
                    foreach (var resource in resources)
                    {
                        availableResources += resource.stackCount;
                    }
                }
            }
            return availableResources;
        }

    }
}
