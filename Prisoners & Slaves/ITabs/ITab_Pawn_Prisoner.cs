using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using CommunityCoreLibrary;

using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonersAndSlaves
{

    public class ITab_Pawn_Prisoner : RimWorld.ITab_Pawn_Prisoner
    {

        private const float innerWidth = 200f;
        private const float innerBorder = 10f;

        private float tabHeight = 0f;

        private List<KeyValuePair<string, Thing>> cachedMarkers;

        public ITab_Pawn_Prisoner() : base()
        {
            this.size.x = innerWidth + innerBorder * 2;
        }

        public override bool IsVisible
        {
            get
            {
                this.size.y = tabHeight + innerBorder * 2;
                if( this.SelPawn.IsSlaveOfColony() )
                {
                    this.labelKey = Data.Strings.ITabSlaveLabel;
                    return true;
                }
                this.labelKey = Data.Strings.ITabPrisonerLabel;
                return ( this.SelPawn.IsPrisonerOfColony );
            }
        }

        protected override void FillTab()
        {
            var compPrisoner = this.SelPawn.TryGetComp<CompPrisoner>();
            bool isSlave = (
                ( this.SelPawn.IsSlaveOfColony() ) &&
                ( compPrisoner != null )
            );

            ConceptDatabase.KnowledgeDemonstrated( ConceptDefOf.PrisonerTab, KnowledgeAmount.GuiFrame );
            Text.Font = GameFont.Small;
            var iTabInnerRect = new Rect( 0.0f, 0.0f, this.size.x, 9999f ).ContractedBy( innerBorder );

            var iTabListing = new Listing_Standard( iTabInnerRect );
            {

                #region Debug
                // Window will automagically grow in height if needed
                if( Prefs.DevMode )
                {
                    var textFont = Text.Font;
                    Text.Font = GameFont.Tiny;
                    var devRect = iTabListing.GetRect( 7f * 16f );
                    devRect.height = 20f;
                    Widgets.Label( devRect, "Debug Info:" );
                    devRect.y += 16f;
                    Widgets.Label( devRect, string.Format( "MTB Prison break (days): {0}", (int) PrisonBreakUtility.InitiatePrisonBreakMtbDays( this.SelPawn ) ) );
                    devRect.y += 16f;
                    Widgets.Label( devRect, string.Format( "Odds of escape attempt: {0}", (int) ( 100f * this.SelPawn.EscapeProbability() ) ) );
                    devRect.y += 16f;
                    Widgets.Label( devRect, string.Format( "Mood: {0}", (int) ( 100f * this.SelPawn.needs.mood.CurLevel ) ) );
                    devRect.y += 16f;
                    Widgets.Label( devRect, string.Format( "Can be seen by colony: {0}", this.SelPawn.CanBeSeenByColony().ToString() ) );
                    devRect.y += 16f;
                    Widgets.Label( devRect, string.Format( "Thinks they can be seen: {0}", this.SelPawn.ThinksTheyCanBeSeenByColony().ToString() ) );
                    devRect.y += 16f;

                    #region Prisoner Recruitment Difficulty
                    if( !isSlave )
                    {
                        Widgets.Label( devRect, Data.Strings.RecruitmentDifficulty.Translate() + ": " + this.SelPawn.RecruitDifficulty( Faction.OfPlayer, false ).ToString( "##0" ) );
                        devRect.y += 16f;
                    }
                    #endregion

                    Text.Font = textFont;
                    Widgets.DrawLineHorizontal( 0f, iTabListing.CurHeight, iTabListing.ColumnWidth );
                    iTabListing.Gap( innerBorder );
                }
                #endregion

                #region Original Slave Faction and Role
                if( isSlave )
                {
                    var textFont = Text.Font;
                    Text.Font = GameFont.Tiny;
                    iTabListing.Label( Data.Strings.FactionLabel.Translate( compPrisoner.originalFaction.Name ) );
                    iTabListing.Label( Data.Strings.PawnKindLabel.Translate( compPrisoner.originalPawnKind.LabelCap ) );
                    Text.Font = textFont;
                }
                #endregion

                #region Prisoner/Slave Interaction Options
                var interactionsRect = iTabListing.GetRect( innerBorder * 2f + 28f * ( isSlave ? 4 : 6 ) );
                Widgets.DrawMenuSection( interactionsRect, true );
                var interactionsInnerRect = interactionsRect.ContractedBy( innerBorder );
                GUI.BeginGroup( interactionsInnerRect );
                {
                    var optionRect = new Rect( 0.0f, 0.0f, interactionsInnerRect.width, 30f );

                    #region Core Options
                    foreach( PrisonerInteractionMode mode in Enum.GetValues( typeof( PrisonerInteractionMode ) ) )
                    {
                        // Can't recruit slaves and they have their own release mechanism
                        bool showThis = (
                            ( !isSlave ) ||
                            (
                                ( mode != PrisonerInteractionMode.AttemptRecruit ) &&
                                ( mode != PrisonerInteractionMode.Release )
                            )
                        );
                        if( showThis )
                        {
                            if( Widgets.RadioButtonLabeled( optionRect, mode.GetLabel(), this.SelPawn.guest.interactionMode == mode ) )
                            {
                                this.SelPawn.guest.interactionMode = mode;
                            }
                            optionRect.y += 28f;
                        }
                    }
                    #endregion

                    #region Prisoner Specific Options
                    if( !isSlave )
                    {
                        // Enslave prisoner
                        if( Widgets.RadioButtonLabeled( optionRect, Data.Strings.Enslave.Translate(), this.SelPawn.guest.interactionMode == Data.PIM.EnslavePrisoner ) )
                        {
                            this.SelPawn.guest.interactionMode = Data.PIM.EnslavePrisoner;
                        }
                        optionRect.y += 28f;
                    }
                    #endregion

                    #region Slave Specific Options
                    if( isSlave )
                    {
                        // Free slave
                        if( Widgets.RadioButtonLabeled( optionRect, Data.Strings.FreeSlave.Translate(), this.SelPawn.guest.interactionMode == Data.PIM.FreeSlave ) )
                        {
                            this.SelPawn.guest.interactionMode = Data.PIM.FreeSlave;
                        }
                        optionRect.y += 28f;
                    }
                    #endregion

                }
                GUI.EndGroup();
                iTabListing.Gap( innerBorder );
                #endregion

                if( compPrisoner != null )
                {

                    #region [Cancel] Transfer Button
                    var transferOrCancelRect = iTabListing.GetRect( 30f );

                    #region Transfer to a valid Marker or Prison Cell

                    if( !compPrisoner.ShouldBeTransfered )
                    {
                        if( Widgets.ButtonText( transferOrCancelRect, Data.Strings.TransferPrisoner.Translate() ) )
                        {
                            var floatMenuOptions = this.TransferTargetFloatMenuOptions();
                            if(
                                ( floatMenuOptions.Count == 0 ) ||
                                (
                                    ( floatMenuOptions.Count == 1 ) &&
                                    ( floatMenuOptions[ 0 ].Label == Data.Strings.PrisonCell.Translate() ) &&
                                    ( this.SelPawn.GetRoom() == this.SelPawn.ownership.OwnedBed.GetRoom() )
                                )
                            )
                            {
                                floatMenuOptions.Add( new FloatMenuOption( Data.Strings.InstallMarker.Translate(), (Action) null ) );
                            }
                            Find.WindowStack.Add( (Window) new FloatMenu( floatMenuOptions ) );
                        }
                        else
                        {
                            this.cachedMarkers = null;
                        }
                    }
                    #endregion

                    #region Cancel Transfer

                    if( compPrisoner.ShouldBeTransfered )
                    {
                        if( Widgets.ButtonText( transferOrCancelRect, Data.Strings.CancelTransfer.Translate() ) )
                        {
                            compPrisoner.haulTarget = null;
                        }
                    }

                    #endregion

                    iTabListing.Gap( innerBorder );
                    #endregion

                    #region Transfer Target

                    var transferTargetRect = iTabListing.GetRect( 30f );
                    var style = new GUIStyle( Text.CurTextFieldStyle );
                    style.alignment = TextAnchor.MiddleCenter;

                    string label = string.Empty;
                    if( compPrisoner.haulTarget != null )
                    {
                        if( compPrisoner.haulTarget is Building_RoomMarker )
                        {
                            label = ( (Building_RoomMarker) compPrisoner.haulTarget ).markerName;
                        }
                        else if( compPrisoner.haulTarget is Building_Bed )
                        {
                            label = Data.Strings.PrisonCell.Translate();
                        }
                    }
                    GUI.Label( transferTargetRect, label, style );
                    iTabListing.Gap( innerBorder );
                    #endregion

                    #region Restraints
                    var restraintsRect = iTabListing.GetRect( innerBorder * 2f + 28f * 2f );
                    Widgets.DrawMenuSection( restraintsRect, true );
                    var restraintsInnerRect = restraintsRect.ContractedBy( innerBorder );
                    GUI.BeginGroup( restraintsInnerRect );
                    {
                        var restrainRect = new Rect( 0.0f, 0.0f, restraintsInnerRect.width, 30f );

                        Widgets.CheckboxLabeled( restrainRect, Data.Strings.RestrainCuff.Translate(), ref compPrisoner.ShouldBeCuffed );
                        restrainRect.y += 28f;

                        Widgets.CheckboxLabeled( restrainRect, Data.Strings.RestrainShackle.Translate(), ref compPrisoner.ShouldBeShackled );
                        restrainRect.y += 28f;
                    }
                    GUI.EndGroup();
                    #endregion

                }

                #region Food
                var getsFood = this.SelPawn.guest.GetsFood;
                iTabListing.CheckboxLabeled( Data.Strings.GetsFood.Translate(), ref getsFood, (string) null );
                this.SelPawn.guest.GetsFood = getsFood;
                #endregion

                #region Medicine
                var medicalCareRect = iTabListing.GetRect( 28f );
                medicalCareRect.width = MedicalCareUtility.CareSetterWidth;
                MedicalCareUtility.MedicalCareSetter( medicalCareRect, ref this.SelPawn.playerSettings.medCare );
                #endregion

            }
            iTabListing.End();
            tabHeight = iTabListing.CurHeight;
        }

        private List<KeyValuePair<string, Thing>> GetHaulToTargets()
        {
            if( this.cachedMarkers != null )
            {
                return this.cachedMarkers;
            }
            var list = new List<KeyValuePair<string, Thing>>();
            if( this.SelPawn.ownership.OwnedBed != null )
            {
                list.Add( new KeyValuePair<string, Thing>( Data.Strings.PrisonCell.Translate(), this.SelPawn.ownership.OwnedBed ) );
            }
            foreach( var marker in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_RoomMarker>() )
            {
                if(
                    ( marker.markerName != null ) &&
                    ( marker.markerName.Trim().Length > 0 ) &&
                    ( marker.IsActive )
                )
                {
                    list.Add( new KeyValuePair<string, Thing>( marker.markerName, marker ) );
                }
            }
            this.cachedMarkers = list;
            return list;
        }

        private List<FloatMenuOption> TransferTargetFloatMenuOptions()
        {
            var list = new List<FloatMenuOption>();
            foreach( var keyValuePair in this.GetHaulToTargets() )
            {
                var action = new Action( () =>
                {
                    var compPrisoner = this.SelPawn.GetComp<CompPrisoner>();
                    if( compPrisoner == null )
                    {
                        return;
                    }
                    compPrisoner.haulTarget = keyValuePair.Value;
                } );
                var key = keyValuePair.Key;
                list.Add( new FloatMenuOption( key, action ) );
            }
            return list;
        }

    }

}
