private static string JollyPlayerSelector_GetPupButtonOffName(On.JollyCoop.JollyMenu.JollyPlayerSelector.orig_GetPupButtonOffName orig, JollyCoop.JollyMenu.JollyPlayerSelector self)
{
    var result = orig(self);
    var playerclass = self.JollyOptions(self.index).playerClass;
    if(playerclass is not null && playerclass == VoidEnums.SlugcatID.Void)
    {
        result = "void_" + "pup_off";
    }
    return result;
}