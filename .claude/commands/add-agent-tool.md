Add a new Semantic Kernel tool (KernelFunction) to the EnrollMate agent.

## Project context

The agent has two plugin classes:
- `src/EnrollMate.Agent/Tools/DataTools.cs` — read-only tools (queries, lookups)
- `src/EnrollMate.Agent/Tools/ActionTools.cs` — write tools (mutations, side effects)

Each method decorated with `[KernelFunction]` and `[Description("...")]` becomes a tool the LLM can call autonomously. The description is what the model reads to decide when and how to call it — make it precise and behavioural.

Dependencies are injected via the constructor (primary constructor syntax). Both classes already receive `IApplicationRepository` and `ISchoolRepository`. If the new tool needs a different repository, add it to the constructor and register it in `Program.cs`.

The system prompt at `src/EnrollMate.Agent/Prompts/SystemPrompt.txt` lists available tools for the LLM. Any new tool must be added to the relevant section there (Data tools or Action tools) so the model knows it exists.

## What to do

The user wants to add this tool: $ARGUMENTS

1. Decide whether it is a read-only tool (DataTools) or a write/action tool (ActionTools).
2. Read the target file before editing it.
3. Add the method with:
   - `[KernelFunction]` attribute
   - `[Description("...")]` with a clear, behavioural description the LLM will read
   - Correct return type (`Task<string>` for async, `Task.FromResult(...)` for sync)
   - Null-guard and meaningful error strings (the LLM reads these as feedback)
   - An `AgentAction` logged to `application.AgentLog` for any write tool
4. Update `SystemPrompt.txt` — add the tool name and one-line description under the correct section.
5. Do not register anything in `Program.cs` unless a new repository dependency was added.

Do not add XML doc comments. Do not add error handling for scenarios that cannot happen.
