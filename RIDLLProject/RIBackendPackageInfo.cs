using UnityEngine;

[System.Serializable]
public class RIBackendPackageMetaData
{
	public string title;
	public string description;
	public string packageName;
	public string folderName;
	public float rank;
}

public class RIBackendPackageInfo : ScriptableObject
{
	public RIBackendPackageMetaData metaData;
}
