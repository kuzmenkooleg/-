namespace Lab09;

internal sealed class GraphSettings
{
    public double P { get; init; } = 2;
    public double TMin { get; init; } = 0;
    public double TMax { get; init; } = 12;
    public double Step { get; init; } = 0.02;
    public bool ShowLowerBranch { get; init; }
    public bool ShowPoints { get; init; }
    public bool AutoScale { get; init; } = true;
}
