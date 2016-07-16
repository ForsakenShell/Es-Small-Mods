using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{

    public class Building_RestrictedDoor : Building_Door
    {

        private Dictionary<Pawn,bool>           cachedRestrictionChecks;

        private bool                            _queueUpdate;

        private CompPower                       _compPower;
        public CompPower                        CompPower
        {
            get
            {
                if( _compPower == null )
                {
                    _compPower = this.TryGetComp<CompPower>();
                }
                return _compPower;
            }
        }

        private List<CompRestricted>            _listRestricted;
        public List<CompRestricted>             ListRestricted
        {
            get
            {
                if( _listRestricted == null )
                {
                    _listRestricted = this.CompsRestricted();
                    _listRestricted.Sort( (x, y) => ( x.Priority > y.Priority ? -1
                                                     : x.Priority < y.Priority ? 1
                                                     : 0 ) );
                }
                return _listRestricted;
            }
        }


        public                                  Building_RestrictedDoor()
        {
            cachedRestrictionChecks = new Dictionary<Pawn, bool>();
        }

        #region Base Overrides

        public override void                    DrawGUIOverlay()
        {
            var restrictedComps = this.ListRestricted;
            if( !restrictedComps.NullOrEmpty() )
            {
                foreach( var comp in restrictedComps )
                {
                    comp.PostDrawGUIOverlay();
                }
            }
            base.DrawGUIOverlay();
        }

        public override bool                    PawnCanOpen( Pawn p )
        {
            var restrictedComps = this.ListRestricted;
            if( restrictedComps.NullOrEmpty() )
            {
                return base.PawnCanOpen( p );
            }

            // Cache isEscaping for the method
            var isEscaping = PrisonBreakUtility.IsPrisonBreaking( p );

            // Check all non-cacheable restricted comps
            foreach( var comp in ListRestricted )
            {
                if( !comp.CacheResults )
                {
                    if( !comp.PawnCanOpen( p, isEscaping ) )
                    {
                        return false;
                    }
                }
            }

            // Check all cacheable restricted comps
            bool rVal;
            if( cachedRestrictionChecks.TryGetValue( p, out rVal ) )
            {   // Return from cache of previous checks
                return rVal;
            }

            // Check pawn against comps PawnCanOpen()
            foreach( var comp in restrictedComps )
            {
                if( !comp.PawnCanOpen( p, isEscaping ) )
                {
                    //Log.Message( string.Format( "Building_RestrictedDoor :: Comp {0} returned false for PawnCanOpen( {1}, {2} )", comp.GetType().ToString(), p.NameStringShort, isEscaping ) );
                    cachedRestrictionChecks.Add( p, false );
                    return false;
                }
            }

            // Check faction hostility
            if( p.HostFaction != null )
            {
                rVal = !p.HostFaction.HostileTo( this.Faction );
                //Log.Message( string.Format( "{0} .HostFaction.HostileTo( {1} ) = {2}", p.NameStringShort, this.Faction.GetCallLabel(), !rVal ) );
            }
            else
            {
                rVal = !p.Faction.HostileTo( this.Faction );
                //Log.Message( string.Format( "{0} .Faction.HostileTo( {1} ) = {2}", p.NameStringShort, this.Faction.GetCallLabel(), !rVal ) );
            }
            cachedRestrictionChecks.Add( p, rVal );
            return rVal;
        }

        public override void                    PostMake()
        {
            base.PostMake();
            var restrictedComps = this.ListRestricted;
            if( !restrictedComps.NullOrEmpty() )
            {
                foreach( var comp in restrictedComps )
                {
                    comp.PostCompMake();
                }
            }
        }

        /*
        protected override void                 ReceiveCompSignal( string signal )
        {
            if(
                ( signal == CompPowerTrader.PowerTurnedOffSignal )||
                ( signal == CompPowerTrader.PowerTurnedOnSignal )||
                ( signal == Data.Signal.InternalRecache )
            )
            {
                this.QueueDoorStatusUpdate();
            }
            base.ReceiveCompSignal( signal );
        }
        */

        public override void                    SpawnSetup()
        {
            base.SpawnSetup();
            UpdateDoorStatus();
        }

        public override void                    Tick()
        {
            base.Tick();
            if(
                ( _queueUpdate )&&
                ( this.IsHashIntervalTick( Data.QueueUpdateTicks ) )
            )
            {
                UpdateDoorStatus();
            }
        }

#if DEBUG
        public override string GetInspectString()
        {   // Report list of cached pawns and PawnCanOpen() results
            if( cachedRestrictionChecks.Count == 0 )
            {
                return base.GetInspectString();
            }
            var str = "cache: ";
            foreach( var pair in cachedRestrictionChecks )
            {
                str += pair.Key.NameStringShort + " (" + pair.Value + "), ";
            }
            return base.GetInspectString() + str;
        }
#endif

        #endregion

        public void                             QueueDoorStatusUpdate( bool immediateUpdate = false )
        {
            if( immediateUpdate )
            {
                UpdateDoorStatus();
            }
            else
            {
                _queueUpdate = true;
            }
        }

        private void                            UpdateDoorStatus()
        {
            // Flush short-term cache
            _queueUpdate = false;
            cachedRestrictionChecks.Clear();
            foreach( var comp in ListRestricted )
            {
                comp.UpdateCompStatus();
            }
            //this.BroadcastCompSignal( Data.Signal.UpdateStatus );
        }

    }

}
