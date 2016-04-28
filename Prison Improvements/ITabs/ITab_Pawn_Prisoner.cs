using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonImprovements
{

	public class ITab_Pawn_Prisoner : RimWorld.ITab_Pawn_Prisoner
	{

		private List<KeyValuePair<string, Thing>> cached;

		protected override void FillTab()
		{
			ConceptDatabase.KnowledgeDemonstrated( ConceptDefOf.PrisonerTab, KnowledgeAmount.GuiFrame );
			Text.Font = GameFont.Small;
			var rect1 = new Rect( 0.0f, 0.0f, this.size.x, this.size.y ).ContractedBy( 10f );

			var listingStandard = new Listing_Standard( rect1 );
			{
				var getsFood = this.SelPawn.guest.GetsFood;
				listingStandard.DoLabelCheckbox( "GetsFood".Translate(), ref getsFood, (string) null );
				this.SelPawn.guest.GetsFood = getsFood;

				var rect2 = listingStandard.GetRect( 28f );
				rect2.width = 140f;
				MedicalCareUtility.MedicalCareSetter( rect2, ref this.SelPawn.playerSettings.medCare );
				listingStandard.DoGap( 4f );

				listingStandard.DoLabel( "RecruitmentDifficulty".Translate() + ": " + this.SelPawn.guest.RecruitDifficulty.ToString( "##0" ) );
				if( Prefs.DevMode )
				{
					listingStandard.DoLabel( "Dev: Prison break MTB days: " + (object) (int) PrisonBreakUtility.InitiatePrisonBreakMtbDays( this.SelPawn ) );
				}

				var rect3 = listingStandard.GetRect( 200f );
				Widgets.DrawMenuSection( rect3, true );
				var position = rect3.ContractedBy( 10f );
				GUI.BeginGroup( position );
				{
					var rect4 = new Rect( 0.0f, 0.0f, position.width, 30f );
					foreach( PrisonerInteractionMode mode in Enum.GetValues( typeof( PrisonerInteractionMode ) ) )
					{
						if( Widgets.LabelRadioButton( rect4, mode.GetLabel(), this.SelPawn.guest.interactionMode == mode ) )
						{
							this.SelPawn.guest.interactionMode = mode;
						}
						rect4.y += 28f;
					}
					if( Widgets.LabelRadioButton( rect4, "PI_Enslave".Translate(), this.SelPawn.guest.interactionMode == (PrisonerInteractionMode) Data.PIM_EnslavePrisoner ) )
					{
						this.SelPawn.guest.interactionMode = (PrisonerInteractionMode) Data.PIM_EnslavePrisoner;
					}
				}
				GUI.EndGroup();

				var compSlave = this.SelPawn.TryGetComp<CompSlave>();
				if( compSlave != null )
				{
					listingStandard.DoGap( 4f );
					var rect5 = listingStandard.GetRect( 30f );

					if( !compSlave.ShouldBeTransfered )
					{
						if( Widgets.TextButton( rect5, "PI_TransferPrisoner".Translate() ) )
						{
							var list = this.GenOptions();
							if(
                                ( list.Count == 0 )||
                                (
                                    ( list.Count == 1 )&&
                                    ( list[ 0 ].label == "PI_Prison_Cell".Translate() )&&
                                    ( this.SelPawn.GetRoom() == this.SelPawn.ownership.OwnedBed.GetRoom() )
                                )
                            )
							{
								list.Add( new FloatMenuOption( "PI_InstallCamera".Translate(), (Action) null ) );
							}
							Find.WindowStack.Add( (Window) new FloatMenu( list, false ) );
						}
						else
						{
							this.cached = null;
						}
					}
					else
					{
						if( Widgets.TextButton( rect5, "PI_CancelTransfer".Translate() ) )
						{
							compSlave.haulTarget = null;
						}

						listingStandard.DoGap( 4f );

						var rect7 = listingStandard.GetRect( 30f );
						var style = new GUIStyle( Text.CurTextFieldStyle );
						style.alignment = TextAnchor.MiddleCenter;

                        string label = string.Empty;
                        if( compSlave.haulTarget is Building_PrisonMarker )
                        {
                            label = ( (Building_PrisonMarker) compSlave.haulTarget ).markerName;
                        }
                        else if( compSlave.haulTarget is Building_Bed )
                        {
                            label = "PI_Prison_Cell".Translate();
                        }
						GUI.Label( rect7, label, style );
					}
				}
			}
			listingStandard.End();
		}

        private List<KeyValuePair<string, Thing>> GetHaulToTargets()
		{
			if( this.cached != null )
			{
				return this.cached;
			}
			var list = new List<KeyValuePair<string, Thing>>();
            if( this.SelPawn.ownership.OwnedBed != null )
            {
                list.Add( new KeyValuePair<string, Thing>( "PI_Prison_Cell".Translate(), this.SelPawn.ownership.OwnedBed ) );
            }
			foreach( var marker in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_PrisonMarker>() )
			{
				if(
					( marker.markerName != null )&&
					( marker.markerName.Trim().Length > 0 )&&
                    ( marker.IsActive )
				)
				{
					list.Add( new KeyValuePair<string, Thing>( marker.markerName, marker ) );
				}
			}
			this.cached = list;
			return list;
		}

		private List<FloatMenuOption> GenOptions()
		{
			var list = new List<FloatMenuOption>();
			foreach( var keyValuePair in this.GetHaulToTargets() )
			{
				var action = new Action( () =>
				{
					var compSlave = this.SelPawn.GetComp<CompSlave>();
					if( compSlave == null )
					{
						return;
					}
					compSlave.haulTarget = keyValuePair.Value;
				} );
				var key = keyValuePair.Key;
				list.Add( new FloatMenuOption( key, action ) );
			}
			return list;
		}

	}

}
