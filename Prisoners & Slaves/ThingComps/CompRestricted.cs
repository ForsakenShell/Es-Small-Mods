using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public abstract class CompRestricted : ThingComp
    {

        // Child classes must implement these
        public abstract bool            CacheResults { get; }
        public abstract int             Priority { get; }
        public abstract bool            PawnCanOpen( Pawn p, bool isEscaping );
        public abstract void            ClearCache();

        // Child classes may override these

        #region UI
        public virtual int              ColumnIndex
        {
            get
            {
                return -1;
            }
        }

        public virtual int              ColumnWidth
        {
            get
            {
                return 1;
            }
        }

        public virtual int              ColumnsVisible
        {
            get
            {
                return 1;
            }
        }

        public virtual bool             CompletelyEnforced
        {
            get
            {
                return false;
            }
        }

        public virtual void             DrawColumns( Listing_Standard listing, float physicalHeight, bool overridden = false )
        {
        }

        public virtual void             PostDrawGUIOverlay()
        {
        }

        #endregion

        #region Update Comp State

        public abstract void            UpdateCompStatus();

        public virtual void             PostCompMake()
        {
        }

        #endregion

        #region Helper Methods

        /*
        public void                     QueueUpdateOnParent( bool immediateUpdate = false )
        {
            if( Door != null )
            {
                Door.QueueDoorStatusUpdate( immediateUpdate );
            }
        }
        */

        /*
        public override void            ReceiveCompSignal( string signal )
        {
            if( signal == Data.Signal.UpdateStatus )
            {
                this.UpdateCompStatus();
            }
            base.ReceiveCompSignal( signal );
        }
        */

        #endregion

        protected Building_RestrictedDoor Door
        {
            get
            {
                return this.parent as Building_RestrictedDoor;
            }
        }

    }

}
