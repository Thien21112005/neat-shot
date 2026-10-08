# NeatShot Agent Development Guidelines & Orchestration

Welcome to the NeatShot repository. All agent interactions in this project follow a structured, high-quality development lifecycle governed by the **Workflow Orchestrator**.

## Master Principle: Automatic Skill Chaining

You do not require the developer to manually invoke slash commands or remember skill names. Instead, you automatically detect the intent and apply the appropriate specialized skills in the correct sequence:

1. **New Features / Architecture Changes / Large Refactorings**:
   - Activate: `writing-plans` (along with `wpf-best-practices` or `dotnet-best-practices`).
   - Create detailed task breakdowns, specifications, and acceptance criteria in `docs/superpowers/plans/`.
2. **Executing Existing Plans / Step-by-Step Tasks**:
   - Activate: `executing-plans` + `test-driven-development`.
   - Update `progress.md` and complete tasks incrementally.
3. **Writing Code / Modifying Logic**:
   - Activate: `test-driven-development` (The Iron Law: Red -> Green -> Refactor. Write failing test first, make it pass, refactor).
   - Complement with: `csharp-xunit` for test structures and `csharp-async` for async/await patterns.
4. **Bugs / Crashes / UI Clipping / Unexpected Behavior**:
   - Activate: `systematic-debugging`.
   - Never guess or shotgun-fix. Reproduce with minimal test/data, identify root cause, fix minimally, verify regression.
5. **UI & Native Interactions**:
   - WPF / XAML / MVVM / Canvas: Follow `wpf-best-practices`.
   - Win32 / P-Invoke / Native APIs: Follow `dotnet-pinvoke`.
6. **Task Completion & Verification Gate**:
   - Activate: `verification-before-completion`.
   - NEVER declare completion without executing `dotnet test tests/NeatShot.Tests/NeatShot.Tests.csproj` and presenting actual passing test counts as evidence.

## NeatShot Architecture Guardrails

- **Per-Monitor DPI**: Win32 coordinates are physical pixels; WPF elements are 96 DPI units. Always account for DPI transforms (`PointFromScreen`, `PointToScreen`, `DpiScale`).
- **STA UI Thread**: Never update UI or observable collections from background threads without `Dispatcher.InvokeAsync`.
- **Formatting**: Always format file links in responses with GitHub-style markdown links using the `file:///` scheme (e.g. `[MainWindow.xaml.cs](file:///d:/Tu-hoc/project/NeatShot/src/NeatShot/Views/MainWindow.xaml.cs)`).
