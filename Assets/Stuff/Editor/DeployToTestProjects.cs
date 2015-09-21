using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using RelationsInspector;
using System;

namespace Stuff
{
    class DeployToTestProjects
    {
        // test projects are located in the TestProjects folder, which lives next to Assets

        // name of the directory containing the test projects
        const string TestProjDirName = "TestProjects";
        const string AssetsDirName = "Assets";
        const string RIDirname = "RelationsInspector";
        const string UserLandDirName = @"RelationsInspectorUserLand\Editor\";

        static readonly string RIProjPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf(AssetsDirName));
        static readonly string TestProjBasePath = Path.Combine(RIProjPath, TestProjDirName);

        // all test project names
        public static readonly string[] testProjNames = new[]
        {
            "SpaceShooter",
            "Stealth",
            "SurvivalShooter",
            "UfoShooter"
        };

        public static readonly string[] backendDirNames = new[]
        {
            UserLandDirName + "GameObjectTag",
            UserLandDirName + "ObjectDependency"
        };

        const string Unity4EditorPath = @"C:\Program Files (x86)\Unity4.6.1\Editor\Unity.exe";
        const string Unity5EditorPath = @"C:\Program Files\Unity5.2\Editor\Unity.exe";

        private static string GetTestProjPath(string testProjName)
        {
            return Path.Combine(TestProjBasePath, testProjName);
        }

        private static string GetTestProjAssetPath(string testProjName)
        {
            return Path.Combine(GetTestProjPath(testProjName), AssetsDirName);
        }

        private static void CopyDirectory(string sourcePath, string destinationPath)
        {
            // remove destination path
            string removeCmdText = string.Format(@"rmdir /s /q {0}", destinationPath);
            Util.RunSystemCmd(removeCmdText);

            // create destination path
            string createCmdText = string.Format(@"mkdir {0}", destinationPath);
            Util.RunSystemCmd(createCmdText);

            // copy source directory to destination
            string copyCmdText = string.Format(@"xcopy /e /v {0} {1}", sourcePath, destinationPath);
            Util.RunSystemCmd(copyCmdText);
        }

        public static void DeployRI(string testProjName)
        {
            string testProjAssetPath = GetTestProjAssetPath(testProjName);
            string testProjRIPath = Path.Combine(testProjAssetPath, RIDirname).Replace('/', '\\');

            string RIsourcePath = Path.Combine(Application.dataPath, RIDirname).Replace('/', '\\');

            CopyDirectory(RIsourcePath, testProjRIPath);
        }

        public static void DeployBackend(string testProjName, string backendName)
        {
            string backendDirName = backendDirNames.Where(str => str.Contains(backendName)).SingleOrDefault();
            if( string.IsNullOrEmpty(backendDirName) )
            {
                Debug.Log("no match for backend name " + backendName );
                return;
            }

            string sourcePath = Path.Combine( Application.dataPath, backendDirName ).Replace('/', '\\');
            string testProjAssetPath = GetTestProjAssetPath(testProjName);
            string destinationPath = Path.Combine(testProjAssetPath, backendDirName).Replace('/', '\\');

            CopyDirectory(sourcePath, destinationPath);
        }

        public static void Open(string projName)
        {
            string cmdText = string.Format("\"{0}\" -projectPath {1}", Unity4EditorPath, GetTestProjPath(projName) );
            Util.RunSystemCmd(cmdText);
        }
    }
}
