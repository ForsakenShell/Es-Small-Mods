using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommunityCoreLibrary;
using CommunityCoreLibrary.MiniMap;
using CommunityCoreLibrary.UI;
                          
using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class MiniMapOverlay_NonColonistPawns : CommunityCoreLibrary.MiniMap.MiniMapOverlay_NonColonistPawns, IConfigurable
    {
        
        private Color                   prisonerColor = new Color( 0.33f, 0.185f, 0.0f, 1.0f );
        private LabeledInput_Color      prisonerColorField;
        private int                     prisonerRadius = 2;
        private LabeledInput_Int        prisonerRadiusField;

        private Color                   slaveColor = Color.yellow;
        private LabeledInput_Color      slaveColorField;
        private int                     slaveRadius = 2;
        private LabeledInput_Int        slaveRadiusField;

        #region Constructors

        public MiniMapOverlay_NonColonistPawns( MiniMap minimap, MiniMapOverlayDef overlayDef ) : base( minimap, overlayDef )
        {
            CreateInputFields();
        }

        #endregion Constructors

        #region Methods

        public new float DrawMCMRegion( Rect InRect )
        {
            var baseHeight = base.DrawMCMRegion( InRect );
            Rect row = InRect;
            row.height = 24f;
            row.y += baseHeight;

            prisonerColorField.Draw( row );
            prisonerColor = prisonerColorField.Value;
            row.y += 30f;

            prisonerRadiusField.Draw( row );
            prisonerRadius = prisonerRadiusField.Value;
            row.y += 30f;

            slaveColorField.Draw( row );
            slaveColor = slaveColorField.Value;
            row.y += 30f;

            slaveRadiusField.Draw( row );
            slaveRadius = slaveRadiusField.Value;
            row.y += 30f;

            return baseHeight + 4 * 30f;
        }

        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue( ref prisonerColor, "prisonerColor" );
            Scribe_Values.LookValue( ref prisonerRadius, "prisonerRadius" );
            Scribe_Values.LookValue( ref slaveColor, "slaveColor" );
            Scribe_Values.LookValue( ref slaveRadius, "slaveRadius" );

            // re-create input fields to update values
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                UpdateInputFields();
            }
        }

        public override IEnumerable<Pawn> GetPawns()
        {
            if( Current.ProgramState == ProgramState.MapPlaying )
            {
                var nonColonists = Find.MapPawns.AllPawnsSpawned.Where( pawn => (
                    ( !pawn.RaceProps.Animal )&&
                    (
                        ( pawn.Faction != Faction.OfPlayer )||
                        ( pawn.IsPrisonerOfColony )||
                        ( pawn.IsSlaveOfColony() )
                    )
                ) );
                var colonists = Find.MapPawns.FreeColonists.ToList();
                var markers = Data.AllRoomMarkersOfColony().Where( marker => marker.CurrentlyMonitored ).ToList();

                foreach( var pawn in nonColonists )
                {
                    if( pawn.CanBeSeenByColony( colonists, markers ) )
                    {
                        yield return pawn;
                    }
                }
            }
        }

        public override Color GetColor( Pawn pawn )
        {
            if( pawn.IsSlaveOfColony() )
            {
                return slaveColor;
            }

            if( pawn.IsPrisonerOfColony )
            {
                return prisonerColor;
            }

            return base.GetColor( pawn );
        }

        public override float GetRadius( Pawn pawn )
        {
            if( pawn.IsSlaveOfColony() )
            {
                return slaveRadius;
            }

            if( pawn.IsPrisonerOfColony )
            {
                return prisonerRadius;
            }

            return base.GetRadius( pawn );
        }

        private void CreateInputFields()
        {
            prisonerColorField = new LabeledInput_Color(
                prisonerColor,
                Data.Strings.OverlayPrisonerColor.Translate(),
                Data.Strings.OverlayPrisonerColorTip.Translate()
            );
            prisonerRadiusField = new LabeledInput_Int(
                prisonerRadius,
                Data.Strings.OverlayPrisonerRadius.Translate(),
                Data.Strings.OverlayPrisonerRadiusTip.Translate()
            );
            slaveColorField    = new LabeledInput_Color(
                slaveColor,
                Data.Strings.OverlaySlaveColor.Translate(),
                Data.Strings.OverlaySlaveColorTip.Translate()
            );
            slaveRadiusField = new LabeledInput_Int(
                slaveRadius,
                Data.Strings.OverlaySlaveRadius.Translate(),
                Data.Strings.OverlaySlaveRadiusTip.Translate()
            );
        }

        private void UpdateInputFields()
        {
            prisonerColorField.Value  = prisonerColor;
            prisonerRadiusField.Value = prisonerRadius;
            slaveColorField.Value     = slaveColor;
            slaveRadiusField.Value    = slaveRadius;
        }

        #endregion Methods

    }

}
