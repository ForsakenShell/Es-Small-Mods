using RimWorld;
using UnityEngine;
using Verse;

namespace PrisonersAndSlaves
{

    public static class PaS_Widgets
    {

        private static void DrawWorkBoxBackground( Rect rect, Pawn p, WorkTypeDef workDef )
        {
            float num = p.skills.AverageOfRelevantSkillsFor( workDef );
            Texture2D texture2D1;
            Texture2D texture2D2;
            float a;
            var foo = p.workSettings.GetPriority( workDef );
            if( (double) num <= 14.0 )
            {
                texture2D1 = WidgetsWork.WorkBoxBGTex_Bad;
                texture2D2 = WidgetsWork.WorkBoxBGTex_Mid;
                a = num / 14f;
            }
            else
            {
                texture2D1 = WidgetsWork.WorkBoxBGTex_Mid;
                texture2D2 = WidgetsWork.WorkBoxBGTex_Excellent;
                a = (float) ( ( (double) num - 14.0 ) / 6.0 );
            }
            GUI.DrawTexture( rect, (Texture) texture2D1 );
            GUI.color = new Color( GUI.color.r, GUI.color.g, GUI.color.b, a );
            GUI.DrawTexture( rect, (Texture) texture2D2 );
            Passion passion = p.skills.MaxPassionOfRelevantSkillsFor( workDef );
            if( passion > Passion.None )
            {
                GUI.color = new Color( 1f, 1f, 1f, 0.4f );
                Rect position = rect;
                position.xMin = rect.center.x;
                position.yMin = rect.center.y;
                if( passion == Passion.Minor )
                    GUI.DrawTexture( position, (Texture) WidgetsWork.PassionWorkboxMinorIcon );
                else if( passion == Passion.Major )
                    GUI.DrawTexture( position, (Texture) WidgetsWork.PassionWorkboxMajorIcon );
            }
            GUI.color = Color.white;
        }

    }

}