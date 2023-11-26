using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using Zat.Shared.InterModComm;
using Zat.Shared.ModMenu.API;
using Zat.Shared.ModMenu.Interactive;
using Newtonsoft.Json;


namespace Elevation
{

    public enum ElevationBiasType
    {
        Rounded,
        Min,
        Max
    }

    

    [Mod("Elevation", "v1.0", "Agentfox")]
    public class Settings
    {
        public InteractiveConfiguration<Settings> Config { get; private set; }
        public ModSettingsProxy Proxy { get; private set; }
        public static Settings inst { get; private set; }

        #region Interactive

        //[Category("Coloring")]
        public Coloring c_Coloring { get; private set; }

        //[Category("Visual")]
        public Visual c_Visual { get; private set; }

        [Category("Terrain")]
        public Terrain c_Terrain { get; private set; }

        [Category("Combat")]
        public Combat c_Combat { get; private set; }

        [Category("Camera Controls")]
        public CameraControls c_CameraControls { get; private set; }

        #region OBSOLETE
        public class Coloring
        {

            //[Setting("Copy Colors", "Copies the current color configuration to the clipboard in text form so you can share it with other people!")]
            //[Button("Copy")]
            //public InteractiveButtonSetting s_copyColorsToClipboard { get; private set; }

            //[Setting("Paste Colors", "Updates your color configuration to match the one copied. ")]
            //[Button("Paste")]
            //public InteractiveButtonSetting s_PasteColorsFromClipboard { get; private set; }


            [Setting("Color Preset", "A set of predetremined colors that can be used for elevation coloring")]
            [Select(0,"Default", "Mesa", "Rocky", "Experimental")]
            public InteractiveSelectSetting s_preset { get; private set; }
            public Dictionary<int, UnityEngine.Color> preset
            {
                get
                {
                    foreach(string opt in s_preset.Options)
                        Mod.dLog(opt);
                    Mod.dLog(s_preset.Value);
                    
                    return s_preset.Value >= 0 && s_preset.Value < elevationColorPresets.Keys.Count 
                        ? elevationColorPresets[elevationColorPresets.Keys.ElementAt(s_preset.Value)] 
                        : null;
                }
            }


            [Category("Tiers")]
            public Tiers c_Tiers { get; private set; }

            public class Tiers
            {
                [Setting("1","")]
                [Color(0.662f, 0.854f, 0.564f)]
                public InteractiveColorSetting t_1 { get; private set; }
                public UnityEngine.Color color1
                {
                    get
                    {
                        return Settings.ZatColorToUnity(t_1.Color);
                    }
                    set
                    {
                        t_1.Color = Settings.UnityColorToZat(value);
                    }
                }

                [Setting("2","")]
                [Color(0.709f, 0.807f, 0.533f)]
                public InteractiveColorSetting t_2 { get; private set; }
                public UnityEngine.Color color2
                {
                    get
                    {
                        return Settings.ZatColorToUnity(t_2.Color);
                    }
                    set
                    {
                        t_2.Color = Settings.UnityColorToZat(value);
                    }
                }

                [Setting("3", "")]
                [Color(0.803f, 0.764f, 0.596f)]
                public InteractiveColorSetting t_3 { get; private set; }
                public UnityEngine.Color color3
                {
                    get
                    {
                        return Settings.ZatColorToUnity(t_3.Color);
                    }
                    set
                    {
                        t_3.Color = Settings.UnityColorToZat(value);
                    }
                }

                [Setting("4", "")]
                [Color(0.819f, 0.811f, 0.780f)]
                public InteractiveColorSetting t_4 { get; private set; }
                public UnityEngine.Color color4
                {
                    get
                    {
                        return Settings.ZatColorToUnity(t_4.Color);
                    }
                    set
                    {
                        t_4.Color = Settings.UnityColorToZat(value);
                    }
                }

                [Setting("5", "")]
                [Color(0.647f, 0.639f, 0.611f)]
                public InteractiveColorSetting t_5 { get; private set; }
                public UnityEngine.Color color5
                {
                    get
                    {
                        return Settings.ZatColorToUnity(t_4.Color);
                    }
                    set
                    {
                        t_5.Color = Settings.UnityColorToZat(value);
                    }
                }

                [Setting("6", "")]
                [Color(0.549f, 0.549f, 0.549f)]
                public InteractiveColorSetting t_6 { get; private set; }
                public UnityEngine.Color color6
                {
                    get
                    {
                        return Settings.ZatColorToUnity(t_6.Color);
                    }
                    set
                    {
                        t_6.Color = Settings.UnityColorToZat(value);
                    }
                }

