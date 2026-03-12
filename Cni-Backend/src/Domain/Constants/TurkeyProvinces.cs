namespace Backend.Domain.Constants;

/// <summary>
/// All 81 provinces (il) of Türkiye with their plate codes.
/// Based on https://en.wikipedia.org/wiki/Provinces_of_Turkey
/// </summary>
public static class TurkeyProvinces
{
    public static readonly IReadOnlyList<ProvinceInfo> All = new List<ProvinceInfo>
    {
        new("01", "Adana"),
        new("02", "Adıyaman"),
        new("03", "Afyonkarahisar"),
        new("04", "Ağrı"),
        new("05", "Amasya"),
        new("06", "Ankara"),
        new("07", "Antalya"),
        new("08", "Artvin"),
        new("09", "Aydın"),
        new("10", "Balıkesir"),
        new("11", "Bilecik"),
        new("12", "Bingöl"),
        new("13", "Bitlis"),
        new("14", "Bolu"),
        new("15", "Burdur"),
        new("16", "Bursa"),
        new("17", "Çanakkale"),
        new("18", "Çankırı"),
        new("19", "Çorum"),
        new("20", "Denizli"),
        new("21", "Diyarbakır"),
        new("22", "Edirne"),
        new("23", "Elazığ"),
        new("24", "Erzincan"),
        new("25", "Erzurum"),
        new("26", "Eskişehir"),
        new("27", "Gaziantep"),
        new("28", "Giresun"),
        new("29", "Gümüşhane"),
        new("30", "Hakkâri"),
        new("31", "Hatay"),
        new("32", "Isparta"),
        new("33", "Mersin"),
        new("34", "İstanbul"),
        new("35", "İzmir"),
        new("36", "Kars"),
        new("37", "Kastamonu"),
        new("38", "Kayseri"),
        new("39", "Kırklareli"),
        new("40", "Kırşehir"),
        new("41", "Kocaeli"),
        new("42", "Konya"),
        new("43", "Kütahya"),
        new("44", "Malatya"),
        new("45", "Manisa"),
        new("46", "Kahramanmaraş"),
        new("47", "Mardin"),
        new("48", "Muğla"),
        new("49", "Muş"),
        new("50", "Nevşehir"),
        new("51", "Niğde"),
        new("52", "Ordu"),
        new("53", "Rize"),
        new("54", "Sakarya"),
        new("55", "Samsun"),
        new("56", "Siirt"),
        new("57", "Sinop"),
        new("58", "Sivas"),
        new("59", "Tekirdağ"),
        new("60", "Tokat"),
        new("61", "Trabzon"),
        new("62", "Tunceli"),
        new("63", "Şanlıurfa"),
        new("64", "Uşak"),
        new("65", "Van"),
        new("66", "Yozgat"),
        new("67", "Zonguldak"),
        new("68", "Aksaray"),
        new("69", "Bayburt"),
        new("70", "Karaman"),
        new("71", "Kırıkkale"),
        new("72", "Batman"),
        new("73", "Şırnak"),
        new("74", "Bartın"),
        new("75", "Ardahan"),
        new("76", "Iğdır"),
        new("77", "Yalova"),
        new("78", "Karabük"),
        new("79", "Kilis"),
        new("80", "Osmaniye"),
        new("81", "Düzce"),
    };

    private static readonly Dictionary<string, ProvinceInfo> ByCode =
        All.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

    public static bool IsValid(string code) => ByCode.ContainsKey(code);
    public static ProvinceInfo? Get(string code) => ByCode.GetValueOrDefault(code);
}

public record ProvinceInfo(string Code, string Name);
