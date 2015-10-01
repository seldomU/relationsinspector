using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace RelationsInspector
{
    class Settings
    {
        const string SettingsFileName = "Settings.asset";

        private static RelationsInspectorSettings storage = LoadStorage();
        public static RelationsInspectorSettings Storage
        {
            get { return storage; }
        }

        static RelationsInspectorSettings LoadStorage()
        {
            string sFilePath = Path.Combine( ProjectSettings.ResourcesPath, SettingsFileName ).Replace( '\\', '/' );
            if ( File.Exists( sFilePath ) )
                return Util.LoadAsset<RelationsInspectorSettings>( sFilePath );

            // doesn't exist, so create it
            var storage = CreateDefaultStorage();
            AssetDatabase.CreateAsset( storage, sFilePath );
            AssetDatabase.SaveAssets();
            return storage;
        }

        static RelationsInspectorSettings CreateDefaultStorage()
        {
            var storage = ScriptableObject.CreateInstance<RelationsInspectorSettings>();
            return storage;
        }
    }
}