                [Setting("7", "")]
                [Color(0.690f, 0.690f, 0.690f)]
                public InteractiveColorSetting t_7 { get; private set; }
                public UnityEngine.Color color7
                {
                    get
                    {
                        return Settings.ZatColorToUnity(t_7.Color);
                    }
                    set
                    {
                        t_7.Color = Settings.UnityColorToZat(value);
                    }
                }

                [Setting("8", "")]
                [Color(0.866f, 0.886f, 0.854f)]
                public InteractiveColorSetting t_8 { get; private set; }
                public UnityEngine.Color color8
                {
                    get
                    {
                        return Settings.ZatColorToUnity(t_8.Color);
                    }
                    set
                    {
                        t_8.Color = Settings.UnityColorToZat(value);
                    }
                }
            }
        }
        public class Visual
        {

            [Setting("Pathfinding Indicator Enabled", "Whether or not the pathfinding indicator shown while placing buildings is enabled")]
            [Toggle(false,"")]
            public InteractiveToggleSetting s_VisualPathfindingIndicatorEnabled { get; private set; }

        }
        #endregion

        public class Terrain
        {
            [Setting("Regenerate Terrain", "Regenerates elevated terrain; acts as bugfix for (rare cases of) visual lack of elevation")]
            [Button("Regenerate")]
            public InteractiveButtonSetting s_Regenerate { get; private set; }

            //[Setting("Color Bias")]
            //[Slider(0f, 0.8f, 0.2f)]
            public InteractiveSliderSetting s_ColorBias { get; private set; }

            [Category("Generation")]
            public Generation c_Generation { get; private set; }

            [Category("Processing")]
            public Processing c_Processing { get; private set; }


            public class Generation
            {
                [Setting("Generation Bias")]
                [Select(1,"Rounded", "Min", "Max")]
                public InteractiveSelectSetting s_ElevationBiasType { get; private set; }
                public ElevationBiasType ElevationBiasType {
                    get
                    {
                        return (ElevationBiasType)s_ElevationBiasType.Value;
                    }
                    set
                    {
                        s_ElevationBiasType.Value = (int)value;
                    }
                }



                //[Category("Noise")]
                public Noise c_Noise { get; private set; }

                public class Noise
                {
                    //[Setting("Scale", "The scale of the noise used to generate elevation")]
                    //[Slider(0.1f, 100f, 50f, "50", false)]
                    //public InteractiveSliderSetting s_Scale { get; private set; }
                    //public float Scale
                    //{
                    //    get
                    //    {

                    //        return s_Scale.Value;
                    //    }
                    //    set
                    //    {
                    //        s_Scale.Value = value;
                    //        MapGenerator.Scale = value;
                    //    }
                    //}

                    //[Setting("Amplitude", "The amplitude multiplier of the y-values")]
                    //[Slider(0.1f, 2f, 0.7f, "0.7", false)]
                    //public InteractiveSliderSetting s_Amplitue { get; private set; }
                    //public float Amplitude
                    //{
                    //    get
                    //    {
                    //        return s_Amplitue.Value;
                            
                    //    }
                    //    set
                    //    {
                    //        s_Amplitue.Value = value;
                    //        MapGenerator.Amplitude = value;
                    //    }
                    //}

                    //[Setting("Smoothing", "Whether or not to smooth elevated terrain after generation; disabling can provide small performance boosts. ")]
                    //[Toggle(true,"")]
                    //public InteractiveToggleSetting s_Smoothing { get; private set; }
                    //public bool Smoothing 
                    //{
                    //    get
                    //    {
                    //        return s_Smoothing.Value;
                    //    }
                    //    set
                    //    {
                    //        s_Smoothing.Value = value;
                    //        MapGenerator.doSmoothing = value;
                    //    }
                    //}


                }
            }

            public class Processing
            {
                [Setting("Show Pruning on Map", "Show visual progress of unreachable tile pruning on map. ")]
                [Toggle(false)]
                public InteractiveToggleSetting s_showPruningOnMap { get; private set; }
            }
        }

        //public class Buildings
        //{

        //}

        public class Combat
        {
            [Setting("Smart Unit Pathing", "Armies try to take the high ground")]
            [Toggle(true)]
            public InteractiveToggleSetting s_smartUnitPathing { get; private set; }

            [Setting("Elevation Damage Multiplier", "The greater this value, the more damage is inflicted by high-elevation units to low-elevation units. ")]
            [Slider(0f, 1f, 0.25f)]
            public InteractiveSliderSetting s_damageIncrement { get; private set; }

