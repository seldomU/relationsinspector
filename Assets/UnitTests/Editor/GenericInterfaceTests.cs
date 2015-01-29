using UnityEngine;
using System.Collections;
using NUnit.Framework;

[TestFixture]
public class GenericInterfaceTests
{
	public interface Foo<A, B, C> { }
	public class FullyGeneric<X, Y, Z> : Foo<Z, Y, X> { }
	public class FullyTyped : Foo<string, object, int> { }
	
	[Test]
	public void Test1()
	{
	}
}
