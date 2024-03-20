namespace Slugpack
{
    /*static class ScreenAnimation
    {
        public static ConditionalWeakTable<RoomCamera, CameraInfo> CamInfo = new();

        public class CameraInfo
        {
            public bool loaded = false;
            public bool reset = false;

            public int timer = 0;

            public int cameraNumber = -1;

            //public Dictionary<string, Texture2D> screens = new Dictionary<string, Texture2D>();

            public Dictionary<string, WWW> asyncLoads = new();

            public Dictionary<string, byte[]> imageData = new();
        }

        internal static void Apply()
        {
            On.RoomCamera.Update += (orig, self) =>
            {
                // GOAL:
                // A method that automatically checks files in a "screens" folder and reads txts/images to determine when and how to change the screen
                // Optimally it would also be able to accept specific slugcats it would work for, allow certain effects, etc

                orig(self);

                var caminfo = CamInfo.GetOrCreateValue(self);

                caminfo.timer++;

                //Debug.Log(self.game.world.region.name);

                var screenFolders = FindScreensFolders("screens");

                var screensInfo = new List<Dictionary<string, List<string>>>();
                foreach (var folderPath in screenFolders)
                {
                    screensInfo.Add(GetImageFiles(AssetManager.ResolveFilePath(folderPath)));
                }

                if (!caminfo.loaded)
                {
                    for (int a = 0; a < screensInfo.Count; a++)
                    {
                        foreach (string b in screensInfo[a].Keys)
                        {
                            for (int c = 0; c < screensInfo[a][b].Count; c++)
                            {
                                string key = screensInfo[a][b][c];
                                if (caminfo.asyncLoads.ContainsKey(key) && caminfo.asyncLoads[key].isDone)
                                {
                                    caminfo.imageData.Add(key, caminfo.asyncLoads[key].texture.GetRawTextureData());
                                    
                                    caminfo.asyncLoads[key].Dispose();
                                    caminfo.asyncLoads[key] = null;

                                    caminfo.asyncLoads.Remove(key);

                                    if (caminfo.asyncLoads.Count == 0)
                                    {
                                        caminfo.loaded = true;
                                        caminfo.reset = true;
                                    }
                                }
                                else
                                {
                                    string text = AssetManager.ResolveFilePath($"screens/{key}.png");

                                    if (File.Exists(text) && self.quenedTexture != text)
                                    {
                                        WWW www = new(text);

                                        caminfo.asyncLoads.Add(key, www);
                                    }
                                }
                            }
                        }
                    }
                }

                string roomCamName = $"{self.room.abstractRoom.name}_{caminfo.cameraNumber + 1}";

                for (int a = 0; a < screensInfo.Count; a++)
                {
                    foreach (string c in screensInfo[a].Keys)
                    {
                        string b = screensInfo[a][c].First();

                        if (b.EndsWith("_") && roomCamName == string.Join("_", b.Split('_').Take(3)))
                        {
                            string key = $"{roomCamName}_{caminfo.timer % int.Parse(b.Split('_')[4])}";

                            if (caminfo.imageData.ContainsKey(key))
                            {
                                self.levelTexture.LoadRawTextureData(caminfo.imageData[key]);

                                self.levelTexture.Apply();
                            }
                            else
                            {
                                string text = AssetManager.ResolveFilePath($"screens/{key}.png");

                                if (File.Exists(text))
                                {
                                    caminfo.loaded = false;
                                    caminfo.imageData.Clear();
                                }
                            }

                            if (b.Split('_')[4] == "0" && caminfo.reset)
                            {
                                caminfo.reset = false;
                                caminfo.loaded = false;
                                caminfo.imageData.Clear();
                            }
                        }
                    }
                }
            };

            On.RoomCamera.MoveCamera2 += (orig, self, roomName, camPos) =>
            {
                orig(self, roomName, camPos);

                var caminfo = CamInfo.GetOrCreateValue(self);

                caminfo.cameraNumber = camPos;
            };
        }

        public static Dictionary<string, List<string>> GetImageFiles(string directoryPath)
        {
            var imageFiles = Directory.GetFiles(directoryPath, "*.png")
                                     .Select(Path.GetFileNameWithoutExtension)
                                     .ToList();

            var dictionary = new Dictionary<string, List<string>>();

            foreach (var file in imageFiles)
            {
                var key = file.Substring(0, file.LastIndexOf('_'));
                if (!dictionary.ContainsKey(key))
                {
                    dictionary[key] = new List<string>();
                }
                dictionary[key].Add(file);
            }

            return dictionary;
        }

        public static List<string> FindScreensFolders(string screensDirectory)
        {
            List<string> screensFolders = new();
            List<string> modFolders = new()
            {
                Path.Combine(RWCustom.Custom.RootFolderDirectory(), "mergedmods")
            };

            for (int i = 0; i < ModManager.ActiveMods.Count; i++)
            {
                modFolders.Add(ModManager.ActiveMods[i].path);
            }
            modFolders.Add(RWCustom.Custom.RootFolderDirectory());

            foreach (string modFolder in modFolders)
            {
                string screensFolder = Path.Combine(modFolder, screensDirectory.ToLowerInvariant());
                if (screensFolder.EndsWith("screens") && Directory.Exists(screensFolder))
                {
                    screensFolders.Add(screensFolder);
                }
            }
            return screensFolders;
        }

    }*/
}