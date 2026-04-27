namespace AntiXray.Commands;

public readonly record struct AdminOreHideResult(
    int Scanned,
    int OreFound,
    int Hidden,
    int AlreadyHidden,
    int PlaceholderMissing);
