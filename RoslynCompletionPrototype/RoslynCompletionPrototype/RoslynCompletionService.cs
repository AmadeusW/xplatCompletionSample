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

namespace RoslynCompletionPrototype
{
    [ExportMetadata("ContentType", "CSharp")]
    [Export(typeof(IAsyncCompletionService))]
    class RoslynCompletionService : IAsyncCompletionService
    {
        private CompletionService CompletionService { get; set; }

        async Task<Prototype.CompletionList> IAsyncCompletionService.UpdateCompletionListAsync(IEnumerable<Prototype.CompletionItem> originalList, ITextSnapshot snapshot, ITrackingSpan applicableSpan)
        {
            var filterText = applicableSpan.GetText(snapshot);
            var filteredList = originalList.Where(n => n.FilterText.Contains(filterText)); // TODO: use pattern matcher
            var sortedList = filteredList.OrderBy(n => n.SortText);
            var availableFilters = filteredList.SelectMany(n => n.Tags).Distinct().Select(n => new CompletionItemFilter(n, n, n[0]));

            // TODO: optimize so we don't iterate three times
            bool suggestionMode = originalList.Any(n => n.IsSuggestion);
            bool softSelection = originalList.Any(n => n.SoftSelected);
            Prototype.CompletionItem suggestionModeItem = originalList.FirstOrDefault(n => n.IsSuggestion);

            return new Prototype.CompletionList(sortedList, 0, softSelection, suggestionMode, suggestionModeItem, availableFilters);
        }
    }
}
