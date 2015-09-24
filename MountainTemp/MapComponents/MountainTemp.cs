using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace esm
{

    public struct NaturalRoom{
        public Room room;
		public float naturalEqualizationFactor;
        public float naturalTemp;
        //public int controlledTemp;
    }

    public class MountainTemp : MapComponent
    {
        // Constant underground temperature
        public const float UNDERGROUND_TEMPERATURE = 0.0f;

        // Constant delta for setting temp
        public const float TEMPERATURE_DELTA = 0.49f;

        // Constant equalization factor
		public const float EQUALIZATION_FACTOR = 120.0f * 90.0f * 6f;

        // Constant invalid control temp (no active temperature controllers)
        public const int INVALID_CONTROL_TEMP = -999999;

        // Ticks between updates
        public const int UPDATE_TICKS = 60;

        // This will house all the rooms in the world which have
        // natural roofs
        List<NaturalRoom> NaturalRooms = new List<NaturalRoom>();

        /// <summary>
        /// Fetchs the mountain rooms with thick roofs and no heating/cooling devices.
        /// </summary>
        void FetchNaturalRooms()
        {
            // Clear our list of natural rooms
            NaturalRooms = new List<NaturalRoom>();

            // Get the list of rooms from the region grid
            List<Room> allRooms = Find.RegionGrid.allRooms;

            // No rooms to check, abort now
            if( ( allRooms == null )||
                ( allRooms.Count < 1 ) )
                return;

            // Get the outdoor temperature
            var outdoorTemp = GenTemperature.OutdoorTemp;

            #if DEBUG
			/*
            var debugDump = "FetchNaturalRooms:" +
                "\n\toutdoorTemp: " + outdoorTemp +
                "\n\tallRooms.Count: " + allRooms.Count;
            */
            #endif

            // Find all the coolers in the world
            //var allCoolers = Find.ListerBuildings.AllBuildingsColonistOfClass<Building_Cooler>().ToList();

            // Itterate the rooms
            foreach( var room in allRooms ){

                //float controlledTemp = 0f;
                //int controlUnits = 0;

                // Open roof?  Not what we want
                if( room.OpenRoofCount > 0 ){
                    goto Skip_Room;
                }

                // Get the cells which are contained in the room
                var roomCells = room.Cells.ToList();

                // No cells, no want
                if( ( roomCells == null )||
                    ( roomCells.Count < 1 ) ){
                    goto Skip_Room;
                }

                // Make sure the roof is at least partly natural
                if( !roomCells.Any(
                    cell => cell.GetRoof().isNatural ) ){
                    goto Skip_Room;
                }

				/*
                // Find all heaters contained in the room
                var heaters = room.AllContainedThings.Where( t =>
                    ( ( t as Building_Heater ) != null ) ).ToList();

                // Does this room have any heaters?
                if( ( heaters != null )&&
                    ( heaters.Count > 0 ) ){

                    // If so, are they powered?
                    foreach( var thing in heaters ){

                        var heater = thing as Building_Heater;
                        if( heater.compPowerTrader.PowerOn == true ){

                            // Add heater temp to controlled temp
                            controlledTemp += heater.compTempControl.targetTemperature;
                            controlUnits++;
                        }
                    }
                }

                // Does this room have any coolers?
                if( ( allCoolers != null )&&
                    ( allCoolers.Count > 0 ) ){

                    // Check to see if any of the coolers effect this room
                    foreach( var cooler in allCoolers ){
                        
                        if( cooler.compPowerTrader.PowerOn == true ){

                            #if DEBUG
							//debugDump += "\n\tcooler rotation: " + cooler.Rotation.FacingCell;
                            
                            #endif
                            
                            // Get heating and cooling sides of the cooler
                            var cellHeated = cooler.Position + cooler.Rotation.FacingCell;
                            var cellCooled = cooler.Position - cooler.Rotation.FacingCell;

                            // Does either side of the cooler effect this room?
                            if( cellHeated.GetRoom() == room ){

                                // Substract this temp from controlled temp
                                controlledTemp -= ( cooler.compTempControl.targetTemperature * 2 );
                                controlUnits++;

                            } else if( cellCooled.GetRoom() == room ){

                                // Add this temp from the controlled temp
                                controlledTemp += cooler.compTempControl.targetTemperature;
                                controlUnits++;
                            }
                        }
                    }
                }
				*/

                // Create new natural room entry
                var naturalRoom = new NaturalRoom();
                naturalRoom.room = room;

                float roofCount = (float)roomCells.Count;
                float thickCount = 0;
				float thinCount = 0;

                // Count thick roofs
                foreach( var cell in roomCells )
				{
					var roof = cell.GetRoof();
                    if( roof.isThickRoof )
					{
                        thickCount++;
					}
					else if ( roof.isNatural )
					{
						thinCount++;
					}
				}

                // Now calculate percent of roof that is thick/thin/constructed
				float thickFactor = thickCount / roofCount;
				float thinFactor = thinCount / roofCount;
				float roofedFactor = 1.0f - thickFactor - thinFactor;
				if( roofedFactor < 0f )
				{
					// Handle rounding errors
					roofedFactor = 0f;
				}

				// Factor for pushing heat
				naturalRoom.naturalEqualizationFactor = thickFactor + thinFactor * 0.5f;

                // Calculate new temp based on roof factors
				float thickRate = thickFactor * UNDERGROUND_TEMPERATURE;
				float thinRate = thinFactor * ( outdoorTemp - UNDERGROUND_TEMPERATURE ) * 0.25f;
				//float roofedRate = roofedFactor * ( outdoorTemp - UNDERGROUND_TEMPERATURE ) * 0.5f;

				// Assign the natural temp based on aggregate ratings
                //naturalRoom.naturalTemp = thickFactor * UNDERGROUND_TEMPERATURE +
                //    ( 1.0f - thickFactor ) * outdoorTemp;
				naturalRoom.naturalTemp = thickRate + thinRate;// + roofedRate;

                // Compute average controlled temperature for room
                /*
				if( controlUnits == 0 ){
                    naturalRoom.controlledTemp = INVALID_CONTROL_TEMP;
                } else {
                    naturalRoom.controlledTemp = (int)( controlledTemp / controlUnits );
                }
                */

                #if DEBUG
				/*
				debugDump += "\n\tID: " + room.ID +
                    "\n\t\troomCells.Count: " + roomCells.Count +
                    "\n\t\troofCount: " + roofCount +
					"\n\t\tCount thick/thin/man: " + thickCount + "/" + thinCount + "/" + ( roofCount - thickCount - thinCount ) +
                    "\n\t\tFactor thick/thin/man: " + thickFactor + "/" + thinFactor + "/" + roofedFactor +
					"\n\t\tRate thick/thin/man: " + thickRate + "/" + thinRate + "/" + roofedRate +
					"\n\t\toutside.Temperature: " + outdoorTemp +
                    "\n\t\troom.Temperature: " + naturalRoom.room.Temperature +
                    "\n\t\tcontrolledTemp: " + naturalRoom.controlledTemp +
                    "\n\t\tdesiredTemp: " + naturalRoom.naturalTemp;
                */
                #endif

                // Add the natural room to the list
                NaturalRooms.Add( naturalRoom );

                Skip_Room:;
                // We skipped this room, need to do it this way because
                // using 'continue' in the controller loops will
				// continue the controller loops and not the room loop
            }
            #if DEBUG
			/*
            Log.Message( debugDump );
            */
            #endif
        }

        /// <summary>
        /// Map tick for component
        /// </summary>
        public override void MapComponentTick()
        {
            // Only do this once every update ticks
            if( ( Find.TickManager.TicksGame % UPDATE_TICKS ) != 0 )
                return;
            
            // Get the all the natural rooms in the world
            FetchNaturalRooms();

            // No rooms, nothing to do
            if (NaturalRooms.Count < 1)
                return;

            // Go through rooms and set the temperature
            foreach (var naturalRoom in NaturalRooms)
            {
				float equalizationRate = naturalRoom.naturalEqualizationFactor;
                float targetTemp = naturalRoom.naturalTemp;

				/*
                if( naturalRoom.controlledTemp != INVALID_CONTROL_TEMP ){
                    // Room has active controllers

                    // If( the control is cooler than the natural and the room is cooler than the natural and the room is hotter than the control)
                    // Or( the control is hotter than the natural and the room is hotter than the natural and the room is cooler than the control)
                    if( ( ( naturalRoom.controlledTemp < naturalRoom.naturalTemp )&&
                            ( naturalRoom.room.Temperature < naturalRoom.naturalTemp )&&
                            ( naturalRoom.room.Temperature > naturalRoom.controlledTemp ) )||
                        ( ( naturalRoom.controlledTemp > naturalRoom.naturalTemp )&&
                            ( naturalRoom.room.Temperature > naturalRoom.naturalTemp )&&
                            ( naturalRoom.room.Temperature < naturalRoom.controlledTemp ) ) ){

                        // Temperature inside of range, adjust slowly to control temp
                        equalizationRate *= 0.25f;
                        targetTemp = naturalRoom.controlledTemp;

                    }
                }
                */

                // Move the room towards the desired temp
                float tempDelta = Mathf.Abs( naturalRoom.room.Temperature - targetTemp );

                if( tempDelta > TEMPERATURE_DELTA ){
                    // Difference is too large, move it
                    EqualizeTemperature( naturalRoom.room, targetTemp, equalizationRate );
                } else {
                    // Difference is within tollerance, set it
                    naturalRoom.room.Temperature = targetTemp;
                }
            }
        }

        /// <summary>
        /// Equalizes the temperature of the room.
        /// </summary>
        /// <param name="room">Room.</param>
        /// <param name="destTemp">Destination temp.</param>
        public void EqualizeTemperature( Room room, float destTemp, float equalizationRate )
        {
            // Temperature delta
            float delta = Mathf.Abs( room.Temperature - destTemp );
            // Movement delta
            float movement = Mathf.Min(
                (float)( delta >= 100.0f ? (float)( (double)0.000300000014249235 * (double)delta - (double)0.025000000372529 ) : 5E-05f * delta ) *
                ( 0.22f * Mathf.Pow( room.CellCount, 0.33f ) ),
                delta );
            // Change delta
            float change = ( ( destTemp > room.Temperature ? movement : -movement ) * ( EQUALIZATION_FACTOR * equalizationRate ) ) / room.CellCount;
            // Push some heat
			//room.PushHeat( change );
            room.Temperature += change;
        }

    }

}