using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Slugpack
{
    static class Constants
    {
        public const string Voyager = "voyager";

        public const string Technomancer = "technomancer";

        public const bool shaders_enabled = true;

        public const int timeReached = 10;

        public static ConditionalWeakTable<Player, WeakTables.ScanLine> ScanLineMemory = new();

        public static ConditionalWeakTable<PlayerGraphics, WeakTables.GraphicsData> Graphics = new();

        public static ConditionalWeakTable<Vulture, WeakTables.VultureStuff> VultureStuff = new();

        public static ConditionalWeakTable<RainWorldGame, WeakTables.ShortcutList> DamagedShortcuts = new();

        public static ConditionalWeakTable<Oracle, WeakTables.OracleData> OracleInfo = new();

        public static ConditionalWeakTable<RainWorld, WeakTables.Shaders> SlugpackShaders = new();

        public static readonly ulong[] FurData = {
            0xD21802F008A81006, 0xD96624837B4AA00B, 0x0004E02005450840, 0x6FCD352033803DE0, 0x5412150E02D034EC, 0x009802900601805A, 0x29A3BAD45402C802, 0x761B67034AE617EB,
            0x53C009802EA0601B, 0x65B91B332D54E033, 0x4018F0666D9490C1, 0x62356600D0006601, 0xC199707385ACD66D, 0x006C02D9080199CD, 0x80268B5AA000A007, 0x8012066D12C0780D,
            0x72C00B000E718019, 0x9412612AB5E6003C, 0x002D066454584384, 0x92D85D4104039E59, 0x940352B1387F632A, 0x04980EC41998054D, 0x58DB6D90000E10B8, 0x405166772FCB13A6,
            0x7B000400150665AA, 0x581C84D49A264044, 0x09A4D9966564D1B0, 0xCE6B5A0120021C6D, 0x999582D05A09B498, 0x03908B9480121660, 0xF88C66F8C0A30216, 0x8700AA58F781689C,
            0x1F80690333199665, 0x50502AC272160210, 0x25EE2019958B1275, 0x69D28422E0136848, 0x151469C082A77900, 0xB6020906A98058AB, 0x018C9E2BC296020D, 0x180103499C12C27A,
            0xB60CE0030C151805, 0x11D20789F68830E0, 0x8A21B66A963F0028, 0xA7F700DB02B28400, 0x485B242696A015C3, 0x7901B30481994A50, 0x142A831E0F082168, 0x40054710E21C2406,
            0x83EA590821A0501C, 0x3E888610369CC74E, 0x5C820051591229E8, 0x4179801708E60801, 0x9962189D6107AEC0, 0x0022F83308058EE2, 0x77842704E0938FA0, 0xA823D626898A10B8,
            0x56304E1906008FD8, 0x1C24F0430A5B2B08, 0xFA066703E2C8A4A1, 0x0E1B9DC029380019, 0x979A62E1844113F5, 0x4692C1806AB66308, 0x5411297CC8726400, 0x020059F02160452C,
            0xDAE169F41AA75368, 0xF0874116A0F9266A, 0x006A9D45AB026404, 0x634E42030BE18014, 0x541D81AE11AC1328, 0x3F4C82A0B6DD7A30, 0xA8471B98C5D0E619, 0x9701C1B815E11E69,
            0xC2F2632066451D94, 0x020200800003791A, 0xE0660CF61E807602, 0x24060372708545B3, 0xD97A2C48091B9809, 0x71221A80AB460804, 0xA02680C85C087060, 0x047529405700A2BB,
            0x24E3814080199C0D, 0x08EC02A6E69DF85A, 0x019851519804200B, 0x2966D390C8140181, 0x42C000802E933B4F, 0x43C0199A85078230, 0x00C46E4329122066, 0x73343081E203352C,
            0x080E348664515962, 0x00D80D5700049904, 0x20000166B08A4037, 0xE608A5850060199D, 0x0C808C00E5B9A69C, 0x33E06611D4B183FE, 0x03BA59A718000601, 0xA8586003600F5DE6,
            0xAA38961A86CD6258, 0x08A03F2E0B732898, 0x198A8D0000000000,
        };

        public static Dictionary<int, int> TrainOffsets = new Dictionary<int, int>()
        {
            { 0, 40 },
            { 1, 50 },
            { 2, 60 },
            { 3, 0 },
            { 4, 50 },
            { 5, 40 },
            { 6, 60 },
            { 7, 0 },
            { 8, 30 },
            { 9, 60 },
            { 10, 36 },
            { 11, 60 },
            { 12, 24 },
            { 13, 52 },
            { 14, 50 },
            { 15, 10 },
            { 16, 58 },
            { 17, -7 },
            { 18, 11 },
            { 19, 41 },
            { 20, 31 },
            { 21, 40 },
            { 22, 40 },
            { 23, 20 },
            { 24, 28 },
        };

        public static Dictionary<int, int> ConnectorOffsets = new Dictionary<int, int>()
        {
            { 0, 35 },
            { 1, 39 },
            { 2, 27 },
        };
    }
}