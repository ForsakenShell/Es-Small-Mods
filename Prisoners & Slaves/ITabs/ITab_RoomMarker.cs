using System;

using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public class ITab_RoomMarker : ITab
    {

        private static readonly Vector2 WinSize;

        static ITab_RoomMarker()
        {
            ITab_RoomMarker.WinSize = new Vector2( 300f, 150f );
        }

        public ITab_RoomMarker()
        {
            this.size = ITab_RoomMarker.WinSize;
            this.labelKey = Data.Strings.ITabSecurityLabel;
        }

        public Building_RoomMarker SelMarker
        {
            get
            {
                return this.SelThing as Building_RoomMarker;
            }
        }

        public override bool IsVisible
        {
            get
            {
                return ( SelMarker != null );
            }
        }

        protected override void FillTab()
        {
            var marker = SelMarker;
            Text.Font = GameFont.Small;
            var listing = new Listing_Standard( new Rect( 10f, 10f, this.size.x - 10f, this.size.y - 10f ).GetInnerRect() );
            {
                #region Marker Name
                var nameRect = listing.GetRect( 30f );
                nameRect.width /= 2.0f;
                Widgets.Label( nameRect, Data.Strings.RoomMarkerName.Translate() );
                nameRect.x += nameRect.width;
                marker.markerName = Widgets.TextField( nameRect, marker.markerName.Substring( 0, Math.Min( marker.markerName.Length, 20 ) ) );
                listing.Gap();
                #endregion

                #region Marker Type
                var typeGroupRect = listing.GetRect( 2 * 28f + 20f);
                Widgets.DrawMenuSection( typeGroupRect, true );
                var position = typeGroupRect.ContractedBy( 10f );
                GUI.BeginGroup( position );
                {
                    var allowRect = new Rect( 0.0f, 0.0f, position.width, 30f );

                    var localValue = SelMarker.AllowPrisoners;
                    ITab_Restricted.DoLabelCheckbox( ref allowRect, Data.Strings.AllowPrisoners, ref localValue );
                    SelMarker.AllowPrisoners = localValue;

                    localValue = SelMarker.AllowSlaves;
                    ITab_Restricted.DoLabelCheckbox( ref allowRect, Data.Strings.AllowSlaves, ref localValue );
                    SelMarker.AllowSlaves = localValue;
                }
                GUI.EndGroup();
                listing.Gap();
                #endregion

            }
            listing.End();
        }

    }

}
