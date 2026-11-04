using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AirportBuilder : MonoBehaviour
{
    class SignInfo
    {
        public bool key;
        public GameObject blocker;
    }

    readonly List<SignInfo> signs = new List<SignInfo>();
    readonly List<GameObject> clutterSigns = new List<GameObject>();
    Material baseMat;
    Transform floors, walls, props, signsRoot, clutterRoot;

    static Color C(float r, float g, float b) { return new Color(r, g, b, 1f); }
    readonly Color wallCol = C(.93f, .93f, .90f);
    readonly Color floorCol = C(.50f, .50f, .53f);
    readonly Color signBlue = C(.05f, .25f, .55f);
    readonly Color signGreen = C(0f, .42f, .22f);
    readonly Color fidsDark = C(.03f, .03f, .08f);
    readonly Color amber = C(1f, .8f, .2f);
    readonly Color blockerCol = C(.45f, .30f, .15f);
    readonly Color clutterPink = C(.72f, .08f, .42f);
    readonly Color clutterOrange = C(.85f, .35f, .05f);
    readonly Color clutterPurple = C(.38f, .12f, .62f);

    public void SetObstructed(bool on)
    {
        SetSignCondition(on ? SignCondition.Obstructed : SignCondition.Clear);
    }

    public void SetSignCondition(SignCondition cond)
    {
        bool showBlockers = (cond == SignCondition.Obstructed || cond == SignCondition.VisualClutter);
        bool showClutter = (cond == SignCondition.VisualClutter);

        foreach (var s in signs)
            if (s.key && s.blocker != null) s.blocker.SetActive(showBlockers);

        foreach (var c in clutterSigns)
            if (c != null) c.SetActive(showClutter);
    }

    public void Build()
    {
        var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseMat = tmp.GetComponent<Renderer>().sharedMaterial;
        Destroy(tmp);

        if (FindAnyObjectByType<Light>() == null)
        {
            var l = new GameObject("Directional Light").AddComponent<Light>();
            l.type = LightType.Directional;
            l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        var root = new GameObject("Airport").transform;
        floors = Sub(root, "Floors");
        walls = Sub(root, "Walls");
        props = Sub(root, "Props");
        signsRoot = Sub(root, "Signage");
        clutterRoot = Sub(root, "ClutterSignage");

        // ---- Floor and zone colour patches ----
        Box("Floor", new Vector3(-12.5f, -0.1f, 30f), new Vector3(140f, 0.2f, 80f), floorCol, floors, true);
        Patch(-15, 0, 15, 20, C(.72f, .76f, .86f));     // Check-in hall
        Patch(-3, 20, 3, 32, C(.88f, .62f, .62f));      // Security lane
        Patch(-3, 32, 3, 60, C(.82f, .82f, .72f));      // Concourse + arrival gate
        Patch(3, 40, 18, 46, C(.82f, .82f, .72f));      // East corridor
        Patch(18, 34, 34, 52, C(.62f, .82f, .66f));     // Gate A1 & Gate A2 lounge
        Patch(-30, 40, -3, 46, C(.82f, .82f, .72f));    // West corridor
        Patch(-50, 30, -30, 56, C(.92f, .82f, .55f));   // Baggage hall
        // Highlight Gate A2 wrong zone patch subtly
        Patch(20, 34.2f, 33.5f, 38.2f, C(.80f, .72f, .64f));

        // ---- Walls (x1,z1,x2,z2) ----
        Wall(-15, 0, 15, 0); Wall(-15, 0, -15, 20); Wall(15, 0, 15, 20);
        Wall(-15, 20, -3, 20); Wall(3, 20, 15, 20);                       // hall north wall with opening
        Wall(-3, 20, -3, 40); Wall(-3, 46, -3, 60);                       // corridor west wall (gap = junction)
        Wall(3, 20, 3, 40); Wall(3, 46, 3, 60);                           // corridor east wall
        Wall(-3, 60, 3, 60);                                              // end wall
        Wall(3, 40, 18, 40); Wall(3, 46, 18, 46);                         // east corridor
        Wall(18, 34, 34, 34); Wall(18, 52, 34, 52); Wall(34, 34, 34, 52);
        Wall(18, 34, 18, 40); Wall(18, 46, 18, 52);                       // gate room
        Wall(-30, 40, -3, 40); Wall(-30, 46, -3, 46);                     // west corridor
        Wall(-50, 30, -30, 30); Wall(-50, 56, -30, 56); Wall(-50, 30, -50, 56);
        Wall(-30, 30, -30, 40); Wall(-30, 46, -30, 56);                   // baggage hall

        // ---- Check-In Hall Props ----
        foreach (float x in new[] { -12f, -8f, 8f, 12f })
        {
            Box("CheckInDesk", new Vector3(x, 0.5f, 18f), new Vector3(3f, 1f, 1f), C(.25f, .3f, .45f), props, true);
            Box("Monitor", new Vector3(x, 1.2f, 18f), new Vector3(0.55f, 0.4f, 0.1f), C(.1f, .1f, .12f), props, false);
            Box("BagScale", new Vector3(x + 1.85f, 0.15f, 18f), new Vector3(0.7f, 0.3f, 0.9f), C(.18f, .18f, .2f), props, true);
        }
        // Check-In Queue Stanchions (visual guide rails in front of desks)
        BuildQueueBelt(new Vector3(-13.5f, 0f, 14f), new Vector3(-6.5f, 0f, 14f), C(.15f, .3f, .65f));
        BuildQueueBelt(new Vector3(6.5f, 0f, 14f), new Vector3(13.5f, 0f, 14f), C(.15f, .3f, .65f));

        // ---- Security Checkpoint Queue Lanes & Screening Arch (Requirement F10) ----
        // Left & Right Queue Stanchion Lanes guiding passenger smoothly down the middle (x in -1.1..1.1 is clear)
        BuildQueueBelt(new Vector3(-1.8f, 0f, 21f), new Vector3(-1.8f, 0f, 25f), C(.75f, .15f, .15f));
        BuildQueueBelt(new Vector3(1.8f, 0f, 21f), new Vector3(1.8f, 0f, 25f), C(.75f, .15f, .15f));
        BuildQueueBelt(new Vector3(-2.6f, 0f, 22f), new Vector3(-1.8f, 0f, 22f), C(.75f, .15f, .15f));
        BuildQueueBelt(new Vector3(1.8f, 0f, 22f), new Vector3(2.6f, 0f, 22f), C(.75f, .15f, .15f));

        // Walk-through Scanner Arch & X-Ray Conveyors
        Box("ScannerPostL", new Vector3(-1.2f, 1.2f, 27f), new Vector3(.3f, 2.4f, .3f), C(.2f, .2f, .25f), props, true);
        Box("ScannerPostR", new Vector3(1.2f, 1.2f, 27f), new Vector3(.3f, 2.4f, .3f), C(.2f, .2f, .25f), props, true);
        Box("ScannerBar", new Vector3(0f, 2.55f, 27f), new Vector3(2.7f, .3f, .3f), C(.2f, .2f, .25f), props, false);
        Box("ScannerLight", new Vector3(0f, 2.55f, 26.82f), new Vector3(0.8f, 0.12f, 0.05f), C(.1f, .9f, .3f), props, false);
        Box("ConveyorL", new Vector3(-2.4f, .45f, 27f), new Vector3(1.2f, .9f, 3.2f), C(.15f, .15f, .15f), props, true);
        Box("ConveyorR", new Vector3(2.4f, .45f, 27f), new Vector3(1.2f, .9f, 3.2f), C(.15f, .15f, .15f), props, true);
        Box("XRayTunnel", new Vector3(-2.4f, 1.25f, 27f), new Vector3(1.1f, 0.8f, 1.4f), C(.28f, .30f, .35f), props, false);
        Box("SecurityTray1", new Vector3(2.4f, 0.95f, 26.2f), new Vector3(0.5f, 0.1f, 0.4f), C(.35f, .35f, .4f), props, false);
        Box("SecurityTray2", new Vector3(2.4f, 0.95f, 27.8f), new Vector3(0.5f, 0.1f, 0.4f), C(.35f, .35f, .4f), props, false);

        // ---- Interactive Kiosks ----
        foreach (var k in AirportData.Kiosks)
        {
            Box("KioskPillar", k + new Vector3(0f, .65f, 0f), new Vector3(.6f, 1.3f, .6f), C(0f, .75f, .85f), props, true);
            Box("KioskScreen", k + new Vector3(0f, 1.15f, 0f), new Vector3(.68f, .45f, .68f), C(.05f, .15f, .25f), props, false);
        }

        // ---- Baggage Claim Carousels (1, 2, 3) & Luggage Props ----
        for (int i = 0; i < 3; i++)
        {
            float cz = 36f + 7f * i;
            Box("Carousel" + (i + 1), new Vector3(-42f, .4f, cz), new Vector3(8f, .8f, 2.4f), C(.2f, .2f, .22f), props, true);
            Box("CarouselCenter" + (i + 1), new Vector3(-42f, .65f, cz), new Vector3(6.2f, .35f, 1.0f), C(.32f, .32f, .36f), props, false);
            // Suitcase props on the belt
            Box("SuitcaseA_" + (i + 1), new Vector3(-39.5f, .88f, cz + 0.7f), new Vector3(0.65f, 0.22f, 0.45f),
                i == 2 ? C(.15f, .65f, .3f) : C(.65f, .2f, .2f), props, false);
            Box("SuitcaseB_" + (i + 1), new Vector3(-44.2f, .88f, cz - 0.7f), new Vector3(0.55f, 0.25f, 0.42f),
                C(.2f, .35f, .65f), props, false);
        }

        // ---- East Wing: Gate A1 (Target) & Gate A2 (Distractor) ----
        foreach (float x in new[] { 22f, 26f, 30f })
        {
            Box("BenchNorth", new Vector3(x, .25f, 41.2f), new Vector3(2.2f, .5f, .6f), C(.3f, .3f, .35f), props, true);
            Box("BenchSouth", new Vector3(x, .25f, 38.8f), new Vector3(2.2f, .5f, .6f), C(.3f, .3f, .35f), props, true);
        }
        // Gate A1 Boarding Desk (North-East corner)
        Box("GateA1Desk", new Vector3(30f, .5f, 50.5f), new Vector3(2.5f, 1f, .8f), C(.15f, .45f, .30f), props, true);
        Box("GateA1Door", new Vector3(33.78f, 1.2f, 47.5f), new Vector3(.1f, 2.4f, 2.6f), C(.2f, .55f, .35f), props, false);

        // Gate A2 Boarding Desk (South-East corner - Distractor gate for flight 6E 517)
        Box("GateA2Desk", new Vector3(30f, .5f, 35.2f), new Vector3(2.5f, 1f, .8f), C(.45f, .28f, .22f), props, true);
        Box("GateA2Door", new Vector3(33.78f, 1.2f, 36.5f), new Vector3(.1f, 2.4f, 2.2f), C(.55f, .35f, .2f), props, false);

        // Arrival Gate Door
        Box("ArrivalDoor", new Vector3(0f, 1.2f, 59.7f), new Vector3(3f, 2.4f, .1f), C(.9f, .5f, .1f), props, false);

        // Ceiling Light Panels for spatial depth & VR comfort
        float[] lightZ = { 5f, 14f, 24f, 35f, 43f, 53f };
        foreach (float lz in lightZ)
            Box("CeilingLight", new Vector3(0f, 3.92f, lz), new Vector3(1.8f, 0.06f, 0.6f), C(.98f, .98f, 1f), props, false);

        // ---- Standard Airport Signage ----
        // yaw = direction the READER looks: 0 = north(+Z), 90 = east, 180 = south, 270 = west
        Sign("CHECK-IN HALL\nCheck-in desks ahead  ^\nSecurity & gates  ^", V(0, 3.2f, 9), 0, 5.5f, 1.5f, false, signBlue, Color.white);
        Sign("CHECK-IN DESKS 1-2\nAI 202 DELHI", V(-10, 3.2f, 17.3f), 0, 4.5f, 1.1f, false, signBlue, Color.white);
        Sign("CHECK-IN DESKS 3-4\n6E 517 CHENNAI / UK 815", V(10, 3.2f, 17.3f), 0, 4.5f, 1.1f, false, signBlue, Color.white);
        Sign("SECURITY  ^\nDEPARTURES  ^", V(0, 3.4f, 19.6f), 0, 4.5f, 1.1f, false, signGreen, Color.white);
        Sign(AirportData.DeparturesBoard, V(-14.78f, 2.5f, 10), 270, 5.8f, 2.3f, false, fidsDark, amber);
        Sign("SECURITY CHECKPOINT\nFollow queue lane - Boarding pass ready", V(0, 3.3f, 23), 0, 4.8f, 1.1f, false, signGreen, Color.white);
        Sign("DEPARTURES / GATES  ^", V(0, 3.3f, 30.5f), 0, 4.8f, .9f, false, signGreen, Color.white);
        Sign(AirportData.DeparturesBoard, V(2.78f, 2.8f, 36), 90, 4.0f, 1.7f, false, fidsDark, amber);
        Sign("INFO KIOSK (Press E)", V(2.4f, 1.85f, 36), 90, 1.4f, .42f, false, signGreen, Color.white);
        Sign("INFO KIOSK (Press E)", V(13.5f, 1.95f, 5), 0, 1.6f, .42f, false, signGreen, Color.white);

        // KEY decision-point signs (hidden by Obstructed / VisualClutter conditions)
        Sign("<< BAGGAGE CLAIM\nGATES A1 - A2 >>", V(0, 3.3f, 38.5f), 0, 5.4f, 1.4f, true, signBlue, Color.white);
        Sign("GATES A1 - A2 <<\nBAGGAGE CLAIM >>", V(0, 3.3f, 47.5f), 180, 5.4f, 1.4f, true, signBlue, Color.white);
        Sign("GATES A1 - A2  ^", V(9, 3.3f, 43), 90, 3.8f, 1f, true, signBlue, Color.white);
        Sign("BAGGAGE CLAIM  ^", V(-9, 3.3f, 43), 270, 4.2f, 1f, true, signBlue, Color.white);
        Sign("ARRIVALS / BAGGAGE CLAIM  ^\nFollow corridor to junction", V(0, 3.3f, 54.5f), 180, 5.2f, 1.2f, true, signBlue, Color.white);

        // Gate Lounge signs (Gate A1 vs Gate A2)
        Sign("<< GATE A1 (AI 202 DELHI)\nGATE A2 (6E 517 CHENNAI) >>", V(20.5f, 3.2f, 43), 90, 5.2f, 1.3f, false, signBlue, Color.white);
        Sign("GATE A1\nAI 202 DELHI - BOARDING", V(33.75f, 3f, 47.5f), 90, 5f, 1.4f, false, signGreen, Color.white);
        Sign("GATE A2\n6E 517 CHENNAI - ON TIME", V(33.75f, 3f, 36.5f), 90, 5f, 1.4f, false, C(.55f, .25f, .1f), Color.white);

        // Baggage Hall signs (Carousels 1, 2, 3 + Arrivals FIDS)
        Sign("BAGGAGE CLAIM HALL\nCheck Arrivals FIDS for Carousel #", V(-49.75f, 3.3f, 43), 270, 6.2f, 1.3f, false, signBlue, Color.white);
        Sign(AirportData.ArrivalsBoard, V(-40, 2.6f, 55.75f), 0, 6.2f, 2.3f, false, fidsDark, amber);

        string[] carouselLabels =
        {
            "CAROUSEL 1\n6E 622 BENGALURU",
            "CAROUSEL 2\nUK 811 KOLKATA",
            "CAROUSEL 3\nAI 305 MUMBAI"
        };
        for (int i = 0; i < 3; i++)
            Sign(carouselLabels[i], V(-42, 2.3f, 36f + 7f * i), 270, 3.4f, .85f, false, signGreen, Color.white);

        // ---- Visual Clutter / Competing Commercial Banners (Scenario 2 Variant) ----
        ClutterSign("DUTY FREE MEGA SALE 50% OFF!\n<< PERFUMES | ELECTRONICS >>", V(0, 2.4f, 37.8f), 0, 5.2f, 1.0f, clutterPink);
        ClutterSign("VIP EXECUTIVE LOUNGE ^\nFAST-TRACK UPGRADE DESK >>", V(-1.6f, 2.6f, 41.2f), 45, 3.8f, 1.1f, clutterPurple);
        ClutterSign("STAR CAFE & BAKERY <<\nCURRENCY EXCHANGE / ATM >>", V(1.6f, 2.6f, 41.2f), 315, 3.8f, 1.1f, clutterOrange);
        ClutterSign("TERMINAL TRANSFER B1-B12 ^\nSHUTTLE BUS LEVEL 0 <<", V(0, 2.4f, 48.2f), 180, 5.2f, 1.0f, clutterPurple);
        ClutterSign("LUXURY WATCHES & GIFTS\nLIMITED TIME OFFER", V(5.5f, 2.5f, 43f), 90, 3.5f, 1.1f, clutterPink);
        ClutterSign("TAX REFUND COUNTER <<\nHOTEL SHUTTLES & TAXI >>", V(-5.5f, 2.5f, 43f), 270, 3.5f, 1.1f, clutterOrange);

        SetSignCondition(SignCondition.Clear);
    }

    // ---------- helpers ----------
    static Vector3 V(float x, float y, float z) { return new Vector3(x, y, z); }

    Transform Sub(Transform parent, string n)
    {
        var g = new GameObject(n); g.transform.SetParent(parent, false); return g.transform;
    }

    Material Mat(Color c) { var m = new Material(baseMat); m.color = c; return m; }

    GameObject Box(string name, Vector3 center, Vector3 scale, Color c, Transform parent, bool collider)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.position = center;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = Mat(c);
        if (!collider) Destroy(g.GetComponent<Collider>());
        return g;
    }

    void BuildQueueBelt(Vector3 p1, Vector3 p2, Color beltCol)
    {
        Box("StanchionPost", p1 + new Vector3(0f, 0.45f, 0f), new Vector3(0.12f, 0.9f, 0.12f), C(.2f, .2f, .22f), props, false);
        Box("StanchionPost", p2 + new Vector3(0f, 0.45f, 0f), new Vector3(0.12f, 0.9f, 0.12f), C(.2f, .2f, .22f), props, false);
        Vector3 mid = (p1 + p2) * 0.5f + new Vector3(0f, 0.78f, 0f);
        float len = Vector3.Distance(p1, p2);
        var belt = Box("QueueBelt", mid, new Vector3(0.05f, 0.1f, len), beltCol, props, false);
        belt.transform.LookAt(p2 + new Vector3(0f, 0.78f, 0f));
    }

    void Wall(float x1, float z1, float x2, float z2)
    {
        const float t = 0.3f, h = 4f, pad = 0.15f;
        if (Mathf.Approximately(x1, x2))
            Box("Wall", new Vector3(x1, h / 2f, (z1 + z2) / 2f), new Vector3(t, h, Mathf.Abs(z2 - z1) + 2 * pad), wallCol, walls, true);
        else
            Box("Wall", new Vector3((x1 + x2) / 2f, h / 2f, z1), new Vector3(Mathf.Abs(x2 - x1) + 2 * pad, h, t), wallCol, walls, true);
    }

    void Patch(float x1, float z1, float x2, float z2, Color c)
    {
        Box("FloorPatch", new Vector3((x1 + x2) / 2f, 0.01f, (z1 + z2) / 2f), new Vector3(x2 - x1, 0.02f, z2 - z1), c, floors, false);
    }

    TextMeshPro MakeText(Transform parent, string text, Vector3 localPos, Vector2 size, Color col)
    {
        var g = new GameObject("Text");
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        g.transform.localRotation = Quaternion.identity;
        var t = g.AddComponent<TextMeshPro>();
        t.rectTransform.sizeDelta = size;
        t.text = text;
        t.color = col;
        t.alignment = TextAlignmentOptions.Center;
        t.enableAutoSizing = true;
        t.fontSizeMin = 0.5f;
        t.fontSizeMax = 8f;
        return t;
    }

    void Sign(string text, Vector3 pos, float yaw, float w, float h, bool key, Color bg, Color fg)
    {
        var r = new GameObject("Sign_" + text.Split('\n')[0]).transform;
        r.SetParent(signsRoot, false);
        r.position = pos;
        r.rotation = Quaternion.Euler(0f, yaw, 0f);

        var board = Box("Board", pos, new Vector3(w, h, 0.1f), bg, r, false);
        board.transform.localPosition = Vector3.zero;
        board.transform.localRotation = Quaternion.identity;

        MakeText(r, text, new Vector3(0f, 0f, -0.07f), new Vector2(w * 0.95f, h * 0.9f), fg);

        var info = new SignInfo { key = key };
        if (key)
        {
            var bl = new GameObject("Blocker");
            bl.transform.SetParent(r, false);
            var panel = Box("Panel", pos, new Vector3(w + 0.1f, h + 0.1f, 0.06f), blockerCol, bl.transform, false);
            panel.transform.localPosition = new Vector3(0f, 0f, -0.14f);
            panel.transform.localRotation = Quaternion.identity;
            MakeText(bl.transform, "ADVERTISEMENT\n[SIGN OBSTRUCTED]", new Vector3(0f, 0f, -0.19f), new Vector2(w * 0.9f, h * 0.65f), Color.white);
            bl.SetActive(false);
            info.blocker = bl;
        }
        signs.Add(info);
    }

    void ClutterSign(string text, Vector3 pos, float yaw, float w, float h, Color bg)
    {
        var r = new GameObject("Clutter_" + text.Split('\n')[0]).transform;
        r.SetParent(clutterRoot, false);
        r.position = pos;
        r.rotation = Quaternion.Euler(0f, yaw, 0f);

        var board = Box("PromoBoard", pos, new Vector3(w, h, 0.08f), bg, r, false);
        board.transform.localPosition = Vector3.zero;
        board.transform.localRotation = Quaternion.identity;

        MakeText(r, text, new Vector3(0f, 0f, -0.06f), new Vector2(w * 0.92f, h * 0.88f), Color.yellow);
        r.gameObject.SetActive(false);
        clutterSigns.Add(r.gameObject);
    }
}
