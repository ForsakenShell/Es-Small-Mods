using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public class Monitor : MapComponent
    {

        private static int                  markerIndex;

        public static List<Room>            hostilesInRooms;

        public static List<LawDef>          laws;

        static                              Monitor()
        {
            hostilesInRooms = new List<Room>();
            laws = DefDatabase<LawDef>.AllDefsListForReading;
            if( !laws.NullOrEmpty() )
            {
                foreach( var law in laws )
                {
                    if( law.lawWorker == null )
                    {
                        law.lawWorker = (LawDriver) Activator.CreateInstance( law.lawDriver, new Object[] { law } );
                    }
                }
            }
        }

        public override void                ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue( ref markerIndex, "markerIndex", 1, false );
            Scribe_Collections.LookList( ref hostilesInRooms, "hostilesInRooms", LookMode.MapReference, null );

            if( !laws.NullOrEmpty() )
            {
                foreach( var law in laws )
                {
                    ScribeLaw( law );
                }
            }

            if( Scribe.mode == LoadSaveMode.ResolvingCrossRefs )
            {
                if( hostilesInRooms == null )
                {
                    hostilesInRooms = new List<Room>();
                }
            }

        }

        public override void                MapComponentTick()
        {
            if( ( Find.TickManager.TicksGame % Data.MonitorRemoteChangeTicks ) == 0 )
            {   // Periodically check for doors to unlock
                UnlockAllAppropriateDoors();
            }
        }

        private void                        UnlockAllAppropriateDoors()
        {
            var allDoors = Find.ListerBuildings.AllBuildingsColonistOfClass<Building_RestrictedDoor>().Where( (door) =>
            {
                var compLock = door.TryGetComp<CompLockable>();
                if( compLock == null )
                {
                    return false;
                }
                return compLock.temporaryLock;
            } ).ToList();
            var doors = new List<Building_RestrictedDoor>();
            if( !hostilesInRooms.NullOrEmpty() )
            {
                foreach( var room in hostilesInRooms )
                {
                    foreach( var door in room.Portals() )
                    {
                        var restrictedDoor = door as Building_RestrictedDoor;
                        if(
                            ( restrictedDoor != null )&&
                            ( restrictedDoor.TryGetComp<CompLockable>() != null )
                        )
                        {
                            doors.AddUnique( restrictedDoor );
                            allDoors.Remove( restrictedDoor );
                        }
                    }
                }
            }
            if( !allDoors.NullOrEmpty() )
            {
                foreach( var door in allDoors )
                {
                    //Log.Message( string.Format( "Door {0} is temporarily locked, lifting", door.ThingID ) );
                    DoorLockToggle( door.TryGetComp<CompLockable>(), door.TryGetComp<CompPowerTrader>(), false );
                }
            }
            if( !doors.NullOrEmpty() )
            {
                foreach( var door in doors )
                {
                    DoorLockToggle( door.TryGetComp<CompLockable>(), door.TryGetComp<CompPowerTrader>(), true );
                }
            }
            hostilesInRooms.Clear();
        }

        private void                        DoorLockToggle( CompLockable compLock, CompPowerTrader compPower, bool value )
        {
            compLock.temporaryLock = value;
            if(
                ( compPower != null )&&
                ( compPower.PowerOn )&&
                (
                    ( !value )||
                    ( compLock.IssueLockToggleJob )
                )
            )
            {   // Auto-toggle auto-doors
                //Log.Message( string.Format( "Auto-toggling door lock on {0}", compLock.parent.ThingID ) );
                compLock.ChangeLockState( value );
            }
        }

        public static int                   GetNextMarkerIndex()
        {
            return ++markerIndex;
        }

        private void                        ScribeLaw( LawDef law )
        {
            if( law.lawWorker == null )
            {
                return;
            }
            if(
                ( Scribe.mode == LoadSaveMode.Saving )||
                (
                    ( Scribe.mode == LoadSaveMode.LoadingVars )&&
                    ( Scribe.curParent.HasChildNode( law.defName ) )
                )
            )
            {
                Scribe.EnterNode( law.defName );
                law.lawWorker.ExposeData();
                Scribe.ExitNode();
            }
        }

    }

}
