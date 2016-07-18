using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonersAndSlaves
{
    
    public class CompRestrictedDoor : CompRestricted
    {

        #region Instance Data

        private bool                            _userAllowPrisoners;
        private bool                            _userAllowSlaves;
        private bool                            _userAllowGuests;

        private bool                            _cacheAllowPrisoners;
        private bool                            _cacheAllowSlaves;
        private bool                            _cacheAllowGuests;

        private Room                            doorRoom;
        private List<Room>                      cachedRooms;

        #endregion

        #region Comp Properties

        public bool                             AllowPrisoners
        {
            get
            {
                return _cacheAllowPrisoners;
            }
        }

        public bool                             AllowSlaves
        {
            get
            {
                return _cacheAllowSlaves;
            }
        }

        public bool                             AllowGuests
        {
            get
            {
                return _cacheAllowGuests;
            }
        }

        public bool                             UserAllowPrisoners
        {
            get
            {
                return _userAllowPrisoners;
            }
            set
            {
                _userAllowPrisoners = value;
                this.QueueUpdateOnParent();
            }
        }

        public bool                             UserAllowSlaves
        {
            get
            {
                return _userAllowSlaves;
            }
            set
            {
                _userAllowSlaves = value;
                this.QueueUpdateOnParent();
            }
        }

        public bool                             UserAllowGuests
        {
            get
            {
                return _userAllowGuests;
            }
            set
            {
                _userAllowGuests = value;
                this.QueueUpdateOnParent();
            }
        }

        #endregion

        #region Reference Properties

        public CompProperties_RestrictedDoor    Props
        {
            get
            {
                return this.props as CompProperties_RestrictedDoor;
            }
        }

        #endregion

        #region Constructor

        public CompRestrictedDoor()
        {
            cachedRooms = new List<Room>();
        }

        #endregion

        #region Base Overrides

        public override bool CacheResults
        {
            get
            {
                return true;
            }
        }

        public override int Priority
        {
            get
            {
                return 1;
            }
        }

        public override int ColumnIndex
        {
            get
            {
                return 0;
            }
        }

        public override void DrawColumns( Listing_Standard listing, float physicalHeight, bool overridden = false )
        {
            #region Restriction Settings
            var restrictionsRect = listing.GetRect( 3 * ( GenUI.ListSpacing - GenUI.GapTiny ) + GenUI.Pad * 2f );
            Widgets.DrawMenuSection( restrictionsRect, true );
            var restrictedPosition = restrictionsRect.ContractedBy( GenUI.Pad );
            GUI.BeginGroup( restrictedPosition );
            {
                var restrictionRect = new Rect( 0.0f, 0.0f, restrictedPosition.width, GenUI.ListSpacing + GenUI.GapTiny );
                bool localValue = false;
                localValue = UserAllowPrisoners;
                ITab_Restricted.DoLabelCheckbox( ref restrictionRect, Data.Strings.AllowPrisoners, ref localValue, overridden );
                UserAllowPrisoners = localValue;

                localValue = UserAllowSlaves;
                ITab_Restricted.DoLabelCheckbox( ref restrictionRect, Data.Strings.AllowSlaves, ref localValue, overridden );
                UserAllowSlaves = localValue;

                localValue = UserAllowGuests;
                ITab_Restricted.DoLabelCheckbox( ref restrictionRect, Data.Strings.AllowGuests, ref localValue, overridden );
                UserAllowGuests = localValue;
            }
            GUI.EndGroup();
            listing.Gap();
            #endregion
        }

        public override void PostCompMake()
        {
            _userAllowPrisoners = Props.DefaultAllowForPrisoners;
            _userAllowSlaves = Props.DefaultAllowForSlaves;
            _userAllowGuests = Props.DefaultAllowForGuests;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            if( !cachedRooms.NullOrEmpty() )
            {
                foreach( var room in cachedRooms )
                {
                    room.DrawFieldEdges();
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.LookValue( ref _userAllowPrisoners, "allowPrisoners" );
            Scribe_Values.LookValue( ref _userAllowSlaves, "allowSlaves" );
            Scribe_Values.LookValue( ref _userAllowGuests, "allowGuests" );

        }

        #endregion

        #region Validate Rooms for Pawn

        public override bool PawnCanOpen( Pawn p, bool isEscaping )
        {
            if( isEscaping )
            {   // Escaping pawns don't care about restrictions
                return true;
            }
            if( this.parent.IsForbiddenToPass( p ) )
            {   // Other factions don't care about forbidden doors
                //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} IsForbiddenToPass", this.parent.ThingID, p.NameStringShort ) );
                return false;
            }
            // Check colonists
            if(
                (
                    ( p.IsColonist )||
                    ( p.Faction == Faction.OfPlayer )
                )&&
                ( !p.IsPrisoner )
            )
            {   // Colonists & colony animals
                //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} IsColonist or Faction.OfPlayer", this.parent.ThingID, p.NameStringShort ) );
                return true;
            }
            // Check non-prisoner guests
            if( p.IsGuestOfColony() )
            {
                if( AllowGuests )
                {   // Guests can use this door
                    //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} IsAllowedGuest", this.parent.ThingID, p.NameStringShort ) );
                    return true;
                }
                // Guests can't use this door
                //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} IsNotAllowedGuest", this.parent.ThingID, p.NameStringShort ) );
                return false;
            }
            if( cachedRooms.NullOrEmpty() )
            {
                // No room cache, can't check
                //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} cachedRooms is null", this.parent.ThingID, p.NameStringShort ) );
                return false;
            }
            var pawnRoom = p.GetRoom();
            bool checkPrisoners = p.IsPrisonerOfColony;
            bool checkSlaves = p.IsSlaveOfColony();
            // Slaves supercede prisoners
            checkPrisoners &= !checkSlaves;
            // Check on prisoners
            if(
                ( AllowPrisoners )&&
                ( checkPrisoners )
            )
            {
                foreach( var room in cachedRooms )
                {
                    // Don't worry about room pawn is leaving
                    if( room == pawnRoom )
                    {
                        continue;
                    }
                    if( !room.isPrisonCell )
                    {   // Prisoners are only valid for prison cells
                        //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} isPrisoner, room {2} !isPrisoncell", this.parent.ThingID, p.NameStringShort, room.ID ) );
                        return false;
                    }
                }
                // Room is valid for prisoners
                //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} isPrisoner, all rooms are prison cells", this.parent.ThingID, p.NameStringShort ) );
                return true;
            }
            // Check on slaves
            if(
                ( AllowSlaves )&&
                ( checkSlaves )
            )
            {
                foreach( var room in cachedRooms )
                {
                    // Don't worry about room pawn is leaving
                    if( room == pawnRoom )
                    {
                        continue;
                    }
                    if( !room.IsSlaveWorkArea() )
                    {   // Room isn't a valid slave work area
                        //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} isSlave, room {2} !IsSlaveWorkArea()", this.parent.ThingID, p.NameStringShort, room.ID ) );
                        return false;
                    }
                }
                // Room is valid for slaves
                //Log.Message( string.Format( "\tCompRestrictedDoor: door {0}, pawn {1} isSlave, all rooms are slave work areas", this.parent.ThingID, p.NameStringShort ) );
                return true;
            }
            // Room is not valid for pawn
            //Log.Message( string.Format( "\tCompRestrictedDoor: door {0} is not valid for pawn {1}", this.parent.ThingID, p.NameStringShort ) );
            return false;
        }

        private string flagString( string label, bool value )
        {
            var str = " " + label + ":";
            if( value )
            {
                str += "t";
            }
            else
            {
                str += "f";
            }
            return str;
        }

#if DEBUG
        public override string CompInspectStringExtra()
        {   // Report actual flags for prisoners, slaves and guests
            var str = "flags:";
            str += flagString( "p", AllowPrisoners );
            str += flagString( "s", AllowSlaves );
            str += flagString( "g", AllowGuests );
            return base.CompInspectStringExtra() + str;
        }
#endif

        #endregion

        #region Update Auto-Detect flags

        public override void UpdateCompStatus()
        {
            //Log.Message( "CompRestrictedDoor.UpdateDoorStatus()" );
            //Log.Message( string.Format( "\tdoor {0}, regionBarrier = {1}", this.parent.ThingID, this.parent.def.regionBarrier ) );
            // Recache markers and rooms
            cachedRooms.Clear();
            doorRoom = this.parent.GetRoom();
            _cacheAllowPrisoners = false;
            _cacheAllowSlaves = false;
            _cacheAllowGuests = false;
            if( doorRoom == null )
            {
                return;
            }
            foreach( var region in this.parent.Position.GetRegion().NonPortalNeighbors )
            {
                if(
                    ( region.Room != doorRoom )&&
                    ( !cachedRooms.Contains( region.Room ) )
                )
                {
                    //Log.Message( string.Format( "\tAdding room {0} to door {1}", region.Room.ID, this.parent.ThingID ) );
                    cachedRooms.Add( region.Room );
                }
            }
            if( cachedRooms.NullOrEmpty() )
            {   // No rooms connected?
                return;
            }
            // Does user setting allow prisoners?
            if( _userAllowPrisoners )
            {   // Set working flag if all rooms are prison cells
                _cacheAllowPrisoners = cachedRooms.All( room => room.isPrisonCell );
            }
            // Update door cell
            doorRoom.isPrisonCell = _cacheAllowPrisoners;
            // Does the user setting allow slaves?
            if( _userAllowSlaves )
            {
                _cacheAllowSlaves = cachedRooms.All( room => room.IsSlaveWorkArea() );
            }
            // Does the user setting allow guests?
            if( _userAllowGuests )
            {   // Generally don't allow guests into prisons
                _cacheAllowGuests = !doorRoom.isPrisonCell;
            }
            // Everything done
            return;
        }

        #endregion

    }

}