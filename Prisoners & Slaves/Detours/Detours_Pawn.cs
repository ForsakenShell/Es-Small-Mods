using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{

    internal class _Pawn : Pawn
    {

        internal string _GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine( this.MainDesc( true ) );
            if( this.MentalStateDef != null )
            {
                stringBuilder.AppendLine( this.MentalStateDef.inspectLine );
            }
            if(
                ( this.equipment != null )&&
                ( this.equipment.Primary != null )
            )
            {
                stringBuilder.AppendLine(
                    Data.Strings.Equipped.Translate() +
                    ": " +
                    (
                        this.equipment.Primary == null
                        ? Data.Strings.EquippedNothing.Translate()
                        : this.equipment.Primary.LabelCap
                    )
                );
            }
            if(
                ( this.carrier != null )&&
                ( this.carrier.CarriedThing != null )
            )
            {
                stringBuilder.Append( Data.Strings.Carrying.Translate() + ": " );
                stringBuilder.AppendLine( this.carrier.CarriedThing.LabelCap );
            }
            if( this.jobs.curJob != null )
            {
                try
                {
                    string report = this.jobs.curDriver.GetReport();
                    stringBuilder.AppendLine( report );
                }
                catch( Exception ex )
                {
                    stringBuilder.AppendLine( "JobDriver.GetReport() exception: " + (object) ex );
                }
            }
            //if( !PrisonBreakUtility.IsEscaping( this ) )
            //{
                // Prisoners are not always in restraints, they must be wearing them    
                if( this.WornRestraints( Data.BodyPartGroupDefOf.Hands ) != null )
                {
                    stringBuilder.AppendLine( Data.Strings.RestrainedCuffed.Translate() );
                }
                if( this.WornRestraints( BodyPartGroupDefOf.Legs ) != null )
                {
                    stringBuilder.AppendLine( Data.Strings.RestrainedShackled.Translate() );
                }
            //}
            stringBuilder.Append( this.InspectStringPartsFromComps() );
            return stringBuilder.ToString();
        }

    }

}