// Menu.cs
/* Initializes and handles the options menu */

using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using System;
using UnityEngine;
using Color = UnityEngine.Color;

using Slugpack;

public class OptionsMenu : OptionInterface
{
    public OptionsMenu(Plugin plugin)
    {
        furDisabled = this.config.Bind<bool>("splugpack_Bool_Checkbox", false);
        alwaysOnHolograms = this.config.Bind<bool>("splugpack_Bool_Holograms", false);
        trainLengthSlider = this.config.Bind<int>("splugpack_Int_TrainLength", 30);
    }
    public override void Initialize()
    {
        var opTab1 = new OpTab(this, "Default Canvas");
        this.Tabs = new[] { opTab1 };

        // Tab 1
        OpContainer tab1Container = new OpContainer(new Vector2(0, 0));
        opTab1.AddItems(tab1Container);

        UIArrayElements = new UIelement[]
        {
                new OpLabel(0f, 550f, "The Technomancer - Remix Menu", true),
                new OpCheckBox(furDisabled, 0, 500),
                new OpLabel(30, 502, "Disable Fur"),

                new OpCheckBox(alwaysOnHolograms, 0, 450),
                new OpLabel(30, 452, "Force-Enable Holograms"),

                new OpHoldButton(new Vector2(450f, 450f), 60f, "Reset", 380f),
                new OpLabel(440, 430, "Warning! You do not have"),
                new OpLabel(435, 417, "the recommended settings!"),
                new OpSlider(trainLengthSlider, new Vector2(0f, 396f), 100, false)
                {
                    max = 100,
                    hideLabel = false
                },
                new OpLabel(107, 402, "Train Length"),
        };
        opTab1.AddItems(UIArrayElements);
    }
    public override void Update()
    {
        base.Update();

        warningFlash += 1;

        (UIArrayElements[5] as OpHoldButton).greyedOut = !(UIArrayElements[1] as OpCheckBox).GetValueBool() && !(UIArrayElements[3] as OpCheckBox).GetValueBool() && ((UIArrayElements[8] as OpSlider).GetValueInt() == 30);

        if (!(UIArrayElements[5] as OpHoldButton).greyedOut)
        {
            float flash = ((float)Math.Cos((3.14159f * warningFlash) / 10f) + 1f) / 2f;
            (UIArrayElements[6] as OpLabel).color = new Color(1f, flash, flash, 1f);
            (UIArrayElements[7] as OpLabel).color = new Color(1f, flash, flash, 1f);
        }
        else
        {
            (UIArrayElements[6] as OpLabel).color = new Color(1f, 0f, 0f, 0f);
            (UIArrayElements[7] as OpLabel).color = new Color(1f, 0f, 0f, 0f);
        }


        // In your Update loop

        // Get field info (you should ideally cache these MethodInfo/FieldInfo objects outside the loop)
        var hasSignalledInfo = typeof(OpHoldButton).GetField("_hasSignalled", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var filledInfo = typeof(OpHoldButton).GetField("_filled", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        var btn = UIArrayElements[5] as OpHoldButton;
        bool hasSignalled = (bool)hasSignalledInfo.GetValue(btn);
        float filled = (float)filledInfo.GetValue(btn);

        if (hasSignalled && !playedSound)
        {
            Menu.Remix.ConfigContainer.PlaySound(SoundID.MENU_Switch_Page_In);
            (UIArrayElements[1] as OpCheckBox).SetValueBool(false);
            (UIArrayElements[3] as OpCheckBox).SetValueBool(false);
            (UIArrayElements[8] as OpSlider).SetValueInt(30);
            playedSound = true;
        }

        if (filled == 0f)
        {
            playedSound = false;
        }
    }

    static public Configurable<bool> furDisabled;
    static public Configurable<bool> alwaysOnHolograms;
    static public Configurable<int> trainLengthSlider;

    UIelement[] UIArrayElements;
    bool playedSound = false;
    float warningFlash = 0f;
}