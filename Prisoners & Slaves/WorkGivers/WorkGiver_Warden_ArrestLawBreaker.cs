using System;
using System.Collections.Generic;
using System.Linq;

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
            if( !otherPawn.CanBeSeenByColony() )
            {   // Pawn can't be seen
                return null;
            }

            LawDef lawBroken = null;
            foreach( var law in Monitor.laws )
            {   // Check if pawn has broken any laws
                if(
                    ( law.lawWorker.CanArrestFor( otherPawn ) )&&
                    ( law.lawWorker.LawBroken( otherPawn ) )
                )
                {
                    lawBroken = law;
                    break;
                }
            }

            if( lawBroken == null )
            {   // No laws broken
                return null;
            }

            Log.Message( string.Format( "{0} wants to arrest {1} for {2}", pawn.NameStringShort, otherPawn.NameStringShort, lawBroken.label ) );

            if( !otherPawn.CanBeArrested() )
            {   // Needs to be subdued first
                // TODO:  Write and issue subdue job
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

            if( lawBroken.takeHomeByDefault )
            {   // Take the offender home
                Log.Message( string.Format( "{0} wants to take {1} home", pawn.NameStringShort, otherPawn.NameStringShort ) );
                bedFor = otherPawn.ownership.OwnedBed;
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

            Job haulJob = null;

            if( !bedFor.ForPrisoners )
            {   // Take the offender home
                Log.Message( string.Format( "{0} is taking {1} home", pawn.NameStringShort, otherPawn.NameStringShort ) );
                haulJob = new Job( JobDefOf.HaulToCell, otherPawn, bedFor.Position );
            }
            else
            {   // Arrest the offender
                Log.Message( string.Format( "{0} is taking {1} to prison", pawn.NameStringShort, otherPawn.NameStringShort ) );
                haulJob = new Job( JobDefOf.Arrest, otherPawn, bedFor );
            }
            haulJob.maxNumToCarry = 1;
            return haulJob;

        }

    }

}
