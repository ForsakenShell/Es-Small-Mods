using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

using CommunityCoreLibrary;

namespace esm
{

	public class Building_SmoothWall : Building
	{
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			base.Destroy( DestroyMode.Kill );

			if( this.Stuff != null )
			{

				var blocks = this.Stuff.defName;
				var rock = blocks.Remove( 0, "Blocks".Length );
				var rockDef = DefDatabase<ThingDef>.GetNamed( rock );

				if(
					( rockDef != null )&&
					( rockDef.building != null )&&
					( rockDef.building.mineableThing != null )
				)
				{
					if( UnityEngine.Random.value < rockDef.building.mineableDropChance )
					{
						var chunkThing = ThingMaker.MakeThing( rockDef.building.mineableThing, null );
						if( chunkThing != null )
						{
							var chunk = GenSpawn.Spawn( chunkThing, Position );
							if( chunk.def.soundDrop != null)
							{
								chunk.def.soundDrop.PlayOneShot( Position );
							}
						}
					}
				}

			}

		}

	}

}
