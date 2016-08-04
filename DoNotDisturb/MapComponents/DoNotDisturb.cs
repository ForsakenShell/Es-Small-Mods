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
		enum LockState
		{
			Untouched,
			WantLock,
			WantUnlock
		}

        private Dictionary<Room,LockState>  roomState;

        public                              DoNotDisturb()
        {
            roomState = new Dictionary<Room, LockState>();
        }

        private LockState                   RoomStateForPawn( Room room, Pawn pawn )
        {
            if(
                ( !room.Owners.Contains( pawn ) )||
                ( pawn.needs.food.CurCategory >= HungerCategory.UrgentlyHungry )||
                ( pawn.needs.joy.CurCategory <= JoyCategory.Low )
            )
            {   // Pawn doesn't own this room, or;
                // Needs food, or;
                // Needs joy
                // don't lock them in
                return LockState.WantUnlock;
            }

            foreach( var owner in room.Owners )
            {
                if( owner.GetRoom() != room )
                {   // Not all the owners are in the room
                    // Don't lock them out
                    return LockState.WantUnlock;
                }
            }

            if(
                ( pawn.CurrentBed() != null )&&
                ( !HealthUtility.PawnShouldGetImmediateTending( pawn ) )
            )
            {
                // In bed,
                // Not needing doctors care,
                return LockState.WantLock;
            }

            if(
                ( pawn.CurJob != null )&&
                ( pawn.CurJob.def.driverClass == typeof( JobDriver_RelaxAlone ) )
            )
            {
                // Doing something that want's privacy
                return LockState.WantLock;
            }

            // Pawn is awake and not doing anything they want privacy for
            return LockState.WantUnlock;
        }

		public override void                MapComponentTick()
		{
			if( ( Find.TickManager.TicksGame % 30 ) != 0 )
			{
				return;
			}

            if( roomState.Count > 0 )
            {
                roomState.Clear();
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
                if( pawn.IsPrisonerOfColony )
                {   // Pawn is a prisoner
                    continue;
                }
                var room = pawn.GetRoom();
				if(
                    ( room == null )||
                    ( room.Owners.Count() == 0 )
                )
				{   // Pawn is a not a room or it has no owners
					continue;
				}

                var lockState = LockState.Untouched;
                if( !roomState.TryGetValue( room, out lockState ) )
                {
                    roomState[ room ] = LockState.Untouched;
                }

                var pawnState = RoomStateForPawn( room, pawn );
                if( pawnState > lockState )
                {
                    lockState = pawnState;
                    roomState[ room ] = lockState;
                }

			}

            foreach( var keyPair in roomState )
            {
                if( keyPair.Value == LockState.WantUnlock )
                {
                    // Unlock room
                    SetRoomDoors( keyPair.Key, false );
                }
                else if( keyPair.Value == LockState.WantLock )
                {
                    // Lock room
                    SetRoomDoors( keyPair.Key, true );
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

