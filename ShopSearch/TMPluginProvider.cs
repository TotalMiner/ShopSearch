using StudioForge.TotalMiner.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ItemSearch
{
    class TMPluginProvider : ITMPluginProvider
    {
        public TMPluginProvider()
        {
            //if(EmbeddedAssembly.Get(""))
            EmbeddedAssembly.Load("ShopSearch.Assemblies.0Harmony.dll", "0Harmony.dll");
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        public ITMPlugin GetPlugin()
        {
            return new ShopSearchPlugin();
        }

        public ITMPluginArcade GetPluginArcade()
        {
            return null;
        }

        public ITMPluginBlocks GetPluginBlocks()
        {
            return null;
        }

        public ITMPluginGUI GetPluginGUI()
        {
            return null;
        }
    }
}
