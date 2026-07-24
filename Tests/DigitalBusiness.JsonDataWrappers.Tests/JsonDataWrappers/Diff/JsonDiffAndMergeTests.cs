using DigitalBusiness.JsonDataWrappers.Diff;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests.Diff
{
    public class JsonDiffAndMergeTests
    {
        private static JsonData Parse(string json) => new JsonData(JsonNode.Parse(json));

        [Fact]
        public void Diff_NoChanges_ReturnsEmptyResult()
        {
            var a = Parse("""{"x":1,"y":"hi"}""");
            var b = Parse("""{"x":1,"y":"hi"}""");

            var result = JsonDiff.Diff(a, b);

            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void Diff_AddedRemovedChangedProperty_ReportsEachKind()
        {
            var baseline = Parse("""{"a":1,"b":2}""");
            var target = Parse("""{"a":1,"c":3}""");

            var result = JsonDiff.Diff(baseline, target);

            Assert.Equal(2, result.Entries.Count);
            Assert.Contains(result.Entries, e => e.Kind == JsonDiffKind.Removed && e.Path.ToString() == "b");
            Assert.Contains(result.Entries, e => e.Kind == JsonDiffKind.Added && e.Path.ToString() == "c");
        }

        [Fact]
        public void Diff_ChangedScalarValue_ReportsChanged()
        {
            var baseline = Parse("""{"a":1}""");
            var target = Parse("""{"a":2}""");

            var result = JsonDiff.Diff(baseline, target);

            var entry = Assert.Single(result.Entries);
            Assert.Equal(JsonDiffKind.Changed, entry.Kind);
            Assert.Equal("a", entry.Path.ToString());
        }

        [Fact]
        public void Diff_ArrayChange_ReportsChangeAtWholeArrayIndex()
        {
            var baseline = Parse("""{"items":[1,2,3]}""");
            var target = Parse("""{"items":[1,9,3]}""");

            var result = JsonDiff.Diff(baseline, target);

            var entry = Assert.Single(result.Entries);
            Assert.Equal(JsonDiffKind.Changed, entry.Kind);
        }

        [Fact]
        public void Diff_ExcludedPathPrefix_IsSkipped()
        {
            var baseline = Parse("""{"a":1,"secret":"old"}""");
            var target = Parse("""{"a":1,"secret":"new"}""");
            var options = new JsonDiffOptions { ExcludedPathPrefixes = ["secret"] };

            var result = JsonDiff.Diff(baseline, target, options);

            Assert.True(result.IsEmpty);
        }

        [Fact]
        public void ToPatch_ThenMerge_ReproducesTarget()
        {
            var baseline = Parse("""{"a":1,"b":{"x":1,"y":2},"c":[1,2],"d":"remove-me"}""");
            var target = Parse("""{"a":1,"b":{"x":1,"y":9},"c":[1,2,3],"e":"added"}""");

            var diff = JsonDiff.Diff(baseline, target);
            var patch = diff.ToPatch();

            var merged = JsonMerge.Apply(baseline, patch);

            Assert.True(merged.DeepEquals(target));
        }

        [Fact]
        public void Merge_DeleteMarker_RemovesProperty()
        {
            var baseline = Parse("""{"a":1,"b":2}""");
            var patchJson = Parse("""{"b":"$$delete"}""");
            var patch = new JsonPatch(patchJson);

            var merged = JsonMerge.Apply(baseline, patch);

            Assert.False(merged.ContainsProperty("b"));
            Assert.True(merged.ContainsProperty("a"));
        }

        [Fact]
        public void Merge_SetNullMarker_SetsExplicitNull()
        {
            var baseline = Parse("""{"a":1}""");
            var patchJson = Parse("""{"a":null}""");
            var patch = new JsonPatch(patchJson);

            var merged = JsonMerge.Apply(baseline, patch);

            Assert.True(merged.ContainsProperty("a"));
            Assert.True(merged.Get("a").IsNull);
        }

        [Fact]
        public void Merge_NestedObjectPatch_MergesRecursively()
        {
            var baseline = Parse("""{"a":{"x":1,"y":2}}""");
            var patchJson = Parse("""{"a":{"y":9}}""");
            var patch = new JsonPatch(patchJson);

            var merged = JsonMerge.Apply(baseline, patch);

            Assert.Equal(1, merged.Get("a").Get("x").Get<int>());
            Assert.Equal(9, merged.Get("a").Get("y").Get<int>());
        }

        [Fact]
        public void Merge_BaseDocumentIsNotMutated_ByApply()
        {
            var baseline = Parse("""{"a":1}""");
            var patchJson = Parse("""{"a":2}""");
            var patch = new JsonPatch(patchJson);

            JsonMerge.Apply(baseline, patch);

            Assert.Equal(1, baseline.Get("a").Get<int>());
        }

        [Fact]
        public void DeepSemanticEquals_NumericToleranceComparesDifferentRawRepresentations()
        {
            using var docA = JsonDocument.Parse("""{"x":1e2}""");
            using var docB = JsonDocument.Parse("""{"x":100}""");
            var a = new JsonData(docA.RootElement);
            var b = new JsonData(docB.RootElement);

            Assert.True(a.DeepSemanticEquals(b));
        }
    }
}
