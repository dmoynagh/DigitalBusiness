using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
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

        private static ImmutableArray<HandlerActionDescriptor<TContext>> OrderHandlers(IEnumerable<HandlerActionDescriptor<TContext>> handlerDescriptors)
        {
            var sortedDescriptors = handlerDescriptors
                .OrderBy(d =>d.ExecutionStage)
                .ThenBy(d => d.ExecutionOrder ?? HandlerDefaults.DefaultExecutionOrder)
                .ThenBy(d => d.RegistrationOrder)
                .ToList();



            // Safety check: prevent infinite loops from circular dependencies
            int maxIterations = sortedDescriptors.Count * sortedDescriptors.Count * 2; // Allow for complex dependency chains
            int iterationCount = 0;

            for (int i = 0; i < sortedDescriptors.Count; i++)
            {
                // Increment and check iteration limit
                if (++iterationCount > maxIterations)
                {
                    throw new InvalidOperationException(
                        $"Unable to resolve handler execution order after {iterationCount} iterations. " +
                        $"This likely indicates circular dependencies or an overly complex dependency graph. " +
                        HandlerActionDescriptorHelper.BuildHandlerActionDescriptorsSummary(sortedDescriptors));
                }

                var handler = sortedDescriptors[i];

                int beforeConstraintMaxIndexLimit = sortedDescriptors.Count;
                int afterConstraintMinIndexLimit = -1;

                if (handler.ExecuteBeforeKeys is null && handler.ExecuteAfterKeys is null)
                {
                    continue; //no dependencies, so skip
                }

                //resolve and before key constraint index limits.
                if (handler.ExecuteBeforeKeys.HasValue && handler.ExecuteBeforeKeys.Value.Length > 0)
                {
                    //check to see if there are any conflicting dependencies (keys in both before and after) that would make it impossible to resolve execution order                        
                    if (handler.ExecuteAfterKeys.HasValue)
                    {

                        if (handler.ExecuteBeforeKeys.Value.Any(k => handler.ExecuteAfterKeys.Value.Contains(k)))
                        {
                            throw new InvalidOperationException($"Cannot resolve execution order for handler '{handler.Name}' due to conflicting dependencies. It cannot have the same key(s) in both ExecuteAfterKeys and ExecuteBeforeKeys.");
                        }
                    }

                    var beforeKeyIndex = sortedDescriptors.IndexOf(h => h.Keys.HasValue && h.Keys.Value.Any(k => handler.ExecuteBeforeKeys.Value.Contains(k)));

                    if (beforeKeyIndex != -1)
                        beforeConstraintMaxIndexLimit = beforeKeyIndex;
                }

                //resolve and after key constraint index limits.
                if (handler.ExecuteAfterKeys.HasValue && handler.ExecuteAfterKeys.Value.Length > 0)
                {
                    var afterKeyIndex = sortedDescriptors.LastIndexOf(h => h.Keys.HasValue && h.Keys.Value.Any(k => handler.ExecuteAfterKeys.Value.Contains(k)));

                    if (afterKeyIndex != -1)
                    {
                        afterConstraintMinIndexLimit = afterKeyIndex;
                    }
                }

                //check limits to see if there is a possibility to resolve the dependencies.
                //If the max index limit for before constraints is less than the min index limit for after constraints, then we cannot resolve the dependencies and should throw an exception
                if (beforeConstraintMaxIndexLimit <= afterConstraintMinIndexLimit)
                {
                    throw new InvalidOperationException($"Cannot resolve execution order for handler '{handler.Name}' due to conflicting dependencies.");
                }


                //work out resolved index based on before constraints (move it before the earliest before constraint handler)

                //apply before constraint by - if i is greater than beforeConstraintMaxIndexLimit then move it back to the beforeConstraintMaxIndexLimit
                var resolvedIndex = Math.Min(i, beforeConstraintMaxIndexLimit);

                //apply after constraint by - if i is less than afterConstraintMinIndexLimit then move it forward to the afterConstraintMinIndexLimit + 1
                resolvedIndex = Math.Max(resolvedIndex, afterConstraintMinIndexLimit + 1);

                //determine if the resolved index is different from the current index, if so move the handler to the resolved index
                if (resolvedIndex != i)
                {
                    //if move back
                    if (resolvedIndex < i)
                    {
                        sortedDescriptors.RemoveAt(i);
                        sortedDescriptors.Insert(resolvedIndex, handler);

                        //move back the index to resolved index so items after the moved handler are reprocessed for dependencies as well
                        //This means:
                        //items before the resolved index do not need to be reprocessed
                        //- they have already been processed and if they have a before dependency on the moved handler they are still before it
                        //- if they had an after dependency on the moved handler they have already been processed and woudl be after it and will be reprocessed again anyway.
                        //- you won't end up with any items before the resovled index that have invalid constraints as a resul of the move.

                        //items after the resolved index need to be reprocessed as they may have dependencies on the moved handler that are now invalid as a result of the move
                        //so we move the index back to the resolved index to ensure they are reprocessed.
                        i = resolvedIndex - 1;
                    }
                    //if move forward
                    else
                    {
                        sortedDescriptors.RemoveAt(i);
                        //the remove causes a left shift so we need to move the resolved index back by 1 to account for this when inserting
                        sortedDescriptors.Insert(resolvedIndex - 1, handler);
                        //move back the index to account for the moved handler
                        //the next item that was i+1 is now at i after the remove. So the i++ in the iterator will cause it to be skipped.
                        //with i-- when the i++ is applied the next item to be processed is correct.
                        i--;
                    }
                }
            }

            return sortedDescriptors.ToImmutableArray();
        }

        public IEnumerator<HandlerActionDescriptor<TContext>> GetEnumerator()
        {
            return ((IEnumerable<HandlerActionDescriptor<TContext>>)HandlerDescriptors).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)HandlerDescriptors).GetEnumerator();
        }
    }
}
