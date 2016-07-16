using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommunityCoreLibrary;
using CommunityCoreLibrary.MiniMap;

using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class MiniMapOverlay_Wildlife : CommunityCoreLibrary.MiniMap.MiniMapOverlay_Wildlife
    {
        
        #region Constructors

        public MiniMapOverlay_Wildlife( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
        {
        }

        #endregion Constructors

        #region Methods

        public override IEnumerable<Pawn> GetPawns()
        {
            if( Current.ProgramState == ProgramState.MapPlaying )
            {
                var animals = Find.MapPawns.AllPawns.Where( pawn => pawn.RaceProps.Animal );
                var colonists = Find.MapPawns.FreeColonists.ToList();
                var markers = Data.AllRoomMarkersOfColony().Where( marker => marker.CurrentlyMonitored ).ToList();

                foreach( var animal in animals )
                {
                    if( animal.CanBeSeenByColony( colonists, markers ) )
                    {
                        yield return animal;
                    }
                }
            }
        }

        #endregion Methods

    }

}
