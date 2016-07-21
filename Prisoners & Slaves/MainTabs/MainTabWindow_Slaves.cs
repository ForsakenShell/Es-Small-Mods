using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text;

using CommunityCoreLibrary;
using CommunityCoreLibrary.UI;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PrisonersAndSlaves
{

    public class MainTabWindow_Slaves : MainTabWindow_PawnList
    {

        private enum SlaveTabs
        {
            WorkAssignment,
            TimeTable
        }

        private SlaveTabs CurrentTab = SlaveTabs.WorkAssignment;

        private const float TopAreaHeight = 65f;
        private const float LabelRowHeight = 50f;

        #region Work Assignment Fields

        private float workColumnSpacing;
        private static List<WorkTypeDef> VisibleWorkTypeDefsInPriorityOrder;

        private static MethodInfo _DrawWorkBoxBackground;

        #endregion

        #region Restrictions Fields

        private const float CopyPasteColumnWidth = 52f;
        private const float CopyPasteIconSize = 24f;
        private const float TimeTablesWidth = 500f;
        private const float AAGapWidth = 6f;
        private TimeAssignmentDef selectedAssignment;
        private List<TimeAssignmentDef> clipboard;
        private float hourWidth;

        private string labelGetsFood;
        private string labelRestrainCuff;
        private string labelRestrainShackle;
        private string labelMedicalCare;

        private float widthGetsFood;
        private float widthRestrainCuff;
        private float widthRestrainShackle;
        private float widthMedicalCare;

        #endregion

        #region General Private Methods

        private static void Reinit()
        {
            var defsInPriorityOrder = WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder;
            VisibleWorkTypeDefsInPriorityOrder = defsInPriorityOrder.Where( def => (
                ( def.visible )
            ) ).ToList();
            // Slaves can't be wardens
            VisibleWorkTypeDefsInPriorityOrder.Remove( WorkTypeDefOf.Warden );
        }

        #endregion

        #region Base Override Methods

        public MainTabWindow_Slaves()
        {
            this.workColumnSpacing = -1f;
            this.selectedAssignment = TimeAssignmentDefOf.Work;

            var originalFont = Text.Font;
            Text.Font = GameFont.Tiny;

            labelGetsFood = Data.Strings.GetsFood.Translate();
            widthGetsFood = Text.CalcSize( labelGetsFood ).x;

            labelRestrainCuff = Data.Strings.RestrainCuff.Translate();
            widthRestrainCuff = Text.CalcSize( labelRestrainCuff ).x;

            labelRestrainShackle = Data.Strings.RestrainShackle.Translate();
            widthRestrainShackle = Text.CalcSize( labelRestrainShackle ).x;

            labelMedicalCare = Data.Strings.MedicalCare.Translate();
            widthMedicalCare = Text.CalcSize( labelMedicalCare ).x;

            Text.Font = originalFont;
        }

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2( 1024.0f, ( 24.0f + 16.0f + 65.0f + this.PawnsCount * 30.0f + TopAreaHeight ) );
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            Reinit();
        }

        public override void DoWindowContents( Rect inRect )
        {
            base.DoWindowContents( inRect );

            var topWidth = 2 * 24f + 4f;
            var topRect = new Rect( ( inRect.width - topWidth ) / 2.0f, 0.0f, 24.0f, 24.0f );

            if( CCL_Widgets.ButtonImage( topRect, Data.Icons.WorkAssignments, "W", Data.Strings.SlaveWorkAssignments.Translate() ) )
            {
                CurrentTab = SlaveTabs.WorkAssignment;
            }
            topRect.x += 24f + 4f;

            if( CCL_Widgets.ButtonImage( topRect, Data.Icons.Restrictions, "A", Data.Strings.SlaveRestrictions.Translate() ) )
            {
                CurrentTab = SlaveTabs.TimeTable;
            }
            topRect.x += 24f + 4f;

            var subRect = new Rect( 0.0f, 24.0f + 16.0f, inRect.width, inRect.height - 24.0f - 4.0f );
            GUI.BeginGroup( subRect );
            {
                switch( CurrentTab )
                {
                case SlaveTabs.WorkAssignment:
                    DoWindowContents_WorkAssignments( subRect.AtZero() );
                    break;
                case SlaveTabs.TimeTable:
                    DoWindowContents_Restrictions( subRect.AtZero() );
                    break;
                }
            }
            GUI.EndGroup();
        }

        protected override void DrawPawnRow( Rect r, Pawn p )
        {
            switch( CurrentTab )
            {
            case SlaveTabs.WorkAssignment:
                DrawPawnRow_WorkAssignments( r, p );
                break;
            case SlaveTabs.TimeTable:
                DrawPawnRow_Restrictions( r, p );
                break;
            }
        }

        protected override void BuildPawnList()
        {
            this.pawns.Clear();
            this.pawns.AddRange( Find.MapPawns.AllPawns.Where( p => ( p.IsSlaveOfColony() ) ) );
        }

        #endregion

        #region Work Assignments

        private static void DrawWorkBoxBackground( Rect rect, Pawn p, WorkTypeDef workDef )
        {
            if( _DrawWorkBoxBackground == null )
            {
                _DrawWorkBoxBackground = typeof( WidgetsWork ).GetMethod( "DrawWorkBoxBackground", BindingFlags.Static | BindingFlags.NonPublic );
            }
            _DrawWorkBoxBackground.Invoke( null, new object[ ] { rect, p, workDef } );
        }

        private static Color ColorOfPriority( int prio )
        {   // May reflect this later
            switch( prio )
            {
            case 1:
                return Color.green;
            case 2:
                return new Color( 1f, 0.9f, 0.6f );
            case 3:
                return new Color( 0.8f, 0.7f, 0.5f );
            case 4:
                return new Color( 0.6f, 0.6f, 0.6f );
            default:
                return Color.grey;
            }
        }

        private static void DrawWorkBoxFor( Vector2 topLeft, Pawn p, WorkTypeDef wType, bool incapableBecauseOfCapacities )
        {
            if(
                ( !p.IsSlaveOfColony() ) ||
                ( p.workSettings == null ) ||
                ( !p.workSettings.EverWork )
            )
            {
                return;
            }
            var rect = new Rect( topLeft.x, topLeft.y, 25f, 25f );
            if( incapableBecauseOfCapacities )
            {
                GUI.color = new Color( 1f, 0.3f, 0.3f );
                DrawWorkBoxBackground( rect, p, wType );
                GUI.color = Color.white;
            }
            else
            {
                DrawWorkBoxBackground( rect, p, wType );
            }
            if( Find.PlaySettings.useWorkPriorities )
            {
                var priority1 = p.workSettings.GetPriority( wType );
                var label = priority1 <= 0 ? string.Empty : priority1.ToString();

                Text.Anchor = TextAnchor.MiddleCenter;

                GUI.color = ColorOfPriority( priority1 );
                Widgets.Label( rect.ContractedBy( -3f ), label );
                GUI.color = Color.white;

                Text.Anchor = TextAnchor.UpperLeft;

                if(
                    ( Event.current.type != EventType.MouseDown ) ||
                    ( !Mouse.IsOver( rect ) )
                )
                {
                    return;
                }

                if( Event.current.button == 0 )
                {
                    int priority2 = p.workSettings.GetPriority( wType ) - 1;
                    if( priority2 < 0 )
                    {
                        priority2 = 4;
                    }
                    p.workSettings.ForcePriority( wType, priority2 );
                    SoundStarter.PlayOneShotOnCamera( SoundDefOf.AmountIncrement );
                }
                if( Event.current.button == 1 )
                {
                    int priority2 = p.workSettings.GetPriority( wType ) + 1;
                    if( priority2 > 4 )
                    {
                        priority2 = 0;
                    }
                    p.workSettings.ForcePriority( wType, priority2 );
                    SoundStarter.PlayOneShotOnCamera( SoundDefOf.AmountDecrement );
                }
                Event.current.Use();
            }
            else
            {
                if( p.workSettings.GetPriority( wType ) > 0 )
                {
                    GUI.DrawTexture( rect, (Texture) WidgetsWork.WorkBoxCheckTex );
                }
                if( !Widgets.ButtonInvisible( rect ) )
                {
                    return;
                }
                if( p.workSettings.GetPriority( wType ) > 0 )
                {
                    p.workSettings.ForcePriority( wType, 0 );
                    SoundStarter.PlayOneShotOnCamera( SoundDefOf.CheckboxTurnedOff );
                }
                else
                {
                    p.workSettings.ForcePriority( wType, 3 );
                    SoundStarter.PlayOneShotOnCamera( SoundDefOf.CheckboxTurnedOn );
                }
            }
        }

        private void DoWindowContents_WorkAssignments( Rect rect )
        {
            var position1 = new Rect( 0.0f, 0.0f, rect.width, 40f );
            GUI.BeginGroup( position1 );
            {
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                var rect1 = new Rect( 5f, 5f, 140f, 30f );
                var flag = Current.Game.playSettings.useWorkPriorities;

                Widgets.CheckboxLabeled( rect1, Data.Strings.ManualPriorities.Translate(), ref Current.Game.playSettings.useWorkPriorities, false );
                if( flag != Current.Game.playSettings.useWorkPriorities )
                {
                    foreach( Pawn pawn in Find.MapPawns.FreeColonists )
                    {
                        pawn.workSettings.Notify_UseWorkPrioritiesChanged();
                    }
                }

                var num1 = position1.width / 3f;
                var num2 = position1.width * 2.0f / 3.0f;
                var rect2 = new Rect( num1 - 50f, 5f, 160f, 30f );
                var rect3 = new Rect( num2 - 50f, 5f, 160f, 30f );

                GUI.color = new Color( 1f, 1f, 1f, 0.5f );
                Text.Anchor = TextAnchor.UpperCenter;
                Text.Font = GameFont.Tiny;

                Widgets.Label( rect2, "<= " + Data.Strings.HigherPriority.Translate() );
                Widgets.Label( rect3, Data.Strings.LowerPriority.Translate() + " =>" );

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            GUI.EndGroup();

            var position2 = new Rect( 0.0f, 40f, rect.width, rect.height - 40f );
            GUI.BeginGroup( position2 );
            {
                Text.Font = GameFont.Small;
                GUI.color = Color.white;

                var outRect = new Rect( 0.0f, LabelRowHeight, position2.width, position2.height - LabelRowHeight );
                this.workColumnSpacing = ( position2.width - 16.0f - 175.0f ) / (float) VisibleWorkTypeDefsInPriorityOrder.Count;
                var num3 = 175f;
                int num4 = 0;

                using( List<WorkTypeDef>.Enumerator enumerator = VisibleWorkTypeDefsInPriorityOrder.GetEnumerator() )
                {
                    while( enumerator.MoveNext() )
                    {
                        var current = enumerator.Current;
                        var vector2 = Text.CalcSize( current.labelShort );
                        var x = num3 + 15f;
                        var rect4 = new Rect( x - vector2.x / 2f, 0.0f, vector2.x, vector2.y );

                        if( num4 % 2 == 1 )
                        {
                            rect4.y += 20f;
                        }
                        if( Mouse.IsOver( rect4 ) )
                        {
                            Widgets.DrawHighlight( rect4 );
                        }

                        Text.Anchor = TextAnchor.MiddleCenter;
                        Widgets.Label( rect4, current.labelShort );
                        TooltipHandler.TipRegion( rect4, new TipSignal(
                            (
                                current.gerundLabel +
                                "\n\n" +
                                current.description +
                                "\n\n" +
                                SpecificWorkListString( current )
                            ),
                            current.GetHashCode()
                        ) );

                        GUI.color = new Color( 1f, 1f, 1f, 0.3f );
                        Widgets.DrawLineVertical( x, rect4.yMax - 3f, ( LabelRowHeight - rect4.yMax + 3.0f ) );
                        Widgets.DrawLineVertical( x + 1f, rect4.yMax - 3f, ( LabelRowHeight - rect4.yMax + 3.0f ) );
                        GUI.color = Color.white;

                        num3 += this.workColumnSpacing;
                        ++num4;
                    }
                }
                this.DrawRows( outRect );
            }
            GUI.EndGroup();
        }

        private static string SpecificWorkListString( WorkTypeDef def )
        {
            var stringBuilder = new StringBuilder();
            for( int index = 0; index < def.workGiversByPriority.Count; ++index )
            {
                stringBuilder.Append( def.workGiversByPriority[ index ].LabelCap );
                if( def.workGiversByPriority[ index ].emergency )
                {
                    stringBuilder.Append( " (" + Data.Strings.EmergencyWorkMarker.Translate() + ")" );
                }
                if( index < def.workGiversByPriority.Count - 1 )
                {
                    stringBuilder.AppendLine();
                }
            }
            return stringBuilder.ToString();
        }

        private void DrawPawnRow_WorkAssignments( Rect rect, Pawn p )
        {
            p.workSettings.Disable( WorkTypeDefOf.Warden );
            var x = 175f;
            Text.Font = GameFont.Medium;
            for( int index = 0; index < VisibleWorkTypeDefsInPriorityOrder.Count; ++index )
            {
                var workTypeDef = VisibleWorkTypeDefsInPriorityOrder[ index ];
                var topLeft = new Vector2( x, rect.y + 2.5f );
                var incapableBecauseOfCapacities = this.IsIncapableOfWholeWorkType( p, VisibleWorkTypeDefsInPriorityOrder[ index ] );
                DrawWorkBoxFor( topLeft, p, workTypeDef, incapableBecauseOfCapacities );
                TooltipHandler.TipRegion( new Rect( topLeft.x, topLeft.y, 25f, 25f ), WidgetsWork.TipForPawnWorker( p, workTypeDef, incapableBecauseOfCapacities ) );
                x += this.workColumnSpacing;
            }
        }

        private bool IsIncapableOfWholeWorkType( Pawn p, WorkTypeDef work )
        {
            for( int index1 = 0; index1 < work.workGiversByPriority.Count; ++index1 )
            {
                for( int index2 = 0; index2 < work.workGiversByPriority[ index1 ].requiredCapacities.Count; ++index2 )
                {
                    var activity = work.workGiversByPriority[ index1 ].requiredCapacities[ index2 ];
                    if( !p.health.capacities.CapableOf( activity ) )
                    {
                        //Log.Message( string.Format( "Pawn {0} - false on {1}", p.NameStringShort, activity.label ) );
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion

        #region Restrictions

        private void DoWindowContents_Restrictions( Rect inRect )
        {
            var position = new Rect( 0.0f, 0.0f, inRect.width, 65f );
            GUI.BeginGroup( position );
            {
                this.DrawTimeAssignmentSelectorGrid( new Rect( 0.0f, 0.0f, 217f, position.height ) );
                var num = 227f;

                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.LowerLeft;

                for( int index = 0; index < 24; ++index )
                {
                    Widgets.Label( new Rect( num + 4f, 0.0f, this.hourWidth, position.height + 3f ), index.ToString() );
                    num += this.hourWidth;
                }

                Text.Font = GameFont.Tiny;

                var left = num + 6f;
                var rect = new Rect( left, 0.0f, widthGetsFood, position.height + 3f );
                Widgets.Label( rect, labelGetsFood );
                rect.x += widthGetsFood + 6f;

                rect.width = widthRestrainCuff;
                Widgets.Label( rect, labelRestrainCuff );
                rect.x += widthRestrainCuff + 6f;

                rect.width = widthRestrainShackle;
                Widgets.Label( rect, labelRestrainShackle );
                rect.x += widthRestrainShackle + 6f;

                rect.width = widthMedicalCare;
                rect.x += ( MedicalCareUtility.CareSetterWidth - widthMedicalCare ) / 2f;
                Widgets.Label( rect, labelMedicalCare );

            }
            GUI.EndGroup();

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            this.DrawRows( new Rect( 0.0f, position.height, inRect.width, inRect.height - position.height ) );
        }

        private void DrawTimeAssignmentSelectorGrid( Rect rect )
        {
            rect.yMax -= 2f;
            var rect1 = rect;
            rect1.xMax = rect1.center.x;
            rect1.yMax = rect1.center.y;
            this.DrawTimeAssignmentSelectorFor( rect1, TimeAssignmentDefOf.Anything );
            rect1.x += rect1.width;
            this.DrawTimeAssignmentSelectorFor( rect1, TimeAssignmentDefOf.Work );
            rect1.y += rect1.height;
            rect1.x -= rect1.width;
            this.DrawTimeAssignmentSelectorFor( rect1, TimeAssignmentDefOf.Joy );
            rect1.x += rect1.width;
            this.DrawTimeAssignmentSelectorFor( rect1, TimeAssignmentDefOf.Sleep );
        }

        private void DrawTimeAssignmentSelectorFor( Rect rect, TimeAssignmentDef ta )
        {
            rect = rect.ContractedBy( 2f );

            GUI.DrawTexture( rect, (Texture) ta.ColorTexture );
            if( Widgets.ButtonInvisible( rect ) )
            {
                this.selectedAssignment = ta;
                SoundStarter.PlayOneShotOnCamera( SoundDefOf.TickHigh );
            }

            GUI.color = Color.white;
            if( Mouse.IsOver( rect ) )
            {
                Widgets.DrawHighlight( rect );
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label( rect, ta.LabelCap );

            Text.Anchor = TextAnchor.UpperLeft;

            if( this.selectedAssignment != ta )
            {
                return;
            }
            Widgets.DrawBox( rect, 2 );
        }

        private void DrawPawnRow_Restrictions( Rect r, Pawn p )
        {
            var compPrisoner = p.TryGetComp<CompPrisoner>();
            if( compPrisoner == null )
            {
                Log.ErrorOnce( string.Format( "Pawn {0} is missing CompPrisoner!", p.NameStringShort ), ( p.thingIDNumber & 0xFFFF ) | 0x0BAD0000 );
                return;
            }
            GUI.BeginGroup( r );
            {
                var rect1 = new Rect( 175f, 0.0f, 24f, 24f );

                if( CCL_Widgets.ButtonImage( rect1, Data.Icons.Copy, "C", Data.Strings.Copy.Translate() ) )
                {
                    this.CopyFrom_Restrictions( p );
                    SoundStarter.PlayOneShotOnCamera( SoundDefOf.TickHigh );
                }

                if( this.clipboard != null )
                {
                    Rect rect2 = rect1;
                    rect2.x = rect1.xMax + 2f;
                    if( CCL_Widgets.ButtonImage( rect2, Data.Icons.Paste, "P", Data.Strings.Paste.Translate() ) )
                    {
                        this.PasteTo_Restrictions( p );
                        SoundStarter.PlayOneShotOnCamera( SoundDefOf.TickLow );
                    }
                }

                var left1 = 227f;
                this.hourWidth = 20.83333f;
                for( int hour = 0; hour < 24; ++hour )
                {
                    this.DoTimeAssignment( new Rect( left1, 0.0f, this.hourWidth, r.height ), p, hour );
                    left1 += this.hourWidth;
                }

                GUI.color = Color.white;

                var left2 = left1 + 6f;
                var vector = new Vector2( 0f, 0f );

                vector.x = left2 + ( widthGetsFood - 24f ) / 2f;
                bool getsFood = p.guest.GetsFood;
                Widgets.Checkbox( vector, ref getsFood );
                p.guest.GetsFood = getsFood;
                left2 += widthGetsFood + 6f;

                vector.x = left2 + ( widthRestrainCuff - 24f ) / 2f;
                Widgets.Checkbox( vector, ref compPrisoner.ShouldBeCuffed );
                left2 += widthRestrainCuff + 6f;

                vector.x = left2 + ( widthRestrainShackle - 24f ) / 2f;
                Widgets.Checkbox( vector, ref compPrisoner.ShouldBeShackled );
                left2 += widthRestrainShackle + 6f;

                var rect3 = new Rect( left2, 0f, MedicalCareUtility.CareSetterWidth, 28f );
                MedicalCareUtility.MedicalCareSetter( rect3, ref p.playerSettings.medCare );
            }
            GUI.EndGroup();
        }

        private void DoTimeAssignment( Rect rect, Pawn p, int hour )
        {
            rect = rect.ContractedBy( 1f );
            var assignment = p.timetable.GetAssignment( hour );
            GUI.DrawTexture( rect, (Texture) assignment.ColorTexture );
            if( !Mouse.IsOver( rect ) )
            {
                return;
            }
            Widgets.DrawBox( rect, 2 );
            if(
                ( assignment == this.selectedAssignment ) ||
                ( !Input.GetMouseButton( 0 ) )
            )
            {
                return;
            }
            SoundStarter.PlayOneShotOnCamera( SoundDefOf.DesignateDragStandardChanged );
            p.timetable.SetAssignment( hour, this.selectedAssignment );
        }

        private void CopyFrom_Restrictions( Pawn p )
        {
            this.clipboard = ( (IEnumerable<TimeAssignmentDef>) p.timetable.times ).ToList<TimeAssignmentDef>();
        }

        private void PasteTo_Restrictions( Pawn p )
        {
            for( int index = 0; index < 24; ++index )
            {
                p.timetable.times[ index ] = this.clipboard[ index ];
            }
        }
    }

    #endregion

}
