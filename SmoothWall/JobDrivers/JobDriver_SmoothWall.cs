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
	public class JobDriver_SmoothWall : JobDriver
	{
		private const int 				TicksPerStrike = 100;
		private const int				DamagePerStrike = 10;
		private int						smoothTicks;
		private int						nextSmoothStrike
		{
			get
			{
				// Get this workers speed based on stats
				var smoothingSpeed = pawn.GetStatValue( StatDef.Named( "SmoothingSpeed" ), true );
				var stonecuttingSpeed = pawn.GetStatValue( StatDef.Named( "StonecuttingSpeed" ), true );
				var sculptingSpeed = pawn.GetStatValue( StatDef.Named( "SculptingSpeed" ), true );
				var averageSpeed = ( smoothingSpeed + stonecuttingSpeed + sculptingSpeed ) / 3f;
                /* Research project cut from core game
                if( SmoothWall.ResearchPneumaticPicks.IsFinished )
				{
					averageSpeed *= 1.2f;
				}
				*/
				return (int)( (float)TicksPerStrike / averageSpeed );
			}
		}
		private Thing					mineable
		{
			get
			{
				return MineUtility.MineableInCell( TargetA.Cell );
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			// Just on the off-chance the rock is on fire...
			this.FailOnBurningImmobile( TargetIndex.A );
			// Reserve the target
			yield return Toils_Reserve.Reserve( TargetIndex.A );
			// Go to the target
			Toil toilGoto = Toils_Goto.GotoCell( TargetIndex.A, PathEndMode.Touch );
			// Fail going to the target if it becomes unreachable
			ToilFailConditions.FailOn< Toil >( toilGoto, ( Func< bool > )(() =>
				{
					if( Reachability.CanReach( pawn, (TargetInfo)TargetLocA, PathEndMode.Touch, pawn.NormalMaxDanger() ) )
						return false;
					return true;
				}));
			yield return toilGoto;
			// Now the work toil itself
			Toil toilWork = new Toil
			{
				// Continue until done
				defaultCompleteMode = ToilCompleteMode.Never,
				// When the job starts...
				initAction = new Action(() =>
					{
						smoothTicks = 0;
					} ),
				// The work tick
				tickAction = new Action(() =>
					{
						if( pawn.skills != null )
						{
							const float constructionXP = 0.11f / 5f;
							const float miningXP = 0.11f / 5f;
							const float artisticXP = 0.11f / 5f;
							pawn.skills.Learn( SkillDefOf.Construction, constructionXP );
							pawn.skills.Learn( SkillDefOf.Mining, miningXP );
							pawn.skills.Learn( SkillDefOf.Artistic, artisticXP );
						}
						smoothTicks += 1;
						if( smoothTicks < nextSmoothStrike ) return;
						// Reset counter, damage rock
						smoothTicks = 0;
						mineable.HitPoints -= DamagePerStrike;
					} )
			};
			// When should we stop?
			toilWork.endConditions.Add( ( Func< JobCondition > )(() =>
				{
					// Go until the rock is fully damaged
					if( mineable.HitPoints > 0 ) return JobCondition.Ongoing;
					return JobCondition.Succeeded;
				} ) );
			// Do something when done
			toilWork.AddFinishAction( new Action(() =>
				{
					// If the job failed, abort
					if( mineable.HitPoints > 0 ) return;
					// Clear the designation at this cell
					Common.RemoveDesignationDefOfAt( SmoothWall.designationDef, TargetA.Cell );
					// Better have associated stone blocks...
					string blocksDef = "Blocks" + mineable.def.defName;
					ThingDef stoneBlocks = DefDatabase<ThingDef>.GetNamed( blocksDef, true );
					// Replace the rock with a stone wall
					var wallThing = ThingMaker.MakeThing( SmoothWall.thingDef, stoneBlocks );
					if( wallThing != null )
					{
						var wall = GenSpawn.Spawn( wallThing, TargetA.Cell );
						if( wall != null )
						{
                        wall.SetFaction( Faction.OfPlayer );
						}
					}
				} ) );
			// Some fun sounds while working
			ToilEffects.WithSustainer( toilWork, ( Func< SoundDef > )(() =>
				{
					return SmoothWall.soundDef;
				} ) );
			// Some fun effects while working
			ToilEffects.WithEffect( toilWork, "Mine", TargetIndex.A );
			yield return toilWork;
			// And we're done.
			yield break;
		}

	}
}