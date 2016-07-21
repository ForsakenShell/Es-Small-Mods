using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace esm
{

	public class DoNotDisturb : MapComponent
	{
		public DoNotDisturb()
		{
		}

		enum LockState
		{
			Untouched,
			WantLock,
			WantUnlock
		}

		public override void MapComponentTick()
		{
			if( ( Find.TickManager.TicksGame % 30 ) != 0 )
			{
				return;
			}

			var pawns = Find.MapPawns.FreeColonists.ToList();
			if( pawns.NullOrEmpty() )
			{
				return;
			}

			// Go through each pawn and see if they are sleeping in their own room
			// If they are, and no other pawn is in the room, forbid the door until
			// the pawn wakes up.
			foreach( var pawn in pawns )
			{
				var lockState = LockState.Untouched;

				var room = pawn.GetRoom();
				if(
                    ( pawn.IsPrisonerOfColony )||
					( room == null )||
					( !room.Owners.Contains( pawn ) )
				)
				{
					// Pawn is a prisoner or not in their own private room
					continue;
				}

				// Find any other pawns in the same room
                if(
                    ( room.AllContainedThings.Any( t => (
                        ( t is Pawn )&&
                        ( t != pawn )&&
                        ( !room.Owners.Contains( (Pawn)t ) )
                    ) ) )||
                    ( room.Owners.Any( p => p.GetRoom() != room ) )
                )
				{
					// Not alone in room or the other pawn isn't an owner or the other owners are not in the room
					lockState = LockState.WantUnlock;
				}
				else if(
					( pawn.CurrentBed() != null )&&
					( !HealthUtility.PawnShouldGetImmediateTending(pawn)) &&
					( pawn.needs.food.CurCategory < HungerCategory.UrgentlyHungry )&&
					( pawn.needs.joy.CurCategory > JoyCategory.Low )
				)
				{
					// In bed,
					// Not needing doctors care,
					// Not urgently hungry
					// Not joy deprived
					lockState = LockState.WantLock;
				}
				else if(
					( pawn.CurJob != null )&&
					( pawn.CurJob.def.driverClass == typeof( JobDriver_RelaxAlone ) )
				)
				{
					// Doing something that want's privacy
					lockState = LockState.WantLock;
				}
				else
				{
					// Pawn is awake and not doing anything they want privacy for
					lockState = LockState.WantUnlock;
				}

				if( lockState == LockState.WantUnlock )
				{
					// Unlock room
					SetRoomDoors( room, false );
				}
				else if( lockState == LockState.WantLock )
				{
					// Lock room
					SetRoomDoors( room, true );
				}

			}

		}

		private void SetRoomDoors( Room room, bool locked )
		{
			foreach( var region in room.Regions )
			{
				foreach( var doorRegion in region.Neighbors.Where( r => (
					( r.portal != null )
				) ) )
				{
					var door = doorRegion.portal;
					if( door != null )
					{
						door.SetForbidden( locked, false );
					}
				}
			}
		}

	}
}

