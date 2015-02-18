namespace Modules.Acl.Internal.Collections.Graphs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    ///   A non-cyclic non-weighted directed graph.
    /// </summary>
    /// 
    /// <remarks>
    ///   <para>
    ///   If you are new to the concept of graphs, the following may be of 
    ///   help to you:
    ///   http://en.wikipedia.org/wiki/Directed_acyclic_graph
    ///   
    ///   </para>
    /// 
    ///   <para>
    ///   A directed graph can be described as having a set of vertices 
    ///   and edges.  
    ///   </para>
    ///   
    ///   <para>
    ///   A vertice represents a data instance, any object or primitive.
    ///   </para>
    ///   
    ///   <para>
    ///   An edge is is used to represent a connection between two vertices.
    ///   As this is a directed graph it's important to note the order of
    ///   vertices when adding an edge.
    ///   </para>
    /// 
    ///   <para>
    ///   A sample directed graph is shown below: 
    ///   </para>
    ///   <code>
    ///        1
    ///      / | \
    ///     /  |  \
    ///    |   |   |
    ///    V   |   V
    ///    2   |   3
    ///   / \  |   |
    ///   |  \ |   |
    ///   V    V   V
    ///   4    5   6
    ///         \ /
    ///          V
    ///          7
    ///   </code>
    /// 
    ///   <para>
    ///   Using this structure you can map complex data relationship structures
    ///   such as organisational charts, acl groups, family trees etc where
    ///   data have multiple parents.
    ///   </para>   
    /// 
    ///   <para>
    ///   This implementation is written with concurrency in mind and trys to 
    ///   ensure that exceptions related to multi-thread modification/access
    ///   will not occur.  However, it is possible that the state of the graph 
    ///   may become invalid (where the vertexes and edges do not match) in 
    ///   high load multi-threaded environments where specific thread race 
    ///   conditions could occur.
    ///   </para>
    /// </remarks>
    [DataContract,
    KnownType(typeof(DirectedGraph<Group>))]
    internal sealed class DirectedGraph<T>
    {
        #region Fields

        /// <summary>
        ///   Dummy data required so that we can use the ConcurrentDictionary
        ///   type.
        /// </summary>
        private const byte EmptyByte = new byte();

        /// <summary>
        ///   The adjacency list. i.e. directed edges.
        /// </summary>
        /// 
        /// <remarks>
        ///   The key is the source vertice, and the value is a set of the 
        ///   target vertices.  We use a ConcurrentDictionary for the value
        ///   as a HashSet doesn't provide us with thread safety.  The byte
        ///   value is not used. 
        /// </remarks>
        private readonly ConcurrentDictionary<T, ConcurrentDictionary<T, byte>> _adjacencyList;

        /// <summary>
        ///   The _adjacency list datamember
        /// </summary>
        [DataMember]
        private readonly Dictionary<int, List<int>> _adjacencyListDm;

        /// <summary>
        ///   The reverse adjacency list. i.e. reverse format of directed edges.
        /// </summary>
        /// 
        /// <remarks>
        ///   The key is the source vertice, and the value is a set of the 
        ///   target vertices.  We use a ConcurrentDictionary for the value
        ///   as a HashSet doesn't provide us with thread safety.  The byte
        ///   value is not used. 
        /// </remarks>
        private readonly ConcurrentDictionary<T, ConcurrentDictionary<T, byte>> _inverseAdjacencyList;

        /// <summary>
        /// The _inverse adjacency list datamember
        /// </summary>
        [DataMember]
        private readonly Dictionary<int, List<int>> _inverseAdjacencyListDm;

        /// <summary>
        ///   Lock used when creating snapshots of the graph
        /// </summary>
        private readonly object _snapshotLock = new object();

        /// <summary>
        ///   The vertices.
        /// </summary>
        /// 
        /// <remarks>
        ///   We use a ConcurrentDictionary for this as a HashSet doesn't 
        ///   provide us with thread safety.  The byte value is not used. 
        /// </remarks>
        private readonly ConcurrentDictionary<T, T> _vertices;

        /// <summary>
        /// The _vertices datamember
        /// </summary>
        [DataMember]
        private readonly Dictionary<int, T> _verticesDm;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="DirectedGraph&lt;T&gt;"/> class.
        /// </summary>
        /// 
        /// <remarks>
        ///   If <c>T</c> is a reference type then it is highly 
        ///   recommended that you use the constructor that allows you
        ///   to provide a formatter for <c>T</c> for the string 
        ///   formatting methods.
        /// </remarks>
        public DirectedGraph()
        {
            _vertices = new ConcurrentDictionary<T, T>();

            _adjacencyList =
                new ConcurrentDictionary<T, ConcurrentDictionary<T, byte>>();

            _inverseAdjacencyList =
                new ConcurrentDictionary<T, ConcurrentDictionary<T, byte>>();

            _verticesDm = new Dictionary<int, T>();

            _adjacencyListDm = new Dictionary<int, List<int>>();

            _inverseAdjacencyListDm = new Dictionary<int, List<int>>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///   Adds a directed edge to the graph.
        /// </summary>
        /// 
        /// <param name="sourceVertex">The source vertex.</param>
        /// <param name="targetVertex">The target vertex.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'sourceVertex' must not be null.
        ///   </para>
        ///   or
        ///   <para>
        ///   Argument 'targetVertex' must not be null.
        ///   </para>
        ///   or
        ///   <para>
        ///   Adding the given edge would cause a cyclical reference within
        ///   the graph.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   The direction runs from the <paramref name="sourceVertex"/>
        ///   to the <paramref name="targetVertex"/>.
        ///   </para>
        /// 
        ///   <para>
        ///   If the associated vertices have not already been added via the
        ///   <see cref="AddEdge"/> method, then they will be automatically
        ///   added by this method.
        ///   </para>
        /// </remarks>
        public void AddEdge(T sourceVertex, T targetVertex)
        {
            if (sourceVertex == null) // if struct, then always false.
            {
                throw new ArgumentException(
                    "Argument 'sourceVertex' must not be null.");
            }
            if (targetVertex == null) // if struct, then always false.
            {
                throw new ArgumentException(
                    "Argument 'targetVertex' must not be null.");
            }

            T foo;
            if (false == _vertices.TryGetValue(sourceVertex, out foo))
            {
                AddVertex(sourceVertex);
            }
            if (false == _vertices.TryGetValue(targetVertex, out foo))
            {
                AddVertex(targetVertex);
            }

            ConcurrentDictionary<T, byte> edgeValues;
            if (_adjacencyList.TryGetValue(sourceVertex, out edgeValues)
                && edgeValues.ContainsKey(targetVertex))
            {
                // Edge already exists
                return;
            }

            // Ensure cycle will not be created, by first cloning this item,
            // adding the proposed edge, and then checking for a cycle.
            var snapshot = Snapshot();
            snapshot.RegisterEdge(sourceVertex, targetVertex);

            DirectedGraph<T> cycle;
            if (snapshot.HasCycle(out cycle))
            {
                throw new ArgumentException(
                    string.Format(
                        "Cannot create an edge with the given source and target as it would create a cyclic reference.  The vertices/edges comprising the cycle would be:\n{0}",
                        cycle));
            }

            // No cycle existed, so now we can go ahead and add the edge.
            RegisterEdge(sourceVertex, targetVertex);
        }

        /// <summary>
        ///   Adds the given data as a vertex within the graph.
        /// </summary>
        /// 
        /// <param name="vertex">The vertex.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'vertex' must not be null, if it is a reference type.
        ///   </para>
        /// </exception>
        public void AddVertex(T vertex)
        {
            if (vertex == null) // this is ok, if value type, then will be false
            {
                throw new ArgumentException("Argument 'vertex' must not be null.");
            }

            T foo;
            if (_vertices.TryGetValue(vertex, out foo))
            {
                // Already exists
                return;
            }

            _vertices.TryAdd(vertex, vertex);

            _adjacencyList.TryAdd(
                vertex, new ConcurrentDictionary<T, byte>());

            _inverseAdjacencyList.TryAdd(
                vertex, new ConcurrentDictionary<T, byte>());
        }

        /// <summary>
        ///   Performs a breadth first search on this instance starting from
        ///   the given <paramref name="startVertex"/>.
        /// </summary>
        /// 
        /// <param name="startVertex">
        ///   The start vertex (i.e. the chosen root node).
        /// </param>
        /// 
        /// <returns>
        ///   An array containing all the traversed vertices in the order that
        ///   they were traversed.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   Breadth-first search (BFS) is a strategy for searching in a graph 
        ///   when search is limited to essentially two operations: (a) visit 
        ///   and inspect a node of a graph; (b) gain access to visit the nodes 
        ///   that neighbor the currently visited node. The BFS begins at a root 
        ///   node and inspects all the neighboring nodes. Then for each of 
        ///   those neighbor nodes in turn, it inspects their neighbor nodes 
        ///   which were unvisited, and so on.
        ///   </para>
        /// 
        ///   <para>
        ///   Comparing Breadth-First-Search and Depth-First-Search, the big 
        ///   advantage of Depth-First-Search-S is that it has much lower memory 
        ///   requirements than BFS, because it’s not necessary to store all of 
        ///   the child pointers at each level. Depending on the data and what 
        ///   you are looking for, either DFS or BFS could be advantageous.
        ///   </para>
        /// 
        ///   <para>
        ///   A snapshot of the graph will be created and used for the duration
        ///   of this method in order to avoid possible issues with data 
        ///   being changed from underneath itself.
        ///   </para>
        /// </remarks>
        public T[] BreathFirstSearch(T startVertex)
        {
            return BreathFirstSearch(startVertex, null);
        }

        /// <summary>
        ///   Performs a breadth first search on this instance starting from
        ///   the given <paramref name="startVertex"/>.
        /// </summary>
        /// 
        /// <param name="startVertex">
        ///   The start vertex (i.e. the chosen root node).
        /// </param>
        /// <param name="action">
        ///   An action to perform on each vertex as they are traversed.
        /// </param>
        /// 
        /// <returns>
        ///   An array containing all the traversed vertices in the order that
        ///   they were traversed.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   Breadth-first search (BFS) is a strategy for searching in a graph 
        ///   when search is limited to essentially two operations: (a) visit 
        ///   and inspect a node of a graph; (b) gain access to visit the nodes 
        ///   that neighbor the currently visited node. The BFS begins at a root 
        ///   node and inspects all the neighboring nodes. Then for each of 
        ///   those neighbor nodes in turn, it inspects their neighbor nodes 
        ///   which were unvisited, and so on.
        ///   </para>
        /// 
        ///   <para>
        ///   Comparing Breadth-First-Search and Depth-First-Search, the big 
        ///   advantage of Depth-First-Search-S is that it has much lower memory 
        ///   requirements than BFS, because it’s not necessary to store all of 
        ///   the child pointers at each level. Depending on the data and what 
        ///   you are looking for, either DFS or BFS could be advantageous.
        ///   </para>
        /// 
        ///   <para>
        ///   A snapshot of the graph will be created and used for the duration
        ///   of this method in order to avoid possible issues with data 
        ///   being changed from underneath itself.
        ///   </para>
        /// </remarks>
        public T[] BreathFirstSearch(T startVertex, Action<T> action)
        {
            if (startVertex == null) // this is ok, if value type, then will be false.
            {
                throw new ArgumentException(
                    "Argument 'startVertex' must not be null.");
            }

            // This action may take a while depending on graph size, and is
            // subject to having the data change underneath it within a multi-
            // threaded environment.  Therefore we shall snapshot the graph in
            // order to avoid it changing from underneath us.
            DirectedGraph<T> snapshot = Snapshot();

            var result = new List<T>();

            var unvistedNodes = new HashSet<T>(snapshot._vertices.Keys);
            unvistedNodes.Remove(startVertex);

            var q = new Queue<T>();
            q.Enqueue(startVertex);

            while (q.Count > 0)
            {
                T head = q.Dequeue();

                if (action != null)
                {
                    action.Invoke(head);
                }
                result.Add(head);

                foreach (T neighbor in snapshot.GetChildrenFor(head))
                {
                    if (unvistedNodes.Remove(neighbor))
                    {
                        q.Enqueue(neighbor);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        ///   Performs a depth first search on this instance starting from
        ///   the given <paramref name="startVertex"/>.
        /// </summary>
        /// 
        /// <param name="startVertex">The start vertex.</param>
        /// 
        /// <returns>
        ///   An array containing all the traversed vertices in the order that
        ///   they were traversed.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   Depth-first search (DFS) is an algorithm for traversing or 
        ///   searching tree or graph data structures. One starts at the root 
        ///   (selecting some arbitrary node as the root in the case of a graph) 
        ///   and explores as far as possible along each branch before 
        ///   backtracking.
        ///   </para>
        /// 
        ///   <para>
        ///   Comparing Breadth-First-Search and Depth-First-Search, the big 
        ///   advantage of Depth-First-Search-S is that it has much lower memory 
        ///   requirements than BFS, because it’s not necessary to store all of 
        ///   the child pointers at each level. Depending on the data and what 
        ///   you are looking for, either DFS or BFS could be advantageous.
        ///   </para>
        /// 
        ///   <para>
        ///   A snapshot of the graph will be created and used for the duration
        ///   of this method in order to avoid possible issues with data 
        ///   being changed from underneath itself.
        ///   </para>
        /// </remarks>
        public T[] DepthFirstSearch(T startVertex)
        {
            return DepthFirstSearch(startVertex, null);
        }

        /// <summary>
        ///   Performs a depth first search on this instance starting from
        ///   the given <paramref name="startVertex"/>.
        /// </summary>
        /// 
        /// <param name="startVertex">
        ///   The start vertex. i.e. the chosen root node for the search.
        /// </param>
        /// 
        /// <param name="action">
        ///   An action to perform on each vertex as they are traversed.
        /// </param>
        ///
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'startVertex' must not be null.
        ///   </para>
        /// </exception>
        ///  
        /// <returns>
        ///   An array containing all the traversed vertices in the order that
        ///   they were traversed.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   Depth-first search (DFS) is an algorithm for traversing or 
        ///   searching tree or graph data structures. One starts at the root 
        ///   (selecting some arbitrary node as the root in the case of a graph) 
        ///   and explores as far as possible along each branch before 
        ///   backtracking.
        ///   </para>
        /// 
        ///   <para>
        ///   Comparing Breadth-First-Search and Depth-First-Search, the big 
        ///   advantage of Depth-First-Search-S is that it has much lower memory 
        ///   requirements than BFS, because it’s not necessary to store all of 
        ///   the child pointers at each level. Depending on the data and what 
        ///   you are looking for, either DFS or BFS could be advantageous.
        ///   </para>
        /// 
        ///   <para>
        ///   A snapshot of the graph will be created and used for the duration
        ///   of this method in order to avoid possible issues with data 
        ///   being changed from underneath itself.
        ///   </para>
        /// </remarks>
        public T[] DepthFirstSearch(T startVertex, Action<T> action)
        {
            if (startVertex == null) // this is ok, if value type, then will be false.
            {
                throw new ArgumentException(
                    "Argument 'startVertex' must not be null.");
            }

            // This action may take a while depending on graph size, and is
            // subject to having the data change underneath it within a multi-
            // threaded environment.  Therefore we shall snapshot the graph in
            // order to avoid it changing from underneath us.
            DirectedGraph<T> snapshot = Snapshot();

            var result = new List<T>();

            var unvistedNodes = new HashSet<T>(snapshot._vertices.Keys);
            unvistedNodes.Remove(startVertex);

            var s = new Stack<T>();
            s.Push(startVertex);

            while (s.Count > 0)
            {
                T top = s.Pop();

                if (action != null)
                {
                    action.Invoke(top);
                }
                result.Add(top);

                foreach (T neighbor in snapshot.GetChildrenFor(top))
                {
                    if (unvistedNodes.Remove(neighbor))
                    {
                        s.Push(neighbor);
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        ///   Gets all the parents for the given vertex.
        /// </summary>
        /// 
        /// <param name="vertex">The vertex.</param>
        /// 
        /// <returns>
        ///   All the parent vertices.
        /// </returns>
        /// 
        /// <remarks>
        ///   Use the <see cref="GetParentsFor"/> method if you only care
        ///   about the direct parents for a vertex and not all of it's 
        ///   ancestors as this method is far more performant.
        /// </remarks>
        public List<T> GetAllParentsFor(T vertex)
        {
            HashSet<T> results = new HashSet<T>();

            // Get the parents of the given vertex.  This is the start point
            // to retrieving all parents.
            List<T> process = GetParentsFor(vertex);

            while (process.Count > 0)
            {
                // This will contain
                List<T> nextToProcess = new List<T>();

                foreach (var parent in process)
                {
                    // As a graph allows for multiple parents it is possible
                    // that we have already processed a parent via an alternative
                    // path.  So let's save ourselves some recursion and skip
                    // processed nodes if need be.
                    if (false == results.Contains(parent))
                    {
                        results.Add(parent);
                        nextToProcess.AddRange(GetParentsFor(parent));
                    }
                }

                process = nextToProcess;
            }

            return results.ToList();
        }

        /// <summary>
        ///   Gets the direct child vertexes for the specified vertex.
        /// </summary>
        /// 
        /// <param name="vertex">The vertex.</param>
        /// 
        /// <exception cref="ArgumentException">
        ///   Argument 'vertex' does not exist within this graph instance.
        /// </exception>
        /// 
        /// <returns>
        ///   The child vertexes.
        /// </returns>
        public List<T> GetChildrenFor(T vertex)
        {
            ConcurrentDictionary<T, byte> edgeValues;

            if (false == _adjacencyList.TryGetValue(vertex, out edgeValues))
            {
                throw new ArgumentException(
                    string.Format(
                        "Argument 'vertex' does not exist within this graph instance: {0}",
                        vertex));
            }

            return edgeValues.Keys.ToList();
        }

        /// <summary>
        ///   Gets the edge count.
        /// </summary>
        /// 
        /// <returns>
        ///   Number of edges this graph contains.
        /// </returns>
        public int GetEdgeCount()
        {
            int result = 0;

            foreach (var pair in _adjacencyList)
            {
                result += pair.Value.Count;
            }

            return result;
        }

        /// <summary>
        ///   Gets the edges for the graph.
        /// </summary>
        /// 
        /// <returns>
        ///   Graph edges multidimensional array.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   Edges are returned in a multidimensional array format that 
        ///   represents all the edges in a target -> source format.
        ///   </para>
        /// 
        ///   <para>
        ///   For example, with the given integer graph:
        ///   </para>
        ///   <code>
        ///        1
        ///      / | \
        ///     /  |  \
        ///    |   |   |
        ///    V   |   V
        ///    2   |   3
        ///   / \  |   |
        ///   |  \ |   |
        ///   V    V   V
        ///   4    5   6
        ///         \ /
        ///          V
        ///          7
        ///   </code>
        ///   
        ///   <para>
        ///   You would get back the following result:
        ///   </para>
        ///   <code>
        ///   new [][] 
        ///   {
        ///     { 1, 2 },
        ///     { 1, 5 },
        ///     { 1, 3 },
        ///     { 2, 4 },
        ///     { 2, 5 },
        ///     { 2, 5 },
        ///     { 5, 7 },
        ///     { 3, 6 },
        ///     { 6, 7 }
        ///   };
        ///   </code>
        /// </remarks>
        public T[,] GetEdges()
        {
            var flattenedEdges = new List<T[]>();

            foreach (var pair1 in _adjacencyList)
            {
                foreach (var pair2 in pair1.Value)
                {
                    flattenedEdges.Add(new[] { pair1.Key, pair2.Key });
                }
            }

            // now convert to two dimensional array
            T[,] result = new T[flattenedEdges.Count, 2];
            for (var i = 0; i < flattenedEdges.Count; i++)
            {
                for (var j = 0; j < flattenedEdges[i].Length; j++)
                {
                    result[i, j] = flattenedEdges[i][j];
                }
            }

            return result;
        }

        /// <summary>
        ///   Gets the leaf vertices for this graph instance.
        /// </summary>
        /// 
        /// <returns>
        ///   The leaf vertices.
        /// </returns>
        /// 
        /// <remarks>
        ///   i.e. Vertices without any children.
        /// </remarks>
        public List<T> GetLeaves()
        {
            var result = new List<T>();

            ConcurrentDictionary<T, byte> edgeValues;

            // We can find the leaves by checking the adjacency list
            // in order to see if there are no targets from a vertex.
            var vertices = _vertices.Keys;

            foreach (var vertex in vertices)
            {
                if (_adjacencyList.TryGetValue(vertex, out edgeValues))
                {
                    if (edgeValues.Count == 0)
                    {
                        result.Add(vertex);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///   Gets the direct parent vertices for the specified vertex.
        /// </summary>
        /// 
        /// <param name="vertex">The vertex.</param>
        /// 
        /// <exception cref="ArgumentException">
        ///   Argument 'vertex' does not exist within this graph instance.
        /// </exception>
        /// 
        /// <returns>
        ///   The parent vertices.
        /// </returns>
        public List<T> GetParentsFor(T vertex)
        {
            ConcurrentDictionary<T, byte> result;

            if (_inverseAdjacencyList.TryGetValue(vertex, out result))
            {
                return result.Keys.ToList();
            }

            throw new ArgumentException(
                string.Format(
                    "Argument 'vertex' does not exist within this graph instance: {0}",
                    vertex));
        }

        /// <summary>
        ///   Gets the root vertices for this graph instance.
        /// </summary>
        /// 
        /// <returns>
        ///   The root vertices.
        /// </returns>
        /// 
        /// <remarks>
        ///   i.e. Vertices without any parents - awwwwww :'(
        /// </remarks>
        public List<T> GetRoots()
        {
            var result = new List<T>();

            // We can find the roots by checking the inverted adjacency list
            // in order to see if there are no targets to a vertex.
            var vertices = _vertices.Keys;

            ConcurrentDictionary<T, byte> edgeValues;

            foreach (var vertex in vertices)
            {
                if (_inverseAdjacencyList.TryGetValue(vertex, out edgeValues))
                {
                    if (edgeValues.Count == 0)
                    {
                        result.Add(vertex);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///   Returns back the vertices in topological order.
        /// </summary>
        /// 
        /// <returns>
        ///   The vertices in topological order.
        /// </returns>
        /// 
        /// <exception cref="System.InvalidOperationException">
        ///   If this graph instance contains a cyclical reference.
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   Topological ordering of a directed graph is a linear ordering of 
        ///   its vertices such that for every directed edge uv from vertex u to 
        ///   vertex v, u comes before v in the ordering. For instance, the 
        ///   vertices of the graph may represent tasks to be performed, and the 
        ///   edges may represent constraints that one task must be performed 
        ///   before another; in this application, a topological ordering is 
        ///   just a valid sequence for the tasks. A topological ordering is 
        ///   possible if and only if the graph has no directed cycles.
        ///   </para>
        /// 
        ///   <para>
        ///   A snapshot of the graph will be created and used for the duration
        ///   of this method in order to avoid possible issues with data 
        ///   being changed from underneath itself.
        ///   </para>
        /// </remarks>
        public T[] GetTopologicalOrder()
        {
            // By the nature of the algorithm used to determine the topological
            // order, we will need to make a clone of the the item to
            // be used, as we will be removing items from our lists as we
            // process them.
            // This also guards us from a case where the data may be changed
            // whilst running in a multi-threaded environment.
            DirectedGraph<T> snapshot = Snapshot();

            // First ensure that this graph is not cyclical
            DirectedGraph<T> cycle;

            if (snapshot.HasCycle(out cycle))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Cannot perform topological order, as the graph contains the following items creating a cyclical reference:\n{0}",
                        cycle));
            }

            var result = new List<T>();

            int vCount = snapshot._vertices.Count;
            for (int i = (vCount -1); i >= 0; i--)
            {
                foreach (var pair in snapshot._inverseAdjacencyList)
                {
                    if (pair.Value.Count == 0)
                    {
                        snapshot.RemoveVertex(pair.Key);
                        result.Add(pair.Key);
                        break;
                    }
                }

            }

            return result.ToArray();
        }

        /// <summary>
        ///   Gets the vertex count.
        /// </summary>
        /// 
        /// <returns>
        ///   Number of vertices this graph contains.
        /// </returns>
        public int GetVertexCount()
        {
            return _vertices.Count;
        }

        /// <summary>
        ///   Gets the vertices for this graph instance.
        /// </summary>
        /// 
        /// <returns>
        ///   The vertices.
        /// </returns>
        public T[] GetVertices()
        {
            return _vertices.Keys.ToArray();
        }

        /// <summary>
        ///   Determines whether this instance has cyclical reference.
        /// </summary>
        /// 
        /// <param name="cycle">
        ///   If a cycle was detected, then this will contain all the vertices
        ///   and edges that represent the cyclical references, otherwise it
        ///   will be null.
        /// </param>
        /// 
        /// <returns>
        ///   <c>true</c> if and only if a cycle was detected, else <c>false</c>.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   A snapshot of the graph will be created and used for the duration
        ///   of this method in order to avoid possible issues with data 
        ///   being changed from underneath itself.
        ///   </para>
        /// </remarks>
        public bool HasCycle(out DirectedGraph<T> cycle)
        {
            cycle = null;

            // By the nature of the algorithm used to detect a cyclical
            // reference, we will need to make a clone of the the item to
            // be used, as we will be removing items from our lists as we
            // process them.  After processing is complete, if there is a cycle
            // then this will only contain items that are involved in the cycle.
            // This also guards us from a case where the data may be changed
            // whilst running in a multi-threaded environment.
            DirectedGraph<T> snapshot = Snapshot();

            bool result = false;

            int vCount = snapshot._vertices.Count;

            for (int i = 0; i < vCount; i++)
            {
                bool cycleDetected = true;

                foreach (var pair in snapshot._inverseAdjacencyList)
                {
                    if (pair.Value.Count == 0)
                    {
                        var victim = pair.Key;

                        snapshot.RemoveVertex(victim);

                        cycleDetected = false;

                        break;
                    }
                }

                if (cycleDetected)
                {
                    result = true;
                    cycle = snapshot;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        ///   Determines whether this graph instance contains the specified 
        ///   edge definition.
        /// </summary>
        /// 
        /// <param name="sourceVertex">The source vertex.</param>
        /// <param name="targetVertex">The target vertex.</param>
        /// 
        /// <returns>
        ///   <c>true</c> if this instance contains the given edge, else
        ///   <c>false</c>.
        /// </returns>
        public bool HasEdge(T sourceVertex, T targetVertex)
        {
            byte foo;
            ConcurrentDictionary<T, byte> edgeValues;

            return _adjacencyList.TryGetValue(sourceVertex, out edgeValues)
                && edgeValues.TryGetValue(targetVertex, out foo);
        }

        /// <summary>
        ///   Determines whether this instance contains the specified vertex.
        /// </summary>
        /// 
        /// <param name="vertex">The vertex to check for.</param>
        /// <param name="instance">
        ///   If the vertex exists, then the actual instance being tracked
        ///   will be set into this out parameter.
        /// </param>
        /// 
        /// <returns>
        ///   <c>true</c> if this instance contains the given vertex, else
        ///   <c>false</c>.
        /// </returns>
        public bool HasVertex(T vertex, out T instance)
        {
            T foo;
            if (_vertices.TryGetValue(vertex, out foo))
            {
                instance = foo;
                return true;
            }

            instance = default(T);
            return false;
        }

        /// <summary>
        ///   Removes the given edge definition from the group.
        /// </summary>
        /// 
        /// <param name="sourceVertex">The source vertex.</param>
        /// <param name="targetVertex">The target vertex.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'sourceVertex' must not be null.
        ///   </para>
        ///   or
        ///   <para>
        ///   Argument 'targetVertex' must not be null.
        ///   </para>
        /// </exception>
        public void RemoveEdge(T sourceVertex, T targetVertex)
        {
            if (sourceVertex == null) // if struct, then always false.
            {
                throw new ArgumentException(
                    "Argument 'sourceVertex' must not be null.");
            }
            if (targetVertex == null) // if struct, then always false.
            {
                throw new ArgumentException(
                    "Argument 'targetVertex' must not be null.");
            }

            byte foo;
            ConcurrentDictionary<T, byte> edgeValues;

            if (_adjacencyList.TryGetValue(sourceVertex, out edgeValues))
            {
                edgeValues.TryRemove(targetVertex, out foo);
            }
            if (_inverseAdjacencyList.TryGetValue(targetVertex, out edgeValues))
            {
                edgeValues.TryRemove(sourceVertex, out foo);
            }
        }

        /// <summary>
        ///   Removes the given vertex, and any edges it is the source/target 
        ///   for, from this graph instance.
        /// </summary>
        /// 
        /// <param name="vertex">The vertex.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'vertex' must not be null.
        ///   </para>
        /// </exception>
        public void RemoveVertex(T vertex)
        {
            if (vertex == null) // if struct, then always false.
            {
                throw new ArgumentException(
                    "Argument 'vertex' must not be null.");
            }

            T foo;

            if (_vertices.TryRemove(vertex, out foo) == false)
            {
                // Vertex doesn't exist
                return;
            }

            ConcurrentDictionary<T, byte> edgeValues;
            byte foo2;

            foreach (var pair in _vertices)
            {
                if (_adjacencyList.TryGetValue(pair.Key, out edgeValues))
                {
                    edgeValues.TryRemove(vertex, out foo2);
                }
            }

            foreach (var pair in _vertices)
            {
                if (_inverseAdjacencyList.TryGetValue(pair.Key, out edgeValues))
                {
                    edgeValues.TryRemove(vertex, out foo2);
                }
            }

            _adjacencyList.TryRemove(vertex, out edgeValues);
            _inverseAdjacencyList.TryRemove(vertex, out edgeValues);
            _vertices.TryRemove(vertex, out foo);
        }

        /// <summary>
        ///   Converts the graph into a string representation in the DOT 
        ///   (graph description language) format.
        /// </summary>
        /// 
        /// <param name="vertexFormatter">
        ///   The vertex formatter is used in order to provide a user with
        ///   a mechanism to provide a string representation of a vertex
        ///   which can then be used within the graph string serialization 
        ///   methods.
        /// </param>
        /// 
        /// <returns>
        ///   DOT formatted string representation of the graph.
        /// </returns>
        /// 
        /// <remarks>
        ///   DOT is a plain text graph description language. It is a simple way 
        ///   of describing graphs that both humans and computer programs can use.
        /// 
        ///   If you provide a custom vertex formatter you must make sure that
        ///   it follows the guidelines of DOT Formatting.
        /// </remarks>
        /// 
        /// <example>
        ///   // TODO: Example, with vertex formatter.
        /// </example>
        public string ToDotFormat(Func<T, string> vertexFormatter = null)
        {
            var builder = new StringBuilder();

            builder.AppendLine("digraph graphname {");

            var sortedVertices = _vertices.Keys.ToList();
            sortedVertices.Sort(); // annoying this isn't fluent.
            foreach (var vertex in sortedVertices)
            {
                builder.AppendFormat("{0};", vertex);
                builder.AppendLine();
            }

            foreach (var pair in _adjacencyList)
            {
                foreach (var pair2 in pair.Value)
                {
                    builder.AppendFormat(
                        "{0} -> {1};",
                        StringSerializeVertex(pair.Key, vertexFormatter),
                        StringSerializeVertex(pair2.Key, vertexFormatter));
                    builder.AppendLine();
                }
            }
            builder.Append("}");

            return builder.ToString();
        }

        /// <summary>
        ///   Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// 
        /// <returns>
        ///   A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("{");

            // First append 'vertices'
            builder.Append("\tVertices : [ ");

            foreach (var pair in _vertices)
            {
                builder.AppendFormat("\"{0}\", ",
                    StringSerializeVertex(pair.Key));
            }

            if (_vertices.Count > 0)
            {
                builder.Remove(builder.Length - 2, 2);
            }

            builder.Append(" ]");
            builder.AppendLine();

            // Now append 'edges'
            builder.Append("\tEdges : [ ");

            bool count = false;
            foreach (var pair in _adjacencyList)
            {
                foreach (T x in pair.Value.Keys)
                {
                    builder.AppendFormat("[\"{0}\",\"{1}\"], ",
                        StringSerializeVertex(pair.Key),
                        StringSerializeVertex(x));
                    count = true;
                }
            }
            if (count)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            builder.Append(" ]");

            return builder.ToString();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            //
            // Repopulate vertices
            _vertices.Clear();

            foreach (var vertex in _verticesDm.Values)
            {
                _vertices.TryAdd(vertex, vertex);
            }

            //
            // Repopulate adjacency list
            _adjacencyList.Clear();
            foreach (var vertexPair in _adjacencyListDm)
            {
                T vertex = _verticesDm[vertexPair.Key];

                var children = new ConcurrentDictionary<T, byte>();

                _adjacencyList.TryAdd(vertex, children);

                foreach (var child in vertexPair.Value)
                {
                    vertex = _verticesDm[child];
                    children.TryAdd(vertex, EmptyByte);
                }
            }

            //
            // Repopulate inverted adjacency list
            _inverseAdjacencyList.Clear();
            foreach (var vertexPair in _inverseAdjacencyListDm)
            {
                T vertex = _verticesDm[vertexPair.Key];

                var parents = new ConcurrentDictionary<T, byte>();

                _adjacencyList.TryAdd(vertex, parents);

                foreach (var parent in vertexPair.Value)
                {
                    vertex = _verticesDm[parent];
                    parents.TryAdd(vertex, EmptyByte);
                }
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            //
            // Populate the vertices datamember
            _verticesDm.Clear();
            foreach (var vertex in _vertices.Values)
            {
                _verticesDm.Add(vertex.GetHashCode(), vertex);
            }

            //
            // Populate adjacency list datamember
            _adjacencyListDm.Clear();
            foreach (var vertexPair in _adjacencyList)
            {
                T vertex = vertexPair.Key;

                var children = new List<int>();

                _adjacencyListDm.Add(vertex.GetHashCode(), children);

                foreach (var childPair in vertexPair.Value)
                {
                    vertex = childPair.Key;
                    children.Add(vertex.GetHashCode());
                }
            }

            //
            // Populate inverted adjacency list datamember
            _inverseAdjacencyListDm.Clear();
            foreach (var vertexPair in _inverseAdjacencyList)
            {
                T vertex = vertexPair.Key;

                var children = new List<int>();

                _inverseAdjacencyListDm.Add(vertex.GetHashCode(), children);

                foreach (var childPair in vertexPair.Value)
                {
                    vertex = childPair.Key;
                    children.Add(vertex.GetHashCode());
                }
            }
        }

        /// <summary>
        ///   Registers the edge within the adjacency lists.
        /// </summary>
        /// 
        /// <param name="sourceVertex">The source vertex.</param>
        /// <param name="targetVertex">The target vertex.</param>
        private void RegisterEdge(T sourceVertex, T targetVertex)
        {
            ConcurrentDictionary<T, byte> edgeValues;

            if (_adjacencyList.TryGetValue(sourceVertex, out edgeValues))
            {
                edgeValues.TryAdd(targetVertex, EmptyByte);
            }
            if (_inverseAdjacencyList.TryGetValue(targetVertex, out edgeValues))
            {
                edgeValues.TryAdd(sourceVertex, EmptyByte);
            }
        }

        /*
        public List<string> GetCriticalPath()
        {
            var result = new List<string>();
            var maxdistance = new Dictionary<string, double>();

            List<string> topological_order = GetTopologicalOrder();
            foreach (string task in topological_order)
            {
                maxdistance[task] = GetVertexWeight(task);
                foreach (string parent in Parents(task))
                    maxdistance[task] = Math.Max(maxdistance[task],
                        GetVertexWeight(task) + maxdistance[parent] +
                        GetEdgeWeight(parent, task));
            }

            List<string> leaf_nodes = GetLeaves();
            string max_node = null;
            foreach (string leaf in leaf_nodes)
                if (max_node == null)
                    max_node = leaf;
                else if (maxdistance[leaf] > maxdistance[max_node])
                    max_node = leaf;

            result.Add(max_node);

            while (Parents(max_node).Count > 0)
            {
                List<string> parents = Parents(max_node);
                string current_node = max_node;
                max_node = null;
                foreach (string parent in parents)
                    if (max_node == null)
                        max_node = parent;
                    else if (maxdistance[parent] +
                             GetEdgeWeight(parent, current_node) >
                             maxdistance[max_node] +
                             GetEdgeWeight(max_node, current_node))
                        max_node = parent;
                result.Add(max_node);
            }

            result.Reverse();
            return result;
        }
        */
        /*
        public double CriticalPathLength()
        {
            var result = new List<string>();
            var maxdistance = new Dictionary<string, double>();

            List<string> topologicalOrder = GetTopologicalOrder();

            foreach (string task in topologicalOrder)
            {
                maxdistance[task] = GetVertexWeight(task);
                foreach (string parent in Parents(task))
                    maxdistance[task] = Math.Max(maxdistance[task],
                        GetVertexWeight(task) + maxdistance[parent] +
                        GetEdgeWeight(parent, task));
            }

            List<string> leafNodes = GetLeaves();
            string maxNode = null;
            foreach (string leaf in leafNodes)
                if (maxNode == null)
                    maxNode = leaf;
                else if (maxdistance[leaf] > maxdistance[maxNode])
                    maxNode = leaf;

            return maxdistance[maxNode];
        }
        */
        /// <summary>
        ///   Takes a snapshot of this instance, creating a new instance.
        /// </summary>
        /// 
        /// <returns>
        ///   The new instance.
        /// </returns>
        private DirectedGraph<T> Snapshot()
        {
            lock (_snapshotLock)
            {
                var result = new DirectedGraph<T>();

                var vertices = GetVertices();
                foreach (T vertex in vertices)
                {
                    result.AddVertex(vertex);
                }

                var edges = GetEdges();
                int i, length = edges.GetLength(0);
                for (i = 0; i < length; i++)
                {
                    // BE CAREFUL NOT TO USE THE AddEdge METHOD AS THIS WILL
                    // CAUSE A STACKOVERFLOW EXCEPTION!
                    result.RegisterEdge(edges[i, 0], edges[i, 1]);
                }

                return result;
            }
        }

        private string StringSerializeVertex(
            T vertex,
            Func<T, string> vertexFormatter = null)
        {
            return vertexFormatter != null
                ? vertexFormatter.Invoke(vertex)
                : vertex.ToString();
        }

        #endregion Methods
    }
}