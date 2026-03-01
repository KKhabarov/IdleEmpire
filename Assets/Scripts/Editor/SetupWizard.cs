#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IdleEmpire.Business;
using IdleEmpire.Managers;
using IdleEmpire.Upgrades;
using IdleEmpire.Achievements;
using IdleEmpire.Tutorial;
using IdleEmpire.Core;
using IdleEmpire.Audio;
using IdleEmpire.Ads;
using IdleEmpire.UI;

namespace IdleEmpire.Editor
{
    public static class SetupWizard
    {
        private static readonly BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Instance;

        [MenuItem("IdleEmpire/Setup Game")]
        public static void SetupGame()
        {
            // ----------------------------------------------------------------
            // 1. Create ScriptableObject assets
            // ----------------------------------------------------------------
            EnsureFolders();

            var businesses   = CreateBusinessAssets();
            var managers     = CreateManagerAssets();
            var upgrades     = CreateUpgradeAssets();
            var achievements = CreateAchievementAssets();
            var tutorialSteps = CreateTutorialAssets();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // ----------------------------------------------------------------
            // 2. Create scene GameObjects
            // ----------------------------------------------------------------

            // --- GameManager ---
            var gmGO = new GameObject("GameManager");
            var gm   = gmGO.AddComponent<GameManager>();
            var cm   = gmGO.AddComponent<CurrencyManager>();
            var sm   = gmGO.AddComponent<SaveManager>();
            SetField(gm, "_currencyManager", cm);
            SetField(gm, "_saveManager", sm);

            // --- AudioManager ---
            var audioGO = new GameObject("AudioManager");
            var audio   = audioGO.AddComponent<AudioManager>();
            var musicSrc = audioGO.AddComponent<AudioSource>();
            var sfxSrc   = audioGO.AddComponent<AudioSource>();
            SetField(audio, "_musicSource", musicSrc);
            SetField(audio, "_sfxSource", sfxSrc);

            // --- AdManager ---
            var adGO = new GameObject("AdManager");
            adGO.AddComponent<AdManager>();

            // --- Business GameObjects ---
            var businessesParent = new GameObject("Businesses");
            var controllers = new BusinessController[businesses.Length];
            for (int i = 0; i < businesses.Length; i++)
            {
                var bGO = new GameObject(businesses[i].BusinessName);
                bGO.transform.SetParent(businessesParent.transform);
                var bc = bGO.AddComponent<BusinessController>();
                SetField(bc, "_businessData", businesses[i]);
                SetField(bc, "_level", i == 0 ? 1 : 0);
                controllers[i] = bc;
                EditorUtility.SetDirty(bGO);
            }

            // --- UpgradeManager ---
            var upgradeGO = new GameObject("UpgradeManager");
            var upgradeManager = upgradeGO.AddComponent<UpgradeManager>();
            SetField(upgradeManager, "_allUpgrades", upgrades);
            SetField(upgradeManager, "_businesses", controllers);

            // --- ManagerController ---
            var managerGO = new GameObject("ManagerController");
            var managerController = managerGO.AddComponent<ManagerController>();
            SetField(managerController, "_allManagers", managers);
            SetField(managerController, "_businesses", controllers);

            // --- AchievementManager ---
            var achievementGO = new GameObject("AchievementManager");
            var achievementManager = achievementGO.AddComponent<AchievementManager>();
            var achievementNotification = achievementGO.AddComponent<AchievementNotification>();
            SetField(achievementManager, "_allAchievements", achievements);
            SetField(achievementManager, "_businesses", controllers);
            SetField(achievementManager, "_managerController", managerController);
            SetField(achievementManager, "_upgradeManager", upgradeManager);
            SetField(achievementManager, "_notification", achievementNotification);

            // --- TutorialManager ---
            var tutorialGO = new GameObject("TutorialManager");
            var tutorialManager = tutorialGO.AddComponent<TutorialManager>();
            SetField(tutorialManager, "_steps", tutorialSteps);
            SetField(tutorialManager, "_businesses", controllers);
            SetField(tutorialManager, "_upgradeManager", upgradeManager);
            SetField(tutorialManager, "_managerController", managerController);

            // --- UIBuilder ---
            var uiGO = new GameObject("UIBuilder");
            uiGO.AddComponent<RuntimeUIBuilder>();

            // --- Wire back-references on GameManager ---
            SetField(gm, "_businesses", controllers);
            SetField(gm, "_upgradeManager", upgradeManager);
            SetField(gm, "_managerController", managerController);

            // ----------------------------------------------------------------
            // 3. Mark dirty and save scene
            // ----------------------------------------------------------------
            EditorUtility.SetDirty(gmGO);
            EditorUtility.SetDirty(audioGO);
            EditorUtility.SetDirty(adGO);
            EditorUtility.SetDirty(businessesParent);
            EditorUtility.SetDirty(upgradeGO);
            EditorUtility.SetDirty(managerGO);
            EditorUtility.SetDirty(achievementGO);
            EditorUtility.SetDirty(tutorialGO);
            EditorUtility.SetDirty(uiGO);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Setup Complete",
                "IdleEmpire scene has been set up successfully!\n\n" +
                "• 10 Businesses\n" +
                "• 10 Managers\n" +
                "• 15 Upgrades\n" +
                "• 10 Achievements\n" +
                "• 5 Tutorial Steps\n" +
                "• All GameObjects created and wired\n\n" +
                "Press Play to test!",
                "OK");
        }

