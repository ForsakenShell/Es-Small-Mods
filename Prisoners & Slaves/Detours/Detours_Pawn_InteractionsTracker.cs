using System;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace PrisonersAndSlaves
{

    internal static class _Pawn_InteractionsTracker
    {

        internal static FieldInfo _pawn;

        internal static Pawn Pawn( this Pawn_InteractionsTracker tracker )
        {
            if( _pawn == null )
            {
                _pawn = typeof( _Pawn_InteractionsTracker ).GetField( "pawn", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (Pawn) _pawn.GetValue( tracker );
        }

        internal static void _StartSocialFight( this Pawn_InteractionsTracker tracker, Pawn otherPawn )
        {
            var pawn = tracker.Pawn();
            if(
                ( PawnUtility.ShouldSendNotificationAbout( pawn ) ) ||
                ( PawnUtility.ShouldSendNotificationAbout( otherPawn ) )
            )
            {
                Thought thought;
                if( !InteractionUtility.TryGetRandomSocialFightProvokingThought( pawn, otherPawn, out thought ) )
                {
                    Log.Warning( "Pawn " + (object) pawn + " started a social fight with " + (string) (object) otherPawn + ", but he has no negative opinion thoughts towards " + (string) (object) otherPawn + "." );
                }
                else
                {
                    Messages.Message( Data.Strings.MessageSocialFight.Translate( pawn.LabelShort, otherPawn.LabelShort, thought.LabelCapSocial ), pawn, MessageSound.SeriousAlert );
                }
            }
            pawn.mindState.mentalStateHandler.TryStartMentalState( MentalStateDefOf.SocialFighting );
            if( pawn.InMentalState )
            {
                var pawnState = pawn.MentalState as MentalState_SocialFight;
                pawnState.otherPawn = otherPawn;
                pawnState.Instigator = pawn;
            }
            otherPawn.mindState.mentalStateHandler.TryStartMentalState( MentalStateDefOf.SocialFighting );
            if( otherPawn.InMentalState )
            {
                var otherState = otherPawn.MentalState as MentalState_SocialFight;
                otherState.pawn = pawn;
                otherState.Instigator = pawn;
            }
        }

    }

}
