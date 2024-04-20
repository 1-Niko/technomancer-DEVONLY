namespace Slugpack;

public class TechyData
{
    public readonly bool IsTechy;

    public readonly Player player;

    public TechyData(Player player)
    {
        IsTechy = player.slugcatStats.name == TnEnums.Technomancer;
        this.player = player;

        if (!IsTechy) return;
        //Add all the values or data for Techy below
    }
}