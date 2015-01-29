using System.Collections;
using UnityEditor;

// condition interface
public interface IContinueCondition
{
	bool Satisfied();
}

// equivalent to WaitForEndOfFrame
public class ContinueASAP : IContinueCondition
{
	public bool Satisfied() { return true; }
}

// equivalent to StartCoroutine
public class ContinueAfterCoroutine : IContinueCondition
{
	EditorCoroutine coroutine;

	public ContinueAfterCoroutine(IEnumerator enumerator)
	{
		this.coroutine = EditorCoroutineManager.Singleton.StartCoroutine(enumerator);
	}

	public bool Satisfied()
	{
		return coroutine == null || coroutine.Finished;
	}
}

// equivalent to WaitForSeconds
public class ContinueAfterSeconds : IContinueCondition
{
	double time;

	public ContinueAfterSeconds(float delay)
	{
		time = delay + EditorApplication.timeSinceStartup;
	}

	public bool Satisfied()
	{
		return time < EditorApplication.timeSinceStartup;
	}
}

