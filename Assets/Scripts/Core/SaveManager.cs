using System;
using UnityEngine;

namespace IdleEmpire.Core
{
    /// <summary>
    /// Serializable container for all persistent game state.
    /// This is the root object that gets JSON-serialized to PlayerPrefs.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Player's current money balance.</summary>
        public double money;

        /// <summary>Upgrade level for each business (index matches <c>BusinessController[]</c>).</summary>
        public int[] businessLevels = Array.Empty<int>();

        /// <summary>Whether each business has an active manager hired.</summary>
        public bool[] managersHired = Array.Empty<bool>();

        /// <summary>Which upgrades have been purchased (index = upgrade index, value = purchased).</summary>
        public bool[] upgradesPurchased = Array.Empty<bool>();

        /// <summary>Active prestige multiplier (1.0 = no prestige yet).</summary>
        public double prestigeMultiplier = 1.0;

        /// <summary>ISO 8601 UTC timestamp of the last save — used for offline earnings.</summary>
        public string lastSaveTime;

        /// <summary>Whether the player has completed or skipped the tutorial.</summary>
        public bool tutorialCompleted;

        /// <summary>Indices of unlocked achievements.</summary>
        public int[] unlockedAchievementIndices = Array.Empty<int>();

        /// <summary>Total money earned across all time (lifetime earnings, survives prestige).</summary>
        public double totalMoneyEarned;

        /// <summary>Number of prestige resets performed.</summary>
        public int prestigeCount;

        /// <summary>Total money spent across all time.</summary>
        public double totalMoneySpent;

        /// <summary>Total business level-ups performed.</summary>
        public int totalBusinessesPurchased;

        /// <summary>Total manual income collections.</summary>
        public int totalIncomeCollections;

        /// <summary>Highest income per second ever achieved.</summary>
        public double highestIncomePerSecond;

        /// <summary>Total play time in seconds.</summary>
        public float totalPlayTimeSeconds;
    }

    /// <summary>
    /// Handles serialization and persistence of <see cref="SaveData"/> using
    /// <c>UnityEngine.JsonUtility</c> + <c>PlayerPrefs</c>.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        #region Constants

        private const string SaveKey = "IdleEmpire_SaveData";

        #endregion

        #region Public API

        /// <summary>
        /// Serializes <paramref name="data"/> to JSON and persists it via <c>PlayerPrefs</c>.
        /// </summary>
        /// <param name="data">The game state to save.</param>
        public void Save(SaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[SaveManager] Attempted to save null SaveData.");
                return;
            }

            data.lastSaveTime = DateTime.UtcNow.ToString("O");

            string json = JsonUtility.ToJson(data, prettyPrint: false);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();

            Debug.Log($"[SaveManager] Game saved. Time: {data.lastSaveTime}");
        }

        /// <summary>
        /// Loads the game state from <c>PlayerPrefs</c>.
        /// Returns <c>null</c> if no save exists or deserialization fails.
        /// </summary>
        /// <returns>Deserialized <see cref="SaveData"/>, or <c>null</c> if no save found.</returns>
        public SaveData Load()
        {
            if (!HasSave())
            {
                Debug.Log("[SaveManager] No save found.");
                return null;
            }

            string json = PlayerPrefs.GetString(SaveKey);

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                {
                    Debug.LogWarning("[SaveManager] Deserialization returned null.");
                    return null;
                }

                Debug.Log("[SaveManager] Game loaded successfully.");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to deserialize save data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes the saved game state from <c>PlayerPrefs</c>.
        /// Use this for "New Game" or debug resets.
        /// </summary>
        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] Save data deleted.");
        }

        /// <summary>
        /// Returns <c>true</c> if a save file exists in <c>PlayerPrefs</c>.
        /// </summary>
        public bool HasSave() => PlayerPrefs.HasKey(SaveKey);

        #endregion
    }
}
