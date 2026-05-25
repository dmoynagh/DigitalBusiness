using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    public static class HandlerActionDescriptorHelper
    {
        internal static string BuildHandlerActionDescriptorsSummary<TContext>(IEnumerable<HandlerActionDescriptor<TContext>> handlerActionDescriptors)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"HandlerActionDescriptors for context type: {typeof(TContext).FullName}");

            var descriptions = handlerActionDescriptors.ToList();
            sb.AppendLine($"Total HandlerActionDescriptors: {descriptions.Count}");


            sb.AppendLine();

            //Dictionary<object, HandlerActionDescriptor<TContext>> keyedDescriptors 
            var keyedDescriptors = descriptions.Where(d => d.Keys.HasValue && d.Keys.Value.Any()).SelectMany(descriptor => descriptor.Keys!.Value.Select(key => (key, descriptor))).ToDictionary(i => i.key, i => i.descriptor);


            foreach (var descriptor in handlerActionDescriptors)
            {
                var name = getDescriptorName(descriptor);
                var type = descriptor.Action != null ? "Action" : descriptor.ActionFactory != null ? "Factory" : "Unknown";
                var keys = descriptor.Keys.HasValue && descriptor.Keys.Value.Any() ? $" Keys[{descriptor.Keys.Value.Length}]" : "";

                var executionStage = descriptor.ExecutionStage switch
                {
                    0 => "",
                    1 => "Init",
                    2 => "Pre",
                    3 => "",
                    4 => "Post",
                    _ => $"Unknown({descriptor.ExecutionStage})"
                };

                var registrationOrder = descriptor.RegistrationOrder.ToString();
                var executionOrder = descriptor.ExecutionOrder.HasValue ? $"Order[{descriptor.ExecutionOrder.Value}]" : "";

                var beforeNames = descriptor.ExecuteBeforeKeys.HasValue && descriptor.ExecuteBeforeKeys.Value.Any() ?
                    descriptor.ExecuteBeforeKeys.Value.Select(k => getKeyedDescriptorName(k)).Where(v=>!string.IsNullOrWhiteSpace(v)).Select(v=>v!).ToArray() : Array.Empty<string>();
                var before = beforeNames.Length > 0 ? $"Before[{string.Join(";", beforeNames)}]" : string.Empty;

                var afterNames = descriptor.ExecuteAfterKeys.HasValue && descriptor.ExecuteAfterKeys.Value.Any() ?
                    descriptor.ExecuteAfterKeys.Value.Select(k => getKeyedDescriptorName(k)).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).ToArray() : Array.Empty<string>();
                var after = afterNames.Length > 0 ? $"After[{string.Join(";", afterNames)}]" : string.Empty;

                sb.AppendLine($"{name}, {type}, {keys}, {executionStage}, {executionOrder}, {before}, {after}; ");
            }

            return sb.ToString();


            string? getDescriptorName(HandlerActionDescriptor<TContext> descriptor) => $"{descriptor.Owner}.{descriptor.Name}";

            string? getKeyedDescriptorName(object key) => keyedDescriptors.TryGetValue(key, out var descriptor) ? getDescriptorName(descriptor) : null;            

        }
    }
}
