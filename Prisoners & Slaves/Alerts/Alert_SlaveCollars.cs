using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

	public class Alert_SlaveCollars : Alert_Medium
	{

		public override string FullLabel
		{
			get
			{
                return Data.Strings.AlertNeedCollarsLabel.Translate();
			}
		}

		public override string FullExplanation
		{
			get
			{
                return Data.Strings.AlertNeedCollarsExplaination.Translate();
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
							if( !collar.IsForbidden( Faction.OfPlayer ) )
							{
								return AlertReport.Inactive;
							}
						}
					}
					foreach( Pawn pawn in prisoners )
					{
						if(
							( pawn.GetRoom().isPrisonCell )&&
							( pawn.guest.interactionMode == Data.PIM.EnslavePrisoner )
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
