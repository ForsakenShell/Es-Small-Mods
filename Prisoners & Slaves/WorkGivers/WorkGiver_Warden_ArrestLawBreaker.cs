using System;
using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace PrisonersAndSlaves
{
    
    public class WorkGiver_Warden_ArrestLawBreaker : WorkGiver_Scanner
    {

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup( ThingRequestGroup.Pawn );
            }
        }

        public override Job JobOnThing( Pawn pawn, Thing t )
        {
            if( Monitor.laws.NullOrEmpty() )
            {   // No laws to enforce
                return null;
            }
            var otherPawn = t as Pawn;
            if( otherPawn == null )
            {   // Thing isn't a pawn
                return null;
            }

            if( !pawn.CanReserveAndReach( otherPawn, PathEndMode.ClosestTouch, Danger.Deadly, 1 ) )
            {   // Can't get to other pawn
                return null;
            }

            var compPrisoner = otherPawn.TryGetComp<CompPrisoner>();
            if( compPrisoner == null )
            {   // Pawn is missing comp
                Log.ErrorOnce( string.Format( "{0} is missing CompPrisoner!", otherPawn.LabelShort ), ( 0x0BAD0000 | ( otherPawn.GetHashCode() & 0x0000FFFF ) ) );
                return null;
            }

            if( compPrisoner.wasArrested )
            {   // Pawn was already arrested
                return null;
            }

            LawDef lawBroken = compPrisoner.lawBroken;

            if( otherPawn.CanBeSeenByColony() )
            {   // Pawn can be seen, check for laws being broken
                foreach( var law in Monitor.laws )
                {
                    if(
                        ( law.lawWorker.CanArrestFor( otherPawn ) )&&
                        ( law.lawWorker.LawBroken( otherPawn ) )
                    )
                    {
                        if(
                            ( lawBroken == null )||
                            ( law.severity > lawBroken.severity )
                        )
                        {   // No outstanding warrant or the new crime is more severe than the current one
                            lawBroken = law;
                        }
                    }
                }
            }

            if( lawBroken == null )
            {   // No laws broken
                return null;
            }

            Log.Message( string.Format( "{0} wants to arrest {1} for {2}", pawn.NameStringShort, otherPawn.NameStringShort, lawBroken.label ) );

            compPrisoner.lawBroken = lawBroken;

            if( !otherPawn.CanBeArrested() )
            {   // Needs to be subdued first
                Log.Message( string.Format( "{0} can't arrest {1}, refuses to be arrested", pawn.NameStringShort, otherPawn.NameStringShort ) );
                var attackVerb = pawn.TryGetAttackVerb();
                bool nonLethal = (
                    ( attackVerb == null )||
                    ( attackVerb.verbProps.meleeDamageDef == DamageDefOf.Stun )
                );
                if(
                    ( !lawBroken.allowLethalForceToSubdue )&&
                    ( !nonLethal )
                )
                {
                    Log.Message( string.Format( "{0} needs to subdue {1} but is using a weapon with lethal force which the law doesn't allow", pawn.NameStringShort, otherPawn.NameStringShort ) );
                    return null;
                }
                Log.Message( string.Format( "{0} is subduing {1}", pawn.NameStringShort, otherPawn.NameStringShort ) );
                var subdueJob = new Job( JobDefOf.AttackStatic, otherPawn );
                return subdueJob;
            }

            Building_Bed bedFor = null;
            List<Building_Door> doors = null;

            if(
                ( lawBroken.takeHomeByDefault )&&
                ( otherPawn.IsColonist )
            )
            {   // Take the offender home (only if they are a colonist)
                Log.Message( string.Format( "{0} wants to take {1} home", pawn.NameStringShort, otherPawn.NameStringShort ) );
                bedFor = otherPawn.ownership.OwnedBed;
                doors = bedFor.GetRoom().Portals();
            }

            if( bedFor == null )
            {   // Take the offender to prison
                Log.Message( string.Format( "{0} wants to take {1} to prison", pawn.NameStringShort, otherPawn.NameStringShort ) );
                bedFor = RestUtility.FindBedFor( otherPawn, pawn, true, false, false );
                if( bedFor == null )
                {   // No prison bed for law breaker
                    Log.Message( string.Format( "No free prisoner bed to assign to {0}", otherPawn.NameStringShort ) );
                    return null;
                }
            }

            if( !pawn.CanReserveAndReach( bedFor, PathEndMode.ClosestTouch, Danger.Deadly, 1 ) )
            {   // Can't get to the bed
                return null;
            }

#if DEBUG
            if( !bedFor.ForPrisoners )
            {   // Take the offender home
                Log.Message( string.Format( "{0} is taking {1} home at {2}", pawn.NameStringShort, otherPawn.NameStringShort, bedFor.Position ) );
            }
            else
            {   // Arrest the offender
                Log.Message( string.Format( "{0} is taking {1} to prison as {2}", pawn.NameStringShort, otherPawn.NameStringShort, bedFor.Position ) );
            }
#endif

            var arrestJob = new Job( JobDefOf.Arrest, otherPawn, null, bedFor );
            if( !doors.NullOrEmpty() )
            {
                arrestJob.targetQueueB = new List<TargetInfo>();
                arrestJob.numToBringList = new List<int>();
                foreach( var door in doors )
                {
                    if( door is Building_RestrictedDoor )
                    {
                        var compLock = door.TryGetComp<CompLockable>();
                        if( compLock != null )
                        {
                            arrestJob.targetQueueB.Add( door );
                            arrestJob.numToBringList.Add( 1 );
                        }
                    }
                }
            }
            arrestJob.maxNumToCarry = 1;
            return arrestJob;

        }

    }

}
