using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonImprovements
{
    
    public class MCM_PrisonImprovements : ModConfigurationMenu
    {
        private const GameFont          fontLabel = GameFont.Small;
        private const GameFont          fontCheckbox = GameFont.Small;
        private const GameFont          fontSlider = GameFont.Tiny;

        private const float             entrySize = 24f;
        private const float             innerPadding = 4f;

        #region Widgets

        private void                    DoCheckbox( Rect rect, ref bool value, string labelKey )
        {
            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;

            Text.Font = fontCheckbox;
            Text.Anchor = TextAnchor.MiddleLeft;

            var label = labelKey.Translate();

            Vector2 radioVec = new Vector2(
                rect.x,
                rect.y + ( ( rect.height - entrySize ) / 2 ) );
            float labelWidth = Text.CalcSize( label ).x;
            Rect labelRect = new Rect(
                rect.x + entrySize + innerPadding,
                rect.y,
                labelWidth,
                rect.height );

            Widgets.Checkbox(
                radioVec,
                ref value );

            Widgets.Label(
                labelRect,
                label
            );

            Text.Anchor = originalAnchor;
            Text.Font = originalFont;
        }

        private void                    DoLabel( Rect rect, string label )
        {
            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;

            Text.Font = fontCheckbox;
            Text.Anchor = TextAnchor.MiddleLeft;

            Widgets.Label(
                rect,
                label
            );

            Text.Anchor = originalAnchor;
            Text.Font = originalFont;
        }

        #endregion

        public override void            ExposeData()
        {
            Scribe_Values.LookValue<bool>( ref Data.BoughtSlavesAsColonists, "BoughtSlavesAsColonists", false, true );
            if( Scribe.mode == LoadSaveMode.LoadingVars )
            {
                Data.UpdateData();
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            Data.UpdateData();
        }

        public override float           DoWindowContents( Rect rect )
        {
            #region Save State
            var originalFont = Text.Font;
            #endregion

            #region MCM Description
            var descriptionLabel = "PI_MCM_Description".Translate();
            var descriptionHeight = Text.CalcHeight( descriptionLabel, rect.width );
            var descriptionRect = new Rect(
                0,
                0,
                rect.width,
                descriptionHeight );
            DoLabel(
                descriptionRect,
                descriptionLabel
            );
            #endregion

            #region Buy As Colonists
            var buyRect = new Rect(
                0,
                descriptionRect.y + descriptionRect.height + innerPadding,
                rect.width,
                entrySize );
            DoCheckbox(
                buyRect,
                ref Data.BoughtSlavesAsColonists,
                "PI_MCM_BoughtAsColonists"
            );
            #endregion

            #region Restore State
            Text.Font = originalFont;
            #endregion

            return buyRect.y + buyRect.height + innerPadding;
        }

    }

}
