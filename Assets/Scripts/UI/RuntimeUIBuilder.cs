using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using IdleEmpire.Business;
using IdleEmpire.Core;
using IdleEmpire.Utils;
using IdleEmpire.Upgrades;
using IdleEmpire.Managers;
using IdleEmpire.Audio;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Attach this script to any GameObject in the scene and press Play.
    /// It will programmatically build the entire idle-tycoon UI at runtime,
    /// discovering all <see cref="BusinessController"/> instances in the scene.
    /// </summary>
    public class RuntimeUIBuilder : MonoBehaviour
    {
        #region Color Palette

        private static readonly Color ColorBackground  = HexColor("#1A1A2E");
        private static readonly Color ColorPanel       = HexColor("#16213E");
        private static readonly Color ColorCard        = HexColor("#0F3460");
        private static readonly Color ColorAccent      = HexColor("#E94560");
        private static readonly Color ColorMoney       = HexColor("#FFD700");
        private static readonly Color ColorIncome      = HexColor("#4ADE80");
        private static readonly Color ColorText        = Color.white;
        private static readonly Color ColorMuted       = HexColor("#94A3B8");

        #endregion

        #region Private State

        // HUD references
        private TextMeshProUGUI _moneyText;
        private TextMeshProUGUI _incomeText;

        // Per-business card data
        private readonly List<BusinessCardRefs> _cardRefs = new List<BusinessCardRefs>();

        // Income update interval
        private float _incomeUpdateTimer;
        private const float IncomeUpdateInterval = 1f;

        // Tab panels
        private GameObject _businessPanel;
        private GameObject _shopPanel;
        private GameObject _prestigePanel;
        private GameObject _settingsPanel;
        private Button[] _tabButtons;
        private int _activeTabIndex = 0;

        // Shop state
        private Transform _shopContent;
        private readonly List<ShopItemRef> _shopItemCosts = new List<ShopItemRef>();

        // Prestige button (needs enable/disable based on income)
        private Button _prestigeActionBtn;

        // Cached manager references
        private UpgradeManager _upgradeManager;
        private ManagerController _managerController;

        #endregion

        #region Inner Types

        private class BusinessCardRefs
        {
            public BusinessController Controller;
            public TextMeshProUGUI LevelText;
            public TextMeshProUGUI IncomeText;
            public TextMeshProUGUI CostText;
            public Image ProgressFill;
            public Button BuyButton;
            public Button CollectButton;
            public GameObject CollectGO;
        }

        private struct ShopItemRef
        {
            public Button Btn;
            public double Cost;
        }

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            StartCoroutine(BuildUIDelayed());
        }

        private IEnumerator BuildUIDelayed()
        {
            // Small delay so all singletons have time to initialize.
            yield return new WaitForEndOfFrame();
            BuildUI();
        }

        private void Update()
        {
            _incomeUpdateTimer += Time.deltaTime;
            if (_incomeUpdateTimer >= IncomeUpdateInterval)
            {
                _incomeUpdateTimer = 0f;
                RefreshIncomeDisplay();
            }
        }

        private void OnDestroy()
        {
            var currency = GameManager.Instance?.CurrencyManager;
            if (currency != null)
                currency.OnMoneyChanged -= OnMoneyChanged;

            foreach (var refs in _cardRefs)
            {
                if (refs.Controller == null) continue;
                refs.Controller.OnLevelChanged    -= OnLevelChanged;
                refs.Controller.OnCycleProgress   -= OnCycleProgress;
            }

            if (_upgradeManager != null)
                _upgradeManager.OnUpgradesChanged -= OnShopDataChanged;

            if (_managerController != null)
                _managerController.OnManagersChanged -= OnShopDataChanged;

            if (GameManager.Instance != null)
                GameManager.Instance.OnPrestigeReset -= OnPrestigeResetHandler;
        }

        #endregion

        #region Build

        private void BuildUI()
        {
            Canvas canvas = BuildCanvas();
            RectTransform root = canvas.GetComponent<RectTransform>();

            BuildBackground(root);

            float hudHeight = 160f;
            float navHeight = 120f;

            BuildHUD(root, hudHeight);

            // Discover managers early so panel builders can use them
            _upgradeManager    = FindObjectOfType<UpgradeManager>();
            _managerController = FindObjectOfType<ManagerController>();

            // Create 4 content panels between HUD and nav
            _businessPanel = CreateContentPanel(root, "BusinessPanel", hudHeight, navHeight);
            _shopPanel      = CreateContentPanel(root, "ShopPanel",      hudHeight, navHeight);
            _prestigePanel  = CreateContentPanel(root, "PrestigePanel",  hudHeight, navHeight);
            _settingsPanel  = CreateContentPanel(root, "SettingsPanel",  hudHeight, navHeight);

            BuildBusinessList(_businessPanel.transform);
            BuildShopPanel(_shopPanel.transform);
            BuildPrestigePanel(_prestigePanel.transform);
            BuildSettingsPanel(_settingsPanel.transform);

            BuildBottomNav(root, navHeight);

            SubscribeToEvents();
            RefreshAll();
            SelectTab(0);
        }

        // ── Helper: panel that fills the area between HUD and nav ────────────

        private GameObject CreateContentPanel(RectTransform root, string name,
            float hudHeight, float navHeight)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(root, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0,  navHeight);
            rt.offsetMax = new Vector2(0, -hudHeight);
            return panel;
        }

        // ── Tab switching ────────────────────────────────────────────────────

        private void SelectTab(int index)
        {
            _activeTabIndex = index;
            _businessPanel.SetActive(index == 0);
            _shopPanel.SetActive(index == 1);
            _prestigePanel.SetActive(index == 2);
            _settingsPanel.SetActive(index == 3);

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                var img = _tabButtons[i].GetComponent<Image>();
                img.color = (i == index) ? ColorAccent : ColorCard;
            }
        }

        // ── Canvas ──────────────────────────────────────────────────────────

        private Canvas BuildCanvas()
        {
            // Reuse existing canvas if present, or create a new one.
            Canvas existing = FindObjectOfType<Canvas>();
            GameObject canvasGO;
            if (existing != null)
            {
                canvasGO = existing.gameObject;
            }
            else
            {
                canvasGO = new GameObject("RuntimeCanvas");
                canvasGO.AddComponent<Canvas>();
            }

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasGO.GetOrAddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.GetOrAddComponent<GraphicRaycaster>();

            // EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return canvas;
        }

        // ── Background ───────────────────────────────────────────────────────

        private void BuildBackground(RectTransform root)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(root, false);
            Image img = bg.AddComponent<Image>();
            img.color = ColorBackground;
            RectTransform rt = bg.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // ── HUD ──────────────────────────────────────────────────────────────

        private RectTransform BuildHUD(RectTransform root, float height)
        {
            GameObject panel = new GameObject("HUD");
            panel.transform.SetParent(root, false);
            Image bg = panel.AddComponent<Image>();
            bg.color = ColorPanel;

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0,  -height);
            rt.offsetMax = new Vector2(0,  0);

            // Money text
            _moneyText = CreateTMPText(panel.transform, "MoneyText", "$0", 48, ColorMoney,
                TextAlignmentOptions.Center);
            RectTransform moneyRT = _moneyText.GetComponent<RectTransform>();
            moneyRT.anchorMin = new Vector2(0, 0.45f);
            moneyRT.anchorMax = new Vector2(1, 1f);
            moneyRT.offsetMin = new Vector2(20, 0);
            moneyRT.offsetMax = new Vector2(-20, -10);

            // Income/s text
            _incomeText = CreateTMPText(panel.transform, "IncomeText", "0/s", 24, ColorIncome,
                TextAlignmentOptions.Center);
            RectTransform incomeRT = _incomeText.GetComponent<RectTransform>();
            incomeRT.anchorMin = new Vector2(0, 0);
            incomeRT.anchorMax = new Vector2(1, 0.45f);
            incomeRT.offsetMin = new Vector2(20, 5);
            incomeRT.offsetMax = new Vector2(-20, 0);

            return rt;
        }

        // ── Bottom Nav ───────────────────────────────────────────────────────

        private void BuildBottomNav(RectTransform root, float height)
        {
            GameObject panel = new GameObject("BottomNav");
            panel.transform.SetParent(root, false);
            Image bg = panel.AddComponent<Image>();
            bg.color = ColorPanel;

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, height);

            // Horizontal layout for tab buttons
            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 8;
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.childForceExpandWidth  = true;
            layout.childForceExpandHeight = true;

            string[] labels = { "Business", "Shop", "Prestige", "Settings" };
            _tabButtons = new Button[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                int capturedIndex = i;
                Button btn = CreateTabButton(panel.transform, labels[i]);
                btn.onClick.AddListener(() => SelectTab(capturedIndex));
                _tabButtons[i] = btn;
            }
        }

        private Button CreateTabButton(Transform parent, string label)
        {
            GameObject go = new GameObject(label + "_Tab");
            go.transform.SetParent(parent, false);

            Image img = go.AddComponent<Image>();
            img.color = ColorCard;

            Button btn = go.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor      = ColorCard;
            colors.highlightedColor = ColorAccent;
            colors.pressedColor     = new Color(ColorAccent.r * 0.8f, ColorAccent.g * 0.8f, ColorAccent.b * 0.8f, ColorAccent.a);
            btn.colors = colors;

            // Label
            TextMeshProUGUI text = CreateTMPText(go.transform, "Label", label, 22, ColorText,
                TextAlignmentOptions.Center);
            RectTransform labelRT = text.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            return btn;
        }

        // ── Business List ────────────────────────────────────────────────────

        private void BuildBusinessList(Transform panelTransform)
        {
            // ScrollRect container — fill the whole panel
            GameObject scrollGO = new GameObject("BusinessScroll");
            scrollGO.transform.SetParent(panelTransform, false);
            RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;

            ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical   = true;

            // Viewport
            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            viewportGO.AddComponent<Image>().color = new Color(0, 0, 0, 0); // transparent mask
            viewportGO.AddComponent<Mask>().showMaskGraphic = false;
            scroll.viewport = viewportRT;

            // Content
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot     = new Vector2(0.5f, 1f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 12;
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRT;

            // Build a card for each business
            BusinessController[] businesses = FindObjectsOfType<BusinessController>();
            foreach (BusinessController bc in businesses)
                BuildBusinessCard(contentGO.transform, bc);
        }

        private void BuildBusinessCard(Transform parent, BusinessController bc)
        {
            const float cardHeight = 180f;

            GameObject card = new GameObject(bc?.BusinessData?.BusinessName ?? "Business");
            card.transform.SetParent(parent, false);

            // Fixed height
            LayoutElement le = card.AddComponent<LayoutElement>();
            le.minHeight       = cardHeight;
            le.preferredHeight = cardHeight;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = ColorCard;

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor   = ColorAccent;
            outline.effectDistance = new Vector2(2, 2);

            RectTransform cardRT = card.GetComponent<RectTransform>();

            // ── Business name
            string name = bc?.BusinessData?.BusinessName ?? "Business";
            TextMeshProUGUI nameText = CreateTMPText(card.transform, "NameText", name, 28, ColorText,
                TextAlignmentOptions.Left, bold: true);
            RectTransform nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.7f);
            nameRT.anchorMax = new Vector2(0.65f, 1f);
            nameRT.offsetMin = new Vector2(12, 0);
            nameRT.offsetMax = new Vector2(0, -8);

            // ── Level text
            string levelStr = $"Lvl {bc?.Level ?? 0}";
            TextMeshProUGUI levelText = CreateTMPText(card.transform, "LevelText", levelStr, 20, ColorMuted,
                TextAlignmentOptions.Left);
            RectTransform levelRT = levelText.GetComponent<RectTransform>();
            levelRT.anchorMin = new Vector2(0, 0.45f);
            levelRT.anchorMax = new Vector2(0.5f, 0.7f);
            levelRT.offsetMin = new Vector2(12, 0);
            levelRT.offsetMax = new Vector2(0, 0);

            // ── Income/s text
            string incomeStr = bc != null
                ? $"{NumberFormatter.FormatNumber(bc.GetIncomePerSecond())}/s"
                : "0/s";
            TextMeshProUGUI incomeText = CreateTMPText(card.transform, "IncomeText", incomeStr, 22, ColorIncome,
                TextAlignmentOptions.Left);
            RectTransform incomeRT = incomeText.GetComponent<RectTransform>();
            incomeRT.anchorMin = new Vector2(0.5f, 0.45f);
            incomeRT.anchorMax = new Vector2(1f, 0.7f);
            incomeRT.offsetMin = new Vector2(0, 0);
            incomeRT.offsetMax = new Vector2(-12, 0);

            // ── Cost text
            string costStr = bc != null
                ? $"Cost: {NumberFormatter.FormatNumber(bc.GetCurrentCost())}"
                : "Cost: 0";
            TextMeshProUGUI costText = CreateTMPText(card.transform, "CostText", costStr, 20, ColorMoney,
                TextAlignmentOptions.Left);
            RectTransform costRT = costText.GetComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0, 0.22f);
            costRT.anchorMax = new Vector2(0.5f, 0.45f);
            costRT.offsetMin = new Vector2(12, 0);
            costRT.offsetMax = new Vector2(0, 0);

            // ── BUY button
            GameObject buyGO = CreateButton(card.transform, "BUY", ColorIncome, ColorBackground, 140, 50);
            RectTransform buyRT = buyGO.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.65f, 0.55f);
            buyRT.anchorMax = new Vector2(1f, 1f);
            buyRT.offsetMin = new Vector2(0, 0);
            buyRT.offsetMax = new Vector2(-10, -8);
            Button buyBtn = buyGO.GetComponent<Button>();

            // ── COLLECT button
            GameObject collectGO = CreateButton(card.transform, "COLLECT", ColorMoney, ColorBackground, 140, 50);
            RectTransform collectRT = collectGO.GetComponent<RectTransform>();
            collectRT.anchorMin = new Vector2(0.65f, 0.1f);
            collectRT.anchorMax = new Vector2(1f, 0.55f);
            collectRT.offsetMin = new Vector2(0, 0);
            collectRT.offsetMax = new Vector2(-10, 0);
            Button collectBtn = collectGO.GetComponent<Button>();

            // ── Progress bar
            GameObject barBg = new GameObject("ProgressBG");
            barBg.transform.SetParent(card.transform, false);
            Image barBgImg = barBg.AddComponent<Image>();
            barBgImg.color = ColorBackground;
            RectTransform barBgRT = barBg.GetComponent<RectTransform>();
            barBgRT.anchorMin = new Vector2(0, 0);
            barBgRT.anchorMax = new Vector2(1, 0);
            barBgRT.pivot     = new Vector2(0.5f, 0f);
            barBgRT.offsetMin = new Vector2(0, 0);
            barBgRT.offsetMax = new Vector2(0, 12);

            GameObject barFill = new GameObject("ProgressFill");
            barFill.transform.SetParent(barBg.transform, false);
            Image fillImg = barFill.AddComponent<Image>();
            fillImg.color      = ColorAccent;
            fillImg.type       = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0f;
            RectTransform fillRT = barFill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            if (bc == null) return;

            // Wire up buttons
            BusinessController captured = bc;
            buyBtn.onClick.AddListener(() => captured.Purchase());

            bool hasManager  = bc.HasManager;
            bool isUnlocked  = bc.IsUnlocked;
            collectGO.SetActive(isUnlocked && !hasManager);
            collectBtn.onClick.AddListener(() => captured.StartCollecting());

            // Store refs
            var refs = new BusinessCardRefs
            {
                Controller    = bc,
                LevelText     = levelText,
                IncomeText    = incomeText,
                CostText      = costText,
                ProgressFill  = fillImg,
                BuyButton     = buyBtn,
                CollectButton = collectBtn,
                CollectGO     = collectGO
            };
            _cardRefs.Add(refs);

            // Initial affordability
            UpdateBuyButton(refs, GameManager.Instance?.CurrencyManager?.GetMoney() ?? 0);
        }

        // ── Shop Panel ───────────────────────────────────────────────────────

        private void BuildShopPanel(Transform parent)
        {
            // ScrollRect fills the whole panel
            GameObject scrollGO = new GameObject("ShopScroll");
            scrollGO.transform.SetParent(parent, false);
            RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;

            ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical   = true;

            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            viewportGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            viewportGO.AddComponent<Mask>().showMaskGraphic = false;
            scroll.viewport = viewportRT;

            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot     = new Vector2(0.5f, 1f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 8;
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            contentGO.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRT;
            _shopContent = contentGO.transform;

            RebuildShopContent();
        }

        private void RebuildShopContent()
        {
            if (_shopContent == null) return;

            foreach (Transform child in _shopContent)
                Destroy(child.gameObject);
            _shopItemCosts.Clear();

            double balance = GameManager.Instance?.CurrencyManager?.GetMoney() ?? 0;

            // ── Upgrades ─────────────────────────────────────────────────────
            AddSectionHeader(_shopContent, "UPGRADES");

            if (_upgradeManager != null)
            {
                int[] upgradeIndices = _upgradeManager.GetAvailableUpgradeIndices();
                UpgradeData[] upgrades = _upgradeManager.GetAvailableUpgrades();

                for (int i = 0; i < upgrades.Length; i++)
                {
                    int idx = upgradeIndices[i];
                    AddShopUpgradeCard(_shopContent, upgrades[i], idx, balance);
                }
            }

            // ── Managers ─────────────────────────────────────────────────────
            AddSectionHeader(_shopContent, "MANAGERS");

            if (_managerController != null)
            {
                ManagerData[] managers = _managerController.GetAllManagers();
                for (int i = 0; i < managers.Length; i++)
                {
                    bool isHired = _managerController.IsManagerHired(i);
                    AddShopManagerCard(_shopContent, managers[i], i, isHired, balance);
                }
            }
        }

        private void AddSectionHeader(Transform parent, string title)
        {
            GameObject go = new GameObject("SectionHeader");
            go.transform.SetParent(parent, false);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight       = 50f;
            le.preferredHeight = 50f;

            go.AddComponent<Image>().color = ColorPanel;

            TextMeshProUGUI text = CreateTMPText(go.transform, "HeaderText", title, 26,
                ColorText, TextAlignmentOptions.Left, bold: true);
            RectTransform rt = text.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(16, 0);
            rt.offsetMax = Vector2.zero;
        }

        private void AddShopUpgradeCard(Transform parent, UpgradeData upgrade, int index,
            double balance)
        {
            const float cardHeight = 130f;

            GameObject card = new GameObject("UpgradeCard_" + index);
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.minHeight       = cardHeight;
            le.preferredHeight = cardHeight;

            card.AddComponent<Image>().color = ColorCard;

            // Name
            TextMeshProUGUI nameText = CreateTMPText(card.transform, "Name",
                upgrade.UpgradeName, 24, ColorText, TextAlignmentOptions.Left, bold: true);
            RectTransform nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.65f);
            nameRT.anchorMax = new Vector2(0.7f, 1f);
            nameRT.offsetMin = new Vector2(12, 0);
            nameRT.offsetMax = new Vector2(0, -6);

            // Description
            TextMeshProUGUI descText = CreateTMPText(card.transform, "Desc",
                upgrade.Description, 18, ColorMuted, TextAlignmentOptions.Left);
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Truncate;
            RectTransform descRT = descText.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.35f);
            descRT.anchorMax = new Vector2(0.7f, 0.65f);
            descRT.offsetMin = new Vector2(12, 0);
            descRT.offsetMax = Vector2.zero;

            // Cost + Multiplier
            string costStr = $"{NumberFormatter.FormatNumber(upgrade.Cost)}  x{upgrade.Multiplier:F1}";
            TextMeshProUGUI costText = CreateTMPText(card.transform, "Cost",
                costStr, 20, ColorMoney, TextAlignmentOptions.Left);
            RectTransform costRT = costText.GetComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0, 0f);
            costRT.anchorMax = new Vector2(0.7f, 0.35f);
            costRT.offsetMin = new Vector2(12, 0);
            costRT.offsetMax = Vector2.zero;

            // BUY button
            GameObject buyGO = CreateButton(card.transform, "BUY", ColorIncome,
                ColorBackground, 100, 50);
            RectTransform buyRT = buyGO.GetComponent<RectTransform>();
            buyRT.anchorMin = new Vector2(0.72f, 0.15f);
            buyRT.anchorMax = new Vector2(1f, 0.85f);
            buyRT.offsetMin = Vector2.zero;
            buyRT.offsetMax = new Vector2(-12, 0);

            Button buyBtn = buyGO.GetComponent<Button>();
            int capturedIndex = index;
            UpgradeManager capturedUM = _upgradeManager;
            buyBtn.onClick.AddListener(() => capturedUM.PurchaseUpgrade(capturedIndex));
            buyBtn.interactable = balance >= upgrade.Cost;

            _shopItemCosts.Add(new ShopItemRef { Btn = buyBtn, Cost = upgrade.Cost });
        }

        private void AddShopManagerCard(Transform parent, ManagerData manager, int index,
            bool isHired, double balance)
        {
            const float cardHeight = 130f;

            GameObject card = new GameObject("ManagerCard_" + index);
            card.transform.SetParent(parent, false);

            LayoutElement le = card.AddComponent<LayoutElement>();
            le.minHeight       = cardHeight;
            le.preferredHeight = cardHeight;

            card.AddComponent<Image>().color = ColorCard;

            // Name
            TextMeshProUGUI nameText = CreateTMPText(card.transform, "Name",
                manager.ManagerName, 24, ColorText, TextAlignmentOptions.Left, bold: true);
            RectTransform nameRT = nameText.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.65f);
            nameRT.anchorMax = new Vector2(0.7f, 1f);
            nameRT.offsetMin = new Vector2(12, 0);
            nameRT.offsetMax = new Vector2(0, -6);

            // Description
            TextMeshProUGUI descText = CreateTMPText(card.transform, "Desc",
                manager.Description, 18, ColorMuted, TextAlignmentOptions.Left);
            descText.enableWordWrapping = true;
            descText.overflowMode = TextOverflowModes.Truncate;
            RectTransform descRT = descText.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.35f);
            descRT.anchorMax = new Vector2(0.7f, 0.65f);
            descRT.offsetMin = new Vector2(12, 0);
            descRT.offsetMax = Vector2.zero;

            // Cost
            TextMeshProUGUI costText = CreateTMPText(card.transform, "Cost",
                NumberFormatter.FormatNumber(manager.Cost), 20, ColorMoney,
                TextAlignmentOptions.Left);
            RectTransform costRT = costText.GetComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0, 0f);
            costRT.anchorMax = new Vector2(0.7f, 0.35f);
            costRT.offsetMin = new Vector2(12, 0);
            costRT.offsetMax = Vector2.zero;

            if (isHired)
            {
                TextMeshProUGUI hiredText = CreateTMPText(card.transform, "Hired",
                    "HIRED", 22, ColorIncome, TextAlignmentOptions.Center, bold: true);
                RectTransform hiredRT = hiredText.GetComponent<RectTransform>();
                hiredRT.anchorMin = new Vector2(0.72f, 0.15f);
                hiredRT.anchorMax = new Vector2(1f, 0.85f);
                hiredRT.offsetMin = Vector2.zero;
                hiredRT.offsetMax = new Vector2(-12, 0);
            }
            else
            {
                GameObject hireGO = CreateButton(card.transform, "HIRE", ColorAccent,
                    ColorText, 100, 50);
                RectTransform hireRT = hireGO.GetComponent<RectTransform>();
                hireRT.anchorMin = new Vector2(0.72f, 0.15f);
                hireRT.anchorMax = new Vector2(1f, 0.85f);
                hireRT.offsetMin = Vector2.zero;
                hireRT.offsetMax = new Vector2(-12, 0);

                Button hireBtn = hireGO.GetComponent<Button>();
                int capturedIndex = index;
                ManagerController capturedMC = _managerController;
                hireBtn.onClick.AddListener(() => capturedMC.HireManager(capturedIndex));
                hireBtn.interactable = balance >= manager.Cost;

                _shopItemCosts.Add(new ShopItemRef { Btn = hireBtn, Cost = manager.Cost });
            }
        }

        // ── Prestige Panel ───────────────────────────────────────────────────

        private void BuildPrestigePanel(Transform parent)
        {
            // Scroll view for safe display on all screen sizes
            GameObject scrollGO = new GameObject("PrestigeScroll");
            scrollGO.transform.SetParent(parent, false);
            RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;

            ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical   = true;

            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            viewportGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            viewportGO.AddComponent<Mask>().showMaskGraphic = false;
            scroll.viewport = viewportRT;

            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot     = new Vector2(0.5f, 1f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 24;
            vlg.padding = new RectOffset(24, 24, 32, 32);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            contentGO.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRT;

            Transform content = contentGO.transform;

            // Title
            AddLayoutText(content, "Title", "PRESTIGE", 44, ColorMoney,
                bold: true, height: 80, alignment: TextAlignmentOptions.Center);

            // Current multiplier (read from save data)
            var saveData = GameManager.Instance?.SaveManager?.Load();
            float currentMultiplier = (float)(saveData?.prestigeMultiplier ?? 1.0);

            AddLayoutText(content, "CurrentMult",
                $"Current Multiplier: x{currentMultiplier:F1}", 28, ColorText,
                height: 50, alignment: TextAlignmentOptions.Center);

            AddLayoutText(content, "PotentialBonus",
                $"After Prestige: x{(currentMultiplier + 0.5f):F1}", 28, ColorIncome,
                height: 50, alignment: TextAlignmentOptions.Center);

            TextMeshProUGUI descTMP = AddLayoutText(content, "Desc",
                "Prestige resets all businesses and money but grants a permanent\n" +
                "income multiplier (+0.5x per prestige).\n\n" +
                "Requires at least $1,000,000/s income.",
                22, ColorMuted, height: 110, alignment: TextAlignmentOptions.Center);
            descTMP.enableWordWrapping = true;

            // PRESTIGE RESET button
            GameObject prestigeBtnGO = CreateButton(content, "PRESTIGE RESET",
                ColorAccent, ColorText, 300, 80);
            LayoutElement prestigeLE = prestigeBtnGO.AddComponent<LayoutElement>();
            prestigeLE.minHeight       = 80f;
            prestigeLE.preferredHeight = 80f;

            _prestigeActionBtn = prestigeBtnGO.GetComponent<Button>();

            // Confirmation panel (absolutely positioned over the prestige panel)
            GameObject confirmPanel = new GameObject("ConfirmPanel");
            confirmPanel.transform.SetParent(parent, false);
            RectTransform confirmRT = confirmPanel.AddComponent<RectTransform>();
            confirmRT.anchorMin = new Vector2(0.05f, 0.25f);
            confirmRT.anchorMax = new Vector2(0.95f, 0.75f);
            confirmRT.offsetMin = Vector2.zero;
            confirmRT.offsetMax = Vector2.zero;
            confirmPanel.AddComponent<Image>().color = ColorPanel;
            Outline confirmOutline = confirmPanel.AddComponent<Outline>();
            confirmOutline.effectColor    = ColorAccent;
            confirmOutline.effectDistance = new Vector2(2, 2);
            confirmPanel.SetActive(false);

            VerticalLayoutGroup confirmVLG = confirmPanel.AddComponent<VerticalLayoutGroup>();
            confirmVLG.childAlignment = TextAnchor.MiddleCenter;
            confirmVLG.spacing = 20;
            confirmVLG.padding = new RectOffset(24, 24, 24, 24);
            confirmVLG.childForceExpandWidth  = true;
            confirmVLG.childForceExpandHeight = false;

            TextMeshProUGUI sureTMP = AddLayoutText(confirmPanel.transform, "AreYouSure",
                "Are you sure?\nThis will reset ALL progress!", 26, ColorText,
                bold: true, height: 90, alignment: TextAlignmentOptions.Center);
            sureTMP.enableWordWrapping = true;

            GameObject confirmBtnGO = CreateButton(confirmPanel.transform,
                "CONFIRM", ColorAccent, ColorText, 260, 70);
            LayoutElement confirmBtnLE = confirmBtnGO.AddComponent<LayoutElement>();
            confirmBtnLE.minHeight       = 70f;
            confirmBtnLE.preferredHeight = 70f;
            confirmBtnGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                confirmPanel.SetActive(false);
                GameManager.Instance?.PrestigeReset();
            });

            GameObject cancelBtnGO = CreateButton(confirmPanel.transform,
                "CANCEL", ColorCard, ColorText, 200, 60);
            LayoutElement cancelBtnLE = cancelBtnGO.AddComponent<LayoutElement>();
            cancelBtnLE.minHeight       = 60f;
            cancelBtnLE.preferredHeight = 60f;
            cancelBtnGO.GetComponent<Button>().onClick.AddListener(
                () => confirmPanel.SetActive(false));

            // Wire main prestige button to show confirm panel
            _prestigeActionBtn.onClick.AddListener(() => confirmPanel.SetActive(true));

            UpdatePrestigeButton();
        }

        private void UpdatePrestigeButton()
        {
            if (_prestigeActionBtn == null) return;
            double ips = GameManager.Instance?.CurrencyManager?.GetIncomePerSecond() ?? 0;
            _prestigeActionBtn.interactable = ips >= 1_000_000;
        }

        // ── Settings Panel ───────────────────────────────────────────────────

        private void BuildSettingsPanel(Transform parent)
        {
            // Scroll view
            GameObject scrollGO = new GameObject("SettingsScroll");
            scrollGO.transform.SetParent(parent, false);
            RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;

            ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical   = true;

            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = Vector2.zero;
            viewportRT.offsetMax = Vector2.zero;
            viewportGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            viewportGO.AddComponent<Mask>().showMaskGraphic = false;
            scroll.viewport = viewportRT;

            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot     = new Vector2(0.5f, 1f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 16;
            vlg.padding = new RectOffset(20, 20, 24, 24);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            contentGO.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRT;

            Transform content = contentGO.transform;

            // Title
            AddLayoutText(content, "Title", "SETTINGS", 44, ColorText,
                bold: true, height: 80, alignment: TextAlignmentOptions.Center);

            // Audio toggles
            bool musicOn = AudioManager.Instance?.IsMusicEnabled ?? true;
            bool sfxOn   = AudioManager.Instance?.IsSfxEnabled   ?? true;

            BuildToggleRow(content, "Music", "Music", musicOn,
                enabled => AudioManager.Instance?.ToggleMusic(enabled));

            BuildToggleRow(content, "SFX", "SFX", sfxOn,
                enabled => AudioManager.Instance?.ToggleSfx(enabled));

            // Save button
            GameObject saveBtnGO = CreateButton(content, "SAVE GAME",
                ColorIncome, ColorBackground, 300, 70);
            LayoutElement saveLE = saveBtnGO.AddComponent<LayoutElement>();
            saveLE.minHeight       = 70f;
            saveLE.preferredHeight = 70f;
            saveBtnGO.GetComponent<Button>().onClick.AddListener(
                () => GameManager.Instance?.SaveGame());

            // Reset progress button
            GameObject resetBtnGO = CreateButton(content, "RESET PROGRESS",
                ColorAccent, ColorText, 300, 70);
            LayoutElement resetLE = resetBtnGO.AddComponent<LayoutElement>();
            resetLE.minHeight       = 70f;
            resetLE.preferredHeight = 70f;

            // Reset confirmation panel (absolutely positioned over settings panel)
            GameObject resetConfirm = new GameObject("ResetConfirmPanel");
            resetConfirm.transform.SetParent(parent, false);
            RectTransform resetConfirmRT = resetConfirm.AddComponent<RectTransform>();
            resetConfirmRT.anchorMin = new Vector2(0.05f, 0.25f);
            resetConfirmRT.anchorMax = new Vector2(0.95f, 0.75f);
            resetConfirmRT.offsetMin = Vector2.zero;
            resetConfirmRT.offsetMax = Vector2.zero;
            resetConfirm.AddComponent<Image>().color = ColorPanel;
            Outline resetOutline = resetConfirm.AddComponent<Outline>();
            resetOutline.effectColor    = ColorAccent;
            resetOutline.effectDistance = new Vector2(2, 2);
            resetConfirm.SetActive(false);

            VerticalLayoutGroup resetVLG = resetConfirm.AddComponent<VerticalLayoutGroup>();
            resetVLG.childAlignment = TextAnchor.MiddleCenter;
            resetVLG.spacing = 20;
            resetVLG.padding = new RectOffset(24, 24, 24, 24);
            resetVLG.childForceExpandWidth  = true;
            resetVLG.childForceExpandHeight = false;

            TextMeshProUGUI resetSureTMP = AddLayoutText(resetConfirm.transform, "ResetSure",
                "Reset ALL progress?\nThis cannot be undone!", 26, ColorText,
                bold: true, height: 90, alignment: TextAlignmentOptions.Center);
            resetSureTMP.enableWordWrapping = true;

            GameObject resetConfirmBtnGO = CreateButton(resetConfirm.transform,
                "CONFIRM RESET", ColorAccent, ColorText, 260, 70);
            LayoutElement rcLE = resetConfirmBtnGO.AddComponent<LayoutElement>();
            rcLE.minHeight       = 70f;
            rcLE.preferredHeight = 70f;
            resetConfirmBtnGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                resetConfirm.SetActive(false);
                GameManager.Instance?.SaveManager?.DeleteSave();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });

            GameObject resetCancelBtnGO = CreateButton(resetConfirm.transform,
                "CANCEL", ColorCard, ColorText, 200, 60);
            LayoutElement rcCancelLE = resetCancelBtnGO.AddComponent<LayoutElement>();
            rcCancelLE.minHeight       = 60f;
            rcCancelLE.preferredHeight = 60f;
            resetCancelBtnGO.GetComponent<Button>().onClick.AddListener(
                () => resetConfirm.SetActive(false));

            resetBtnGO.GetComponent<Button>().onClick.AddListener(
                () => resetConfirm.SetActive(true));

            // Version
            AddLayoutText(content, "Version",
                $"v{Application.version}", 20, ColorMuted,
                height: 40, alignment: TextAlignmentOptions.Center);
        }

        private void BuildToggleRow(Transform parent, string id, string label,
            bool initialValue, Action<bool> onToggle)
        {
            GameObject row = new GameObject(id + "_Row");
            row.transform.SetParent(parent, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight       = 70f;
            le.preferredHeight = 70f;

            row.AddComponent<Image>().color = ColorCard;

            TextMeshProUGUI labelTMP = CreateTMPText(row.transform, "Label", label,
                26, ColorText, TextAlignmentOptions.Left);
            RectTransform labelRT = labelTMP.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(0.7f, 1f);
            labelRT.offsetMin = new Vector2(16, 0);
            labelRT.offsetMax = Vector2.zero;

            bool[] state = { initialValue };
            Color  onColor  = ColorIncome;
            Color  offColor = ColorMuted;

            string onLabel  = "ON";
            string offLabel = "OFF";

            Color  initialColor = state[0] ? onColor : offColor;
            string initialLabel = state[0] ? onLabel  : offLabel;

            GameObject btnGO = CreateButton(row.transform, initialLabel, initialColor,
                ColorBackground, 100, 50);
            RectTransform btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.72f, 0.15f);
            btnRT.anchorMax = new Vector2(1f, 0.85f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = new Vector2(-16, 0);

            Image btnImg        = btnGO.GetComponent<Image>();
            TextMeshProUGUI btnTMP = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            Button btn          = btnGO.GetComponent<Button>();

            btn.onClick.AddListener(() =>
            {
                state[0] = !state[0];
                onToggle?.Invoke(state[0]);
                btnImg.color = state[0] ? onColor : offColor;
                if (btnTMP != null)
                    btnTMP.text = state[0] ? onLabel : offLabel;
            });
        }

        // ── Layout text helper ───────────────────────────────────────────────

        private TextMeshProUGUI AddLayoutText(Transform parent, string goName, string content,
            int fontSize, Color color, bool bold = false, float height = 40,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent, false);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight       = height;
            le.preferredHeight = height;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = content;
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.alignment = alignment;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
                tmp.font = font;

            return tmp;
        }

        #endregion

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            var currency = GameManager.Instance?.CurrencyManager;
            if (currency != null)
                currency.OnMoneyChanged += OnMoneyChanged;

            foreach (var refs in _cardRefs)
            {
                if (refs.Controller == null) continue;
                refs.Controller.OnLevelChanged  += OnLevelChanged;
                refs.Controller.OnCycleProgress += OnCycleProgress;
            }

            // _upgradeManager and _managerController already set in BuildUI
            if (_upgradeManager != null)
                _upgradeManager.OnUpgradesChanged += OnShopDataChanged;

            if (_managerController != null)
                _managerController.OnManagersChanged += OnShopDataChanged;

            if (GameManager.Instance != null)
                GameManager.Instance.OnPrestigeReset += OnPrestigeResetHandler;
        }

        private void OnMoneyChanged(double newBalance)
        {
            if (_moneyText != null)
                _moneyText.text = NumberFormatter.FormatNumber(newBalance);

            foreach (var refs in _cardRefs)
                UpdateBuyButton(refs, newBalance);

            // Update shop button affordability
            foreach (var item in _shopItemCosts)
                if (item.Btn != null)
                    item.Btn.interactable = newBalance >= item.Cost;
        }

        private void OnLevelChanged(BusinessController bc)
        {
            var refs = _cardRefs.Find(r => r.Controller == bc);
            if (refs == null) return;

            if (refs.LevelText  != null) refs.LevelText.text  = $"Lvl {bc.Level}";
            if (refs.IncomeText != null) refs.IncomeText.text = $"{NumberFormatter.FormatNumber(bc.GetIncomePerSecond())}/s";
            if (refs.CostText   != null) refs.CostText.text   = $"Cost: {NumberFormatter.FormatNumber(bc.GetCurrentCost())}";

            // Show/hide collect button based on manager and unlock state
            if (refs.CollectGO != null)
                refs.CollectGO.SetActive(bc.IsUnlocked && !bc.HasManager);

            double balance = GameManager.Instance?.CurrencyManager?.GetMoney() ?? 0;
            UpdateBuyButton(refs, balance);
        }

        private void OnCycleProgress(BusinessController bc, float progress)
        {
            var refs = _cardRefs.Find(r => r.Controller == bc);
            if (refs?.ProgressFill != null)
                refs.ProgressFill.fillAmount = progress;
        }

        private void OnShopDataChanged()
        {
            RebuildShopContent();
        }

        private void OnPrestigeResetHandler()
        {
            RebuildShopContent();
            UpdatePrestigeButton();
        }

        #endregion

        #region Helpers

        private void RefreshAll()
        {
            double balance = GameManager.Instance?.CurrencyManager?.GetMoney() ?? 0;
            if (_moneyText != null)
                _moneyText.text = NumberFormatter.FormatNumber(balance);

            RefreshIncomeDisplay();

            foreach (var refs in _cardRefs)
            {
                if (refs.Controller == null) continue;
                OnLevelChanged(refs.Controller);
                UpdateBuyButton(refs, balance);
            }
        }

        private void RefreshIncomeDisplay()
        {
            if (_incomeText == null) return;
            double ips = GameManager.Instance?.CurrencyManager?.GetIncomePerSecond() ?? 0;
            _incomeText.text = $"{NumberFormatter.FormatNumber(ips)}/s";
            UpdatePrestigeButton();
        }

        private void UpdateBuyButton(BusinessCardRefs refs, double balance)
        {
            if (refs?.BuyButton == null || refs.Controller == null) return;
            refs.BuyButton.interactable = balance >= refs.Controller.GetCurrentCost();
        }

        private TextMeshProUGUI CreateTMPText(Transform parent, string goName, string content,
            int fontSize, Color color, TextAlignmentOptions alignment, bool bold = false)
        {
            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = content;
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.alignment = alignment;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font != null)
                tmp.font = font;

            return tmp;
        }

        private GameObject CreateButton(Transform parent, string label, Color bgColor,
            Color textColor, float width, float height)
        {
            GameObject go = new GameObject(label + "_Button");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            Button btn = go.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor      = bgColor;
            cb.highlightedColor = new Color(Mathf.Min(bgColor.r * 1.2f, 1f), Mathf.Min(bgColor.g * 1.2f, 1f), Mathf.Min(bgColor.b * 1.2f, 1f), bgColor.a);
            cb.pressedColor     = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, bgColor.a);
            cb.disabledColor    = ColorMuted;
            btn.colors = cb;

            TextMeshProUGUI txt = CreateTMPText(go.transform, "Label", label, 22, textColor,
                TextAlignmentOptions.Center, bold: true);
            RectTransform txtRT = txt.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;

            return go;
        }

        private static Color HexColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
                return c;
            return Color.white;
        }

        #endregion
    }

    // ── Extension helper (keep in same file to avoid extra file) ─────────────

    internal static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            return comp != null ? comp : go.AddComponent<T>();
        }
    }
}
