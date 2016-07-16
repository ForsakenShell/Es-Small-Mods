using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using CommunityCoreLibrary.MiniMap;
using CommunityCoreLibrary.UI;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public class MiniMapOverlay_SecurityCameras : MiniMapOverlay, IConfigurable
    {

        private Color                   unmonitoredColor;
        private LabeledInput_Color      unmonitoredInput;

        private Color                   monitoredColor;
        private LabeledInput_Color      monitoredInput;

        public MiniMapOverlay_SecurityCameras( MiniMap minimap, MiniMapOverlayDef overlayDef ) : base( minimap, overlayDef )
        {
            unmonitoredColor = new Color( 0.125f, 0.125f, 0.0f, 1.0f );
            unmonitoredInput = new LabeledInput_Color(
                this.unmonitoredColor,
                Data.Strings.OverlayUnmonitoredColor.Translate(),
                Data.Strings.OverlayUnmonitoredColorTip.Translate()
            );
            monitoredColor = new Color( 0.25f, 0.25f, 0.0f, 1.0f );
            monitoredInput = new LabeledInput_Color(
                this.monitoredColor,
                Data.Strings.OverlayMonitoredColor.Translate(),
                Data.Strings.OverlayMonitoredColorTip.Translate()
            );
        }

        public override void Update()
        {
            if( Current.ProgramState != ProgramState.MapPlaying )
            {
                return;
            }

            ClearTexture();

            var markers = Data.AllRoomMarkersOfColony().Where( marker => marker.AllowRemoteMonitoring ).ToList();

            if( markers.NullOrEmpty() )
            {
                return;
            }

            // Sort markers so the markers which are currently being viewed recently are at the bottom
            markers.Sort( (x,y)=>
            {
                return x.CurrentlyMonitored ? 1 : y.CurrentlyMonitored ? -1 : 0;
            } );

            foreach( var marker in markers )
            {
                var compRemote = marker.compRemote;
                var observableCells = compRemote.ObservableCells;
                if( !observableCells.NullOrEmpty() )
                {
                    var color = marker.CurrentlyMonitored ? monitoredColor : unmonitoredColor;
                    foreach( var cell in observableCells )
                    {
                        texture.SetPixel( cell.x, cell.z, color );
                    }
                }
            }

        }

        public float DrawMCMRegion( Rect InRect )
        {
            Rect canvas = InRect;
            canvas.height = 24f;

            unmonitoredInput.Draw( canvas );
            unmonitoredColor = unmonitoredInput.Value;
            canvas.y += 30f;

            monitoredInput.Draw( canvas );
            monitoredColor = monitoredInput.Value;
            canvas.y += 30f;

            return 2f * 30f;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<Color>( ref unmonitoredColor, "PaS_UnmonitoredColor", new Color(), false );
            Scribe_Values.LookValue<Color>( ref monitoredColor, "PaS_MonitoredColor", new Color(), false );
            if( Scribe.mode != LoadSaveMode.LoadingVars )
                return;
            unmonitoredInput.Value = unmonitoredColor;
            monitoredInput.Value = monitoredColor;
        }

    }

}
