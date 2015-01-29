using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RelationsInspector.Backend.DialogGraph
{
	public class DialogItem : ScriptableObject
	{
		public string text;
		public List<DialogItemOption> options;
	}

	[System.Serializable]
	public class DialogItemOption
	{
		public string text;
		public DialogItem target;
	}
}
