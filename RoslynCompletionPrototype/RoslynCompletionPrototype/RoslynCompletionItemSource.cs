using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition;
using Prototype = Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition;

namespace RoslynCompletionPrototype
{
    [ExportMetadata("ContentType", "CSharp")]
    [Export(typeof(IAsyncCompletionItemSource))]
    class RoslynCompletionItemSource : IAsyncCompletionItemSource
    {
        static readonly ImmutableArray<char> CommitChars = ImmutableArray.Create<char>('.', ',', '(', ')', '[', ']');
        static readonly ImmutableArray<char> TriggerChars = ImmutableArray.Create<char>('a', 'b', 'c', 'd', 'e', 'f'); // TODO: There is more trigger chars than non-trigger chars. Find best implementation.
        private readonly ImageMoniker StructurePublicMoniker = new ImageMoniker { Guid = new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), Id = 2996 };

        private CompletionService CompletionService { get; set; }

        public async Task<Prototype.CompletionContext> GetCompletionContextAsync(Prototype.CompletionTrigger trigger, SnapshotPoint triggerLocation)
        {
            var snapshot = triggerLocation.Snapshot;
            var buffer = triggerLocation.Snapshot.TextBuffer;
            InitializeCompletionService(buffer);

            Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            var completionList = await CompletionService.GetCompletionsAsync(document, triggerLocation.Position);
            var span = CompletionService.GetDefaultCompletionListSpan(await document.GetTextAsync(), triggerLocation.Position);
            var applicableSpan = new SnapshotSpan(triggerLocation.Snapshot, span.Start, span.Length);

            if (completionList == null)
                return default(Prototype.CompletionContext);

            var items = completionList.Items.Select(roslynItem => Prototype.CompletionItem.Create(roslynItem.DisplayText, roslynItem.SortText, roslynItem.FilterText, this, roslynItem.Tags, false, false, false, roslynItem, StructurePublicMoniker));
            return new Prototype.CompletionContext(items, applicableSpan);
        }

        public async Task<object> GetDescriptionAsync(Prototype.CompletionItem item)
        {
            return item.DisplayText;
        }

        public async Task CustomCommit(ITextBuffer buffer, Prototype.CompletionItem item, ITrackingSpan applicableSpan, char? commitCharacter)
        {
            InitializeCompletionService(buffer);

            // HACK ALERT: We're sneaking the Roslyn CompletionItem as item.SortData. Updated Roslyn provider should consume our CompletionItem
            var roslynItem = item.Hack as Microsoft.CodeAnalysis.Completion.CompletionItem;
            var document = buffer.GetRelatedDocuments().First();
            var roslynChange = await CompletionService.GetChangeAsync(document, roslynItem, commitCharacter);

            var edit = buffer.CreateEdit();
            edit.Replace(new Span(roslynChange.TextChange.Span.Start, roslynChange.TextChange.Span.Length), roslynChange.TextChange.NewText);
            edit.Apply();
        }

        public ImmutableArray<char> GetPotentialCommitCharacters()
        {
            return CommitChars;
        }

        public bool ShouldCommitCompletion(char typedChar, SnapshotPoint location)
        {
            return CommitChars.Contains(typedChar);
        }

        public bool ShouldTriggerCompletion(char ch, SnapshotPoint location)
        {
            // identifier-start-character:
            //   letter-character
            //   _ (the underscore character U+005F)

            if (ch < 'a') // '\u0061'
            {
                if (ch < 'A') // '\u0041'
                {
                    return false;
                }

                return ch <= 'Z'  // '\u005A'
                    || ch == '_' // '\u005F'
                    || ch == '@'; // ?
            }

            if (ch <= 'z') // '\u007A'
            {
                return true;
            }

            if (ch <= '\u007F') // max ASCII
            {
                return false;
            }
            return false; // See Roslyn.Utilities.UnicodeCharacterUtilities.IsIdentifierStartCharacter
        }

        private void InitializeCompletionService(ITextBuffer buffer)
        {
            if (CompletionService == null)
            {
                if (!Workspace.TryGetWorkspace(buffer.AsTextContainer(), out var workspace))
                {
                    throw new InvalidOperationException("Unable to obtain Roslyn workspace for this buffer");
                }

                CompletionService = workspace.Services.GetLanguageServices(LanguageNames.CSharp).GetService<CompletionService>();
                if (CompletionService == null)
                    throw new InvalidOperationException("Unable to obtain completion service for this buffer");
            }
        }
    }
}
