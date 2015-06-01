using NUnit.Framework;
using RelationsInspector;
using System.Collections.Generic;
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
        graph.AddEdge(new Edge<string, string>("a", "a", string.Empty));
        graph.AddEdge(new Edge<string, string>("a", "b", string.Empty));
        graph.AddEdge(new Edge<string, string>("a", "c", string.Empty));
        var children = graph.GetChildren("a").ToList();
        Assert.That(children.Contains("a"));
        Assert.That(children.Contains("b"));
        Assert.That(children.Contains("c"));
    }
    
    [Test]
    public void TestTreeDetector()
    {
        var graph = new Graph<string, string>();
        graph.AddVertex("a");
        graph.AddVertex("b");
        graph.AddVertex("c");
        graph.AddVertex("d");
        graph.AddEdge(new Edge<string, string>("a", "a", string.Empty));
        graph.AddEdge(new Edge<string, string>("a", "b", string.Empty));
        graph.AddEdge(new Edge<string, string>("b", "a", string.Empty));
        graph.AddEdge(new Edge<string, string>("b", "d", string.Empty));
        graph.AddEdge(new Edge<string, string>("c", "b", string.Empty));
        graph.AddEdge(new Edge<string, string>("c", "d", string.Empty));
        graph.AddEdge(new Edge<string, string>("d", "a", string.Empty));
        Assert.That(!graph.IsTree(true));
    }
}
