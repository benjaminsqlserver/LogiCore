namespace LogiCore.Client.Shared
{
    /// <summary>
    /// Generic label/value pair for RadzenDropDown.
    /// Radzen resolves TextProperty and ValueProperty via reflection —
    /// it requires real named properties on a class, not C# tuple fields.
    /// </summary>
    public class DropDownItem<T>
    {
        public string Label { get; init; } = string.Empty;
        public T Value { get; init; } = default!;
    }
}