            [Setting("Catapult Speed Decrement", "Catapults move (this number) times the tier of an elevation block slower than at sea level. ")]
            [Slider(0f, 0.1f, 0.05f)]
            public InteractiveSliderSetting s_catapultSpeedDecrement { get; private set; }

            [Setting("Ogre Speed Decrement", "Ogres move (this number) times the tier of an elevation block slower than at sea level. ")]
            [Slider(0f, 0.1f, 0.05f)]
            public InteractiveSliderSetting s_ogreSpeedDecrement { get; private set; }
        }

        public class CameraControls
        {
            [Setting("Activation Key", "The key pressed to activate the top-down view camera. ")]
            [Hotkey(KeyCode.T,false,false,false)]
            public InteractiveHotkeySetting s_activateKey { get; private set; }

            [Setting("Snap", "The snap multiple for the top-down view camera; setting to 0 eliminates all snap. ")]
            [Slider(0f, 5f, 0f, "0", true)]
            public InteractiveSliderSetting s_snap { get; private set; }
            [Setting("Speed", "The maximum speed of the top-down view camera. ")]
            [Slider(0.1f, 2f, 0.5f, "0.5", false)]
            public InteractiveSliderSetting s_speed { get; private set; }
            [Setting("Speed Boost", "The speed boost the camera gains when the camera speed (default of SHIFT) key is pressed. ")]
            [Slider(1f,4f,2f,"2",false)]
            public InteractiveSliderSetting s_shiftSpeed { get; private set; }



        }

        #endregion

        #region Debug

        public static bool debug = false;


        public static KeyCode keycode_refreshTerrain { get; } = KeyCode.Minus;
        public static KeyCode keycode_refreshTile { get; } = KeyCode.Equals;

        public static KeyCode keycode_raise { get; } = KeyCode.R;
        public static KeyCode keycode_lower { get; } = KeyCode.F;

        public static KeyCode keycode_pruneCells { get; } = KeyCode.H;

        public static KeyCode keycode_sampleCell { get; } = KeyCode.G;
        public static KeyCode keycode_directionReference { get; } = KeyCode.B;

        public static KeyCode keycode_updatePathView { get; } = KeyCode.P;

        public static KeyCode keycode_toggleLoadingDialog { get; } = KeyCode.L;

        #endregion

        // TODO: Use GUIUtility.systemCopyBuffer for copy-pasting text from clipboard
        internal static string Clipboard
        {//https://flystone.tistory.com/138
            get
            {
                TextEditor _textEditor = new TextEditor();
                _textEditor.Paste();
                return _textEditor.text;
            }
            set
            {
                TextEditor _textEditor = new TextEditor
                { text = value };

                _textEditor.OnFocus();
                _textEditor.Copy();
            }
        }


        public static KeyCode keycode_topDownView = KeyCode.T;
        public static ElevationBiasType elevationBiasType = ElevationBiasType.Min;

        public static bool useTerrainTexture = true;
        public static Dictionary<string, Dictionary<int,UnityEngine.Color>> elevationColorPresets = new Dictionary<string, Dictionary<int, UnityEngine.Color>>() 
        {
            { "Default", new Dictionary<int, UnityEngine.Color>()
                {
                    {1, new UnityEngine.Color(1f, 1f, 1f) },
                    {2, new UnityEngine.Color(0.95f, 0.95f, 0.95f) },
                    {3, new UnityEngine.Color(0.8f, 0.8f, 0.8f) },
                    {4, new UnityEngine.Color(0.6f, 0.6f, 0.6f) },
                    {5, new UnityEngine.Color(0.5f, 0.5f, 0.5f) },
                    {6, new UnityEngine.Color(0.45f, 0.45f, 0.45f) },
                    {7, new UnityEngine.Color(0.4f, 0.4f, 0.4f) },
                    {8, new UnityEngine.Color(0.35f, 0.35f, 0.35f) }
                }
            },
            { "Mesa", new Dictionary<int, UnityEngine.Color>()
                {
                    {1, new UnityEngine.Color(0.847f, 0.635f, 0.431f) },
                    {2, new UnityEngine.Color(0.819f, 0.525f, 0.239f) },
                    {3, new UnityEngine.Color(0.682f, 0.376f, 0.254f) },
                    {4, new UnityEngine.Color(0.631f, 0.443f, 0.368f) },
                    {5, new UnityEngine.Color(0.588f, 0.588f, 0.588f) },
                    {6, new UnityEngine.Color(0.439f, 0.439f, 0.439f) },
                    {7, new UnityEngine.Color(0.619f, 0.619f, 0.619f) },
                    {8, new UnityEngine.Color(0.749f, 0.749f, 0.682f) }
                } 
            },
            { "Rocky", new Dictionary<int, UnityEngine.Color>()
                {
                    {1, new UnityEngine.Color(0.360784322f, 0.6901961f, 0.13333334f) },
                    {2, new UnityEngine.Color(0.517f, 0.725f, 0.517f) },
                    {3, new UnityEngine.Color(0.552f, 0.552f, 0.572f) },
                    {4, new UnityEngine.Color(0.458f, 0.462f, 0.474f) },
                    {5, new UnityEngine.Color(0.364f, 0.368f, 0.376f) },
                    {6, new UnityEngine.Color(0.290f, 0.313f, 0.309f) },
                    {7, new UnityEngine.Color(0.211f, 0.254f, 0.243f) },
                    {8, new UnityEngine.Color(0.843f, 0.839f, 0.839f) }
                }
            },
            { "Experimental", new Dictionary<int, UnityEngine.Color>()
                {
                    {1, new UnityEngine.Color(0.360784322f, 0.6901961f, 0.13333334f) },
                    {2, new UnityEngine.Color(0.517f, 0.725f, 0.517f) },
                    {3, new UnityEngine.Color(0.552f, 0.552f, 0.572f) },
                    {4, new UnityEngine.Color(0.458f, 0.462f, 0.474f) },
                    {5, new UnityEngine.Color(0.364f, 0.368f, 0.376f) },
                    {6, new UnityEngine.Color(0.290f, 0.313f, 0.309f) },
                    {7, new UnityEngine.Color(0.211f, 0.254f, 0.243f) },
                    {8, new UnityEngine.Color(0.565f, 0.573f, 0.569f) }
                }
            }
        };

