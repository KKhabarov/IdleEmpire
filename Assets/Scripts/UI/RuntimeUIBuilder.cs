using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleEmpire.Business;
using IdleEmpire.Core;
using IdleEmpire.Utils;

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

        #endregion

        #region Inner Type

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
        }

        #endregion

        #region Build

        private void BuildUI()
        {
            Canvas canvas = BuildCanvas();
            RectTransform root = canvas.GetComponent<RectTransform>();

            BuildBackground(root);

            float hudHeight    = 160f;
            float navHeight    = 120f;

            RectTransform hud = BuildHUD(root, hudHeight);
            BuildBottomNav(root, navHeight);
            BuildBusinessList(root, hudHeight, navHeight);

            SubscribeToEvents();
            RefreshAll();
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

            string[] labels = { "🏭 Business", "🛒 Shop", "⭐ Prestige", "⚙️ Settings" };
            foreach (string label in labels)
                CreateTabButton(panel.transform, label);
        }

        private void CreateTabButton(Transform parent, string label)
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
        }

        // ── Business List ────────────────────────────────────────────────────

        private void BuildBusinessList(RectTransform root, float hudHeight, float navHeight)
        {
            // ScrollRect container
            GameObject scrollGO = new GameObject("BusinessScroll");
            scrollGO.transform.SetParent(root, false);
            RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0);
            scrollRT.anchorMax = new Vector2(1, 1);
            scrollRT.offsetMin = new Vector2(0,  navHeight);
            scrollRT.offsetMax = new Vector2(0, -hudHeight);

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
        }

        private void OnMoneyChanged(double newBalance)
        {
            if (_moneyText != null)
                _moneyText.text = NumberFormatter.FormatNumber(newBalance);

            foreach (var refs in _cardRefs)
                UpdateBuyButton(refs, newBalance);
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
