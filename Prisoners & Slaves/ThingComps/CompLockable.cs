using System.Collections.Generic;

using CommunityCoreLibrary;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    public class CompLockable : CompRestricted
    {

        #region Constants

        private const int LockAttemptTimeOut = 800;

        private const int BaseTimeToLock = 120;
        private const int BaseTimeToUnlock = 120;
        private const int BaseTimeToPickLock = 360;

        private const float LockFactorForRopeRestraints = 8.0f;
        private const float LockFactorPerQualityLevel = 0.05f;
        private const float UnlockFactorPerQualityLevel = 0.05f;
        private const float PickFactorPerQualityLevel = 0.375f;

        private const int NonQualityQuality = (int) QualityCategory.Normal;

        #endregion

        #region Instance Data

        private bool locked; // Only drafted colonists and wardens may pass locked doors
        private bool picked; // Unless it's been picked
        public int changeStateAfterTick;
        public bool setLockState;
        public bool temporaryLock; // Used using prison escapes

        #endregion

        #region Properties

        public bool Picked
        {
            get
            {
                if( !locked )
                {
                    return false;
                }
                return picked;
            }
            set
            {
                picked = value;
            }
        }

        public bool Locked
        {
            get
            {
                if( picked )
                {
                    return false;
                }
                return locked;
            }
            set
            {
                setLockState = value;
            }
        }

        public bool IssueLockToggleJob
        {
            get
            {
                if( temporaryLock )
                {
                    if(
                        ( locked != temporaryLock ) &&
                        ( Find.TickManager.TicksGame >= changeStateAfterTick )
                    )
                    {
                        return true;
                    }
                    return false;
                }
                if( locked != setLockState )
                {
                    return ( Find.TickManager.TicksGame >= changeStateAfterTick );
                }
                return false;
            }
        }

        public void ChangeLockState( bool value )
        {
            if( value == locked )
            {
                return;
            }
            Log.Message( string.Format( "{0}.ChangeLockState( {1} )", parent.ThingID, value ) );
            locked = value;
            picked = false;
            changeStateAfterTick = Find.TickManager.TicksGame + LockAttemptTimeOut;
        }

        #endregion

        #region Other Comps etc

        private CompQuality _compQuality;
        public CompQuality CompQuality
        {
            get
            {
                if( _compQuality == null )
                {
                    _compQuality = this.parent.TryGetComp<CompQuality>();
                }
                return _compQuality;
            }
        }

        private ThingDef _stuff;
        public ThingDef Stuff
        {
            get
            {
                if( _stuff == null )
                {
                    _stuff = this.parent.Stuff;
                }
                return _stuff;
            }
        }

        public CompEquippable Weapon( Pawn p )
        {
            if(
                ( p == null ) ||
                ( p.equipment == null )
            )
            {
                return null;
            }
            return p.equipment.PrimaryEq;
        }

        public Pawn Wearer
        {
            get
            {
                var apparel = this.parent as Apparel;
                if( apparel == null )
                {
                    return null;
                }
                return apparel.wearer;
            }
        }

        #endregion

        #region Constructor

        public CompLockable()
        {
            locked = false;
            picked = false;
        }

        #endregion

        #region Base Overrides

        public override bool CacheResults
        {
            get
            {
                return false;
            }
        }

        public override int Priority
        {
            get
            {
                return 10;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.LookValue( ref locked, "locked" );
            Scribe_Values.LookValue( ref picked, "picked" );
            Scribe_Values.LookValue( ref changeStateAfterTick, "changeStateAfterTick" );
            Scribe_Values.LookValue( ref setLockState, "setToLockedStatus" );
            Scribe_Values.LookValue( ref temporaryLock, "temporaryLock" );
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if(
                ( parent is Building_RestrictedDoor )&&
                ( Locked )
            )
            {
                CommunityCoreLibrary.OverlayDrawer.RenderPulsingOverlay( parent, Data.Materials.Locked, 6 );
            }
        }

        public override string CompInspectStringExtra()
        {
            var str = string.Empty;
            var baseState = temporaryLock
                ? Data.Strings.DoorLockAutoLockdown
                : Data.Strings.DoorLockInspect;
            str += baseState.Translate(
                locked
                ? Data.Strings.DoorLockLocked.Translate()
                : picked
                ? Data.Strings.DoorLockPicked.Translate()
                : Data.Strings.DoorLockUnlocked.Translate() );
            str += base.CompInspectStringExtra();
            return str;
        }

        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if( this.parent.Faction == Faction.OfPlayer )
            {
                var lockCommand = new Command_Toggle();
                lockCommand.defaultLabel = Data.Strings.DoorLockToggle.Translate();
                lockCommand.defaultDesc = Data.Strings.DoorLockToggleDesc.Translate();
                lockCommand.groupKey = 912515;
                //lockCommand.hotKey = KeyBindingDefOf.CommandItemForbid;
                lockCommand.icon = Data.Icons.Lock;
                lockCommand.isActive = () =>
                {
                    return locked;
                };
                lockCommand.toggleAction = () =>
                {
                    if( setLockState == locked )
                    {
                        Locked = !Locked;
                    }
                    else
                    {
                        Locked = locked;
                    }
                };
                yield return lockCommand;
            }
            foreach( var baseCommand in base.CompGetGizmosExtra() )
            {
                yield return baseCommand;
            }
        }

        #endregion

        #region Check if Locked to Pawn

        public override bool PawnCanOpen( Pawn p, bool isEscaping )
        {
            // Not locked or picked
            if( !Locked )
            {   // Not locked to anyone
                return true;
            }
            if( p.IsPrisonerOfColony )
            {   // Locked to prisoners (and slaves)
                return false;
            }
            if( p.Faction != Faction.OfPlayer )
            {   // Locked to non-colonists
                return false;
            }
            //Log.Message( string.Format( "{0}.Drafted = {1}", p.NameStringShort, p.Drafted ) );
            if(
                ( Data.LockedDoorsAllowDrafted )&&
                ( p.Drafted )
            )
            {   // Is it unlocked to drafted?
                return true;
            }
            if( // Is it unlocked to wardens or doctors?
                ( p.workSettings != null )&&
                ( p.workSettings.EverWork )&&
                (
                    (
                        ( Data.LockedDoorsAllowWardens )&&
                        ( p.workSettings.WorkIsActive( WorkTypeDefOf.Warden ) )
                    ) ||
                    (
                        ( Data.LockedDoorsAllowDoctors )&&
                        ( p.workSettings.WorkIsActive( WorkTypeDefOf.Doctor ) )
                    )
                )
            )
            {
                return true;
            }
            // Locked to everyone else
            return false;
        }

        #endregion

        #region Get ticks to toggle lock

        public int LockToggleTime( Pawn pawn )
        {
            if( setLockState )
            {
                return (int) LockTime( pawn );
            }
            return (int) UnlockTime( pawn );
        }

        #endregion

        #region Get ticks to lock

        public float LockTime( Pawn locker )
        {
            if( Locked )
            {   // Already locked
                return 0.0f;
            }
            var baseFactor = 1.0f;
            var qualityInt = NonQualityQuality;
            if( CompQuality != null )
            {
                qualityInt = (int) CompQuality.Quality;
            }
            baseFactor -= qualityInt * LockFactorPerQualityLevel;
            if(
                ( this.Stuff != null )&&
                ( !this.Stuff.stuffCategories.NullOrEmpty() )
            )
            {
                if( this.Stuff.stuffCategories.Contains( StuffCategoryDefOf.Fabric ) )
                {
                    baseFactor += LockFactorForRopeRestraints;
                }
            }
            // Adjust for lockers manipulation
            baseFactor /= locker.health.capacities.GetEfficiency( PawnCapacityDefOf.Manipulation );
            var lockTime = BaseTimeToLock * baseFactor;
            // Return time for colonists
            return lockTime;
        }

        #endregion

        #region Get ticks to unlock

        public float UnlockTime( Pawn unlocker )
        {
            if( Picked )
            {   // Already picked
                return 0.0f;
            }
            var baseFactor = 1.0f;
            var qualityInt = NonQualityQuality;
            if( CompQuality != null )
            {
                qualityInt = (int) CompQuality.Quality;
            }
            baseFactor -= qualityInt * UnlockFactorPerQualityLevel;
            if( // Trying to remove own restraints or is a prisoner
                ( unlocker == Wearer )||
                ( unlocker.IsPrisoner )
            )
            {   // Pickers take time to pick
                return PickTime( unlocker );
            }
            // Adjust for unlockers manipulation
            baseFactor /= unlocker.health.capacities.GetEfficiency( PawnCapacityDefOf.Manipulation );
            var unlockTime = BaseTimeToUnlock * baseFactor;
            // Return time for colonists
            return unlockTime;
        }

        #endregion

        #region Get ticks to pick

        public float PickTime( Pawn picker )
        {
            if( Picked )
            {   // Already picked
                return 0.0f;
            }
            var baseFactor = 1.0f;
            var qualityInt = NonQualityQuality;
            if( CompQuality != null )
            {
                qualityInt = (int) CompQuality.Quality;
            }
            baseFactor += qualityInt * PickFactorPerQualityLevel;
            if( this.Stuff != null )
            {
                var tool = Weapon( picker );
                if( tool != null )
                {
                    var toolStuff = tool.parent.Stuff;
                    if( toolStuff != null )
                    {
                        var meleeVerb = tool.AllVerbs.Find( verb => verb is Verb_MeleeAttack ) as Verb_MeleeAttack;
                        if( meleeVerb != null )
                        {
                            if(
                                ( meleeVerb.verbProps.meleeDamageDef == DamageDefOf.Cut ) &&
                                ( this.Stuff.stuffCategories.Contains( StuffCategoryDefOf.Fabric ) )
                            )
                            {   // Bonus for cutting through fabrics
                                baseFactor /= tool.parent.GetStatValue( StatDefOf.SharpDamageMultiplier );
                                baseFactor *= this.parent.GetStatValue( StatDefOf.ArmorRating_Sharp );
                            }
                            else if( // Bonus for picking locks
                                    ( meleeVerb.verbProps.meleeDamageDef == DamageDefOf.Stab )
                                   )
                            {
                                baseFactor /= tool.parent.GetStatValue( StatDefOf.SharpDamageMultiplier );
                            }
                        }
                    }
                }
                if( this.Stuff.stuffProps.categories.Contains( StuffCategoryDefOf.Metallic ) )
                {
                    baseFactor *= this.parent.GetStatValue( StatDefOf.ArmorRating_Sharp );
                }
            }
            if( picker != null )
            {   // Adjust for picker manipulation capacity
                baseFactor /= picker.health.capacities.GetEfficiency( PawnCapacityDefOf.Manipulation );
            }
            return BaseTimeToPickLock * baseFactor;
        }

        #endregion

    }

}
