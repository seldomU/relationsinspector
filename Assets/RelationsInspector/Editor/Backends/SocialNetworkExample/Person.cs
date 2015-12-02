using UnityEngine;
using System.Collections.Generic;

namespace RelationsInspector.Backend.SocialNetwork
{
	public class Person : ScriptableObject
	{
		public int age;
		public int favoriteNumber;
		public Color favoriteColor;
		public List<Acquaintance> acquaintances;
		public List<Person> studyPartners;
	}

	[System.Serializable]
	public class Acquaintance
	{
		public Person person;
		public Feeling feeling;
	}

	public enum Feeling { Indifference, Love, Hate, ItsComplicated }
}
