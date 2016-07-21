using System;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace PrisonersAndSlaves
{

	public class WorkGiver_Warden_TakeToBed : WorkGiver_Warden
	{

		public override Job JobOnThing( Pawn pawn, Thing t )
		{
            //Log.Message( string.Format( "WorkGiver_Warden_TakeToBed( {0}, {1} )", pawn.LabelShort, t.ThingID ) );
			if( !this.ShouldTakeCareOfPrisoner( pawn, t ) )
			{
				return null;
			}
			var warden = pawn;
			var prisoner = t as Pawn;
			var compPrisoner = prisoner.GetComp<CompPrisoner>();
            if(
                ( prisoner.IsColonist )&&
                ( compPrisoner.wasArrested )
            )
            {
                if( prisoner.ownership.OwnedBed != null )
                {  
                    if( prisoner.GetRoom() == prisoner.ownership.OwnedBed.GetRoom() )
                    {   // Colonist is already in their room
                        //Log.Message( string.Format( "\t{0} is already in their room", prisoner.LabelShort ) );
                        return null;
                    }
                    //Log.Message( string.Format( "\t{0} should haul {1} back to their room", pawn.LabelShort, prisoner.LabelShort ) );
                    var haulToLoc = prisoner.ownership.OwnedBed.GetRoom().Cells.RandomElement();
                    var job = new Job( Data.JobDefOf.TransferPrisoner, prisoner, haulToLoc );
                    job.maxNumToCarry = 1;
                    return job;
                }
                //Log.Message( string.Format( "\t{0} does not have a private bed", prisoner.LabelShort ) );
            }
			if( compPrisoner.ShouldBeTransfered )
			{
				//Log.Message( string.Format( "Prisoner {0} should be transfered and {1} wants to escort", prisoner.Name, warden.Name ) );
				var haulToLoc = compPrisoner.haulTarget.GetRoom().Cells.RandomElement();
                var job = new Job( Data.JobDefOf.TransferPrisoner, prisoner, haulToLoc );
				job.maxNumToCarry = 1;
				return job;
			}
			var prisonerRoom = prisoner.GetRoom();
            if(
                ( prisoner.IsSlaveOfColony() )&&
                ( prisonerRoom != null )&&
                (
                    ( prisonerRoom.CellCount == 1 )||
                    ( prisonerRoom.IsSlaveWorkArea() )
                )
            )
            {   // Slave in an allowed area
                return null;
            }
			if(
				( !prisoner.Downed )&&
				( !prisonerRoom.isPrisonCell )
			)
			{
				//Log.Message( string.Format( "Prisoner {0} is not in a prison room and {1} wants to take back to bed", prisoner.Name, warden.Name ) );
                bool bedNotInPrisonCell = ( prisoner.ownership.OwnedBed != null )&&( prisoner.ownership.OwnedBed.Position.GetRoom() != prisonerRoom );
                bool foundFreeBedInPrisonCell = false;
				if(
					( !bedNotInPrisonCell )&&
					( prisonerRoom != null )&&
					( !prisonerRoom.TouchesMapEdge )
				)
				{
					foreach( var buildingBed in prisonerRoom.ContainedBeds )
					{
						if(
							( buildingBed.ForPrisoners )&&
							(
								( buildingBed.owners != null )&&
								( buildingBed.owners.Contains( prisoner ) )&&
								( buildingBed.AnyUnoccupiedSleepingSlot )
							)||
							(
								( prisoner.InBed() )&&
								( prisoner.CurrentBed() == buildingBed )
							)&&
							(
								( !buildingBed.Medical )||
								(
									( prisoner.health.PrefersMedicalRest )&&
									( prisoner.health.ShouldEverReceiveMedicalCare )
								)
							)
						)
						{
							foundFreeBedInPrisonCell = true;
							break;
						}
					}
				}
				if(
					( bedNotInPrisonCell )||
					( !foundFreeBedInPrisonCell )
				)
				{
					var bedFor = RestUtility.FindBedFor( prisoner, warden, true, false, false );
					if( bedFor != null )
					{
						if( bedFor.GetRoom() == prisonerRoom )
						{
							Log.Error( (string) (object) pawn + (object) " tried to escort prisoner " + (string) (object) prisoner + " to bed at " + (string) (object) bedFor.Position + " which is in the prisoner's room already." );
						}
						else
						{
							var job = new Job( JobDefOf.EscortPrisonerToBed, prisoner, bedFor );
							job.maxNumToCarry = 1;
							return job;
						}
					}
				}
			}
			if(
				( prisoner.Downed )&&
				( prisoner.health.NeedsMedicalRest )&&
				( !prisoner.InBed() )
			)
			{
				var bedFor = RestUtility.FindBedFor( prisoner, warden, true, true, false );
				if(
					( bedFor != null )&&
					( prisoner.CanReserve( bedFor, bedFor.SleepingSlotsCount ) )
				)
				{
					var job = new Job( JobDefOf.TakeWoundedPrisonerToBed, prisoner, bedFor );
					job.maxNumToCarry = 1;
					return job;
				}
			}
			return null;
		}

	}

}
