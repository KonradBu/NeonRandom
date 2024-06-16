using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MelonLoader;
using UniverseLib.Input;
using System.Reflection;
using HarmonyLib;


namespace NeonRandom
{

    public class NeonRandom : MelonMod
    {
        #region EntryDefinitions
        public static MelonPreferences_Category Config_NeonRandom { get; private set; }
        public static MelonPreferences_Entry<int> Setting_NeonRandom_Time;
        public static MelonPreferences_Entry<bool> Setting_NeonRandom_Enabled;
        public static MelonPreferences_Entry<bool> Setting_NeonRandom_WarningEnabled;
        public static MelonPreferences_Entry<KeyCode> Setting_NeonRandom_NewLevel;

        #endregion

        // All the Levels as IDs
        public static String[] levels = { "FLOATING", "GRID_1GUN", "GRID_ANTFARM", "GRID_APARTMENT", "GRID_ARCS", "GRID_ARRIVAL", "GRID_BALLOONLAIR", "GRID_BARRAGE", "GRID_BOLT", "GRID_BOMBS_AHOY", "GRID_BOOM", "GRID_BOOP", "GRID_BOSS_GODSDEATHTEMPLE", "GRID_BOSS_RAPTURE", "GRID_BOSS_YELLOW", "GRID_CANNONS", "GRID_CEILING", "GRID_CHARGE", "GRID_CLIMBANG", "GRID_CLOSER", "GRID_CRASHLAND", "GRID_CRUISE", "GRID_DASHDANCE", "GRID_DESCEND", "GRID_DESTRUCTION", "GRID_DRAGON2", "GRID_ESCALATE", "GRID_EXTERMINATOR", "GRID_FALLING", "GRID_FAST_BALLOON", "GRID_FEVER", "GRID_FIRECRACKER_2", "GRID_FLOCK", "GRID_FORTRESS", "GRID_GLASSPATH", "GRID_GLASSPATH2", "GRID_GLASSPATH3", "GRID_GODTEMPLE_ENTRY", "GRID_HEAT", "GRID_HECK", "GRID_HELLVATOR", "GRID_HIKE", "GRID_HOPHOP", "GRID_HOPSCOTCH", "GRID_HUNT", "GRID_JUMPDASH", "GRID_MEATY_BALLOONS", "GRID_MIMICFINALE", "GRID_MIMICPOP", "GRID_MINEFIELD", "GRID_MOUNTAIN", "GRID_PAGODA", "GRID_PON", "GRID_PORT", "GRID_PREPARE", "GRID_RACE", "GRID_RINGER_EXPLORATION", "GRID_RINGER_TUTORIAL", "GRID_ROCKETUZI", "GRID_SHIELD", "GRID_SKIP", "GRID_SKIPSLIDE", "GRID_SMACKDOWN", "GRID_SNAKE_IN_MY_BOOT", "GRID_SPIDERCLAUS", "GRID_SPIDERMAN", "GRID_SPRINT", "GRID_STAMPEROUT", "GRID_SUPERKINETIC", "GRID_SWARM", "GRID_SWITCH", "GRID_TANGLED", "GRID_TRAPS2", "GRID_TRIPMAZE", "GRID_TRIPRAP", "GRID_TUT_BALLOON", "GRID_VERTICAL", "GRID_ZIPRAP", "SIDEQUEST_ALL_SEEING_EYE", "SIDEQUEST_ARENASIXNINE", "SIDEQUEST_ATTITUDE_ADJUSTMENT", "SIDEQUEST_BARREL_CLIMB", "SIDEQUEST_DODGER", "SIDEQUEST_FISHERMAN_SUPLEX", "SIDEQUEST_OBSTACLE_MACHINEGUN", "SIDEQUEST_OBSTACLE_PISTOL", "SIDEQUEST_OBSTACLE_PISTOL_SHOOT", "SIDEQUEST_OBSTACLE_RIFLE_2", "SIDEQUEST_OBSTACLE_ROCKETLAUNCHER", "SIDEQUEST_OBSTACLE_SHOTGUN", "SIDEQUEST_OBSTACLE_UZI2", "SIDEQUEST_RAPTURE_QUEST", "SIDEQUEST_RESIDENTSAW", "SIDEQUEST_RESIDENTSAWB", "SIDEQUEST_ROCKETGODZ", "SIDEQUEST_STF", "SIDEQUEST_SUNSET_FLIP_POWERBOMB", "SLUGGER", "TUT_BOMB", "TUT_BOMB2", "TUT_BOMBJUMP", "TUT_DASHENEMY", "TUT_FASTTRACK", "TUT_FORCEFIELD2", "TUT_FROG", "TUT_GUARDIAN", "TUT_JUMP", "TUT_JUMPER", "TUT_MIMIC", "TUT_MOVEMENT", "TUT_ORIGIN", "TUT_RIFLE", "TUT_RIFLEJOCK", "TUT_ROCKETJUMP", "TUT_SHOCKER", "TUT_SHOCKER2", "TUT_SHOOTINGRANGE", "TUT_TRIPWIRE", "TUT_UZI", "TUT_ZIPLINE" };
        public static Game Game { get; private set; }
        public static GameObject ModObject { get; private set; }

