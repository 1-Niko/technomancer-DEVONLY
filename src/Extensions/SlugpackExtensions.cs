﻿using SlugBase.Features;

namespace Slugpack;

public static class SlugpackExtensions
{
    //Extension for Technomancer
    private static readonly ConditionalWeakTable<Player, TechyData> _cwttn = new();

    public static TechyData Techy(this Player player) => _cwttn.GetValue(player, _ => new TechyData(player));

    public static bool IsTechy(this Player player) => player.Techy().IsTechy;

    public static bool IsTechy(this Player player, out TechyData Techy)
    {
        Techy= player.Techy();
        return Techy.IsTechy;
    }

    //Extension for Voyager
    private static readonly ConditionalWeakTable<Player, VoyagerData> _cwtvy = new();

    public static VoyagerData Voyager(this Player player) => _cwtvy.GetValue(player, _ => new VoyagerData(player));

    public static bool IsVoyager(this Player player) => player.Voyager().IsVoyager;

    public static bool IsVoyager(this Player player, out VoyagerData Voyager)
    {
        Voyager = player.Voyager();
        return Voyager.IsVoyager;
    }
}