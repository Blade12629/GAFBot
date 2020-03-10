using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GAFBot.Modules
{
    public static class ModuleHandler
    {
        private static List<IModule> _activeModules;

        [AutoInit(1)]
        public static void Initialize()
        {
            _activeModules = new List<IModule>();

            List<FileInfo> modules = FindModules(Path.Combine(Program.CurrentPath + @"data\"));
            InitializeModules(ref modules);

            if (_activeModules.Count > 0)
            {
                foreach (IModule module in _activeModules)
                    module.Initialize();

                Program.ExitEvent += Dispose;
            }

            Logger.Log($"Initialized {_activeModules.Count} modules, failed: {modules.Count}", (modules.Count == 0 ? LogLevel.Info : LogLevel.ERROR));
        }
        
        public static IModule Get(string name)
        {
            return _activeModules.FirstOrDefault(m => m.ModuleName.Equals(name));
        }

        public static void Dispose()
        {
            while (_activeModules.Count > 0)
            {
                IModule module = _activeModules[0];

                module.Dispose();
                _activeModules.RemoveAt(0);
            }
        }

        /// <summary>
        /// Searches for modules
        /// </summary>
        /// <param name="path">module folder</param>
        /// <returns>modules, if none found returns empty list</returns>
        private static List<FileInfo> FindModules(string path)
        {
            List<FileInfo> result = new List<FileInfo>();

            DirectoryInfo dir = new DirectoryInfo(path);

            if (!dir.Exists)
                throw new FileNotFoundException("Could not find module folder " + path);

            result.AddRange(dir.EnumerateFiles("*.mod", SearchOption.AllDirectories));

            return result;
        }

        private static void InitializeModules(ref List<FileInfo> moduleFiles)
        {
            if (moduleFiles.Count == 0)
                return;

            for (int i = 0; i < moduleFiles.Count; i++)
            {
                if (!moduleFiles[i].Exists)
                    continue;

                try
                {
                    Assembly ass = Assembly.LoadFile(moduleFiles[i].FullName);
                    Type[] types = ass.GetTypes();

                    foreach (Type t in types)
                    {
                        if (t.IsAssignableFrom(typeof(IModule)))
                        {
                            IModule mod = Activator.CreateInstance(t) as IModule;

                            if (mod == null || _activeModules.Contains(mod))
                                continue;

                            _activeModules.Add(mod);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.ToString(), LogLevel.ERROR);
                    continue;
                }

                moduleFiles.RemoveAt(i);
                i--;
            }
        }
    }
}