        public static bool timerfinished = false;
        public static bool lasttry = false;

        [Obsolete]
        public override void OnApplicationStart()
        {
            //Settings
            Config_NeonRandom = MelonPreferences.CreateCategory("NeonRandom Settings");
            Setting_NeonRandom_Enabled = Config_NeonRandom.CreateEntry("NeonRandom Enabled", true, description: "Enables and disables NeonRandom (Quit out of levle to start the timer)");
            Setting_NeonRandom_NewLevel = Config_NeonRandom.CreateEntry("New level manual keybind", KeyCode.Z , description: "Press this key, to randomly select and load a new level while in the level loading screen");
            Setting_NeonRandom_Time = Config_NeonRandom.CreateEntry("Time to load new level", 300, description: "Time in seconds, after which the mod will load a new random level");
            Setting_NeonRandom_WarningEnabled = Config_NeonRandom.CreateEntry("Warning Enabled", true, description: "During your last try a warning will be shown that after this try the level will change to a new one. Press the manual random level key while during the last try, to prevent it from loading a new level and instead restart the timer.");
        }

        
        public override void OnApplicationLateStart()
        {
            
            Game = Singleton<Game>.Instance;
            Game.OnLevelLoadComplete += OnLevelLoadComplete;
            LastTryWarning lastTryWarning = new LastTryWarning();
            lastTryWarning.Initialize();
            ModObject = new GameObject("Neon Random");
            UnityEngine.Object.DontDestroyOnLoad(ModObject);
            
            Canvas canvas = ModObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            /*
             * [17:53:30.202] [NeonRandom] System.NullReferenceException
  at (wrapper managed-to-native) UnityEngine.Object.DontDestroyOnLoad(UnityEngine.Object)
  at NeonRandom.NeonRandom.OnApplicationLateStart () [0x0004b] in <1a86fa5382bb4f3da0eb44f003fa9454>:0
  at MelonLoader.MelonEvent+<>c.<Invoke>b__1_0 (MelonLoader.LemonAction x) [0x00000] in <112bf5057ecd4569be296005e32014ff>:0
  at MelonLoader.MelonEventBase`1[T].Invoke (System.Action`1[T] delegateInvoker) [0x00018] in <112bf5057ecd4569be296005e32014ff>:0
             */

        }

        public override void OnUpdate()
        {
            //Manually loading a new level
            if (InputManager.GetKeyDown(Setting_NeonRandom_NewLevel.Value)){
                NewLevel();
            }
        }
  
        private void OnLevelLoadComplete()
        {
            //Start timer when a level first loads
            if (Setting_NeonRandom_Enabled.Value)
            {
                StartTimer();
            }
            new GameObject("LastTryWarning").AddComponent<LastTryWarning>();
        }
        public static System.Timers.Timer newTimer;
        private static void StartTimer()
        {
            //Timer
            newTimer = new System.Timers.Timer(Setting_NeonRandom_Time.Value * 1000);
            newTimer.Elapsed += OnTimedEvent;
            newTimer.Enabled = true;
        }

        private static void OnTimedEvent(System.Object source, ElapsedEventArgs e)
        {
            timerfinished = true;
        }

        private static void NewLevel()
        {
            //Loading the new level
            if (!LevelRush.IsLevelRush())
            {
                lasttry = false;
                var rand = new System.Random();
                int randomlevel = rand.Next(levels.Length);
                string nextlevel = levels[randomlevel];
                //Check for if the level is the same?
                Singleton<Game>.Instance.PlayLevel(nextlevel, true);

            }
        }
        public static void CheckforNewLevel()
        {
            //Called everytime a level is restarted
            //Enables Last Try
            if (timerfinished && Setting_NeonRandom_Enabled.Value && lasttry == false)
            {
                lasttry = true;
            }
            //last try has concluded
            else if (timerfinished && Setting_NeonRandom_Enabled.Value)
            {
                timerfinished = false;
                NewLevel();
            }
        }
    }

    //Patching the Die() Method, called everytime the level is restarted
    [HarmonyPatch]
    internal class DiePatch : MelonMod
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MechController), "Die")]
        private static void PostFix(MechController __instance)
            {
            NeonRandom.CheckforNewLevel();
            }
    }
}
