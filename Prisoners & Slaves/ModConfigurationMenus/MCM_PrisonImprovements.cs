using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonersAndSlaves
{
    
    public class MCM_PrisonersAndSlaves : ModConfigurationMenu
    {
        private const GameFont          fontLabel = GameFont.Small;
        private const GameFont          fontCheckbox = GameFont.Small;
        private const GameFont          fontSlider = GameFont.Tiny;

        private const float             entrySize = 24f;
        private const float             innerPadding = 4f;

        #region Widgets

        private void                    DoCheckbox( ref Listing_Standard listing, ref bool value, string labelKey )
        {
            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;

            var rect = listing.GetRect( entrySize );

            var label = labelKey.Translate();
            var labelWidth = rect.width - entrySize - innerPadding;

            var labelHeight = Text.CalcHeight( label, labelWidth );
            if( labelHeight > entrySize )
            {
                rect.height = labelHeight;
                listing.Gap( labelHeight - entrySize );
            }

            Text.Font = fontCheckbox;
            Text.Anchor = TextAnchor.MiddleLeft;

            Vector2 checkVec = new Vector2(
                rect.x,
                rect.y );
            Rect labelRect = new Rect(
                rect.x + entrySize + innerPadding,
                rect.y,
                labelWidth,
                rect.height );

            Widgets.Checkbox(
                checkVec,
                ref value );

            Widgets.Label(
                labelRect,
                label
            );

            Text.Anchor = originalAnchor;
            Text.Font = originalFont;
            listing.Gap();
        }

        private void                    DoLabel( ref Listing_Standard listing, string labelKey )
        {
            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;

            var label = labelKey.Translate();
            var rect = listing.GetRect( entrySize );

            var labelHeight = Text.CalcHeight( label, rect.width );
            if( labelHeight > entrySize )
            {
                rect.height = labelHeight;
                listing.Gap( labelHeight - entrySize );
            }

            Text.Font = fontCheckbox;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(
                rect,
                label
            );

            Text.Anchor = originalAnchor;
            Text.Font = originalFont;
            listing.Gap();
        }

        #endregion

        public override void            ExposeData()
        {
            Scribe_Values.LookValue( ref Data.BoughtSlavesAsColonists, "BoughtSlavesAsColonists", false, false );
            Scribe_Values.LookValue( ref Data.LockedDoorsAllowDrafted, "AllowDraftedToUseLockedDoors", true, false );
            Scribe_Values.LookValue( ref Data.LockedDoorsAllowWardens, "AllowWardensToUseLockedDoors", true, false );
            Scribe_Values.LookValue( ref Data.LockedDoorsAllowDoctors, "AllowDoctorsToUseLockedDoors", false, false );

            if( Scribe.mode == LoadSaveMode.LoadingVars )
            {
                Data.UpdateSlavesBoughtData();
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            Data.UpdateSlavesBoughtData();
        }

        public override float           DoWindowContents( Rect rect )
        {
            #region Save State
            var originalFont = Text.Font;
            #endregion

            var listing = new Listing_Standard( rect );
            {
                listing.ColumnWidth = rect.width;

                #region MCM description
                DoLabel(
                    ref listing,
                    Data.Strings.MCMDescription
                );
                #endregion

                #region Buy as colonists
                DoCheckbox(
                    ref listing,
                    ref Data.BoughtSlavesAsColonists,
                    Data.Strings.MCMBoughtAsColonists
                );
                #endregion

                #region Locked doors

                DoLabel(
                    ref listing,
                    Data.Strings.MCMLockedDoors
                );

                #region Drafted can use locked doors
                DoCheckbox(
                    ref listing,
                    ref Data.LockedDoorsAllowDrafted,
                    Data.Strings.MCMLockedDoorsAllowDrafted
                );
                #endregion

                #region Wardens can use locked doors
                DoCheckbox(
                    ref listing,
                    ref Data.LockedDoorsAllowWardens,
                    Data.Strings.MCMLockedDoorsAllowWardens
                );
                #endregion

                #region Doctors can use locked doors
                DoCheckbox(
                    ref listing,
                    ref Data.LockedDoorsAllowDoctors,
                    Data.Strings.MCMLockedDoorsAllowDoctors
                );
                #endregion

                #endregion

            }
            listing.End();

            #region Restore State
            Text.Font = originalFont;
            #endregion

            return listing.CurHeight;
        }

    }

}
