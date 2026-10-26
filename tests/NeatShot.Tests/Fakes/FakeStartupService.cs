using NeatShot.Core.Services.Interfaces;

namespace NeatShot.Tests.Fakes;

/// <summary>
/// Startup service giả: không ghi registry thật.
/// </summary>
public sealed class FakeStartupService : IStartupService
{
    public string? RegisteredPath { get; private set; }

    public bool IsEnabled() => RegisteredPath != null;
    public void Enable(string exePath) => RegisteredPath = exePath;
    public void Disable() => RegisteredPath = null;
}
