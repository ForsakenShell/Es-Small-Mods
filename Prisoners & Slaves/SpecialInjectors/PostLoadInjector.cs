using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CommunityCoreLibrary;

using RimWorld;
using Verse;

namespace PrisonersAndSlaves
{
    
    public class PostLoadInjector : SpecialInjector
    {

        private void RemoveCollarsAndRestraintsFromOutfits()
        {
            var allPaSApparel = Data.AllSlaveCollarDefs();
            allPaSApparel.AddRange( Data.AllRestraintDefs() );
            foreach( var outfit in Current.Game.outfitDatabase.AllOutfits )
            {
                foreach( var apparel in allPaSApparel )
                {
                    outfit.filter.SetAllow( apparel, false );
                }
            }
        }

        public override bool Inject()
        {
            // Make sure pawns don't try to put collars and restraints on by default
            RemoveCollarsAndRestraintsFromOutfits();

            return true;
        }

    }

}