        public static float topDownViewCamSnap = 1f;

        public static bool showMapProcessing = false;


        #region Base

        public static void Init()
        {
            var config = new InteractiveConfiguration<Settings>();
            Settings.inst = config.Settings;
            Settings.inst.Config = config;

            AddListeners();

            ModSettingsBootstrapper.Register(config.ModConfig,
                (proxy, oldSettings) =>
                {
                    OnModRegistered(proxy, oldSettings);
                },
                (ex) => {
                    OnModRegistrationFailed(ex);
                });
        }

        private static void AddListeners()
        {
            // Generator
            Settings.inst.c_Terrain.s_Regenerate.OnButtonPressed.AddListener(OnRegenerateButtonClicked);

            //Settings.inst.c_Generator.c_Advanced.c_Noise.s_Amplitue.OnUpdate.AddListener(UpdateSlider);
            //Settings.inst.c_Generator.c_Advanced.c_Noise.s_Scale.OnUpdate.AddListener(UpdateSlider);
            
            Settings.inst.c_Terrain.c_Processing.s_showPruningOnMap.OnUpdate.AddListener((entry) => { showMapProcessing = entry.toggle.value; });

            //// Coloring
            //Settings.inst.c_Terrain.s_ColorBias.OnUpdatedRemotely.AddListener((entry) =>
            //{
            //    ColorManager.coloringBias = entry.slider.value;

            //    UpdateSlider(entry);
            //});

            //Settings.inst.c_Coloring.s_copyColorsToClipboard.OnButtonPressed.AddListener(CopyColorsToClipboard);
            //Settings.inst.c_Coloring.s_PasteColorsFromClipboard.OnButtonPressed.AddListener(PasteColorsFromClipboard);


            //Settings.inst.c_Coloring.s_preset.OnUpdatedRemotely.AddListener(OnColorPresetChanged);

            //Settings.inst.c_Coloring.c_Tiers.t_1.OnUpdatedRemotely.AddListener(OnColorChanged);
            //Settings.inst.c_Coloring.c_Tiers.t_2.OnUpdatedRemotely.AddListener(OnColorChanged);
            //Settings.inst.c_Coloring.c_Tiers.t_3.OnUpdatedRemotely.AddListener(OnColorChanged);
            //Settings.inst.c_Coloring.c_Tiers.t_4.OnUpdatedRemotely.AddListener(OnColorChanged);
            //Settings.inst.c_Coloring.c_Tiers.t_5.OnUpdatedRemotely.AddListener(OnColorChanged);
            //Settings.inst.c_Coloring.c_Tiers.t_6.OnUpdatedRemotely.AddListener(OnColorChanged);
            //Settings.inst.c_Coloring.c_Tiers.t_7.OnUpdatedRemotely.AddListener(OnColorChanged);
            //Settings.inst.c_Coloring.c_Tiers.t_8.OnUpdatedRemotely.AddListener(OnColorChanged);

            // Combat
            Settings.inst.c_Combat.s_smartUnitPathing.OnUpdatedRemotely.AddListener((entry) =>
            {
                Elevation.Combat.smartUnitPathing = entry.toggle.value;

                Update(entry);
                ElevationManager.RefreshTerrain();
            });
            Settings.inst.c_Combat.s_damageIncrement.OnUpdatedRemotely.AddListener((entry) => 
            { 
                Elevation.Combat.elevationDamageIncrement = entry.slider.value;

                UpdateSlider(entry);
            });
            Settings.inst.c_Combat.s_catapultSpeedDecrement.OnUpdatedRemotely.AddListener((entry) => 
            { 
                Elevation.Combat.catapultSpeedDecrement = entry.slider.value;

                UpdateSlider(entry);
            });
            Settings.inst.c_Combat.s_ogreSpeedDecrement.OnUpdatedRemotely.AddListener((entry) => 
            { 
                Elevation.Combat.ogreSpeedDecrement = entry.slider.value;

                UpdateSlider(entry);
            });

            // Camera Controls
            Settings.inst.c_CameraControls.s_shiftSpeed.OnUpdatedRemotely.AddListener(UpdateSlider);
            Settings.inst.c_CameraControls.s_snap.OnUpdatedRemotely.AddListener(UpdateSlider);
            Settings.inst.c_CameraControls.s_speed.OnUpdatedRemotely.AddListener(UpdateSlider);
        }

