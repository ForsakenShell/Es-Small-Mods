using System;

using RimWorld;
using Verse;
using UnityEngine;

namespace PrisonImprovements
{

	public class ITab_Pawn_Slave : ITab
	{

		private static readonly Vector2 WinSize;

		static ITab_Pawn_Slave()
		{
			ITab_Pawn_Slave.WinSize = new Vector2( 300f, 94f );
		}

		public ITab_Pawn_Slave()
		{
			this.size = ITab_Pawn_Slave.WinSize;
			this.labelKey = "PI_Pawn_Slave_Label";
		}

		public override bool IsVisible
		{
			get
			{
				var slave = this.SelPawn;
				var compSlave = slave.TryGetComp<CompSlave>();
				return (
					( slave.Faction == Faction.OfColony ) &&
					( compSlave != null ) &&
					( slave.WornCollar() != null )
				);
			}
		}

		protected override void FillTab()
		{
			var prisoner = this.SelPawn;
			var compSlave = prisoner.TryGetComp<CompSlave>();
			Widgets.Label( new Rect( 10f, 10f, WinSize.x - 20f, 20f ), "PI_Pawn_Slave_Faction".Translate( compSlave.originalFaction.name ) );
			Widgets.Label( new Rect( 10f, 30f, WinSize.x - 20f, 20f ), "PI_Pawn_Slave_PawnKind".Translate( compSlave.originalPawnKind.LabelCap ) );
			Widgets.Checkbox( new Vector2( 10f, 54f ), ref compSlave.freeSlave );
			Widgets.Label( new Rect( 38f, 54f, WinSize.x - 44f, 20f ), "PI_FreeSlave".Translate() );
			prisoner.guest.interactionMode = compSlave.freeSlave ? (PrisonerInteractionMode) Data.PIM_FreeSlave : PrisonerInteractionMode.NoInteraction;
		}

	}

}
