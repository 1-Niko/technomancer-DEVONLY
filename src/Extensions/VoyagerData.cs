namespace Slugpack;

public class VoyagerData
{
    public readonly bool IsVoyager;

    public readonly Player player;

    public VoyagerData(Player player)
    {
        IsVoyager = player.slugcatStats.name == TnEnums.Voyager;
        this.player = player;

        if (!IsVoyager) return;
        //Add all the values or data for Voyager below
    }
}