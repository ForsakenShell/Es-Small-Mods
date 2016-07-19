using System;
using System.Collections.Generic;
using System.Reflection;

using CommunityCoreLibrary;
using RimWorld;
using Verse;
using Verse.AI;

namespace PrisonersAndSlaves
{
    
    internal static class _Reachability
    {

        internal static Type                _ReachabilityCache;
        internal static Type                _ReachabilityCache_CachedEntry;

        internal static FieldInfo           _cache;
        internal static FieldInfo           _cacheDict;

        internal static void                _ClearCache()
        {
            if( _ReachabilityCache == null )
            {
                _ReachabilityCache = CommunityCoreLibrary.Controller.Data.Assembly_CSharp.GetType( "Verse.ReachabilityCache" );
                if( _ReachabilityCache == null )
                {
                    Log.ErrorOnce( "Unable to get \"Verse.ReachabilityCache\"", 0x0DEAD001 );
                }
            }
            if( _ReachabilityCache_CachedEntry == null )
            {
                _ReachabilityCache_CachedEntry = CommunityCoreLibrary.Controller.Data.Assembly_CSharp.GetType( "Verse.ReachabilityCache+CachedEntry" );
                if( _ReachabilityCache_CachedEntry == null )
                {
                    Log.ErrorOnce( "Unable to get \"Verse.ReachabilityCache.CachedEntry\"", 0x0DEAD006 );
                }
            }
            if( _cache == null )
            {
                _cache = typeof( Verse.Reachability ).GetField( "cache", BindingFlags.Static | BindingFlags.NonPublic );
                if( _cache == null )
                {
                    Log.ErrorOnce( "Unable to get \"Verse.Reachability.cache\"", 0x0DEAD002 );
                }
            }
            if( _cacheDict == null )
            {
                _cacheDict = _ReachabilityCache.GetField( "cacheDict", BindingFlags.Instance | BindingFlags.NonPublic );
                if( _cacheDict == null )
                {
                    Log.ErrorOnce( "Unable to get \"Verse.ReachabilityCache.cacheDict\"", 0x0DEAD003 );
                }
            }
            var cache = _cache.GetValue( null );
            if( cache == null )
            {
                Log.ErrorOnce( "Unable to get static field value of \"Verse.Reachability.cache\"", 0x0DEAD004 );
            }
            var cacheDict = (Dictionary<object, bool>)_cacheDict.GetValue( cache );
            if( cacheDict == null )
            {
                Log.ErrorOnce( "Unable to get instance field value \"Verse.ReachabilityCache.cacheDict\"", 0x0DEAD005 );
            }
            if( cacheDict.Count <= 0 )
            {
                return;
            }
            cacheDict.Clear();
            if( Current.ProgramState != ProgramState.MapPlaying )
            {
                return;
            }
            foreach( var door in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_RestrictedDoor>() )
            {
                door.QueueDoorStatusUpdate( true );
            }
        }

    }

}
