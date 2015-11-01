using NUnit.Framework;
using RelationsInspector;
using System.Collections.Generic;
using RelationsInspector.Extensions;
using System.Linq;

[TestFixture]
public class GraphTests
{
    [Test]
    public void GetVertexChildrenTest()
    {
        var graph = new Graph<string, string>();
        graph.AddVertex("a");
        graph.AddVertex("b");
        graph.AddVertex("c");
        graph.AddEdge(new Relation<string, string>("a", "a", string.Empty));
        graph.AddEdge(new Relation<string, string>("a", "b", string.Empty));
        graph.AddEdge(new Relation<string, string>("a", "c", string.Empty));
        var children = graph.GetChildren("a").ToList();
        Assert.That(children.Contains("a"));
        Assert.That(children.Contains("b"));
        Assert.That(children.Contains("c"));
    }

    [Test]
    public void DisjuctGraphIsNoTree()
    {
        // IsTree should fail: A is root, C not connected to it
        var graph = new GraphWithRoots<string, string>();
        graph.AddVertex( "a" );
        graph.AddVertex( "b" );
        graph.AddVertex( "c" );
        graph.AddEdge( new Relation<string, string>( "a", "b", string.Empty ) );
        Assert.That( !graph.IsTree() );
    }

    [Test]
    public void GraphWithSelfEdgeIsNoTree()
    {
        // IsTree should fail: A is it's own child
        var graph = new GraphWithRoots<string, string>();
        graph.AddVertex( "a" );
        graph.AddEdge( new Relation<string, string>( "a", "a", string.Empty ) );
        Assert.That( !graph.IsTree() );
    }

    [Test]
    public void SimpleTree()
    {
        // IsTree should succeed: A has children B and C. B has child D
        var graph = new GraphWithRoots<string, string>();
        graph.AddVertex( "a" );
        graph.AddVertex( "b" );
        graph.AddVertex( "c" );
        graph.AddVertex( "d" );
        graph.AddEdge( new Relation<string, string>( "a", "b", string.Empty ) );
        graph.AddEdge( new Relation<string, string>( "a", "c", string.Empty ) );
        graph.AddEdge( new Relation<string, string>( "b", "d", string.Empty ) );
        UnityEngine.Debug.Log( graph.GetNeighbors( "a" ).ToDelimitedString() );
        Assert.That( graph.IsTree() );
    }
}
