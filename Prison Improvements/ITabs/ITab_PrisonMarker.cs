using System;

using UnityEngine;
using Verse;

namespace PrisonImprovements
{

    public class ITab_PrisonMarker : ITab
    {

        private static readonly Vector2 WinSize;

        static ITab_PrisonMarker()
        {
            ITab_PrisonMarker.WinSize = new Vector2( 300f, 100f );
        }

        public ITab_PrisonMarker()
        {
            this.size = ITab_PrisonMarker.WinSize;
            this.labelKey = "PI_PrisonMarker_Label";
        }

        protected override void FillTab()
        {
            var marker = this.SelThing as Building_PrisonMarker;
            Text.Font = GameFont.Small;
            GUI.BeginGroup( new Rect( 10f, 10f, this.size.x - 10f, this.size.y - 10f ).GetInnerRect() );
            Widgets.Label( new Rect( 0.0f, 5f, this.size.x / 2f, 30f ), "PI_Camera_Name".Translate() );
            marker.markerName = Widgets.TextField( new Rect( 90f, 0.0f, this.size.x / 2f, 30f ), marker.markerName.Substring( 0, Math.Min( marker.markerName.Length, 20 ) ) );
            GUI.EndGroup();
        }

    }

}
