using Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynCompletionPrototype
{
    public static class CompletionFilters
    {
        public static CompletionFilter EnumFilter = new CompletionFilter("Enumerations", "e", new ImageMoniker(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 1120));
        public static CompletionFilter EventFilter = new CompletionFilter("Events", "v", new ImageMoniker(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 1152));
        public static CompletionFilter MethodFilter = new CompletionFilter("Methods", "m", new ImageMoniker(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 1880));
        public static CompletionFilter NamespaceFilter = new CompletionFilter("Namespaces", "n", new ImageMoniker(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 1955));
        public static CompletionFilter TypeFilter = new CompletionFilter("Types", "t", new ImageMoniker(new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}"), 3244));
    }
}
