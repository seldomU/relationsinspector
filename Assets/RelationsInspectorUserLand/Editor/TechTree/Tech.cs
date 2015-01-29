using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace RelationsInspector.Backend.Techtree
{
	public class Tech : ScriptableObject
	{
		public string title;
		public string description;
		public List<Tech> dependentTechs;
		public int developmentCost;
		public TechAchievement achievement;
	}

	public enum TechAchievement { CanBuildLibrary, CanSailTheOcean };
}
