/*namespace Slugpack;

[BepInPlugin("AwriLynn.Remix_Menu_Template_examples", "Remix Menu Template examples", "1.2")]
public class Remix_Menu_Template_examples_by_Awri_Lynn : BaseUnityPlugin // I personally prefer snake_case instead of CamelCase, but in C# camelCase is used. I've been told to rename this entire source file to camelCase and it took a whole hour xD
{
    private OptionsMenu optionsMenuInstance;
    private bool initialized;
    public void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (this.initialized)
        {
            return;
        }
        this.initialized = true;

        //Loading custom assets put png in folder next to mod  and import with line: Futile.atlasManager.LoadImage("atlases/your_png");
        Futile.atlasManager.LoadImage("atlases/Awri_Lynn/Awri_BG");
        Futile.atlasManager.LoadImage("atlases/Awri_Lynn/Awri_Hat_Badge_MadeByPeskm_112");
        Futile.atlasManager.LoadImage("atlases/Awri_Lynn/Awri_real_magic_stuff_112");
        Futile.atlasManager.LoadImage("atlases/Awri_Lynn/AwriBlush_112");
        Futile.atlasManager.LoadImage("atlases/Awri_Lynn/AwriHappy_112");
        optionsMenuInstance = new OptionsMenu(this);
        try
        {
            MachineConnector.SetRegisteredOI("AwriLynn.Remix_Menu_Template_examples", optionsMenuInstance);
        }
        catch (Exception ex)
        {
            Debug.Log($"Remix Menu Template examples: Hook_OnModsInit options failed init error {optionsMenuInstance}{ex}");
            Logger.LogError(ex);
            Logger.LogMessage("WHOOPS");
        }
    }
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }
}
public class OptionsMenu : OptionInterface
{
    public OptionsMenu(Plugin plugin)
    {
        testCheckBox = this.config.Bind<bool>("YourModNameHere_Bool_Checkbox", true); // All of these are where the game saves your settings, the important part is the "Key" field, make sure this will never conflict with another mod's key by having a prefix, like your mod name
        testRadio = this.config.Bind<int>("YourModNameHere_Int_RadioButton", 0);
        testSlider = this.config.Bind<int>("YourModNameHere_Int_Slider", 30);
        testFloatSlider = this.config.Bind<float>("YourModNameHere_Float_Slider", 720f);
        testComboBox = this.config.Bind<string>("YourModNameHere_String_ComboBox", "ComboBox");
        testListBox = this.config.Bind<string>("YourModNameHere_String_ListBox", "Test Listbox");
        testKeyCode = this.config.Bind<KeyCode>("YourModNameHere_KeyCode_Keybind", new KeyCode());

        testDragger = this.config.Bind<int>(null, 7); // Keys that are null or starting with an underscore are considered cosmetic and will not save
        testSliderTick = this.config.Bind<int>(null, 7);
        testUpDown = this.config.Bind<float>(null, 5.4f);
        testColorPicker = this.config.Bind<Color>(null, Color.red);
        testResourceSelectorRegions = this.config.Bind<string>(null, "Resource Selector Regions");
        testResourceSelectorDecals = this.config.Bind<string>(null, "Resource Selector Decals");
        testResourceSelectorIllustrations = this.config.Bind<string>(null, "Resource Selector Illustrations");
        testResourceSelectorPalettes = this.config.Bind<string>(null, "Resource Selector Palettes");
        testResourceSelectorShaders = this.config.Bind<string>(null, "Resource Selector Shaders");
        testResourceSelectorSlugcatNames = this.config.Bind<string>(null, "Resource Selector SlugcatNames");
        testResourceListRegions = this.config.Bind<string>(null, "Resource List Shaders");
    }
    public override void Initialize()
    {
        var opTab1 = new OpTab(this, "Default Canvas");
        var opTab2 = new OpTab(this, "Tab 2 :D");
        this.Tabs = new[] { opTab1, opTab2 }; // Add the tabs into your list of tabs. If there is only a single tab, it will not show the flap on the side because there is not need to.

        // Tab 1
        OpContainer tab1Container = new OpContainer(new Vector2(0, 0));
        opTab1.AddItems(tab1Container);
        for (int i = 0; i <= 600; i += 10) // Line grid to help align things, don't leave this in your final code. Almost every element starts from bottom-left.
        {
            Color c;
            c = Color.grey;
            if (i % 50 == 0) { c = Color.yellow; }
            if (i % 100 == 0) { c = Color.red; }
            FSprite lineSprite = new FSprite("pixel");
            lineSprite.color = c;
            lineSprite.alpha = 0.2f;
            lineSprite.SetAnchor(new Vector2(0.5f, 0f));
            Vector2 a = new Vector2(i, 0);
            lineSprite.SetPosition(a);
            Vector2 b = new Vector2(i, 600);
            float rot = Custom.VecToDeg(Custom.DirVec(a, b));
            lineSprite.rotation = rot;
            lineSprite.scaleX = 2f;
            lineSprite.scaleY = Custom.Dist(a, b);
            tab1Container.container.AddChild(lineSprite);
            a = new Vector2(0, i);
            b = new Vector2(600, i);
            lineSprite = new FSprite("pixel");
            lineSprite.color = c;
            lineSprite.alpha = 0.2f;
            lineSprite.SetAnchor(new Vector2(0.5f, 0f));
            lineSprite.SetPosition(a);
            rot = Custom.VecToDeg(Custom.DirVec(a, b));
            lineSprite.rotation = rot;
            lineSprite.scaleX = 2f;
            lineSprite.scaleY = Custom.Dist(a, b);
            tab1Container.container.AddChild(lineSprite);
        }

        // You can put sprites with effects in the Remix Menu by using an OpContainer
        sprite1 = new FSprite("Futile_White") { x = 500, y = 350, width = 100, height = 100 };
        sprite2 = new FSprite("Futile_White") { x = 500, y = 250, width = 100, height = 100 };
        sprite1.shader = Custom.rainWorld.Shaders["SmokeTrail"];
        sprite2.shader = Custom.rainWorld.Shaders["GhostDistortion"];
        tab1Container.container.AddChild(sprite1);
        tab1Container.container.AddChild(sprite2);

        int sizeTest = 100;
        UIelement[] UIArrayElements = new UIelement[] // Labels in a fixed box size + alignment
        {
                new OpLabelLong(new Vector2(0,sizeTest), new Vector2(150, 150),"OpLabels down there with set size allows for better alignment positioning but the size itself is ignored, in this case use OpLabelLong for auto-wrap and newline \n with \\n", true, FLabelAlignment.Left){verticalAlignment = OpLabel.LabelVAlignment.Bottom },
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Left",FLabelAlignment.Left),
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Center",FLabelAlignment.Center),
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Right",FLabelAlignment.Right),
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Top L",FLabelAlignment.Left){verticalAlignment = OpLabel.LabelVAlignment.Top },
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Top C",FLabelAlignment.Center){verticalAlignment = OpLabel.LabelVAlignment.Top },
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Top R",FLabelAlignment.Right){verticalAlignment = OpLabel.LabelVAlignment.Top },
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Bot L",FLabelAlignment.Left){verticalAlignment = OpLabel.LabelVAlignment.Bottom },
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Bot C",FLabelAlignment.Center){verticalAlignment = OpLabel.LabelVAlignment.Bottom },
                new OpLabel(new Vector2(0,0), new Vector2(sizeTest, sizeTest),"Bot R",FLabelAlignment.Right){verticalAlignment = OpLabel.LabelVAlignment.Bottom },
        };
        opTab1.AddItems(UIArrayElements);

        OpRadioButtonGroup testRadioGroup = new OpRadioButtonGroup(testRadio); // Radio buttons needs to be created through a Radio button group which must be added to the canvas before adding its buttons.
        opTab1.AddItems(testRadioGroup);
        testRadioGroup.SetButtons(new OpRadioButton[]
        {
                new OpRadioButton(50, 450){description = "test radio button 1, there can only be one"},
                new OpRadioButton(100, 450){description = "test radio button 2, there can only be one"}
        });

        UIelement[] UIArrayElements2 = new UIelement[] //create an array of ui elements
        {
                new OpLabel(0f, 550f, "Awri Lynn's Remix Menu Template Example", true),
                new OpCheckBox(testCheckBox, 50, 500), // Try to make your boolean toggles as "positive is true" simple wording, I accidentally write double negative long sentences a lot for my boolean names.
                new OpLabel(80, 500, "Text 30 pixels in front of checkbox X position"),
                new OpLabel(130, 450, "Radio buttons"), // Radio buttons needs to be created through a Radio button group which must be added to the canvas before adding its buttons, they are not here, this is just a label next to them.
                new OpSlider(testSlider, new Vector2(50, 400), 100){max = 100, hideLabel = false}, // Using "hideLabel = true" makes the number disappear but the shadow of where the number would be still appears, why lol.
                new OpLabel(160, 400, "OpSlider"),
                new OpRect(new Vector2(50,350),new Vector2(50,50), 0.3f){doesBump = true, description = "Rect shouldn't bump, don't enable this, this is wrong, ew no"}, // doesBump gives the feeling it's clickable but it's not, why is this even an option xD lmao. OpRect is one of my favourites to make the page look nice, just make sure doesBump isn't true.
                new OpLabel(110, 350, "OpRect"),
                new OpDragger(testDragger,50,260){description = "Hold and drag up-down, ranges from 0 to 99 only ???"}, // Cannot set min and max value??, defaults to range from 0 to 99, I checked on dnSpy the constructor seemed to set range from max but I couldn't figure out, it's weird // I recommend to avoid using this
                new OpLabel(80, 260, "OpDragger"),
                new OpSliderTick(testSliderTick, new Vector2(200,200),100){max = 15, hideLabel = false, description = "Slider Tick nobs graphic is broken"}, // Slider Tick Max only up to 31 according to the code inside but it's only checked on creation but you can't set max on creation, the nobs seems to be based on max on creation but you can't set max on creation, which seems to break the graphic as it has a fixed 15 nobs. // I recommend to avoid using this, use normal slider instead
                new OpLabel(310, 200, "OpSliderTick"),
                new OpUpdown(testUpDown, new Vector2(50,230), 50), // Float version seems to cap range from -10.0 to 100.0 with decimal at 1, but holding the arrow break the value way above past that as I got 1111.0 same with negative, the arrow refuses to raise-lower until within the range again, weird // Also with decimal at 0 it just goes up infinitely and loops back down somehow, it looks that it goes back to 100 if reaching 1000 // The int version kept crashing the whole UI, I recommend avoid using this
                new OpLabel(110, 230, "OpUpdown"),
                new OpComboBox(testComboBox, new Vector2(50,300), 100, new List<ListItem>{ new ListItem("List Item A", 42), new ListItem("List Item B", 69), new ListItem("List Item C", 420)}), // The constructor with the "Display" strings causes a crash for some reason, only this constructor works probably
                new OpLabel(0, 320, "OpComboBox (Drop-down List)"),
                new OpColorPicker(testColorPicker, new Vector2(450,450)),
                new OpImage(new Vector2(200,350), "Futile_White"){alpha = 1f, color = Color.white, anchor = new Vector2(0,0)}, // OpImage cannot be resized nor rotated, if sprite manipulation is desired use OpContainer instead and manage your own FSprites, but this one is quicker to setup!
                new OpSimpleButton(new Vector2(200,300),new Vector2(20,20),"Simple Button"), // Needs to be created before, outside of this array and link callbacks to its actions 
                new OpHoldButton(new Vector2(350,450),50, "Display Text", 80f), // Needs to be created before, outside of this array and link callbacks to its actions
                new OpKeyBinder(testKeyCode, new Vector2(200,250),new Vector2(20,20)),
                new OpLabel(250, 252, "Key Binder, click and press a key"),
                new OpLabel(150, 150, "OpResourceSelector picks any loaded resource in the game by category"),
                new OpLabel(310, 0, "Every Slugcat Name"),
                new OpResourceSelector(testResourceSelectorSlugcatNames, new Vector2(150, 0), 150, (OpResourceSelector.SpecialEnum)6),
                new OpLabel(310, 25, "Every Shader"),
                new OpResourceSelector(testResourceSelectorShaders, new Vector2(150, 25), 150, (OpResourceSelector.SpecialEnum)5),
                new OpLabel(310, 50, "Every Palette"),
                new OpResourceSelector(testResourceSelectorPalettes, new Vector2(150, 50), 150, (OpResourceSelector.SpecialEnum)4),
                new OpLabel(310, 75, "Every Illustration"),
                new OpResourceSelector(testResourceSelectorIllustrations, new Vector2(150, 75), 150, (OpResourceSelector.SpecialEnum)3),
                new OpLabel(310, 100, "Every Decal"),
                new OpResourceSelector(testResourceSelectorDecals, new Vector2(150, 100), 150, (OpResourceSelector.SpecialEnum)2),
                new OpLabel(310, 125, "Every Region"),
                new OpResourceSelector(testResourceSelectorRegions, new Vector2(150, 125), 150, (OpResourceSelector.SpecialEnum)1),
                new OpLabel(410, 125, "OpResourceList, same but as list,"),
                new OpLabel(460, 100, "but it feels stuck open lol"),
                new OpResourceList(testResourceListRegions, new Vector2(450,0), 100, (OpResourceSelector.SpecialEnum)1, 3),
                new OpListBox(testListBox, new Vector2(300,300), 100, new List<ListItem>{new ListItem("ListBox 1"),new ListItem("ListBox 2"),new ListItem("ListBox 3"),new ListItem("ListBox 4"),new ListItem("ListBox 5") }),

            // Not adding a OpScrollBox, it's just a canvas to add more elements in less space, basically a pocket tab
            // Also not adding an OpSimpleImageButton as it's just a simple button with an OpImage stuck to it, has problems of both at once lmao
        };
        opTab1.AddItems(UIArrayElements2);

        OpContainer containerTab2 = new OpContainer(new Vector2(0, 0));
        opTab2.AddItems(containerTab2);
        FSprite AwriBG = new FSprite("atlases/Awri_Lynn/Awri_BG") { x = 300, y = 300, width = 600, height = 600 };
        FSprite AwriRainbow = new FSprite("atlases/Awri_Lynn/Awri_Hat_Badge_MadeByPeskm_112") { x = 200, y = 456, width = 112, height = 112 };
        FSprite AwriMagic = new FSprite("atlases/Awri_Lynn/Awri_real_magic_stuff_112") { x = 100, y = 500, width = 112, height = 112 };
        AwriBlush = new FSprite("atlases/Awri_Lynn/AwriBlush_112") { x = 400, y = 344, width = 112, height = 112 };
        FSprite AwriHappy = new FSprite("atlases/Awri_Lynn/AwriHappy_112") { x = 200, y = 344, width = 112, height = 112 };
        containerTab2.container.AddChild(AwriBG);
        containerTab2.container.AddChild(AwriRainbow);
        containerTab2.container.AddChild(AwriMagic);
        containerTab2.container.AddChild(AwriBlush);
        containerTab2.container.AddChild(AwriHappy);
        UIArrayElements2 = new UIelement[]
        {
                    new OpLabel(0f, 550f, "Awri Lynn's Remix Menu Template Example", true),
                    new OpFloatSlider(testFloatSlider, new Vector2(85, 245), 200, 2, true){max = 1000, min = 100, hideLabel = false},
                    new OpLabelLong(new Vector2(25, 185), new Vector2(200,50), "Awri Lynn's awesomeness level\n and coolness factor slider", true),
                    new OpLabelLong(new Vector2(100,0),new Vector2(400,100), "Remix Menu Template examples is entirely just a menu mod to help modders make their own Remix menu, using dnSpy to look into the mod's code or downloading it from the wiki / Community.\n Try out the menu widgets then take the parts that you want from the code knowing that they will work", true),
        };
        opTab2.AddItems(UIArrayElements2);
    }
    public override void Update()
    {
        base.Update();
        sprite1.rotation++;
        sprite2.rotation++;
        numberGoUp++;
        AwriBlush.rotation = Mathf.Sin(numberGoUp / 10) * 15;
    }

    // Configurable values. They are bound to the config in constructor, and then passed to UI elements.
    // They will contain values set in the menu. And to fetch them in your code use their NAME.Value. For example to get the boolean testCheckBox.Value, to get the integer testSlider.Value
    //public readonly Configurable<TYPE> NAME;        
    public readonly Configurable<bool> testCheckBox;
    public readonly Configurable<int> testRadio;
    public readonly Configurable<int> testSlider;
    public readonly Configurable<float> testFloatSlider;
    public readonly Configurable<string> testComboBox;
    public readonly Configurable<int> testDragger;
    public readonly Configurable<int> testSliderTick;
    public readonly Configurable<float> testUpDown;
    public readonly Configurable<Color> testColorPicker;
    public readonly Configurable<KeyCode> testKeyCode;
    public readonly Configurable<string> testResourceSelectorRegions;
    public readonly Configurable<string> testResourceSelectorDecals;
    public readonly Configurable<string> testResourceSelectorIllustrations;
    public readonly Configurable<string> testResourceSelectorPalettes;
    public readonly Configurable<string> testResourceSelectorShaders;
    public readonly Configurable<string> testResourceSelectorSlugcatNames;
    public readonly Configurable<string> testResourceListRegions;
    public readonly Configurable<string> testListBox;
    // Not these ones tho
    float numberGoUp = 0;
    FSprite AwriBlush;
    private FSprite sprite1 = new FSprite("Futile_White");
    private FSprite sprite2 = new FSprite("Futile_White");
}
*/