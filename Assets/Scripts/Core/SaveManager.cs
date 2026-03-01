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
        public bool[] managerStates = Array.Empty<bool>();

        /// <summary>Unix timestamp (UTC seconds) of the last save — used for offline earnings.</summary>
        public long lastSaveTimestamp;

        /// <summary>Active prestige multiplier (1.0 = no prestige yet).</summary>
        public float prestigeMultiplier = 1f;

        /// <summary>Indices of upgrades that have already been purchased.</summary>
        public int[] purchasedUpgradeIndices = Array.Empty<int>();
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

            data.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string json = JsonUtility.ToJson(data, prettyPrint: false);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();

            Debug.Log($"[SaveManager] Game saved. Timestamp: {data.lastSaveTimestamp}");
        }

        /// <summary>
        /// Loads the game state from <c>PlayerPrefs</c>.
        /// Returns a default <see cref="SaveData"/> if no save exists.
        /// </summary>
        /// <returns>Deserialized <see cref="SaveData"/>.</returns>
        public SaveData Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                Debug.Log("[SaveManager] No save found. Returning default SaveData.");
                return CreateDefaultSaveData();
            }

            string json = PlayerPrefs.GetString(SaveKey);

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null)
                {
                    Debug.LogWarning("[SaveManager] Deserialization returned null. Returning default.");
                    return CreateDefaultSaveData();
                }

                Debug.Log("[SaveManager] Game loaded successfully.");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to deserialize save data: {ex.Message}");
                return CreateDefaultSaveData();
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

        #endregion

        #region Helpers

        private SaveData CreateDefaultSaveData()
        {
            return new SaveData
            {
                money = 0,
                businessLevels = Array.Empty<int>(),
                managerStates = Array.Empty<bool>(),
                lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                prestigeMultiplier = 1f,
                purchasedUpgradeIndices = Array.Empty<int>()
            };
        }

        #endregion
    }
}
