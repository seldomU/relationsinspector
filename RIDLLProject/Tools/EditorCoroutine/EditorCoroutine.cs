using System.Collections;
using UnityEditor;

public class EditorCoroutine
{
	public IEnumerator enumerator;
	IContinueCondition continueCondition;
	public bool Finished { get; private set; }	// true if the enumerator is through
	public bool Paused { get; private set; }	// if true, don't execute the enumerator

	public EditorCoroutine(IEnumerator enumerator)
	{
		this.enumerator = enumerator;
		continueCondition = new ContinueASAP();
	}

	public void Update()
	{
		if (!continueCondition.Satisfied())
			return;

		if (Paused)
			return;

		// execute one step
		Finished = !enumerator.MoveNext();
		continueCondition = (enumerator.Current as IContinueCondition) ?? new ContinueASAP();
	}

	public void Stop()
	{
		Finished = true;
		Paused = true;
	}

	public void Pause()
	{
		Paused = true;
	}

	public void UnPause()
	{
		Paused = false;
	}
}
