using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GAFBot.plugins
{
    public class PluginHandler
    {
        private Dictionary<IPlugin, Assembly> _loadedPlugins;
        private Dictionary<IPlugin, object> _activePlugins;

        public string PluginPath { get; set; }

        public bool Loaded { get; private set; }
        public bool Ready { get; private set; }

        public PluginHandler(string path)
        {
            PluginPath = path;
        }

        public void Load()
        {
            if (!Directory.Exists(PluginPath))
                return;

            if (Loaded)
                _loadedPlugins = new Dictionary<IPlugin, Assembly>();

            GC.Collect();

            List<FileInfo> pluginFiles = new DirectoryInfo(PluginPath).EnumerateFiles().Where(fi => fi.Extension.ToLower().Contains("dll")).ToList();

            byte[] pluginData;

            pluginFiles.ForEach(fi =>
            {
                pluginData = File.ReadAllBytes(fi.FullName);
                Assembly pluginAssembly = Assembly.Load(pluginData);
                IPlugin pluginInfo = Activator.CreateInstance(pluginAssembly.GetType("GAFBot.PluginInfo")) as IPlugin;
                _loadedPlugins.Add(pluginInfo, pluginAssembly);
            });
            
            Loaded = true;
        }

        public void LoadPlugins()
        {
            if (Loaded)
                _activePlugins = new Dictionary<IPlugin, object>();

            GC.Collect();

            foreach (var kvp in _loadedPlugins)
            {
                Type pluginType = kvp.Value.GetType(kvp.Key.TypeString);
                MethodInfo pluginEntry = pluginType.GetMethod(kvp.Key.MethodString);
                object plugin = Activator.CreateInstance(pluginType);

                _activePlugins.Add(kvp.Key, plugin);
                try
                {
                    if (pluginEntry.IsStatic)
                        pluginEntry.Invoke(null, null);
                    else
                        pluginEntry.Invoke(plugin, null);
                }
                catch (Exception ex)
                {
                    Program.Logger.Log(ex.ToString(), showConsole: Program.Config.Debug);
                }
            }
        }

        public void LoadPlugin(string file)
        {
            if (!File.Exists(file))
                return;

            byte[] data = File.ReadAllBytes(file);
            Assembly pluginAssembly = Assembly.Load(data);
            IPlugin pluginInfo = Activator.CreateInstance(pluginAssembly.GetType("GAFBot.PluginInfo")) as IPlugin;
            _loadedPlugins.Add(pluginInfo, pluginAssembly);
            object plugin = Activator.CreateInstance(pluginAssembly.GetType(pluginInfo.TypeString));
            MethodInfo pluginEntry = pluginAssembly.GetType(pluginInfo.TypeString).GetMethod(pluginInfo.MethodString);

            _activePlugins.Add(pluginInfo, plugin);
            try
            {
                if (pluginEntry.IsStatic)
                    pluginEntry.Invoke(null, null);
                else
                    pluginEntry.Invoke(plugin, null);
            }
            catch (Exception ex)
            {
                Program.Logger.Log(ex.ToString(), showConsole: Program.Config.Debug);
            }
        }
    }

    public interface IPlugin
    {
        string Author { get; set; }
        int Version { get; set; }
        string Name { get; set; }
        string TypeString { get; set; }
        string MethodString { get; set; }
    }
}
