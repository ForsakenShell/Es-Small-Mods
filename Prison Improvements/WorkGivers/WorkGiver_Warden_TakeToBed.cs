using System;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace PrisonImprovements
{

	public class WorkGiver_Warden_TakeToBed : WorkGiver_Warden
	{

		public override Job JobOnThing( Pawn pawn, Thing t )
		{
			if( !this.ShouldTakeCareOfPrisoner( pawn, t ) )
			{
				return (Job) null;
			}
			var warden = pawn;
			var prisoner = t as Pawn;
			var compSlave = prisoner.GetComp<CompSlave>();
			if(
				( compSlave != null )&&
				( compSlave.ShouldBeTransfered )&&
				( warden.CanReserve( prisoner, 1 ) )
			)
			{
				//Log.Message( string.Format( "Prisoner {0} should be transfered and {1} wants to escort", prisoner.Name, warden.Name ) );
                var haulToLoc = compSlave.haulTarget.GetRoom().Cells.RandomElement();
				var job = new Job( Data.TransferPrisonerJobDef, prisoner, haulToLoc );
				job.maxNumToCarry = 1;
				return job;
			}
			var prisonerRoom = prisoner.GetRoom();
			if(
				( !prisoner.Downed ) &&
				( !prisonerRoom.isPrisonCell ) &&
				( warden.CanReserve( prisoner, 1 ) )
			)
			{
				//Log.Message( string.Format( "Prisoner {0} is not in a prison room and {1} wants to take back to bed", prisoner.Name, warden.Name ) );
				bool flag1 = ( prisoner.ownership.OwnedBed != null ) && ( prisoner.ownership.OwnedBed.Position.GetRoom() != prisonerRoom );
				bool flag2 = false;
				if(
					( !flag1 ) &&
					( prisonerRoom != null ) &&
					( !prisonerRoom.TouchesMapEdge )
				)
				{
					foreach( var buildingBed in prisonerRoom.ContainedBeds )
					{
						if(
							( buildingBed.ForPrisoners ) &&
							(
								( buildingBed.owners != null ) &&
								( buildingBed.owners.Contains( prisoner ) ) &&
								( buildingBed.AnyUnoccupiedSleepingSlot )
							) ||
							(
								( prisoner.InBed() ) &&
								( prisoner.CurrentBed() == buildingBed )
							) &&
							(
								( !buildingBed.Medical ) ||
								(
									( prisoner.health.PrefersMedicalRest ) &&
									( prisoner.health.ShouldEverReceiveMedicalCare )
								)
							)
						)
						{
							flag2 = true;
							break;
						}
					}
				}
				if(
					( flag1 ) ||
					( !flag2 )
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
							var job = new Job( JobDefOf.EscortPrisonerToBed, (TargetInfo) ( (Thing) prisoner ), (TargetInfo) ( (Thing) bedFor ) );
							job.maxNumToCarry = 1;
							return job;
						}
					}
				}
			}
			if(
				( prisoner.Downed ) &&
				( prisoner.health.NeedsMedicalRest ) &&
				( !prisoner.InBed() ) &&
				( warden.CanReserve( prisoner, 1 ) )
			)
			{
				var bedFor = RestUtility.FindBedFor( prisoner, warden, true, true, false );
				if(
					( bedFor != null ) &&
					( prisoner.CanReserve( bedFor, bedFor.SleepingSlotsCount ) )
				)
				{
					var job = new Job( JobDefOf.TakeWoundedPrisonerToBed, prisoner, bedFor );
					job.maxNumToCarry = 1;
					return job;
				}
			}
			return (Job) null;
		}

	}

}
