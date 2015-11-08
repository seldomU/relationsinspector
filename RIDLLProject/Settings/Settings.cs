using System.IO;
using UnityEngine;
using UnityEditor;

namespace RelationsInspector
{
    public class Settings
    {
        const string SettingsFileName = "Settings.asset";
        public static string SettingsPath = Path.Combine( ProjectSettings.ResourcesPath, SettingsFileName ).Replace( '\\', '/' );

        static RelationsInspectorSettings storage = LoadStorage();
        internal static RelationsInspectorSettings Instance
        {
            get { return storage; }
        }

        static RelationsInspectorSettings LoadStorage()
        {
            if ( File.Exists( SettingsPath ) )
                return Util.LoadAsset<RelationsInspectorSettings>( SettingsPath );

            // doesn't exist, so create it
            var storage = CreateDefaultStorage();
            AssetDatabase.CreateAsset( storage, SettingsPath );
            AssetDatabase.SaveAssets();
            return storage;
        }

        static RelationsInspectorSettings CreateDefaultStorage()
        {
            var storage = ScriptableObject.CreateInstance<RelationsInspectorSettings>();

            // initialize with default values
            storage.maxGraphNodes = 100;
            storage.treeRootLocation = TreeRootLocation.Left;
            storage.showMinimap = true;
            storage.minimapLocation = MinimapLocation.TopLeft;
            storage.layoutParams = new GraphLayoutParams();
            storage.logToConsole = true;
            return storage;
        }
    }
}