        #endregion

        #region Listener Callbacks

        private static void OnRegenerateButtonClicked()
        {
            if (GameState.inst.CurrMode != GameState.inst.playingMode) 
            {
                Grid.Setup();
                MapGenerator.Generate();
                ElevationManager.RefreshTerrain();
            }
            else
            {
                ElevationManager.RefreshTerrain(true);
            }
        }

        private static void OnColorPresetChanged(SettingsEntry entry)
        {
            Dictionary<int, UnityEngine.Color> preset = inst.c_Coloring.preset;
            foreach(int tier in preset.Keys)
            {
                Settings.inst.c_Coloring.c_Tiers.GetType().GetProperty("color" + tier.ToString()).SetValue(Settings.inst.c_Coloring.c_Tiers, preset[tier]);
                ColorManager.SetColor(tier, preset[tier]);
            }

            ColorManager.Update();
        }


        private static void OnColorChanged(SettingsEntry entry)
        {
            int tier = int.Parse(entry.GetName());
            ColorManager.SetColor(tier, ZatColorToUnity(entry.color));

            ColorManager.Update();
        }

        public static void Update(SettingsEntry entry)
        {
            Settings.inst.Proxy.UpdateSetting(entry, OnSuccesfulSettingUpdate, OnUnsuccesfulSettingUpdate);
        }


        private static void UpdateSlider(SettingsEntry entry)
        {
            entry.slider.label = Utils.Util.RoundToFactor(entry.slider.value, 0.01f).ToString();
            Update(entry);
        }

        private static void CopyColorsToClipboard()
        {
            string json = "{";
            json += JsonConvert.SerializeObject(Settings.inst.c_Coloring.c_Tiers.color1);
            json += "}";

            Clipboard = "sample text";
        }


        private static void PasteColorsFromClipboard()
        {
            string json = Clipboard;

            DebugExt.dLog(json);
        }

        #endregion

        #region Handling

        private static void OnModRegistered(ModSettingsProxy proxy, SettingsEntry[] oldSettings)
        {
            Settings.inst.Proxy = proxy;
            Settings.inst.Config.Install(proxy, oldSettings);
            Mod.helper.Log("Mod registration to ModMenu Succesful");
        }

        private static void OnModRegistrationFailed(Exception ex)
        {
            Mod.helper.Log("Mod registration to ModMenu failed");
            DebugExt.HandleException(ex);
        }

        private static void OnSuccesfulSettingUpdate()
        {
            Mod.dLog("Setting Update Successful");
        }

        private static void OnUnsuccesfulSettingUpdate(Exception ex)
        {
            Mod.Log("Setting Update Unsuccsesful");
            DebugExt.HandleException(ex);
        }


        #endregion

        #region Utils

        private static UnityEngine.Color ZatColorToUnity(Zat.Shared.ModMenu.API.Color color)
        {
            return new UnityEngine.Color() 
            {
                r = color.r,
                g = color.g,
                b = color.b,
                a = color.a
            };
        }

        private static Zat.Shared.ModMenu.API.Color UnityColorToZat(UnityEngine.Color color)
        {
            return new Zat.Shared.ModMenu.API.Color()
            {
                r = color.r,
                g = color.g,
                b = color.b,
                a = color.a
            };
        }


        #endregion

    }
}
