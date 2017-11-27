using Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis;
using Prototype = Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Utilities;

namespace RoslynCompletionPrototype
{
    [Export(typeof(IAsyncCompletionService))]
    [Name("Roslyn completion service")]
    [ContentType("CSharp")]
    class RoslynCompletionService : IAsyncCompletionService
    {
        private CompletionService CompletionService { get; set; }

        async Task<Prototype.CompletionList> IAsyncCompletionService.UpdateCompletionListAsync(IEnumerable<Prototype.CompletionItem> originalList, ITextSnapshot snapshot, ITrackingSpan applicableSpan, IEnumerable<ICompletionFilter> availableFilters)
        {
            var filterText = applicableSpan.GetText(snapshot);
            var filteredList = originalList.Where(n => n.FilterText.Contains(filterText)); // TODO: use pattern matcher
            var sortedList = filteredList.OrderBy(n => n.SortText);
            // Filtering (with filter buttons) should happen here rather than in the viewmodel, because viewmodel operates on UI thread
            // and language service may want to do something interesting when there are no available items

            // For now, just see which filters are available for all items
            var allTags = originalList.SelectMany(n => n.Tags).Distinct();
            var allFilters = availableFilters.Where(n => FilterMatchesTag(n, allTags));
            // And which filters are available just for the visible items
            var filteredTags = filteredList.SelectMany(n => n.Tags).Distinct();
            var enabledFilters = availableFilters.Where(n => FilterMatchesTag(n, filteredTags));
            // Create a filter dictionary where value indicates whether it's enabled
            var filters = ImmutableDictionary.Create<ICompletionFilter, bool>();
            filters = filters.AddRange(allFilters.Select(n => new KeyValuePair<ICompletionFilter, bool>(n, enabledFilters.Contains(n))));

            // TODO: optimize so we don't iterate three times
            bool suggestionMode = originalList.Any(n => n.IsSuggestion);
            bool softSelection = originalList.Any(n => n.SoftSelected);
            Prototype.CompletionItem suggestionModeItem = originalList.FirstOrDefault(n => n.IsSuggestion);

            return new Prototype.CompletionList(sortedList, 0, softSelection, suggestionMode, suggestionModeItem, filters);
        }

        private static bool FilterMatchesTag(ICompletionFilter filter, IEnumerable<string> allTags)
        {
            foreach (var tag in allTags)
            {
                if (filter.Tags.Contains(tag))
                {
                    return true;
                }
            }
            return false;
        }

        // TODO: Bring ShouldBeFilteredOutOfCompletionList from Roslyn
    }
}
