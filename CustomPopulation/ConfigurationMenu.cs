using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using CommunityCoreLibrary.UI;
using RimWorld;
using Verse;
using UnityEngine;

namespace CustomPopulation
{
    
    public class ConfigurationMenu : ModConfigurationMenu
    {
        private const GameFont          fontLabel = GameFont.Small;
        private const GameFont          fontCheckbox = GameFont.Small;
        private const GameFont          fontSlider = GameFont.Tiny;

        private const float             textEntrySize = 48f;
        private const float             entrySize = 24f;
        private const float             innerPadding = 4f;

        private const string            description = "CP_Description";

        private const string            minPopLabel = "CP_MinPopLabel";
        private const string            maxPopLabel = "CP_MaxPopLabel";
        private const string            critPopLabel = "CP_CritPopLabel";

        private const string            minPopTip = "CP_MinPopTip";
        private const string            maxPopTip = "CP_MaxPopTip";
        private const string            critPopTip = "CP_CritPopTip";

        private List<StorytellerDef>     StorytellerDefs;
        private PopulationOverride[]    populationOverrides;

        #region Base Overrides

        public override float           DoWindowContents( Rect rect )
        {
            var standardListing = new Listing_Standard( rect );
            {

                var descTR = description.Translate();

                var descHeight = Text.CalcHeight( descTR, standardListing.ColumnWidth );
                var descRect = standardListing.GetRect( descHeight );

                CCL_Widgets.Label( descRect, descTR, Color.gray, GameFont.Small, TextAnchor.UpperLeft );

                standardListing.Gap();

                for( var index = 0; index < populationOverrides.Length; index++ )
                {
                    var storytellerLabel = populationOverrides[ index ].storytellerDef.LabelCap;
                    var storytellerRect = standardListing.GetRect( Text.CalcHeight( storytellerLabel, standardListing.ColumnWidth ) );
                    CCL_Widgets.Label( storytellerRect, storytellerLabel, Color.white, GameFont.Small, TextAnchor.UpperLeft );

                    DrawValueGetter( ref standardListing, ref populationOverrides[ index ].desiredPopulationMin, minPopLabel, minPopTip );
                    DrawValueGetter( ref standardListing, ref populationOverrides[ index ].desiredPopulationMax, maxPopLabel, maxPopTip );
                    DrawValueGetter( ref standardListing, ref populationOverrides[ index ].desiredPopulationCritical, critPopLabel, critPopTip );

                    standardListing.Gap();

                }

            }

            return standardListing.CurHeight;
        }

        public override void            ExposeData()
        {
            for( var index = 0; index < populationOverrides.Length; index++ )
            {
                if( Scribe.EnterNode( populationOverrides[ index ].storytellerDef.defName ) )
                {
                    Scribe_Values.LookValue( ref populationOverrides[ index ].desiredPopulationMin, "desiredPopulationMin", populationOverrides[ index ].storytellerDef.desiredPopulationMin, true );
                    Scribe_Values.LookValue( ref populationOverrides[ index ].desiredPopulationMax, "desiredPopulationMax", populationOverrides[ index ].storytellerDef.desiredPopulationMax, true );
                    Scribe_Values.LookValue( ref populationOverrides[ index ].desiredPopulationCritical, "desiredPopulationCritical", populationOverrides[ index ].storytellerDef.desiredPopulationCritical, true );
                    Scribe.ExitNode();
                }
            }
            if( Scribe.mode == LoadSaveMode.LoadingVars )
            {
                UpdateStorytellerValues();
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            StorytellerDefs = DefDatabase<StorytellerDef>.AllDefsListForReading;
            populationOverrides = new PopulationOverride[ StorytellerDefs.Count ];
            int index = 0;
            foreach( var storytellerDef in StorytellerDefs )
            {
                populationOverrides[ index ].storytellerDef = storytellerDef;
                populationOverrides[ index ].desiredPopulationMin = storytellerDef.desiredPopulationMin;
                populationOverrides[ index ].desiredPopulationMax = storytellerDef.desiredPopulationMax;
                populationOverrides[ index ].desiredPopulationCritical = storytellerDef.desiredPopulationCritical;
                index++;
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            UpdateStorytellerValues();
        }
        #endregion

        private void                    UpdateStorytellerValues()
        {
            for( var index = 0; index < populationOverrides.Length; index++ )
            {
                populationOverrides[ index ].storytellerDef.desiredPopulationMin = populationOverrides[ index ].desiredPopulationMin;
                populationOverrides[ index ].storytellerDef.desiredPopulationMax = populationOverrides[ index ].desiredPopulationMax;
                populationOverrides[ index ].storytellerDef.desiredPopulationCritical = populationOverrides[ index ].desiredPopulationCritical;
            }
        }

        #region Widgets

        private void                    DrawValueGetter( ref Listing_Standard listing, ref float value, string labelKey, string tipKey )
        {
            var baseRect = listing.GetRect( entrySize + innerPadding );
            var labelRect = new Rect( baseRect );
            var entryRect = new Rect( baseRect );

            labelRect.width -= textEntrySize;
            entryRect.x += labelRect.width;
            entryRect.width = textEntrySize;
            labelRect.width -= innerPadding;

            var valInt = (int)value;
            var buffer = valInt.ToString();

            CCL_Widgets.Label( labelRect, labelKey.Translate(), Color.gray, GameFont.Small, TextAnchor.MiddleRight, tipKey.Translate() );
            var result = Widgets.TextField( entryRect, buffer );

            if( int.TryParse( result, out valInt ) )
            {
                value = valInt;
            }
        }

        #endregion

    }

}
