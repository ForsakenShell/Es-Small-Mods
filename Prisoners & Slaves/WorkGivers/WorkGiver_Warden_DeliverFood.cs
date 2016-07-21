using System.Collections.Generic;
using System.Linq;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{
    
    public class WorkGiver_Warden_DeliverFood : WorkGiver_Warden
    {
        
        public override Job JobOnThing( Pawn pawn, Thing t )
        {
            //Log.Message( string.Format( "WorkGiver_Warden_DeliverFood( {0}, {1} )", pawn.LabelShort, t.ThingID ) );
            if( !this.ShouldTakeCareOfPrisoner( pawn, t ) )
            {
                //Log.Message( string.Format( "\t!ShouldTakeCareOfPrisoner( {0}, {1} )", pawn.LabelShort, (t as Pawn) != null ? ((Pawn)t).LabelShort :  t.ThingID ) );
                return null;
            }
            var prisoner = (Pawn) t;
            var compPrisoner = prisoner.TryGetComp<CompPrisoner>();
            if( prisoner.IsPrisonerOfColony )
            {
                if( !prisoner.guest.ShouldBeBroughtFood )
                {
                    //Log.Message( string.Format( "\t{0} !ShouldBeBroughtFood", prisoner.LabelShort ) );
                    return null;
                }
            }
            else if( !compPrisoner.wasArrested )
            {
                //Log.Message( string.Format( "\t{0} !wasArrested", prisoner.LabelShort ) );
                return null;
            }
            if( (double) prisoner.needs.food.CurLevelPercentage >= (double) prisoner.needs.food.PercentageThreshHungry + 0.0199999995529652 )
            {
                //Log.Message( string.Format( "\t{0} food.CurLevelPercentage < food.PercentageThreshHungry", prisoner.LabelShort ) );
                return null;
            }
            if( WardenFeedUtility.ShouldBeFed( prisoner ) )
            {
                //Log.Message( string.Format( "\t{0} ShouldBeFed", prisoner.LabelShort ) );
                return null;
            }
            if( FoodAvailableInRoomTo( prisoner ) )
            {
                //Log.Message( string.Format( "\t{0} FoodAvailableInRoomTo", prisoner.LabelShort ) );
                return null;
            }
            Thing foodSource;
            ThingDef foodDef;
            if( !FoodUtility.TryFindBestFoodSourceFor( pawn, prisoner, prisoner.needs.food.CurCategory == HungerCategory.Starving, out foodSource, out foodDef, false, true, false, false ) )
            {
                //Log.Message( string.Format( "\t{0} !TryFindBestFoodSourceFor", prisoner.LabelShort ) );
                return null;
            }
            if( prisoner.CanReach( foodSource, PathEndMode.ClosestTouch, prisoner.NormalMaxDanger(), false, TraverseMode.ByPawn ) )
            {
                //Log.Message( string.Format( "\t{0} CanReach( {1} )", prisoner.LabelShort, foodSource.ThingID ) );
                return null;
            }
            //Log.Message( string.Format( "\t{0} DeliverFood {1} to {2}", pawn.LabelShort, foodSource.ThingID, prisoner.LabelShort ) );
            var job = new Job( JobDefOf.DeliverFood, foodSource, prisoner );
            job.maxNumToCarry = FoodUtility.WillEatStackCountOf( prisoner, foodDef );
            job.targetC = RCellFinder.SpotToChewStandingNear( prisoner, foodSource );
            return job;
        }

        public static bool FoodAvailableInRoomTo( Pawn prisoner )
        {
            if(
                ( prisoner.carrier.CarriedThing != null )&&
                ( NutritionAvailableForFrom( prisoner, prisoner.carrier.CarriedThing ) > 0.0f )
            )
            {
                //Log.Message( "Prisoner is carrying food" );
                return true;
            }
            var neededNutrition = 0.0f;
            var foodNutrition = 0.0f;
            var room = prisoner.Position.GetRoom();
            if( room == null )
            {   // This should never actually happen...
                //Log.Message( "Prisoner is not in a room!" );
                return false;
            }
            for( int regionIndex = 0; regionIndex < room.RegionCount; ++regionIndex )
            {
                var region = room.Regions[ regionIndex ];
                var foodSources = region.ListerThings.ThingsInGroup( ThingRequestGroup.FoodSourceNotPlantOrTree );
                if(
                    ( prisoner.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )&&
                    ( foodSources.Any( (source) =>
                {
                    if( source.def.IsFoodMachine() )
                    {
                        if(
                            ( source is Building_NutrientPasteDispenser )&&
                            ( ((Building_NutrientPasteDispenser)source).CanDispenseNow )
                        )
                        {
                            return true;
                        }
                        if(
                            ( source is Building_AutomatedFactory )&&
                            ( ((Building_AutomatedFactory)source).BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal ) != null )
                        )
                        {
                            return true;
                        }
                    }
                    return false;
                } ) )
                )
                {
                    //Log.Message( "Prisoner has access to a stocked food machine" );
                    return true;
                }
                for( int foodIndex = 0; foodIndex < foodSources.Count; ++foodIndex )
                {
                    var foodSource = foodSources[ foodIndex ];
                    if(
                        ( foodSource.def.IsFoodMachine() )||
                        (
                            ( foodSource.def.IsNutritionGivingIngestible )&&
                            ( foodSource.def.ingestible.preferability > FoodPreferability.NeverForNutrition )
                        )
                    )
                    {
                        foodNutrition += NutritionAvailableForFrom( prisoner, foodSource );
                    }
                }
                var pawns = region.ListerThings.ThingsInGroup( ThingRequestGroup.Pawn );
                for( int pawnIndex = 0; pawnIndex < pawns.Count; ++pawnIndex )
                {
                    var pawn = pawns[ pawnIndex ] as Pawn;
                    var compPrisoner = pawn.TryGetComp<CompPrisoner>();
                    if(
                        (
                            ( pawn.IsPrisonerOfColony )||
                            (
                                ( compPrisoner != null )&&
                                ( compPrisoner.wasArrested )
                            )
                        )&&
                        ( pawn.needs.food.CurLevelPercentage < ( pawn.needs.food.PercentageThreshHungry + 0.0199999995529652 ) )&&
                        (
                            ( pawn.carrier.CarriedThing == null )||
                            ( !pawn.RaceProps.WillAutomaticallyEat( pawn.carrier.CarriedThing ) )
                        )
                    )
                    {
                        neededNutrition += pawn.needs.food.NutritionWanted;
                    }
                }
            }
            //Log.Message( string.Format( "return {0} + 0.5f >= {1};", foodNutrition, neededNutrition ) );
            return foodNutrition + 0.5f >= neededNutrition;
        }

        public static float NutritionAvailableForFrom( Pawn p, Thing foodSource )
        {
            if(
                ( foodSource.def.IsNutritionGivingIngestible )&&
                ( p.RaceProps.WillAutomaticallyEat( foodSource ) )
            )
            {
                return foodSource.def.ingestible.nutrition * (float) foodSource.stackCount;
            }
            if(
                ( p.RaceProps.ToolUser )&&
                ( p.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )&&
                ( foodSource.def.IsFoodMachine() )
            )
            {
                if( foodSource is Building_NutrientPasteDispenser )
                {
                    var NPD = foodSource as Building_NutrientPasteDispenser;
                    if( NPD.CanDispenseNow )
                    {
                        return 99999f;
                    }
                }
                if( foodSource is Building_AutomatedFactory )
                {
                    var FS = foodSource as Building_AutomatedFactory;
                    var foodDef = FS.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                    if( foodDef != null )
                    {
                        return 99999f;
                    }
                }
            }
            return 0.0f;
        }

    }

}
