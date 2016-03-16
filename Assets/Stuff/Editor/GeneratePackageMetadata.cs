using UnityEngine;
using UnityEditor;

public class GeneratePackageMetadata
{
	static RIBackendPackageMetaData[] allMetaData = new RIBackendPackageMetaData[]
		{
			new RIBackendPackageMetaData()
			{
				title = "Master Audio events",
				description = 
					"Shows a scene's audio events\n" +
					"and their handlers.",
				packageName = "MasterAudioEvents",
				folderName = "MasterAudioEvents",
				rank = 0
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "S-Quest quest editor",
				description = 
				"Displays the dependencies between quests\n" + 
				"and lets you create and edit them.",
				packageName = "SQuest",
				folderName = "SQuest",
				rank = 10
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "Asset reference and dependencies",
				description = 
					"A graph showing where an asset is referenced.\n" +
					"A graph showing an asset's dependencies.",
				packageName = "AssetReferences",
				folderName = "AssetReferences",
				rank = 20
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "Project view (Example)",
				description = 
					"Shows a tree of all project assets,\n" +
					"like Unity's projectview.",
				packageName = "ProjectView",
				folderName = "ProjectView",
				rank = 40
			}
		};

	[MenuItem( "Window/Generate package metadata" )]
	public static void Run()
	{
		foreach ( var metadata in allMetaData )
		{
			var asset = ScriptableObject.CreateInstance<RIBackendPackageInfo>();
			asset.metaData = metadata;
			string path = System.IO.Path.Combine( RelationsInspector.ProjectSettings.PackagesPath, metadata.packageName + ".asset" );
			AssetDatabase.DeleteAsset( path );
			AssetDatabase.CreateAsset( asset, path );
		}
		AssetDatabase.SaveAssets();
	}
}
