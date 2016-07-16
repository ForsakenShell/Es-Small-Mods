using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CommunityCoreLibrary.MiniMap;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    internal static class Data
    {

        #region Defs

        internal static class BodyPartGroupDefOf
        {

            public static BodyPartGroupDef  Hands                       = DefDatabase<BodyPartGroupDef>.GetNamed( "Hands", true );

        }

        internal static class HediffDefOf
        {

            public static HediffDef     HandCuffed                      = DefDatabase<HediffDef>.GetNamed( "HandCuffed", true );
            public static HediffDef     LegShackled                     = DefDatabase<HediffDef>.GetNamed( "LegShackled", true );

        }

        internal static class JobDefOf
        {

            public static JobDef        DoorLockToggle                  = DefDatabase<JobDef>.GetNamed( "DoorLockToggle", true );
            public static JobDef        EnslavePrisoner                 = DefDatabase<JobDef>.GetNamed( "EnslavePrisoner", true );
            public static JobDef        FreeSlave                       = DefDatabase<JobDef>.GetNamed( "FreeSlave", true );
            public static JobDef        MonitorSecurityStation          = DefDatabase<JobDef>.GetNamed( "MonitorSecurityStation", true );
            public static JobDef        RestainPawn                     = DefDatabase<JobDef>.GetNamed( "RestrainPawn", true );
            public static JobDef        TransferPrisoner                = DefDatabase<JobDef>.GetNamed( "TransferPrisoner", true );
            public static JobDef        UnrestainPawn                   = DefDatabase<JobDef>.GetNamed( "UnrestrainPawn", true );

        }

        internal static class PawnKindDefOf
        {
            
            public static PawnKindDef   Slave                           = DefDatabase<PawnKindDef>.GetNamed( "Slave", true );

        }

        internal static class ThoughtDefOf
        {
            
            public static ThoughtDef    Enslaved                        = DefDatabase<ThoughtDef>.GetNamed( "Enslaved", true );
            public static ThoughtDef    Freed                           = DefDatabase<ThoughtDef>.GetNamed( "FreedSlave", true );

            public static ThoughtDef    KnowPrisonerSoldEmpathic        = DefDatabase<ThoughtDef>.GetNamed( "KnowPrisonerSold", true );
            public static ThoughtDef    KnowPrisonerSoldPsychopath      = DefDatabase<ThoughtDef>.GetNamed( "KnowPrisonerSoldPsychopath", true );

            public static ThoughtDef    KnowGuestExecutedEmpathic       = DefDatabase<ThoughtDef>.GetNamed( "KnowGuestExecuted", true );
            public static ThoughtDef    KnowGuestExecutedPsychopath     = DefDatabase<ThoughtDef>.GetNamed( "KnowGuestExecutedPsychopath", true );

            public static ThoughtDef    KnowColonistExecutedEmpathic    = DefDatabase<ThoughtDef>.GetNamed( "KnowColonistExecuted", true );
            public static ThoughtDef    KnowColonistExecutedPsychopath  = DefDatabase<ThoughtDef>.GetNamed( "KnowColonistExecutedPsychopath", true );

            public static ThoughtDef    KnowGuestOrganHarvestedEmpathic = DefDatabase<ThoughtDef>.GetNamed( "KnowGuestOrganHarvested", true );
            public static ThoughtDef    KnowGuestOrganHarvestedPsychopath = DefDatabase<ThoughtDef>.GetNamed( "KnowGuestOrganHarvestedPsychopath", true );

            public static ThoughtDef    KnowColonistOrganHarvestedEmpathic = DefDatabase<ThoughtDef>.GetNamed( "KnowColonistOrganHarvested", true );
            public static ThoughtDef    KnowColonistOrganHarvestedPsychopath = DefDatabase<ThoughtDef>.GetNamed( "KnowColonistOrganHarvestedPsychopath", true );

        }

        internal static class TraitDefOf
        {

            public static TraitDef      Enslaved                        = DefDatabase<TraitDef>.GetNamed( "Enslaved", true );

        }

        #endregion

        #region Textures

        [StaticConstructorOnStartup]
        internal static class Icons
        {
            
            public static Texture2D     WorkAssignments;
            public static Texture2D     Restrictions;

            public static Texture2D     TransferOwners;
            public static Texture2D     ClearOwners;
            public static Texture2D     BackArrow;

            public static Texture2D     Copy;
            public static Texture2D     Paste;

            public static Texture2D     Lock;

            static                      Icons()
            {
                // Prisoners & Slaves Icons
                WorkAssignments         = ContentFinder<Texture2D>.Get( "UI/Icons/WorkAssignments", true );
                Restrictions            = ContentFinder<Texture2D>.Get( "UI/Icons/Restrictions", true );
                TransferOwners          = ContentFinder<Texture2D>.Get( "UI/Icons/ListSwap", true );
                ClearOwners             = ContentFinder<Texture2D>.Get( "UI/Icons/ListClear", true );
                BackArrow               = ContentFinder<Texture2D>.Get( "UI/Icons/ListClose", true );

                // Core Icons
                Copy                    = ContentFinder<Texture2D>.Get( "UI/Buttons/Copy", true );
                Paste                   = ContentFinder<Texture2D>.Get( "UI/Buttons/Paste", true );

                // Deprecated Core Icons (used with permission)
                Lock                    = ContentFinder<Texture2D>.Get( "UI/Icons/Lock", true );
            }

        }

        [StaticConstructorOnStartup]
        internal static class Materials
        {
            
            public static Material      Locked;

            static                      Materials()
            {
                // Deprecated Core Overlays (used with permission)
                Locked                  = MatFrom( "UI/Overlays/Locked", ShaderDatabase.MetaOverlay );
            }

            private static Material     MatFrom( string texPath, Shader shader )
            {
                var material = MaterialPool.MatFrom( texPath, shader );
                if( material.NullOrBad() )
                {
                    Log.Error( string.Format( "Unable to load Material \"{0}\"", texPath ) );
                }
                return  material;
            }

        }

        #endregion

        #region Constants

        internal static class Strings
        {

            public const string MCMDescription                          = "PaS_MCM_Description";
            public const string MCMBoughtAsColonists                    = "PaS_MCM_BoughtAsColonists";
            public const string MCMLockedDoors                          = "PaS_MCM_LockedDoors";
            public const string MCMLockedDoorsAllowDrafted              = "PaS_MCM_LockedDoorsAllowDrafted";
            public const string MCMLockedDoorsAllowWardens              = "PaS_MCM_LockedDoorsAllowWardens";
            public const string MCMLockedDoorsAllowDoctors              = "PaS_MCM_LockedDoorsAllowDoctors";

            public const string AlertNeedCollarsLabel                   = "PaS_NeedCollars_Label";
            public const string AlertNeedCollarsExplaination            = "PaS_NeedCollars_Explaination";

            public const string AlertNeedRestraintsLabel                = "PaS_NeedRestraints_Label";
            public const string AlertNeedRestraintsExplaination         = "PaS_NeedRestraints_Explaination";

            public const string RoomMarkerCameraName                    = "PaS_Default_Camera_Name";
            public const string RoomMarkerSignName                      = "PaS_Default_Marker_Name";
            public const string RoomMarkerName                          = "PaS_Marker_Name";

            public const string OwnableMoreThanTwoPawns                 = "PaS_Owned_Label";
            public const string OwnableOnlyOwners                       = "PaS_Only_Owners";
            public const string OwnableAssign                           = "PaS_Assign_Ownership";
            public const string OwnableReset                            = "PaS_Reset_Ownership";
            public const string OwnableBack                             = "PaS_Back_Arrow";

            public const string FactionLabel                            = "PaS_Pawn_Slave_Faction";
            public const string PawnKindLabel                           = "PaS_Pawn_Slave_PawnKind";

            public const string TransferPrisoner                        = "PaS_TransferPrisoner";
            public const string CancelTransfer                          = "PaS_CancelTransfer";
            public const string PrisonCell                              = "PaS_Prison_Cell";
            public const string InstallMarker                           = "PaS_InstallMarker";

            public const string ITabSlaveLabel                          = "PaS_Pawn_Slave_Label";
            public const string ITabSecurityLabel                       = "PaS_Security_Label";

            public const string DoorRestrictions                        = "PaS_Restriction_Label";
            public const string DoorLockInspect                         = "PaS_DoorLock_Inspect";
            public const string DoorLockLocked                          = "PaS_DoorLock_Locked";
            public const string DoorLockUnlocked                        = "PaS_DoorLock_Unlocked";
            public const string DoorLockPicked                          = "PaS_DoorLock_Picked";
            public const string DoorLockToggle                          = "PaS_DoorLock_Toggle";
            public const string DoorLockToggleDesc                      = "PaS_DoorLock_Toggle_Desc";
            public const string DoorLockAutoLockdown                    = "Pas_DoorLock_Auto_Lockdown";

            public const string AllowPrisoners                          = "PaS_Allow_Prisoners";
            public const string AllowSlaves                             = "PaS_Allow_Slaves";
            public const string AllowGuests                             = "PaS_Allow_Guests";

            public const string MedicalCare                             = "PaS_Slave_MedicalCare";

            public const string SlaveWorkAssignments                    = "PaS_Slave_WorkAssignments";
            public const string SlaveRestrictions                       = "PaS_Slave_Restrictions";

            public const string OverlayPrisonerColor                    = "PaS_MCM_Overlay_PrisonerColor";
            public const string OverlayPrisonerColorTip                 = "PaS_MCM_Overlay_RadiusColor_Tip";
            public const string OverlayPrisonerRadius                   = "PaS_MCM_Overlay_PrisonerRadius";
            public const string OverlayPrisonerRadiusTip                = "PaS_MCM_Overlay_PrisonerRadius_Tip";
            public const string OverlaySlaveColor                       = "PaS_MCM_Overlay_SlaveColor";
            public const string OverlaySlaveColorTip                    = "PaS_MCM_Overlay_SlaveColor_Tip";
            public const string OverlaySlaveRadius                      = "PaS_MCM_Overlay_SlaveRadius";
            public const string OverlaySlaveRadiusTip                   = "PaS_MCM_Overlay_SlaveRadius_Tip";
            public const string OverlayUnmonitoredColor                 = "PaS_MCM_Overlay_UnmonitoredColor";
            public const string OverlayUnmonitoredColorTip              = "PaS_MCM_Overlay_UnmonitoredColor_Tip";
            public const string OverlayMonitoredColor                   = "PaS_MCM_Overlay_MonitoredColor";
            public const string OverlayMonitoredColorTip                = "PaS_MCM_Overlay_MonitoredColor_Tip";

            public const string NoColonistBeds                          = "PaS_NoColonistBeds";

            public const string RestrainCuff                            = "PaS_Restrain_Cuff";
            public const string RestrainShackle                         = "PaS_Restrain_Shackle";

            public const string RestrainedCuffed                        = "PaS_Restrained_Cuffed";
            public const string RestrainedShackled                      = "PaS_Restrained_Shackled";

            // Additional Prisoner (Slave) Interaction Modes
            public const string Enslave                                 = "PaS_Enslave";
            public const string FreeSlave                               = "PaS_FreeSlave";

            // Vanilla strings
            public const string Copy                                    = "Copy";
            public const string Paste                                   = "Paste";
            public const string Equipped                                = "Equipped";
            public const string EquippedNothing                         = "EquippedNothing";
            public const string Carrying                                = "Carrying";
            public const string RecruitmentDifficulty                   = "RecruitmentDifficulty";
            public const string GetsFood                                = "GetsFood";
            public const string ManualPriorities                        = "ManualPriorities";
            public const string HigherPriority                          = "HigherPriority";
            public const string LowerPriority                           = "LowerPriority";
            public const string EmergencyWorkMarker                     = "EmergencyWorkMarker";

            public const string ITabPrisonerLabel                       = "TabPrisoner";

            public const string MessageSocialFight                      = "MessageSocialFight";
            public const string MessagePrisonerEscaping                 = "MessagePrisonerIsEscaping";

        }

        internal static class PIM
        {

            public const PrisonerInteractionMode EnslavePrisoner        = (PrisonerInteractionMode) 127;
            public const PrisonerInteractionMode FreeSlave              = (PrisonerInteractionMode) 128;

        }

        /*
        internal static class Signal
        {

            public const string         InternalRecache                 = "PaS_DOOR_RECACHE";
            public const string         UpdateStatus                    = "PaS_DOOR_UPDATE";

        }
        */

        public const int                QueueUpdateTicks                = 5;

        public const int                MonitorRemoteCheckTicks         = 10;
        public const int                MonitorRemoteChangeTicks        = MonitorRemoteCheckTicks * 30;
        public const int                MonitorRemoteJobTicks           = MonitorRemoteChangeTicks * 10;

        public const string             SlaveCollarApparalTag           = "Slave";

        public const float              MaxRangeMultiplierForChecks     = 1.5f;
        public const float              MinRangeMultiplierForChecks     = 0.5f;

        public const float              DefaultColumnWidth = 200f;

        #endregion

        #region Static Vars

        public static bool              BoughtSlavesAsColonists         = false;
        public static bool              LockedDoorsAllowDrafted         = true;
        public static bool              LockedDoorsAllowWardens         = true;
        public static bool              LockedDoorsAllowDoctors         = false;

        private static MiniMapOverlay   overlayWildLife;
        private static MiniMapOverlay   overlayNonColonistPawns;
        private static MiniMapOverlay   overlaySecurity;

        #endregion

        #region Static Methods

        private static void             DirtyOverlay( ref MiniMapOverlay overlay, string overlayDefName )
        {
            if( overlay == null )
            {
                overlay = MiniMapController.FindOverlay( overlayDefName );
            }
            if( overlay != null )
            {
                overlay.dirty = true;
            }
        }

        public static void              DirtyOverlays()
        {
            DirtyOverlay( ref overlayWildLife           , "MiniMapOverlay_Wildlife" );
            DirtyOverlay( ref overlayNonColonistPawns   , "MiniMapOverlay_NonColonistPawns" );
            DirtyOverlay( ref overlaySecurity           , "MiniMapOverlay_SecurityCameras" );
        }

        public static void              UpdateSlavesBoughtData()
        {
            // For some reason, this requires a restart.
            if( BoughtSlavesAsColonists )
            {
                if( Data.PawnKindDefOf.Slave.apparelTags.Contains( SlaveCollarApparalTag ) )
                {
                    Data.PawnKindDefOf.Slave.apparelTags.Remove( SlaveCollarApparalTag );
                }
                Data.PawnKindDefOf.Slave.apparelAllowHeadwearChance = 0;
            }
            else
            {
                if( !Data.PawnKindDefOf.Slave.apparelTags.Contains( SlaveCollarApparalTag ) )
                {
                    Data.PawnKindDefOf.Slave.apparelTags.Add( SlaveCollarApparalTag );
                }
                Data.PawnKindDefOf.Slave.apparelAllowHeadwearChance = 100;
            }
            Data.PawnKindDefOf.Slave.ResolveReferences();
        }

        public static void              UpdateAllDoors()
        {
            foreach( var building in Find.ListerBuildings.allBuildingsColonist )
            {
                var door = building as Building_RestrictedDoor;
                if( door != null )
                {
                    door.QueueDoorStatusUpdate();
                }
            }
        }

        public static List<Building_RoomMarker> AllRoomMarkersOfColony()
        {
            var list = new List<Building_RoomMarker>();
            foreach( var thing in Find.ListerBuildings.allBuildingsColonist.Where( building => ( building is Building_RoomMarker ) ) )
            {
                list.Add( (Building_RoomMarker) thing );
            }
            return list;
        }

        public static List<Thing>       AllSlaveCollarsOfColony()
        {
            return Find.ListerThings.AllThings.Where( thing => (
                ( thing.IsSlaveCollar() )//&&
                /*( thing.Faction == Faction.OfPlayer )*/
            ) ).ToList();
        }

        public static List<Thing>       AllRestraintsOfColony( BodyPartGroupDef bodyPartGroupDef )
        {
            return Find.ListerThings.AllThings.Where( thing => (
                ( thing.IsRestraints() )&&
                ( thing.def.apparel.bodyPartGroups.Contains( bodyPartGroupDef ) )//&&
                /*( thing.Faction == Faction.OfPlayer )*/
            ) ).ToList();
        }

        public static List<ThingDef>    AllSlaveCollarDefs()
        {
            return DefDatabase<ThingDef>.AllDefs.Where( def => def.IsSlaveCollar() ).ToList();
        }

        public static List<ThingDef>    AllRestraintDefs()
        {
            return DefDatabase<ThingDef>.AllDefs.Where( def => def.IsRestraints() ).ToList();
        }

        public static bool              LineOfSight( IntVec3 start, IntVec3 end, bool skipFirstCell = false )
        {
            if(
                ( !start.InBounds() )||
                ( !end.InBounds() )
            )
            {
                return false;
            }
            int num1 = Mathf.Abs( end.x - start.x );
            int num2 = Mathf.Abs( end.z - start.z );
            int num3 = start.x;
            int num4 = start.z;
            int num5 = 1 + num1 + num2;
            int num6 = end.x <= start.x ? -1 : 1;
            int num7 = end.z <= start.z ? -1 : 1;
            int num8 = num1 - num2;
            int num9 = num1 * 2;
            int num10 = num2 * 2;
            IntVec3 c = new IntVec3();
            for( ; num5 > 1; --num5 )
            {
                c.x = num3;
                c.z = num4;
                if(
                    (
                        ( !skipFirstCell ) ||
                        ( c != start )
                    ) &&
                    ( !CanBeSeenOver( c ) )
                )
                {
                    return false;
                }
                if( num8 > 0 )
                {
                    num3 += num6;
                    num8 -= num10;
                }
                else
                {
                    num4 += num7;
                    num8 += num9;
                }
            }
            return true;
        }

        public static bool              CanBeSeenOver( this IntVec3 c )
        {
            var edifice = c.GetEdifice();
            if( edifice == null )
            {
                return true;
            }
            if( edifice.def.Fillage != FillCategory.Full )
            {
                return true;
            }
            if( !edifice.def.blockLight )
            {
                return true;
            }
            var buildingDoor = edifice as Building_Door;
            return (
                ( buildingDoor != null )&&
                ( buildingDoor.Open )
            );
        }

        #endregion

    }

}
