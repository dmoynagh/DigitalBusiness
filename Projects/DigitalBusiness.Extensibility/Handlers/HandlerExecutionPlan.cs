using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using DigitalBusiness.Collections;

namespace DigitalBusiness.Extensibility.Handlers
{
    internal sealed class HandlerExecutionPlan<TContext> : IEnumerable<HandlerActionDescriptor<TContext>>
    {
        public HandlerExecutionPlan(IEnumerable<HandlerActionDescriptor<TContext>> handlerDescriptors)
        {
            HandlerDescriptors = OrderHandlers(handlerDescriptors);
            HasActionFactories = HandlerDescriptors.Any(d => d.ActionFactory != null);
        }

        public ImmutableArray<HandlerActionDescriptor<TContext>> HandlerDescriptors { get; }

        public bool HasActionFactories { get; }

        private static ImmutableArray<HandlerActionDescriptor<TContext>> OrderHandlers(
            IEnumerable<HandlerActionDescriptor<TContext>> handlerDescriptors)
        {
            // ── 1. Assign stable indices using the baseline priority sort ─────────
            // This sort defines the natural priority of each handler when no
            // dependency constraints are involved. The topological sort below
            // respects this ordering as a tie-breaker via the priority queue.
            var nodes = handlerDescriptors
                .OrderBy(d => d.ExecutionStage)
                .ThenBy(d => d.ExecutionOrder ?? HandlerDefaults.DefaultExecutionOrder)
                .ThenBy(d => d.RegistrationOrder)
                .ToArray();

            int count = nodes.Length;

            if (count == 0)
                return ImmutableArray<HandlerActionDescriptor<TContext>>.Empty;

            // ── 2. Build key → handler index map ──────────────────────────────────
            // A single key can be held by multiple handlers (key groups), so we map
            // each key to every handler index that carries it.
            var keyToIndices = new Dictionary<object, List<int>>();

            for (int i = 0; i < count; i++)
            {
                var keys = nodes[i].Keys;
                if (!keys.HasValue) continue;

                foreach (var key in keys.Value)
                {
                    if (!keyToIndices.TryGetValue(key, out var list))
                        keyToIndices[key] = list = [];

                    list.Add(i);
                }
            }

            // ── 3. Validate self-contradicting constraints ─────────────────────────
            // A handler cannot declare the same key in both ExecuteBeforeKeys and
            // ExecuteAfterKeys — that would require it to be both before and after
            // the same handler simultaneously.
            for (int i = 0; i < count; i++)
            {
                var handler = nodes[i];

                if (!handler.ExecuteBeforeKeys.HasValue || !handler.ExecuteAfterKeys.HasValue)
                    continue;

                foreach (var key in handler.ExecuteBeforeKeys.Value)
                {
                    if (handler.ExecuteAfterKeys.Value.Contains(key))
                    {
                        throw new InvalidOperationException(
                            $"Cannot resolve execution order for handler '{handler.FullName}': " +
                            $"key '{key}' appears in both ExecuteBeforeKeys and ExecuteAfterKeys.");
                    }
                }
            }

            // ── 4. Build directed edge graph + compute in-degrees ─────────────────
            // An edge  A → B  means "A must execute before B".
            //
            // ExecuteBeforeKeys on H: for every key K in that set, add edge H → each
            //   handler that carries K. H must run before ALL of them.
            //
            // ExecuteAfterKeys on H: for every key K in that set, add edge each
            //   handler that carries K → H. H must run after ALL of them.
            var adjacency = new List<int>[count];   // adjacency[i] = list of nodes that must come AFTER i
            var inDegree = new int[count];

            for (int i = 0; i < count; i++)
                adjacency[i] = [];

            // Adds a directed edge from → to, guarding against self-loops and
            // duplicates (duplicate edges would inflate in-degree and deadlock Kahn's).
            var addedEdges = new HashSet<(int, int)>();

            void AddEdge(int from, int to)
            {
                if (from == to) return;
                if (!addedEdges.Add((from, to))) return;
                adjacency[from].Add(to);
                inDegree[to]++;
            }

            for (int i = 0; i < count; i++)
            {
                var handler = nodes[i];

                // ExecuteBeforeKeys: handler i must run before every handler carrying each key
                if (handler.ExecuteBeforeKeys.HasValue)
                {
                    foreach (var key in handler.ExecuteBeforeKeys.Value)
                    {
                        if (!keyToIndices.TryGetValue(key, out var targets)) continue;
                        foreach (var target in targets)
                            AddEdge(i, target);
                    }
                }

                // ExecuteAfterKeys: every handler carrying each key must run before handler i
                if (handler.ExecuteAfterKeys.HasValue)
                {
                    foreach (var key in handler.ExecuteAfterKeys.Value)
                    {
                        if (!keyToIndices.TryGetValue(key, out var sources)) continue;
                        foreach (var source in sources)
                            AddEdge(source, i);
                    }
                }
            }

            // ── 5. Kahn's algorithm with a min-priority queue ─────────────────────
            // The priority queue uses the node's baseline sort index as its priority,
            // so when multiple handlers are simultaneously ready (in-degree = 0),
            // the one with the highest natural priority (lowest index) is emitted first.
            // This preserves the stage → executionOrder → registrationOrder semantics
            // from the baseline sort for all unconstrained handlers.
            var queue = new PriorityQueue<int, int>();
            var result = new List<HandlerActionDescriptor<TContext>>(count);

            // Seed queue with all zero-in-degree nodes
            for (int i = 0; i < count; i++)
            {
                if (inDegree[i] == 0)
                    queue.Enqueue(i, i);  // priority = baseline sort index
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                result.Add(nodes[current]);

                foreach (int neighbour in adjacency[current])
                {
                    if (--inDegree[neighbour] == 0)
                        queue.Enqueue(neighbour, neighbour);
                }
            }

            // ── 6. Cycle detection ────────────────────────────────────────────────
            // If any nodes remain with in-degree > 0, they were never enqueued —
            // they are part of a dependency cycle and could not be resolved.
            if (result.Count != count)
            {
                var cycleParticipants = nodes
                    .Where((_, i) => inDegree[i] > 0)
                    .ToList();

                var summary = HandlerActionDescriptorHelper.BuildHandlerActionDescriptorsSummary(cycleParticipants);

                throw new InvalidOperationException(
                    $"Cannot resolve handler execution order: a circular dependency was detected " +
                    $"among {cycleParticipants.Count} handler(s).\n{summary}");
            }

            return result.ToImmutableArray();
        }

        public IEnumerator<HandlerActionDescriptor<TContext>> GetEnumerator()
            => ((IEnumerable<HandlerActionDescriptor<TContext>>)HandlerDescriptors).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable)HandlerDescriptors).GetEnumerator();
    }
}