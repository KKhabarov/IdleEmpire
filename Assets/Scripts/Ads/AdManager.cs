using System;
using UnityEngine;

namespace IdleEmpire.Ads
{
    /// <summary>
    /// Stub/placeholder for ad integration.
    /// Replace the <c>Debug.Log</c> bodies with real AdMob SDK calls once integrated.
    /// </summary>
    /// <remarks>
    /// <b>Integration steps (Google AdMob):</b>
    /// <list type="number">
    ///   <item>Import the <c>Google Mobile Ads Unity Plugin</c> from the Unity Asset Store
    ///         or the official GitHub release.</item>
    ///   <item>Add your <c>ca-app-pub-XXXXXXXXXXXXXXXX~NNNNNNNNNN</c> App ID in
    ///         Assets → Google Mobile Ads → Settings.</item>
    ///   <item>Replace the stub bodies below with real
    ///         <c>RewardedAd</c> / <c>InterstitialAd</c> loading and showing calls
    ///         from the <c>GoogleMobileAds</c> namespace.</item>
    ///   <item>Handle <c>OnAdRewarded</c> to grant in-game rewards (e.g. 2× earnings for 30 min).</item>
    /// </list>
    /// </remarks>
    public class AdManager : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Fired when the player successfully earns a reward from a rewarded ad.
        /// The argument is the reward multiplier (e.g. 2.0 for 2× earnings).
        /// </summary>
        public event Action<double> OnRewardClaimed;

        #endregion

        #region Singleton

        /// <summary>Shared instance of the AdManager.</summary>
        public static AdManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // TODO: Initialize AdMob SDK here.
            // MobileAds.Initialize(initStatus => { Debug.Log("AdMob initialized."); });
            Debug.Log("[AdManager] AdMob stub initialized.");
        }

        #endregion

        #region Inspector Fields

        [Header("Ad Unit IDs (replace with real IDs for production)")]
        [SerializeField] private string _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";   // AdMob test ID
        [SerializeField] private string _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // AdMob test ID

        #endregion

        #region Rewarded Ads

        /// <summary>
        /// Loads and shows a rewarded ad.
        /// On success, <see cref="OnAdRewarded"/> is called with the reward amount.
        /// </summary>
        public void ShowRewardedAd()
        {
            // TODO: Replace with real AdMob rewarded ad code:
            // RewardedAd ad = new RewardedAd(_rewardedAdUnitId);
            // ad.OnUserEarnedReward += (sender, args) => OnAdRewarded(args.Amount);
            // ad.LoadAd(new AdRequest.Builder().Build());
            // ad.Show();

            Debug.Log("[AdManager] ShowRewardedAd() — stub called (AdMob not integrated yet).");
            OnAdRewarded(2.0);
        }

        /// <summary>
        /// Called when the player successfully earns a reward from a rewarded ad.
        /// Fires <see cref="OnRewardClaimed"/> so listeners (e.g. <see cref="IdleEmpire.UI.OfflineEarningsPopup"/>)
        /// can apply the reward.
        /// </summary>
        /// <param name="rewardMultiplier">Multiplier to apply as the reward (e.g. 2.0 for 2× earnings).</param>
        public void OnAdRewarded(double rewardMultiplier)
        {
            Debug.Log($"[AdManager] OnAdRewarded — reward multiplier: {rewardMultiplier}x (stub).");
            OnRewardClaimed?.Invoke(rewardMultiplier);
        }

        #endregion

        #region Interstitial Ads

        /// <summary>
        /// Loads and shows an interstitial ad.
        /// Typically shown between major game events (e.g. prestige screens).
        /// </summary>
        public void ShowInterstitialAd()
        {
            // TODO: Replace with real AdMob interstitial ad code:
            // InterstitialAd ad = new InterstitialAd(_interstitialAdUnitId);
            // ad.OnAdClosed += HandleAdClosed;
            // ad.LoadAd(new AdRequest.Builder().Build());
            // ad.Show();

            Debug.Log("[AdManager] ShowInterstitialAd() — stub called (AdMob not integrated yet).");
        }

        #endregion
    }
}
