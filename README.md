# Idle Empire Tycoon

> A mobile idle/clicker game built with Unity — build your business empire from a humble lemonade stand all the way to a space station!

---

## 📖 Description

**Idle Empire Tycoon** is a mobile idle/clicker game where the player taps to earn money, upgrades businesses, hires managers, and prestigiously resets for permanent bonuses. The game features a classic idle-game loop with offline earnings so players always return to a reward.

---

## 🎮 Features

- **Tap to Earn** — manually collect income from each business
- **Upgrade Businesses** — level up businesses to increase income
- **Hire Managers** — automate income collection so you earn while away
- **Offline Earnings** — earn income for up to 8 hours while the app is closed
- **Upgrades Shop** — purchase one-time upgrades to multiply specific businesses' income
- **Prestige System** — reset progress for a permanent income multiplier bonus
- **Rewarded Ads** — optional ad viewing for temporary income boosts (AdMob)
- **Auto Save** — game state saved every 60 seconds and on app pause/quit
- **Large Number Formatting** — human-readable values: 1K, 1M, 1B, 1T, etc.

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| Game Engine | Unity 2022 LTS (or newer) |
| Language | C# (.NET Standard 2.1) |
| Ads | Google AdMob (Unity Plugin) |
| Persistence | `PlayerPrefs` + `JsonUtility` |
| UI | Unity UI (uGUI) + TextMeshPro |

---

## 🗂 Project Structure

```
Assets/
└── Scripts/
    ├── Core/
    │   ├── GameManager.cs          # Singleton — game init, pause/resume, auto-save, offline earnings
    │   ├── CurrencyManager.cs      # Thread-safe money management + OnMoneyChanged event
    │   └── SaveManager.cs          # JSON serialization via PlayerPrefs
    ├── Business/
    │   ├── BusinessData.cs         # ScriptableObject — static business configuration
    │   ├── BusinessController.cs   # Per-business logic (levels, income, manager automation)
    │   └── BusinessUI.cs           # Business card UI (name, level, IPS, cost, progress bar)
    ├── Upgrades/
    │   ├── UpgradeData.cs          # ScriptableObject — upgrade configuration
    │   └── UpgradeManager.cs       # Upgrade purchase + multiplier application
    ├── Managers/
    │   ├── ManagerData.cs          # ScriptableObject — manager configuration
    │   └── ManagerController.cs    # Manager hiring + business automation
    ├── UI/
    │   ├── MainUI.cs               # HUD — total money + income per second
    │   ├── ShopUI.cs               # Shop panel — upgrades and managers
    │   └── PrestigeUI.cs           # Prestige panel — reset flow + confirmation
    ├── Utils/
    │   ├── NumberFormatter.cs      # Static formatter: 1000 → "1.00K", 1e9 → "1.00B"
    │   └── OfflineCalculator.cs    # Static helper for offline earnings calculation
    └── Ads/
        └── AdManager.cs            # Singleton stub — rewarded & interstitial ad placeholders
```

---

## 🚀 Opening in Unity

1. **Clone the repository**
   ```bash
   git clone https://github.com/KKhabarov/IdleEmpire.git
   ```
2. **Open with Unity Hub** — click *Open* and select the cloned folder.
   - Recommended: **Unity 2022 LTS** or newer.
3. **Install TextMeshPro** if prompted (Unity will ask on first open).
4. **Build for Android/iOS** via *File → Build Settings* → select platform → *Build*.

---

## 💰 Monetization Strategy

| Revenue Stream | Implementation |
|---|---|
| **Rewarded Ads** | Player can watch an ad to earn a temporary 2× income multiplier. Implemented in `AdManager.ShowRewardedAd()`. |
| **Interstitial Ads** | Non-rewarded full-screen ads shown between major events (e.g. prestige screen). Implemented in `AdManager.ShowInterstitialAd()`. |
| **In-App Purchases** *(future)* | Premium currency, ad-free experience, starter packs — integration point ready in `AdManager`. |

To activate ads, follow the integration steps in `Assets/Scripts/Ads/AdManager.cs`.

---

## 📐 Architecture Notes

- **Singleton GameManager** coordinates all subsystems; other scripts communicate via events.
- **ScriptableObjects** (`BusinessData`, `UpgradeData`, `ManagerData`) allow designers to configure the game without writing code.
- **Observer Pattern** — `CurrencyManager.OnMoneyChanged`, `BusinessController.OnBusinessChanged`, etc. keep UI and logic decoupled.
- **Thread Safety** — `CurrencyManager` uses `lock` to guard balance mutations; a lightweight dispatcher marshals callbacks to Unity's main thread.

---

## 📝 License

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) for details.

```
MIT License

Copyright (c) 2024 KKhabarov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