        // ----------------------------------------------------------------
        // Asset creation helpers
        // ----------------------------------------------------------------

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/GameData");
            EnsureFolder("Assets/GameData/Businesses");
            EnsureFolder("Assets/GameData/Managers");
            EnsureFolder("Assets/GameData/Upgrades");
            EnsureFolder("Assets/GameData/Achievements");
            EnsureFolder("Assets/GameData/Tutorial");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                int lastSlash = path.LastIndexOf('/');
                AssetDatabase.CreateFolder(path.Substring(0, lastSlash), path.Substring(lastSlash + 1));
            }
        }

        private static BusinessData[] CreateBusinessAssets()
        {
            var data = new[]
            {
                new object[] { "Lemonade Stand",   "Run a lemonade stand.",          4d,           1d,    1d, 1.07d },
                new object[] { "Newspaper Route",  "Deliver newspapers.",            60d,          3d,    3d, 1.15d },
                new object[] { "Car Wash",         "Wash cars for profit.",          720d,         8d,    6d, 1.14d },
                new object[] { "Pizza Delivery",   "Deliver delicious pizzas.",      8640d,        20d,   12d, 1.13d },
                new object[] { "Donut Shop",       "Sell mouth-watering donuts.",    103680d,      50d,   24d, 1.12d },
                new object[] { "Shrimp Boat",      "Harvest shrimp from the sea.",   1244160d,     120d,  48d, 1.11d },
                new object[] { "Hockey Team",      "Own a hockey franchise.",        14929920d,    300d,  96d, 1.10d },
                new object[] { "Movie Studio",     "Produce blockbuster films.",     179159040d,   750d,  192d, 1.09d },
                new object[] { "Bank",             "Run a profitable bank.",         2149908480d,  1800d, 384d, 1.08d },
                new object[] { "Oil Company",      "Drill for black gold.",          25798901760d, 4500d, 768d, 1.07d },
            };

            var assets = new BusinessData[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                string assetName = ((string)data[i][0]).Replace(" ", "");
                string path = $"Assets/GameData/Businesses/{assetName}.asset";
                var asset = LoadOrCreate<BusinessData>(path);
                SetField(asset, "_businessName",   (string)data[i][0]);
                SetField(asset, "_description",    (string)data[i][1]);
                SetField(asset, "_baseCost",       (double)data[i][2]);
                SetField(asset, "_baseIncome",     (double)data[i][3]);
                SetField(asset, "_cycleDuration",  (float)(double)data[i][4]);
                SetField(asset, "_costMultiplier", (float)(double)data[i][5]);
                EditorUtility.SetDirty(asset);
                assets[i] = asset;
            }
            return assets;
        }

        private static ManagerData[] CreateManagerAssets()
        {
            var data = new[]
            {
                new object[] { "Lemonade Manager",  "Automates the Lemonade Stand.",   1000d,          0 },
                new object[] { "Newspaper Manager", "Automates the Newspaper Route.",  15000d,         1 },
                new object[] { "Car Wash Manager",  "Automates the Car Wash.",         150000d,        2 },
                new object[] { "Pizza Manager",     "Automates Pizza Delivery.",       1500000d,       3 },
                new object[] { "Donut Manager",     "Automates the Donut Shop.",       15000000d,      4 },
                new object[] { "Shrimp Manager",    "Automates the Shrimp Boat.",      100000000d,     5 },
                new object[] { "Hockey Manager",    "Automates the Hockey Team.",      750000000d,     6 },
                new object[] { "Movie Manager",     "Automates the Movie Studio.",     5000000000d,    7 },
                new object[] { "Bank Manager",      "Automates the Bank.",             25000000000d,   8 },
                new object[] { "Oil Manager",       "Automates the Oil Company.",      150000000000d,  9 },
            };

            var assets = new ManagerData[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                string assetName = ((string)data[i][0]).Replace(" ", "");
                string path = $"Assets/GameData/Managers/{assetName}.asset";
                var asset = LoadOrCreate<ManagerData>(path);
                SetField(asset, "_managerName",          (string)data[i][0]);
                SetField(asset, "_description",          (string)data[i][1]);
                SetField(asset, "_cost",                 (double)data[i][2]);
                SetField(asset, "_targetBusinessIndex",  (int)data[i][3]);
                EditorUtility.SetDirty(asset);
                assets[i] = asset;
            }
            return assets;
        }

        private static UpgradeData[] CreateUpgradeAssets()
        {
            var data = new[]
            {
                new object[] { "Better Lemons",    "Squeeze more from every lemon.",     100d,          2f,   0  },
                new object[] { "Fast Papers",      "Speed up newspaper delivery.",       1000d,         2f,   1  },
                new object[] { "Wax On",           "Faster car wash cycles.",            10000d,        2f,   2  },
                new object[] { "Express Delivery", "Pizza delivered even faster.",       100000d,       2f,   3  },
                new object[] { "Secret Recipe",    "The donut formula revealed.",        500000d,       2f,   4  },
                new object[] { "Bigger Nets",      "Catch twice the shrimp.",            2500000d,      2f,   5  },
                new object[] { "Star Players",     "All-star lineup boosts revenue.",    10000000d,     2f,   6  },
                new object[] { "Blockbuster",      "A guaranteed hit at the box office.",50000000d,     2f,   7  },
                new object[] { "High Interest",    "Better interest rates.",             250000000d,    2f,   8  },
                new object[] { "Deep Drilling",    "Reach untapped oil reserves.",       1000000000d,   2f,   9  },
                new object[] { "Global Boost I",   "Boost all businesses x1.5.",         50000d,        1.5f, -1 },
                new object[] { "Global Boost II",  "Another boost for all businesses.",  5000000d,      1.5f, -1 },
                new object[] { "Global Boost III", "Double the empire output.",          25000000d,     2f,   -1 },
                new object[] { "Global Boost IV",  "Massive empire-wide multiplier.",    500000000d,    2f,   -1 },
                new object[] { "Global Boost V",   "Triple all income sources.",         5000000000d,   3f,   -1 },
            };

            var assets = new UpgradeData[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                string assetName = ((string)data[i][0]).Replace(" ", "");
                string path = $"Assets/GameData/Upgrades/{assetName}.asset";
                var asset = LoadOrCreate<UpgradeData>(path);
                SetField(asset, "_upgradeName",          (string)data[i][0]);
                SetField(asset, "_description",          (string)data[i][1]);
                SetField(asset, "_cost",                 (double)data[i][2]);
                SetField(asset, "_multiplier",           (float)data[i][3]);
                SetField(asset, "_targetBusinessIndex",  (int)data[i][4]);
                EditorUtility.SetDirty(asset);
                assets[i] = asset;
            }
            return assets;
        }

        private static AchievementData[] CreateAchievementAssets()
        {
            var data = new[]
            {
                new object[] { "First Dollar",      "Earn your first dollar.",             AchievementType.TotalMoneyEarned, 1d,          -1, 10d    },
                new object[] { "Hundredaire",       "Earn $100 total.",                    AchievementType.TotalMoneyEarned, 100d,        -1, 50d    },
                new object[] { "Thousandaire",      "Earn $1,000 total.",                  AchievementType.TotalMoneyEarned, 1000d,       -1, 200d   },
                new object[] { "Millionaire",       "Earn $1,000,000 total.",              AchievementType.TotalMoneyEarned, 1000000d,    -1, 5000d  },
                new object[] { "Billionaire",       "Earn $1,000,000,000 total.",          AchievementType.TotalMoneyEarned, 1000000000d, -1, 50000d },
                new object[] { "Business Starter",  "Own your first business.",            AchievementType.BusinessOwned,    1d,          -1, 20d    },
                new object[] { "Diversified",       "Own 5 different businesses.",         AchievementType.BusinessOwned,    5d,          -1, 1000d  },
                new object[] { "Empire Builder",    "Own all 10 businesses.",              AchievementType.BusinessOwned,    10d,         -1, 10000d },
                new object[] { "First Manager",     "Hire your first manager.",            AchievementType.ManagersHired,    1d,          -1, 500d   },
                new object[] { "First Prestige",    "Perform your first prestige.",        AchievementType.PrestigeCount,    1d,          -1, 0d     },
            };

            var assets = new AchievementData[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                string assetName = ((string)data[i][0]).Replace(" ", "").Replace(",", "");
                string path = $"Assets/GameData/Achievements/{assetName}.asset";
                var asset = LoadOrCreate<AchievementData>(path);
                SetField(asset, "_achievementName",      (string)data[i][0]);
                SetField(asset, "_description",          (string)data[i][1]);
                SetField(asset, "_type",                 (AchievementType)data[i][2]);
                SetField(asset, "_targetValue",          (double)data[i][3]);
                SetField(asset, "_targetBusinessIndex",  (int)data[i][4]);
                SetField(asset, "_rewardMoney",          (double)data[i][5]);
                EditorUtility.SetDirty(asset);
                assets[i] = asset;
            }
            return assets;
        }

        private static TutorialStep[] CreateTutorialAssets()
        {
            var data = new[]
            {
                new object[] { "Welcome!",      "Welcome to Idle Empire! Tap to earn money and build your business empire.", false, 5f },
                new object[] { "Buy a Business","Tap the BUY button to purchase your first business.",                        true,  0f },
                new object[] { "Collect Income","Your business is earning money! Tap COLLECT to gather income.",              false, 8f },
                new object[] { "Keep Growing",  "Buy more levels to increase your income. The more you invest, the more you earn!", true, 0f },
                new object[] { "You're Ready!", "You've got the basics! Explore upgrades, managers, and prestige to grow your empire!", false, 6f },
            };

            var assets = new TutorialStep[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                string assetName = "TutorialStep" + i;
                string path = $"Assets/GameData/Tutorial/{assetName}.asset";
                var asset = LoadOrCreate<TutorialStep>(path);
                SetField(asset, "_title",             (string)data[i][0]);
                SetField(asset, "_message",           (string)data[i][1]);
                SetField(asset, "_waitForPurchase",   (bool)data[i][2]);
                SetField(asset, "_autoAdvanceDelay",  (float)data[i][3]);
                EditorUtility.SetDirty(asset);
                assets[i] = asset;
            }
            return assets;
        }

        // ----------------------------------------------------------------
        // Utility helpers
        // ----------------------------------------------------------------

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, _flags);
                type = type.BaseType;
            }

            if (field == null)
            {
                Debug.LogWarning($"[SetupWizard] Field '{fieldName}' not found on {obj.GetType().Name}");
                return;
            }

            field.SetValue(obj, value);
        }
    }
}
#endif
