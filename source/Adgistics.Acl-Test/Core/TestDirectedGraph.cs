using System;
using System.Collections.Generic;
using System.Text;
using Modules.Acl.Internal.Collections.Graphs;
using NUnit.Framework;

namespace Modules.Acl.Core
{
    [TestFixture]
    public class TestDirectedGraph
    {
        [Test]
        public void AddVertex()
        {
            var graph = new DirectedGraph<string>();
            graph.AddVertex("A");

            CollectionAssert.AreEqual(new[] {"A"}, graph.GetVertices(), "1.1");

            graph.AddVertex("B");
            graph.AddVertex("C");

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, graph.GetVertices(), "1.1");
        }

        [Test]
        public void AddEdge()
        {
            var graph = new DirectedGraph<string>();

            //        A
            //      /   \
            //     /     \
            //    |       |
            //    V       V
            //    B       E
            //   / \   
            //   |  \  
            //   V    V
            //   C    D
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("A", "E");

            var actual = graph.GetEdges();

            var expected = new[,]
            {
                {"A", "B"},
                {"B", "C"},
                {"B", "D"},
                {"A", "E"}
            };

            CollectionAssert.AreEquivalent(expected, actual, "1.1");
        }

        [Test]
        public void TopologicalOrder()
        {
            var graph = new DirectedGraph<string>();

            //    A    
            //    |     
            //    V     
            //    B     
            //   / \   
            //   |  |  
            //   V  V
            //   C  D
            //   |/ |
            //   V  V
            //   F  E
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("D", "E");
            graph.AddEdge("D", "F");
            graph.AddEdge("C", "F");
            

            var actual = graph.GetTopologicalOrder();

            var expected = new[] { "A", "B", "D", "E", "C", "F" };

            CollectionAssert.AreEqual(expected, actual, "1.1");
        }

        [Test]
        public void BreadthFirstSearch()
        {
            var graph = new DirectedGraph<string>();

            //        A
            //      /   \
            //     /     \
            //    |       |
            //    V       V
            //    B       E
            //   / \   
            //   |  \  
            //   V    V
            //   C    D
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("A", "E");

            List<string> processedList = new List<string>();
            var actual = graph.BreathFirstSearch("B", (v) =>
            {
                processedList.Add(v);
            });

            var expected = new[] { "B", "D", "C" };

            CollectionAssert.AreEqual(expected, actual, "1.1");
            CollectionAssert.AreEqual(expected, processedList, "1.2");
        }

        [Test]
        public void DepthFirstSearch()
        {
            var graph = new DirectedGraph<string>();

            //        A
            //      /   \
            //     /     \
            //    |       |
            //    V       V
            //    B       E
            //   / \   
            //   |  \  
            //   V    V
            //   C    D
            //   |    |  
            //   |    |  
            //   V    V
            //   F    G
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("A", "E");
            graph.AddEdge("C", "F");
            graph.AddEdge("D", "G");

            List<string> processedList = new List<string>();
            var actual = graph.DepthFirstSearch("B", (v) =>
            {
                processedList.Add(v);
            });

            var expected = new[] { "B", "C", "F", "D", "G" };

            CollectionAssert.AreEqual(expected, actual, "1.1");
            CollectionAssert.AreEqual(expected, processedList, "1.2");
        }

        [Test]
        public void ToDotFormat()
        {
            var graph = new DirectedGraph<string>();

            //        A
            //      /   \
            //     /     \
            //    |       |
            //    V       V
            //    B       E
            //   / \   
            //   |  \  
            //   V    V
            //   C    D
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("A", "E");

            var actual = graph.ToDotFormat();

            var expected = new StringBuilder();

            expected.AppendLine(@"digraph graphname {");
            expected.AppendLine("A;");
            expected.AppendLine("B;");
            expected.AppendLine("C;");
            expected.AppendLine("D;");
            expected.AppendLine("E;");
            expected.AppendLine("A -> E;");
            expected.AppendLine("A -> B;");
            expected.AppendLine("B -> D;");
            expected.AppendLine("B -> C;");
            expected.Append("}");

            Assert.AreEqual(expected.ToString(), actual, "1.1");
        }

        [Test]
        public void Test()
        {
            var di_graph = new DirectedGraph<string>();
            di_graph.AddVertex("v1");
            di_graph.AddVertex("v2");
            di_graph.AddVertex("v3");
            di_graph.AddVertex("v4");
            di_graph.AddVertex("v5");
            di_graph.AddVertex("v6");
            di_graph.AddVertex("v7");
            di_graph.AddEdge("v1", "v2");
            di_graph.AddEdge("v1", "v3");
            di_graph.AddEdge("v1", "v5");
            di_graph.AddEdge("v2", "v4");
            di_graph.AddEdge("v2", "v5");
            di_graph.AddEdge("v3", "v5");
            di_graph.AddEdge("v3", "v6");
            di_graph.AddEdge("v4", "v7");
            di_graph.AddEdge("v5", "v7");
            di_graph.AddEdge("v6", "v7");

            Console.WriteLine("The Graph:");
            Console.WriteLine(di_graph);
            Console.WriteLine("========================================");

            Console.WriteLine("The Graph in DOT lang:");
            Console.WriteLine(di_graph.ToDotFormat());
            Console.WriteLine("========================================");

            //DFS
            var dfs = di_graph.DepthFirstSearch("v1");
            StringBuilder str_builder = new StringBuilder();
            foreach (var node in dfs)
                str_builder.AppendFormat("{0},", node);
            str_builder.Remove(str_builder.Length - 1, 1);
            Console.WriteLine("DFS: {0}", str_builder);
            Console.WriteLine("========================================");

            //BFS
            var bfs = di_graph.BreathFirstSearch("v1");
            str_builder = new StringBuilder();
            foreach (var node in bfs)
                str_builder.AppendFormat("{0},", node);
            str_builder.Remove(str_builder.Length - 1, 1);
            Console.WriteLine("BFS: {0}", str_builder);
            Console.WriteLine("========================================");

            //Topoligical Order
            var topological = di_graph.GetTopologicalOrder();
            str_builder = new StringBuilder();
            foreach (var node in topological)
                str_builder.AppendFormat("{0},", node);
            str_builder.Remove(str_builder.Length - 1, 1);
            Console.WriteLine("Topological Order: {0}", str_builder);
            Console.WriteLine("========================================");

            //Critical path
            //var critical_path = di_graph.GetCriticalPath();
            //str_builder = new StringBuilder();
            //foreach (var node in critical_path)
            //    str_builder.AppendFormat("{0},", node);
            //str_builder.Delete(str_builder.Length - 1, 1);
            //Console.WriteLine("Critical path: {0}", str_builder.ToString());
            //Console.WriteLine("Critical path length: {0}", di_graph.CriticalPathLength());
            //Console.WriteLine("========================================");

            // Manuplating Vertices and Edges
            //di_graph.UpdateVertex("v1", 32);
            //di_graph.UpdateEdge("v1", "v2", 12);
            //di_graph.ZeroEdge("v1", "v2");
            di_graph.RemoveEdge("v1", "v2");
            di_graph.RemoveVertex("v7");

            Console.WriteLine("Press Enter to Continue...");
            Console.ReadLine();
        }
    }
}