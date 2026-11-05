using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    ExperimentManager exp;
    NavigationManager nav;
    PlayerController player;

    Canvas mainCanvas;
    TextMeshProUGUI hudText, instrText, arrowText, promptText, toastText, kioskBody, resultsBody, analyticsBody, pidLabel;
    TextMeshProUGUI surveySummaryText;
    GameObject hudPanel, kioskPanel, resultsPanel, analyticsPanel, instrPanel;
    RectTransform arrowRt;
    float toastUntil;
    bool worldSpaceVrUi;

    public bool KioskOpen { get { return kioskPanel != null && kioskPanel.activeSelf; } }
    public bool ResultsOpen { get { return resultsPanel != null && resultsPanel.activeSelf; } }
    public bool AnalyticsOpen { get { return analyticsPanel != null && analyticsPanel.activeSelf; } }

    public void Init(ExperimentManager e, NavigationManager n, PlayerController p = null)
    {
        exp = e;
        nav = n;
        player = p != null ? p : FindAnyObjectByType<PlayerController>();
        Build();
    }

    // ---------- public API used by ExperimentManager ----------
    public void Toast(string msg)
    {
        if (toastText == null) return;
        toastText.text = msg;
        toastText.gameObject.SetActive(true);
        toastUntil = Time.time + 3.0f;
    }

    public void OpenKiosk() { if (kioskPanel != null) kioskPanel.SetActive(true); }
    public void CloseKiosk() { if (kioskPanel != null) kioskPanel.SetActive(false); }

    public void ShowResults(string s)
    {
        if (resultsBody != null) resultsBody.text = s;
        RefreshSurveySummary();
        if (resultsPanel != null) resultsPanel.SetActive(true);
    }

    public void UpdateResultsText(string s)
    {
        if (resultsBody != null) resultsBody.text = s;
        RefreshSurveySummary();
    }

    public void HideResults() { if (resultsPanel != null) resultsPanel.SetActive(false); }

    public void OpenAnalytics()
    {
        if (analyticsPanel == null || exp == null) return;
        analyticsBody.text = exp.BuildAnalyticsSummary();
        analyticsPanel.SetActive(true);
        if (player != null)
        {
            player.MovementLocked = true;
            player.SetCursorFree(true);
        }
    }

    public void CloseAnalytics()
    {
        if (analyticsPanel != null) analyticsPanel.SetActive(false);
        if (player != null && !KioskOpen && !ResultsOpen)
        {
            player.MovementLocked = false;
            player.SetCursorFree(false);
        }
    }

    public void ToggleWorldSpaceVrUi()
    {
        if (mainCanvas == null) return;
        worldSpaceVrUi = !worldSpaceVrUi;
        var rt = mainCanvas.GetComponent<RectTransform>();
        if (worldSpaceVrUi)
        {
            mainCanvas.renderMode = RenderMode.WorldSpace;
            mainCanvas.worldCamera = Camera.main;
            rt.sizeDelta = new Vector2(1920, 1080);
            rt.localScale = Vector3.one * 0.0012f;
            PositionWorldCanvas();
            Toast("UI Mode: VR World-Space Diegetic HUD");
        }
        else
        {
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rt.localScale = Vector3.one;
            Toast("UI Mode: Standard Screen-Space Overlay");
        }
    }

    void PositionWorldCanvas()
    {
        if (!worldSpaceVrUi || mainCanvas == null || Camera.main == null) return;
        Transform camT = Camera.main.transform;
        Vector3 fwd = camT.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        fwd.Normalize();
        mainCanvas.transform.position = camT.position + fwd * 2.2f;
        mainCanvas.transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
    }

    // ---------- build ----------
    void Build()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            AirportInput.AttachInputModule(es);
        }

        var cg = new GameObject("Canvas");
        mainCanvas = cg.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = cg.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight = 0.5f;
        cg.AddComponent<GraphicRaycaster>();
        Transform root = cg.transform;

        Vector2 TL = new Vector2(0, 1), TR = new Vector2(1, 1), BL = new Vector2(0, 0),
                BC = new Vector2(.5f, 0), TC = new Vector2(.5f, 1), C = new Vector2(.5f, .5f);
        Color dark = new Color(0.02f, 0.04f, 0.08f, .72f);

        // 1. HUD (top-left)
        var hud = Panel("HudPanel", root, dark, TL, new Vector2(20, -20), new Vector2(560, 395));
        hudPanel = hud.gameObject;
        hudText = Label("HudText", hud, "", 22, TextAlignmentOptions.TopLeft, TL, new Vector2(16, -12), new Vector2(530, 370));

        // 2. Controls hint (bottom-left)
        Label("Hint", root,
              "WASD walk | Right-Click: VR Teleport | Shift sprint | E kiosk | Tab cursor | [1-4] Gate & Carousel | [X] Arrivals Exit | [7-8] Gates B | [9-0, L] Stores & Lounge | F3 Analytics",
              16, TextAlignmentOptions.BottomLeft, BL, new Vector2(20, 12), new Vector2(1480, 32));

        // 3. Control & Scenario Button Column (top-right)
        var col = Panel("ButtonPanel", root, dark, TR, new Vector2(-15, -10), new Vector2(360, 945));

        // Participant ID selector row at top of ButtonPanel
        Btn(col, "<", TL, new Vector2(16, -10), new Vector2(56, 38), () => exp.StepParticipant(-1));
        pidLabel = Label("PidLabel", col, "ID: P01", 21, TextAlignmentOptions.Center, TL, new Vector2(76, -10), new Vector2(208, 38));
        pidLabel.fontStyle = FontStyles.Bold;
        pidLabel.color = new Color(0.4f, 1f, 0.6f);
        Btn(col, ">", TL, new Vector2(288, -10), new Vector2(56, 38), () => exp.StepParticipant(1));

        string[] names =
        {
            "1  Check-In Desk",
            "2  Security Checkpoint",
            "3  Gate A1 (Runway View)",
            "4  Baggage Claim (Moving)",
            "X  Arrivals Exit (Taxis)",
            "5  Arrival Journey",
            "6  Full Departure Journey",
            "7  Gate B1 (Dubai EK 501)",
            "8  Gate B2 (Goa 6E 244)",
            "9  Duty Free Galleria",
            "0  Food Court & Cafe",
            "L  VIP SkyClub Lounge",
            "O  Signage (Clear/Obstr/Clut)",
            "G  Guidance Assist ON/OFF",
            "T  VR Teleport Locomotion",
            "V  VR World-Space UI",
            "F3 Analytics & Pilot CSV",
            "R  Reset to Spawn"
        };
        UnityAction[] acts =
        {
            () => exp.StartTrial(Dest.CheckIn, false),
            () => exp.StartTrial(Dest.Security, false),
            () => exp.StartTrial(Dest.GateA1, false),
            () => exp.StartTrial(Dest.Baggage, false),
            () => exp.StartTrial(Dest.Exit, true),
            () => exp.StartTrial(Dest.Baggage, true),
            () => exp.StartFullJourney(),
            () => exp.StartTrial(Dest.GateB1, false),
            () => exp.StartTrial(Dest.GateB2, false),
            () => exp.StartTrial(Dest.DutyFree, false),
            () => exp.StartTrial(Dest.FoodCourt, false),
            () => exp.StartTrial(Dest.VIPLounge, false),
            () => exp.ToggleObstructed(),
            () => exp.ToggleAssist(),
            () => exp.ToggleTeleportMode(),
            () => ToggleWorldSpaceVrUi(),
            () => { if (AnalyticsOpen) CloseAnalytics(); else OpenAnalytics(); },
            () => exp.ResetAll()
        };
        for (int i = 0; i < names.Length; i++)
            Btn(col, names[i], TL, new Vector2(16, -56 - i * 48), new Vector2(328, 42), acts[i], 18);

        // 4. Instruction bar + rotating direction arrow (bottom-centre)
        var ip = Panel("InstructionPanel", root, dark, BC, new Vector2(0, 52), new Vector2(1180, 78));
        instrPanel = ip.gameObject;
        instrText = Label("InstructionText", ip, "", 26, TextAlignmentOptions.Center, C, Vector2.zero, new Vector2(1140, 70));
        arrowText = Label("Arrow", root, "^", 84, TextAlignmentOptions.Center, BC, new Vector2(0, 165), new Vector2(100, 100));
        arrowText.color = Color.cyan;
        arrowText.fontStyle = FontStyles.Bold;
        arrowRt = arrowText.rectTransform;

        Label("Crosshair", root, "+", 36, TextAlignmentOptions.Center, C, Vector2.zero, new Vector2(50, 50));
        promptText = Label("KioskPrompt", root, "Press E to open Information Kiosk & FIDS Directory", 32, TextAlignmentOptions.Center, C, new Vector2(0, -120), new Vector2(920, 55));
        promptText.color = Color.yellow;
        toastText = Label("Toast", root, "", 30, TextAlignmentOptions.Center, TC, new Vector2(0, -24), new Vector2(1080, 56));
        toastText.color = new Color(1f, .92f, .3f);
        toastText.gameObject.SetActive(false);

        // 5. Kiosk panel
        var kp = Panel("KioskPanel", root, new Color(.04f, .08f, .18f, .96f), C, Vector2.zero, new Vector2(1160, 820));
        kioskPanel = kp.gameObject;
        Label("Title", kp, "INFORMATION KIOSK & FLIGHT DISPLAY SYSTEM (FIDS)", 38, TextAlignmentOptions.Center, TC, new Vector2(0, -20), new Vector2(1100, 60)).color = new Color(1f, .8f, .2f);
        kioskBody = Label("Body", kp, AirportData.KioskText, 26, TextAlignmentOptions.TopLeft, TL, new Vector2(40, -95), new Vector2(1080, 620));
        Btn(kp, "Close Kiosk (E)", BC, new Vector2(0, 22), new Vector2(300, 58), () => exp.CloseKiosk());
        kioskPanel.SetActive(false);

        // 6. Results + Post-Trial SUS & Wayfinding Confidence Survey Panel
        var rp = Panel("ResultsPanel", root, new Color(.04f, .14f, .09f, .97f), C, Vector2.zero, new Vector2(1080, 840));
        resultsPanel = rp.gameObject;
        resultsBody = Label("Body", rp, "", 24, TextAlignmentOptions.TopLeft, TL, new Vector2(35, -22), new Vector2(1010, 340));

        // Subjective Survey Section inside ResultsPanel
        var survBox = Panel("SurveyBox", rp, new Color(0f, 0f, 0f, 0.38f), TL, new Vector2(35, -365), new Vector2(1010, 365));
        Label("SurveyHeader", survBox, "<b>POST-TRIAL SUBJECTIVE EVALUATION (SUS & WAYFINDING QUESTIONNAIRE)</b>",
              23, TextAlignmentOptions.Left, TL, new Vector2(18, -10), new Vector2(970, 36)).color = new Color(1f, 0.88f, 0.35f);

        surveySummaryText = Label("SurveySummary", survBox, "", 20, TextAlignmentOptions.Left, TL, new Vector2(18, -48), new Vector2(970, 34));

        BuildRatingRow(survBox, "1. System Usability (SUS Ease of Use):", -92f, v => { exp.RatingUsability = v; RefreshSurveySummary(); });
        BuildRatingRow(survBox, "2. Signage Clarity & Legibility:", -144f, v => { exp.RatingClarity = v; RefreshSurveySummary(); });
        BuildRatingRow(survBox, "3. Wayfinding Confidence (Low Anxiety):", -196f, v => { exp.RatingConfidence = v; RefreshSurveySummary(); });
        BuildRatingRow(survBox, "4. Visual Comfort & VR Presence:", -248f, v => { exp.RatingComfort = v; RefreshSurveySummary(); });
        BuildRatingRow(survBox, "5. Operational Control Ease:", -300f, v => { exp.RatingControl = v; RefreshSurveySummary(); });

        Btn(rp, "Save Survey to CSV", BC, new Vector2(-340, 22), new Vector2(280, 56), () => exp.SubmitSurveyAndSave(), 23, new Color(.12f, .55f, .28f, 1f));
        Btn(rp, "Retry Same Task", BC, new Vector2(-35, 22), new Vector2(260, 56), () => exp.RetryTrial(), 23);
        Btn(rp, "Analytics (F3)", BC, new Vector2(230, 22), new Vector2(220, 56), () => { HideResults(); OpenAnalytics(); }, 23);
        Btn(rp, "Close", BC, new Vector2(440, 22), new Vector2(150, 56), () => { HideResults(); player.MovementLocked = false; player.SetCursorFree(false); }, 23);
        resultsPanel.SetActive(false);

        // 7. Analytics & Pilot Study Generator Panel (F3)
        var ap = Panel("AnalyticsPanel", root, new Color(.06f, .09f, .18f, .97f), C, Vector2.zero, new Vector2(1180, 820));
        analyticsPanel = ap.gameObject;
        Label("AnalyticsTitle", ap, "EXPERIMENT ANALYTICS & PILOT STUDY SUMMARY (N = 15-20)", 34, TextAlignmentOptions.Center, TC, new Vector2(0, -18), new Vector2(1120, 52)).color = new Color(0.4f, 0.9f, 1f);
        analyticsBody = Label("AnalyticsBody", ap, "", 22, TextAlignmentOptions.TopLeft, TL, new Vector2(36, -80), new Vector2(1108, 630));
        Btn(ap, "Generate 18-Participant Pilot Dataset (54 Trials)", BC, new Vector2(-190, 22), new Vector2(560, 58),
            () => { exp.GeneratePilotStudyData(); analyticsBody.text = exp.BuildAnalyticsSummary(); }, 23, new Color(.15f, .52f, .28f, 1f));
        Btn(ap, "Close Analytics (F3)", BC, new Vector2(320, 22), new Vector2(320, 58), () => CloseAnalytics(), 23);
        analyticsPanel.SetActive(false);
    }

    void BuildRatingRow(Transform parent, string label, float yOffset, UnityAction<int> onSelect)
    {
        Vector2 TL = new Vector2(0, 1);
        Label("Lbl_" + label, parent, label, 20, TextAlignmentOptions.Left, TL, new Vector2(18, yOffset), new Vector2(460, 42));
        string[] tags = { "1 (Low)", "2", "3 (Mod)", "4", "5 (High)" };
        for (int i = 1; i <= 5; i++)
        {
            int ratingVal = i;
            Btn(parent, tags[i - 1], TL, new Vector2(480 + (i - 1) * 102, yOffset), new Vector2(94, 42),
                () => onSelect(ratingVal), 18, new Color(.2f, .38f, .65f, 1f));
        }
    }

    void RefreshSurveySummary()
    {
        if (surveySummaryText == null || exp == null) return;
        surveySummaryText.text =
            $"Current Ratings ->  Usability: <b>{exp.RatingUsability}/5</b>  |  Sign Clarity: <b>{exp.RatingClarity}/5</b>  |  " +
            $"Confidence: <b>{exp.RatingConfidence}/5</b>  |  Comfort: <b>{exp.RatingComfort}/5</b>  |  Control: <b>{exp.RatingControl}/5</b>   " +
            $"=>  <color=#66FF88><b>SUS = {exp.ComputedSusScore:0.0} / 100</b></color>";
    }

    RectTransform Panel(string name, Transform parent, Color col, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var g = new GameObject(name, typeof(RectTransform), typeof(Image));
        g.transform.SetParent(parent, false);
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        g.GetComponent<Image>().color = col;
        return rt;
    }

    TextMeshProUGUI Label(string name, Transform parent, string text, float size, TextAlignmentOptions align, Vector2 anchor, Vector2 pos, Vector2 sz)
    {
        var g = new GameObject(name, typeof(RectTransform));
        g.transform.SetParent(parent, false);
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = sz;
        var t = g.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.alignment = align; t.color = Color.white; t.raycastTarget = false;
        return t;
    }

    void Btn(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, UnityAction act, float fontSize = 24f, Color? customCol = null)
    {
        var rt = Panel("Btn_" + label, parent, customCol ?? new Color(.15f, .35f, .7f, 1f), anchor, pos, size);
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = rt.GetComponent<Image>();
        b.onClick.AddListener(act);
        Label("Text", rt, label, fontSize, TextAlignmentOptions.Center, new Vector2(.5f, .5f), Vector2.zero, size);
    }

    // ---------- per frame ----------
    void Update()
    {
        if (exp == null) return;
        if (AirportInput.GetKeyDown(KeyCode.F2) && hudPanel != null)
            hudPanel.SetActive(!hudPanel.activeSelf);
        if (AirportInput.GetKeyDown(KeyCode.F3))
        {
            if (AnalyticsOpen) CloseAnalytics();
            else OpenAnalytics();
        }
        if (AirportInput.GetKeyDown(KeyCode.V))
            ToggleWorldSpaceVrUi();

        if (worldSpaceVrUi) PositionWorldCanvas();

        if (toastText != null && toastText.gameObject.activeSelf && Time.time > toastUntil)
            toastText.gameObject.SetActive(false);

        if (pidLabel != null)
            pidLabel.text = "ID: " + exp.ParticipantId;

        string condColored = exp.Condition == SignCondition.Clear
            ? "<color=#66FF88>CLEAR (Baseline)</color>"
            : (exp.Condition == SignCondition.Obstructed
                ? "<color=#FF8855>OBSTRUCTED (Ad Blockers)</color>"
                : "<color=#FF66CC>VISUAL CLUTTER (Promo Banners)</color>");

        string locoMode = (player != null && player.TeleportModeOn)
            ? "<color=#55EEFF>VR TELEPORT + WALK</color>"
            : "SMOOTH WALK (Right-Click = Teleport)";

        string task = exp.Running
            ? AirportData.Task(exp.Current, exp.ArrivalMode, exp.FullJourneyMode, exp.JourneyStage)
            : "None (Press 1-6 or Tab to click a scenario)";

        if (hudText != null)
        {
            hudText.text =
                $"<b>VR AIRPORT WAYFINDING SIMULATOR</b>  [{exp.ParticipantId}]\n" +
                $"Signage: {condColored}\n" +
                $"Guidance Assist: {(nav.AssistOn ? "<color=#66FF88>ON</color>" : "<color=#FFAA55>OFF (Sign-Only)</color>")}\n" +
                $"Locomotion: {locoMode}\n" +
                $"Task: {task}\n" +
                $"Time: <b>{exp.Elapsed:0.0} s</b>   |   Distance: <b>{exp.PathLength:0.0} m</b>\n" +
                $"Wrong Turns: <b>{exp.WrongTurns}</b>   |   Carousel Errors: <b>{exp.CarouselErrors}</b>\n" +
                $"Hesitation: <b>{exp.Hesitation:0.0} s</b> (Junction: <b>{exp.JunctionHesitation:0.0} s</b>)\n" +
                $"Kiosk Uses: <b>{exp.KioskUses}</b>   |   Reorient: <b>{exp.ReorientationTime:0.0} s</b>\n" +
                "<size=72%>Tab: free/lock mouse | Right-Click: VR Teleport | F3: Analytics</size>";
        }

        if (instrText != null)
        {
            instrText.text = exp.Running
                ? nav.Instruction
                : "Select destination: [1-4] South Gates | [5] Arrival | [6] Full Journey | [7-8] Gates B1-B2 | [9-0] Retail & Dining | [L] VIP Lounge";
        }

        bool showArrow = exp.Running && nav.AssistOn;
        if (arrowText != null)
        {
            arrowText.gameObject.SetActive(showArrow);
            if (showArrow && arrowRt != null)
                arrowRt.localRotation = Quaternion.Euler(0f, 0f, -nav.ArrowAngle);
        }

        if (promptText != null)
            promptText.gameObject.SetActive(exp.NearKiosk() && !KioskOpen && !ResultsOpen && !AnalyticsOpen);
    }
}
