using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using CommunityCoreLibrary.UI;

using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonersAndSlaves
{
    
    public class CompOwnable : CompRestricted
    {

        #region Instance Data

        private bool                            _userOnlyOwners;

        private bool                            _cachedOnlyOwners;

        public string                           OwnersNamesShort;
        public string                           OwnersNamesFull;
        private List<string>                    roomOwners;
        private List<string>                    userAddedOwners;

        private bool                            drawOwnerSwap = false;
        private Vector2                         vecCurrentOwners = new Vector2();
        private Vector2                         vecPotentialOwners = new Vector2();

        private List<Pawn>                      allPotentialOwners;
        private List<Pawn>                      potentialOwners;

        #endregion

        #region Properties

        public bool                             OnlyOwners
        {
            get
            {
                return _cachedOnlyOwners;
            }
        }

        public bool                             UserOnlyOwners
        {
            get
            {
                return _userOnlyOwners;
            }
            set
            {
                _userOnlyOwners = value;
                this.QueueUpdateOnParent();
            }
        }

        public List<Pawn>                       Owners
        {
            get
            {
                var owners = new List<Pawn>();
                var ownersThingIDs = OwnersByThingID;
                foreach( var thingID in ownersThingIDs )
                {
                    var pawn = Find.MapPawns.FreeColonists.FirstOrDefault( p => p.ThingID == thingID );
                    if( pawn != null )
                    {
                        owners.AddUnique( pawn );
                    }
                }
                return owners;
            }
        }

        public List<string>                     OwnersByThingID
        {
            get
            {
                var owners = roomOwners.ListFullCopy();
                owners.AddRangeUnique( userAddedOwners );
                return owners;
            }
        }

        public CompProperties_Ownable           Props
        {
            get
            {
                return this.props as CompProperties_Ownable;
            }
        }

        #endregion

        #region Constructor

        public CompOwnable()
        {
            _userOnlyOwners = false;
            roomOwners = new List<string>();
            userAddedOwners = new List<string>();
        }

        #endregion

        #region Base Overrides

        public override void PostDrawGUIOverlay()
        {   // Called by Building_RestrictedDoor.DrawGUIOverlay()
            if(
                ( !OnlyOwners )||
                ( Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest )
            )
            {
                return;
            }
            if( !Owners.NullOrEmpty() )
            {
                GenWorldUI.DrawThingLabel( this.parent, OwnersNamesShort, Color.white );
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.LookValue( ref _userOnlyOwners, "onlyOwners" );

            Scribe_Collections.LookList( ref userAddedOwners, "Owners" );

        }

        #endregion

        #region Try to Add an Owner

        public bool TryAddOwner( Pawn p )
        {
            if( p == null )
            {
                return false;
            }
            return TryAddOwnerByThingID( p.ThingID );
        }

        public bool TryAddOwnerByThingID( string thingID )
        {
            if( thingID.Equals( "" ) )
            {
                return false;
            }
            if( roomOwners.Contains( thingID ) )
            {
                return false;
            }
            if( !userAddedOwners.Contains( thingID ) )
            {
                userAddedOwners.Add( thingID );
            }
            this.QueueUpdateOnParent();
            return true;
        }

        #endregion

        #region Try to Remove an Owner, I dare ya

        public bool TryRemoveOwner( Pawn p )
        {
            if( p == null )
            {
                return false;
            }
            return TryRemoveOwnerByThingID( p.ThingID );
        }

        public bool TryRemoveOwnerByThingID( string thingID )
        {
            if( thingID.Equals( "" ) )
            {
                return false;
            }
            if( roomOwners.Contains( thingID ) )
            {
                return false;
            }
            if( userAddedOwners.Contains( thingID ) )
            {
                userAddedOwners.Remove( thingID );
            }
            this.QueueUpdateOnParent();
            return true;
        }

        #endregion

        #region Check if Pawn is an Owner

        public override bool PawnCanOpen( Pawn p, bool isEscaping )
        {
            if( isEscaping )
            {   // Escaping pawns don't care about restrictions
                return true;
            }
            if( // Not set for owners or no owners assigned
                ( !OnlyOwners )||
                ( Owners.Count() == 0 )
            )
            {   // Not restricted by ownership
                return true;
            }
            if( p.Drafted )
            {   // Drafted pawns ignore restrictions
                return true;
            }
            if( // Always unrestricted to wardens and doctors
                ( p.workSettings != null )&&
                ( p.workSettings.EverWork )&&
                (
                    ( p.workSettings.WorkIsActive( WorkTypeDefOf.Warden ) )||
                    ( p.workSettings.WorkIsActive( WorkTypeDefOf.Doctor ) )
                )
            )
            {
                return true;
            }
            if( Owners.Contains( p ) )
            {   // Pawn is an owner
                return true;
            }
            // Pawn isn't an owner
            return false;
        }

        #endregion

        #region Update Auto-Detect flags

        public override void UpdateCompStatus()
        {
            RebuildOwners();
        }

        public bool RebuildOwners( bool clearUserAdded = false )
        {
            roomOwners.Clear();
            _cachedOnlyOwners = false;
            OwnersNamesShort = string.Empty;
            OwnersNamesFull = string.Empty;
            if( clearUserAdded )
            {
                userAddedOwners.Clear();
            }
            var thisRoom = this.parent.GetRoom();
            if( thisRoom == null )
            {
                return false;
            }
            if( UserOnlyOwners )
            {
                roomOwners.Clear();
                if( this.parent is Building_Door )
                {   // Look at rooms this connects to
                    foreach( var region in this.parent.Position.GetRegion().NonPortalNeighbors )
                    {
                        if(
                            ( region.Room != thisRoom )&&
                            ( region.Room.Owners.Count() > 0 )
                        )
                        {
                            foreach( var owner in region.Room.Owners )
                            {
                                roomOwners.AddUnique( owner.ThingID );
                            }
                        }
                    }
                }
                else
                {   // Look at contained room only
                    if(
                        ( thisRoom != null )&&
                        ( thisRoom.Owners.Count() > 0 )
                    )
                    {
                        foreach( var owner in thisRoom.Owners )
                        {
                            roomOwners.AddUnique( owner.ThingID );
                        }
                    }
                }
                _cachedOnlyOwners = !roomOwners.NullOrEmpty();
            }
            RebuildNameStrings();
            RecachePotentialOwners();
            return true;
        }

        private void RecachePotentialOwners()
        {
            allPotentialOwners = Find.MapPawns.FreeColonists.ToList().ListFullCopy();
            potentialOwners = new List<Pawn>();
            //var dump = "allPotentialOwners:";
            foreach( var owner in allPotentialOwners )
            {
                //dump += "\n\tName: " + owner.Name.ToStringFull;
                if( !Owners.Contains( owner ) )
                {
                    //dump += " (!Owner)";
                    potentialOwners.Add( owner );
                }
            }
            //Log.Message( dump );
        }

        private void RebuildNameStrings()
        {
            var owners = Owners;
            var ownersCount = owners.Count;
            OwnersNamesShort = "";
            OwnersNamesFull = "";
            if( owners.Count > 2 )
            {
                OwnersNamesShort = Data.Strings.OwnableMoreThanTwoPawns.Translate();
            }
            for( int i = 0; i < ownersCount; ++i )
            {
                AddNameForIndex( i, ref OwnersNamesFull, owners[ i ].NameStringShort );
                if( owners.Count <= 2 )
                {
                    AddNameForIndex( i, ref OwnersNamesShort, owners[ i ].NameStringShort );
                }
            }
        }

        private void AddNameForIndex( int i, ref string str, string addStr )
        {
            if( i > 0 )
            {
                str += ", ";
            }
            str += addStr;
        }

        #endregion

        #region Priority and Rendering

        public override bool CacheResults
        {
            get
            {
                return true;
            }
        }

        public override int             Priority
        {
            get
            {
                return 4;
            }
        }

        public override bool            CompletelyEnforced
        {
            get
            {
                return UserOnlyOwners;
            }
        }

        public override int             ColumnIndex
        {
            get
            {
                return 0;
            }
        }

        public override int             ColumnWidth
        {
            get
            {
                if( !UserOnlyOwners )
                {
                    return 1;
                }
                return !drawOwnerSwap ? 2 : 3;
            }
        }

        public override void            DrawColumns( Listing_Standard listing, float physicalHeight, bool overridden = false )
        {
            #region Owner Settings
            var ownerToggleMainRect = listing.GetRect( 1 * GenUI.ListSpacing + GenUI.Pad * 2f );
            Widgets.DrawMenuSection( ownerToggleMainRect, true );
            var ownerTogglePosition = ownerToggleMainRect.ContractedBy( GenUI.Pad );
            GUI.BeginGroup( ownerTogglePosition );
            {
                var ownerToggleRect = new Rect( 0.0f, 0.0f, ownerTogglePosition.width, GenUI.ListSpacing + GenUI.GapTiny );
                bool localValue = false;
                localValue = UserOnlyOwners;
                ITab_Restricted.DoLabelCheckbox( ref ownerToggleRect, Data.Strings.OwnableOnlyOwners, ref localValue, overridden );
                UserOnlyOwners = localValue;
            }
            GUI.EndGroup();
            listing.Gap();
            if( !UserOnlyOwners )
            {
                return;
            }
            #endregion

            #region Draw List of Owners
            listing.NewColumn();

            #region Transfer, Clear buttons
            var buttonRect = listing.GetRect( GenUI.ListSpacing - GenUI.GapTiny );
            buttonRect.x += buttonRect.width - GenUI.ListSpacing - GenUI.GapTiny;
            buttonRect.width = GenUI.ListSpacing - GenUI.GapTiny;
            if( CCL_Widgets.ButtonImage( buttonRect, Data.Icons.TransferOwners, "=", Data.Strings.OwnableAssign.Translate() ) )
            {
                drawOwnerSwap = true;
            }
            buttonRect.x -= GenUI.ListSpacing;
            if( CCL_Widgets.ButtonImage( buttonRect, Data.Icons.ClearOwners, "0", Data.Strings.OwnableReset.Translate() ) )
            {
                RebuildOwners( true );
            }
            listing.Gap();
            #endregion

            #region Draw list of owners
            var ownerHeight = physicalHeight - listing.CurHeight;
            var ownerRect = listing.GetRect( ownerHeight );
            Widgets.DrawMenuSection( ownerRect, true );
            ITab_Restricted.DrawOwnerScroll( ownerRect, ref vecCurrentOwners, Owners.ToList(), OwnerScrollRemoveOwner );
            #endregion

            #endregion

            #region Draw list of potential owners
            if( drawOwnerSwap )
            {
                listing.NewColumn();

                #region Back button
                buttonRect = listing.GetRect( GenUI.ListSpacing - GenUI.GapTiny );
                buttonRect.width = GenUI.ListSpacing - GenUI.GapTiny;
                if( CCL_Widgets.ButtonImage( buttonRect, Data.Icons.BackArrow, "<", Data.Strings.OwnableBack.Translate() ) )
                {
                    drawOwnerSwap = false;
                }
                listing.Gap();
                #endregion

                #region Draw list
                ownerHeight = physicalHeight - listing.CurHeight;
                ownerRect = listing.GetRect( ownerHeight );
                Widgets.DrawMenuSection( ownerRect, true );
                ITab_Restricted.DrawOwnerScroll( ownerRect, ref vecPotentialOwners, potentialOwners, OwnerScrollAddOwner );
                #endregion
            }
            #endregion
        }

        #endregion

        private void OwnerScrollAddOwner( Pawn p )
        {
            if( TryAddOwner( p ) )
            {
                potentialOwners.Remove( p );
            }
        }

        private void OwnerScrollRemoveOwner( Pawn p )
        {
            if( TryRemoveOwner( p ) )
            {
                potentialOwners.Add( p );
            }
        }

    }

}