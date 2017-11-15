# Sample language service using the xplat completion API

This project provides a language service that implements `IAsyncCompletionItemSource` and `IAsyncCompletionService`.

This language service is consumed by modified VSEditor which implements the xplat completion API.

## Dependencies

* Microsoft.VisualStudio.Text.UI (and its dependencies)
* Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition

the prototype dll is available at `"\\scratch2\scratch\amwieczo\Completion\dlls\2017 11 14\net46\Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition.net46.dll"`

## How to run

* To run, install the modified VSEditor .vsix into experimental hive, then F5 this project.
* Alternatively, build the modified VSEditor from source, F5 it, then F5 this project.