# Sample language service using the xplat completion API

This project provides a language service that implements `IAsyncCompletionItemSource` and `IAsyncCompletionService`.

This language service is consumed by modified VSEditor whose VSIX I shared over email.

## Dependencies

* Microsoft.VisualStudio.Text.UI (and its dependencies)
* Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition

the prototype dll is available at `"\\scratch2\scratch\amwieczo\Completion\dlls\2017 11 14\net46\Microsoft.VisualStudio.Language.Intellisense.Prototype.Definition.net46.dll"`

## How to run
To run, install the VSEditor vsix I shared over email. Then F5 this project.