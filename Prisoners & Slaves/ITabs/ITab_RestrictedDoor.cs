using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommunityCoreLibrary;

using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public class ITab_Restricted : ITab
    {

        private const float             defaultHeight = GenUI.Gap + 236f + GenUI.Gap;
        private const float             defaultWidth = GenUI.Gap + Data.DefaultColumnWidth + GenUI.Gap;

        private static readonly Vector2 WinSize;

        private static List<CompRestricted> _comps;


        static ITab_Restricted()
        {
            ITab_Restricted.WinSize = new Vector2( defaultWidth, defaultHeight );
        }

        public ITab_Restricted()
        {
            this.size = ITab_Restricted.WinSize;
            this.labelKey = Data.Strings.ITabSecurityLabel;
        }

        private ThingWithComps          _parentThing;
        public List<CompRestricted>     Comps
        {
            get
            {
                if( _parentThing != this.SelThing )
                {
                    _comps = null;
                }
                _parentThing = this.SelThing as ThingWithComps;
                if( _parentThing == null )
                {
                    return null;
                }
                if( _comps == null )
                {
                    _comps = _parentThing.CompsRestricted().Where( comp => comp.ColumnIndex >= 0 ).ToList();
                    _comps.Sort( (x,y) =>
                    {
                        if( x.ColumnIndex == y.ColumnIndex )
                        {
                            return x.Priority > y.Priority ? 1
                                        : x.Priority < y.Priority ? -1
                                        : 0;
                        }
                        return x.ColumnIndex > y.ColumnIndex ? 1
                                    : x.ColumnIndex < y.ColumnIndex ? -1
                                    : 0;
                    } );
                }
                return _comps;
            }
        }

        public override bool IsVisible
        {
            get
            {
                return ( !Comps.NullOrEmpty() );
            }
        }

        public override void OnOpen()
        {
            this.size = new Vector2( defaultWidth, defaultHeight );
            this.UpdateSize();
            base.OnOpen();
        }

        private int HighestPriorityEnforced()
        {
            if( Comps.NullOrEmpty() )
            {
                return 999999;
            }
            int largest = -1;
            foreach( var comp in Comps )
            {
                if(
                    ( comp.CompletelyEnforced )&&
                    ( comp.Priority > largest )
                )
                {
                    largest = comp.Priority;
                }
            }
            return largest;
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            var columnsNeeded = 1;

            var listingRect = new Rect( 0f, 0f, this.size.x, 9999f ).ContractedBy( GenUI.Gap );
            var listing = new Listing_Standard( listingRect );
            {
                listing.ColumnWidth = Data.DefaultColumnWidth;

                #region Security Header
                listing.Label( Data.Strings.DoorRestrictions.Translate() );
                var curY = listing.CurHeight;
                Widgets.DrawLine( new Vector2( 0f, curY ), new Vector2( Data.DefaultColumnWidth, curY ), Color.white, 1.0f );
                listing.Gap( GenUI.Pad );
                #endregion

                #region Do Comps
                var columnIndex = 0;
                var highestEnforced = HighestPriorityEnforced();
                foreach( var comp in Comps )
                {
                    if( comp.ColumnIndex != columnIndex )
                    {
                        listing.NewColumn();
                        columnIndex = comp.ColumnIndex;
                    }
                    var colNeeds = comp.ColumnIndex + comp.ColumnWidth;
                    if( colNeeds > columnsNeeded )
                    {
                        columnsNeeded = colNeeds;
                    }
                    comp.DrawColumns( listing, defaultHeight - GenUI.Gap * 2f, comp.Priority < highestEnforced );
                }
                #endregion

            }
            listing.End();

            #region Finalize ITab
            var newSize = new Vector2( GenUI.Gap + ( columnsNeeded * Data.DefaultColumnWidth ) + ( ( columnsNeeded - 1 ) * Listing_Standard.ColumnSpacing ) + GenUI.Gap, defaultHeight );
            if( newSize != this.size )
            {
                this.size = newSize;
                this.UpdateSize();
            }
            #endregion

        }

        #region Helper methods for rendering

        public static void DoLabelCheckbox( ref Rect rect, string labelKey, ref bool value, bool disabled = false )
        {
            var oldColor = GUI.color;
            if( disabled )
            {
                GUI.color = Color.grey;
            }
            Widgets.CheckboxLabeled( rect, labelKey.Translate(), ref value, disabled );
            rect.y += GenUI.ListSpacing - GenUI.GapTiny;
            GUI.color = oldColor;
        }

        public static void DrawOwnerScroll( Rect rect, ref Vector2 vec, List<Pawn> list, Action<Pawn> action )
        {
            var scrollRect = rect.ContractedBy( GenUI.Pad );
            var viewRect = scrollRect;
            if( list.Count > 0 )
            {
                viewRect.height = list.Count * GenUI.ListSpacing;
            }
            viewRect.width -= GenUI.ScrollBarWidth;
            GUI.BeginGroup( scrollRect );
            vec = GUI.BeginScrollView( scrollRect.AtZero(), vec, viewRect.AtZero() );
            {
                //var dump = "list:";
                var pawnRect = new Rect( 0.0f, 0.0f, viewRect.width, GenUI.ListSpacing - GenUI.GapTiny );
                for( int index = list.Count - 1; index >= 0; --index )
                {
                    var pawn = list[ index ];
                    if( pawn != null )
                    {
                        //dump += "\n\tName: " + pawn.Name.ToStringFull;
                        //if( !SelDoor.CompDoor.Owners.Contains( pawn ) )
                        //{
                        //    dump += " (!Owner)";
                        //}
                        Widgets.Label( pawnRect, pawn.Name.ToStringFull );
                        if( Widgets.ButtonInvisible( pawnRect ) )
                        {
                            action.Invoke( pawn );
                        }
                        pawnRect.y += GenUI.ListSpacing;
                    }
                }
                //Log.Message( dump );
            }
            GUI.EndScrollView();
            GUI.EndGroup();
        }

        #endregion

    }

}
