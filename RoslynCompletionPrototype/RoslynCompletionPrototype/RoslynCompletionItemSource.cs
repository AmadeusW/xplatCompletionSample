﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition;
using Prototype = Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;

namespace RoslynCompletionPrototype
{
    [Export(typeof(IAsyncCompletionItemSource))]
    [Name("C# completion item source")]
    [ContentType("CSharp")]
    class RoslynCompletionItemSource : IAsyncCompletionItemSource
    {
        static readonly ImmutableArray<string> CommitChars = ImmutableArray.Create<string>(".", ",", "(", ")", "[", "]", " ", "\t");
        private ImageMoniker RandomMoniker => new ImageMoniker { Guid = new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), Id = 2996 + (int)(r.NextDouble()*20) };
        readonly Random r = new Random();
        const string RoslynItem = nameof(RoslynItem);

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

            var items = completionList.Items.Select(roslynItem =>
            {
                var item = Prototype.CompletionItem.Create(roslynItem.DisplayText, roslynItem.SortText, roslynItem.FilterText, this, GetFilters(roslynItem.Tags), false, RandomMoniker);
                item.Properties.AddProperty(RoslynItem, roslynItem);
                return item;
            });
            var availableFilters = items.SelectMany(n => n.Filters).Distinct().ToImmutableArray();
            return new Prototype.CompletionContext(items, applicableSpan, availableFilters, false, false);
        }

        private ImmutableArray<CompletionFilter> GetFilters(ImmutableArray<string> tags)
        {
            foreach (var tag in tags)
            {
                switch (tag)
                {
                    case "Enum":
                        return ImmutableArray.Create(CompletionFilters.EnumFilter);
                    case "Event":
                        return ImmutableArray.Create(CompletionFilters.EventFilter);
                    case "Class":
                        return ImmutableArray.Create(CompletionFilters.TypeFilter);
                    case "Struct":
                        return ImmutableArray.Create(CompletionFilters.TypeFilter);
                    case "Method":
                        return ImmutableArray.Create(CompletionFilters.MethodFilter);
                    case "Namespace":
                        return ImmutableArray.Create(CompletionFilters.NamespaceFilter);
                }
            }
            return ImmutableArray.Create<CompletionFilter>();
        }

        public async Task<object> GetDescriptionAsync(Prototype.CompletionItem item)
        {
            return item.DisplayText;
        }

        public void CustomCommit(ITextView view, ITextBuffer buffer, Prototype.CompletionItem item, ITrackingSpan applicableSpan, string textEdit)
        {
            InitializeCompletionService(buffer);

            var roslynItem = item.Properties.GetProperty<Microsoft.CodeAnalysis.Completion.CompletionItem>(RoslynItem); // We're using custom data we deposited in GetCompletionContextAsync
            var document = buffer.GetRelatedDocuments().First();
            char? commitCharacter = String.IsNullOrEmpty(textEdit) ? null : new char?(textEdit[0]);
            var roslynChange = CompletionService.GetChangeAsync(document, roslynItem, commitCharacter).Result;

            var edit = buffer.CreateEdit();
            edit.Replace(new Span(roslynChange.TextChange.Span.Start, roslynChange.TextChange.Span.Length), roslynChange.TextChange.NewText);
            edit.Apply();
        }

        public ImmutableArray<string> GetPotentialCommitCharacters()
        {
            return CommitChars;
        }

        public bool ShouldCommitCompletion(string typedChar, SnapshotPoint location)
        {
            return CommitChars.Contains(typedChar);
        }

        public bool ShouldTriggerCompletion(string edit, SnapshotPoint location)
        {
            if (String.IsNullOrEmpty(edit)) return false;

            // We support only characters
            char ch = edit[0];

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
