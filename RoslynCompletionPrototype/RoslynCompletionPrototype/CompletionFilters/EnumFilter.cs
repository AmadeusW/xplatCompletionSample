using Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynCompletionPrototype.CompletionFilters
{
    [Export(typeof(ICompletionFilter))]
    [Name(nameof(EnumFilter))]
    [Order(After = nameof(EventFilter))]
    [ContentType("CSharp")]
    public class EnumFilter : ICompletionFilter
    {
        public readonly ImmutableArray<string> _tags = ImmutableArray.Create("Enum");
        public const char _accessCharacter = 'e';
        public const string _displayText = "Enumerations";
        private readonly ImageMoniker _knownMonikerTypePublic = new ImageMoniker(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 1120);

        public ImageMoniker Icon => _knownMonikerTypePublic;

        string ICompletionFilter.DisplayText => _displayText;

        ImmutableArray<string> ICompletionFilter.Tags => _tags;

        char ICompletionFilter.AccessCharacter => _accessCharacter;
    }
}
