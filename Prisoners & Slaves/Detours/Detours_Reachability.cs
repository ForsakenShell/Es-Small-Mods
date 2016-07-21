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

        internal static Type                _dictType;
        internal static PropertyInfo        _dictCount;
        internal static MethodInfo          _dictClear;

        internal static object              cache;
        internal static object              cacheDict;

        static _Reachability()
        {
            _ReachabilityCache = CommunityCoreLibrary.Controller.Data.Assembly_CSharp.GetType( "Verse.ReachabilityCache" );
            if( _ReachabilityCache == null )
            {
                Log.ErrorOnce( "Unable to get \"Verse.ReachabilityCache\"", 0x0DEAD001 );
            }
            _ReachabilityCache_CachedEntry = CommunityCoreLibrary.Controller.Data.Assembly_CSharp.GetType( "Verse.ReachabilityCache+CachedEntry" );
            if( _ReachabilityCache_CachedEntry == null )
            {
                Log.ErrorOnce( "Unable to get \"Verse.ReachabilityCache.CachedEntry\"", 0x0DEAD006 );
            }
            _cache = typeof( Verse.Reachability ).GetField( "cache", BindingFlags.Static | BindingFlags.NonPublic );
            if( _cache == null )
            {
                Log.ErrorOnce( "Unable to get \"Verse.Reachability.cache\"", 0x0DEAD002 );
            }
            _cacheDict = _ReachabilityCache.GetField( "cacheDict", BindingFlags.Instance | BindingFlags.NonPublic );
            if( _cacheDict == null )
            {
                Log.ErrorOnce( "Unable to get \"Verse.ReachabilityCache.cacheDict\"", 0x0DEAD003 );
            }
            cache = _cache.GetValue( null );
            if( cache == null )
            {
                Log.ErrorOnce( "Unable to get static field value of \"Verse.Reachability.cache\"", 0x0DEAD004 );
            }
            cacheDict = _cacheDict.GetValue( cache );
            if( cacheDict == null )
            {
                Log.ErrorOnce( "Unable to get instance field value \"Verse.ReachabilityCache.cacheDict\"", 0x0DEAD005 );
            }
            _dictType = cacheDict.GetType();
            _dictCount = _dictType.GetProperty( "Count" );
            _dictClear = _dictType.GetMethod( "Clear" );
        }

        internal static int                 cacheDictCount()
        {
            return (int)_dictCount.GetValue( cacheDict, null );
        }

        internal static void                cacheDictClear()
        {
            _dictClear.Invoke( cacheDict, null );
        }

        internal static void                _ClearCache()
        {
            if( Current.ProgramState == ProgramState.Entry )
            {
                return;
            }
            if( cacheDictCount() <= 0 )
            {
                return;
            }
            cacheDictClear();
            foreach( var door in Find.ListerBuildings.AllBuildingsColonistOfClass<Building_RestrictedDoor>() )
            {
                door.ClearCache();
            }
        }

    }

}
