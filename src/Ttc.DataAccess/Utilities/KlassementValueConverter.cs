namespace Ttc.DataAccess.Utilities;

/// <summary>
/// Convert a ranking in Vttl/Sporta to its competition value
/// </summary>
internal class KlassementValueConverter
{
    private static readonly IDictionary<string, int> _sporta;
    private static readonly IDictionary<string, int> _vttl;

    static KlassementValueConverter()
    {
        _vttl = new Dictionary<string, int>
        {
            ["A"] = 18,
            ["B0"] = 17,
            ["B2"] = 16,
            ["B4"] = 15,
            ["B6"] = 14,
            ["C0"] = 13,
            ["C2"] = 12,
            ["C4"] = 11,
            ["C6"] = 10,
            ["D0"] = 9,
            ["D2"] = 8,
            ["D4"] = 7,
            ["D6"] = 6,
            ["E0"] = 5,
            ["E2"] = 4,
            ["E4"] = 3,
            ["E6"] = 2,
            ["NG"] = 1
        };

        _sporta = new Dictionary<string, int>
        {
            ["A"] = 19,
            ["B0"] = 18,
            ["B2"] = 17,
            ["B4"] = 16,
            ["B6"] = 15,
            ["C0"] = 14,
            ["C2"] = 13,
            ["C4"] = 12,
            ["C6"] = 11,
            ["D0"] = 10,
            ["D2"] = 9,
            ["D4"] = 8,
            ["D6"] = 7,
            ["E0"] = 6,
            ["E2"] = 5,
            ["E4"] = 4,
            ["E6"] = 3,
            ["F"] = 2,
            ["NG"] = 1
        };
    }

    public static int Vttl(string ranking)
    {
        if (_vttl.TryGetValue(ranking, out int value))
        {
            return value;
        }
        return -1;
    }

    public static int Sporta(string ranking)
    {
        if (_sporta.TryGetValue(ranking, out int value))
        {
            return value;
        }
        return -1;
    }
}
