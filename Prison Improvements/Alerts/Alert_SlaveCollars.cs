using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace PrisonImprovements
{

	public class Alert_SlaveCollars : Alert_Medium
	{

		public override string FullLabel
		{
			get
			{
				return "PI_NeedCollars_Label".Translate();
			}
		}

		public override string FullExplanation
		{
			get
			{
				return "PI_NeedCollars_Explaination".Translate();
			}
		}

		public override AlertReport Report
		{
			get
			{
				var prisoners = Find.MapPawns.PrisonersOfColony;
				if( prisoners.Count() > 0 )
				{
                    var collars = Data.AllSlaveCollarsOfColony();
					if( !collars.NullOrEmpty() )
					{
						foreach( var collar in collars )
						{
							if( !collar.IsForbidden( Faction.OfColony ) )
							{
								return AlertReport.Inactive;
							}
						}
					}
					foreach( Pawn pawn in prisoners )
					{
						if(
							( pawn.GetRoom().isPrisonCell ) &&
							( pawn.guest.interactionMode == (PrisonerInteractionMode) Data.PIM_EnslavePrisoner )
						)
						{
							return pawn;
						}
					}
				}
				return AlertReport.Inactive;
			}
		}

	}

}
