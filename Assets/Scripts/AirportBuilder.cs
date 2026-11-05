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
    Transform floors, walls, roof, props, signsRoot, clutterRoot;

    static Color C(float r, float g, float b, float a = 1f) { return new Color(r, g, b, a); }

    // Sophisticated, contemporary non-white architectural palette
    readonly Color wallCol = C(.27f, .29f, .33f);          // Sleek architectural slate / graphite (NEVER WHITE)
    readonly Color wallTrimCol = C(.14f, .15f, .18f);      // Dark graphite baseboard trim
    readonly Color wallAccentNavy = C(.15f, .22f, .35f);   // Aviation sapphire accent
    readonly Color wallAccentSteel = C(.21f, .23f, .27f);  // Matte gunmetal steel
    readonly Color wallAccentCafe = C(.38f, .26f, .21f);   // Warm terracotta bistro accent
    readonly Color wallAccentLounge = C(.12f, .17f, .28f); // Royal executive lounge accent
    readonly Color wallAccentRetail = C(.18f, .16f, .21f); // Luxury boutique dark espresso
    readonly Color floorCol = C(.46f, .47f, .50f);         // Neutral terrazzo airport floor
    readonly Color tarmacCol = C(.26f, .27f, .29f);        // Heavy runway tarmac concrete
    readonly Color roofCol = C(.22f, .24f, .27f);          // Dark architectural ceiling panels
    readonly Color roofTrussCol = C(.16f, .18f, .21f);     // Heavy steel I-beam trusses

    // Signage colors
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

        RenderSettings.ambientLight = C(.62f, .64f, .70f);
        RenderSettings.ambientIntensity = 1.3f;

        if (FindAnyObjectByType<Light>() == null)
        {
            var l = new GameObject("Directional Light").AddComponent<Light>();
            l.type = LightType.Directional;
            l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            l.color = C(.98f, .96f, .92f);
            l.intensity = 1.6f;
        }

        var root = new GameObject("Airport").transform;
        floors = Sub(root, "Floors");
        walls = Sub(root, "Walls");
        roof = Sub(root, "Roof");
        props = Sub(root, "Props");
        signsRoot = Sub(root, "Signage");
        clutterRoot = Sub(root, "ClutterSignage");

        // =========================================================================
        // 1. FLOOR & ZONE COLOR PATCHES
        // =========================================================================
        Box("Floor", new Vector3(-12f, -0.1f, 55f), new Vector3(150f, 0.2f, 130f), floorCol, floors, true);

        // Exterior Runway & Apron Tarmac Slab outside East Gate Lounges
        Box("ApronTarmac", new Vector3(75f, -0.08f, 45f), new Vector3(85f, 0.2f, 75f), tarmacCol, floors, true);
        // Yellow taxiway centerlines and aircraft stand markings
        Box("TaxiwayLine1", new Vector3(54f, 0.02f, 44f), new Vector3(0.4f, 0.02f, 65f), C(.95f, .8f, .1f), floors, false);
        Box("TaxiwayLine2", new Vector3(75f, 0.02f, 44f), new Vector3(40f, 0.02f, 0.4f), C(.95f, .8f, .1f), floors, false);
        Box("AircraftStopBar", new Vector3(48f, 0.02f, 44f), new Vector3(0.6f, 0.02f, 6.0f), C(.95f, .2f, .2f), floors, false);
        // Runway white threshold stripes (piano keys)
        for (int i = 0; i < 8; i++)
            Box("RunwayStripe_" + i, new Vector3(92f, 0.02f, 25f + i * 4.5f), new Vector3(12f, 0.02f, 1.4f), Color.white, floors, false);

        // Original terminal zone floor patches
        Patch(-15, 0, 15, 20, C(.68f, .72f, .82f));        // Check-in hall
        Patch(-3, 20, 3, 32, C(.82f, .58f, .58f));         // Security lane
        Patch(-3, 32, 3, 60, C(.78f, .78f, .70f));         // Concourse corridor
        Patch(3, 40, 18, 46, C(.78f, .78f, .70f));         // East corridor
        Patch(18, 34, 34, 52, C(.58f, .78f, .62f));        // Gate A1 & Gate A2 lounge
        Patch(-30, 40, -3, 46, C(.78f, .78f, .70f));       // West corridor
        Patch(-50, 30, -30, 56, C(.88f, .78f, .52f));      // Baggage hall
        Patch(20, 34.2f, 33.5f, 38.2f, C(.76f, .68f, .60f));// Gate A2 distractor patch

        // Arrivals Exit concourse floor patch
        Patch(-68, 34, -50, 52, C(.38f, .42f, .46f));      // Arrivals Exit & Ground Transport

        // Expanded North Terminal floor patches
        Patch(-10, 60, 10, 86, C(.52f, .54f, .58f));       // Central Departure Plaza (Polished Terrazzo)
        Patch(-5, 70, 5, 80, C(.38f, .42f, .48f));         // Central Plaza Feature Ring
        Patch(-34, 60, -10, 86, C(.40f, .38f, .44f));      // Duty Free Galleria (Polished Luxury Marble)
        Patch(10, 60, 34, 86, C(.55f, .44f, .36f));        // Food Court & Skyline Cafe (Warm Bistro Tile)
        Patch(-8, 86, 8, 92, C(.36f, .40f, .44f));         // Restrooms & Amenities Hub (Slate Tile)
        Patch(-34, 86, 6, 110, C(.32f, .42f, .38f));       // Concourse B Gates Lounge (Airport Carpet)
        Patch(6, 86, 34, 110, C(.18f, .24f, .36f));        // VIP SkyClub Lounge (Executive Royal Carpet)

        // =========================================================================
        // 2. WALLS (SLATE GRAPHITE NON-WHITE WITH KICKPLATE TRIMS)
        // =========================================================================
        // --- South Check-In Hall ---
        Wall(-15, 0, 15, 0, wallAccentNavy);
        Wall(-15, 0, -15, 20); Wall(15, 0, 15, 20);
        Wall(-15, 20, -3, 20); Wall(3, 20, 15, 20);

        // --- Security Lane ---
        Wall(-3, 20, -3, 40, wallAccentSteel);
        Wall(3, 20, 3, 40, wallAccentSteel);

        // --- Central Concourse & Corridors ---
        Wall(-3, 46, -3, 60); Wall(3, 46, 3, 60);
        Wall(3, 40, 18, 40); Wall(3, 46, 18, 46);           // East corridor to Gates A1/A2
        Wall(-30, 40, -3, 40); Wall(-30, 46, -3, 46);       // West corridor to Baggage

        // --- Gate A Lounge (East Wing) ---
        Wall(18, 34, 34, 34, wallAccentCafe);
        Wall(18, 52, 34, 52, wallAccentCafe);
        Wall(18, 34, 18, 40); Wall(18, 46, 18, 52);
        // EAST SIDE (x = 34, z = 34..52): Glass panel removed! Open observation guardrail so the plane is 100% visible!
        BuildOpenObservationRailing(34f, 34f, 52f);

        // --- Baggage Claim Hall (West Wing) ---
        Wall(-50, 30, -30, 30, wallAccentSteel);
        Wall(-50, 56, -30, 56, wallAccentSteel);
        Wall(-30, 30, -30, 40); Wall(-30, 46, -30, 56);
        // West wall with wide opening (z = 44..50) aligned with open aisle for Arrivals Exit
        Wall(-50, 30, -50, 44, wallAccentSteel);
        Wall(-50, 50, -50, 56, wallAccentSteel);

        // --- Arrivals Exit & Ground Transportation Concourse (x: -68..-50, z: 34..52) ---
        Wall(-68, 34, -50, 34, wallAccentNavy);
        Wall(-68, 52, -50, 52, wallAccentNavy);
        Wall(-68, 34, -68, 52, wallCol);

        // --- Expanded North Terminal Perimeter Walls (Seamless connection at z = 60, x = ±3) ---
        Wall(-34, 60, -3, 60); Wall(3, 60, 34, 60);
        Wall(-34, 60, -34, 110);
        Wall(34, 60, 34, 110);
        Wall(-34, 110, 34, 110, wallCol);

        // --- Interior Partitions of Expanded Areas (with wide, unobstructed archways) ---
        Wall(-34, 86, -20, 86, wallAccentRetail);
        Wall(-10, 86, -8, 86, wallAccentRetail);
        Wall(8, 86, 12, 86, wallAccentCafe);
        Wall(22, 86, 34, 86, wallAccentCafe);

        // Restroom corridor side walls
        Wall(-8, 86, -8, 92); Wall(8, 86, 8, 92);

        // VIP SkyClub Lounge partition wall with wide entrance portal (z = 95..102 open)
        Wall(6, 92, 6, 95, wallAccentLounge);
        Wall(6, 102, 6, 110, wallAccentLounge);

        // Concourse B Gate Divider near north wall (between Gate B1 and Gate B2 desks)
        Wall(-16, 102, -16, 110, wallAccentSteel);

        // =========================================================================
        // 3. COMPLETE SEAMLESS ROOF & CEILING STRUCTURE (EXACT ROOM BOUNDS)
        // =========================================================================
        BuildRoof();

        // =========================================================================
        // 4. CHECK-IN HALL PROPS & FIXTURES
        // =========================================================================
        foreach (float x in new[] { -12f, -8f, 8f, 12f })
        {
            Box("CheckInDesk", new Vector3(x, 0.5f, 18f), new Vector3(3f, 1f, 1f), C(.18f, .24f, .38f), props, true);
            Box("Monitor", new Vector3(x, 1.2f, 18f), new Vector3(0.55f, 0.4f, 0.1f), C(.1f, .1f, .12f), props, false);
            Box("BagScale", new Vector3(x + 1.85f, 0.15f, 18f), new Vector3(0.7f, 0.3f, 0.9f), C(.16f, .16f, .18f), props, true);
        }
        BuildQueueBelt(new Vector3(-13.5f, 0f, 14f), new Vector3(-6.5f, 0f, 14f), C(.15f, .3f, .65f));
        BuildQueueBelt(new Vector3(6.5f, 0f, 14f), new Vector3(13.5f, 0f, 14f), C(.15f, .3f, .65f));

        // =========================================================================
        // 5. SECURITY CHECKPOINT
        // =========================================================================
        BuildQueueBelt(new Vector3(-1.8f, 0f, 21f), new Vector3(-1.8f, 0f, 25f), C(.75f, .15f, .15f));
        BuildQueueBelt(new Vector3(1.8f, 0f, 21f), new Vector3(1.8f, 0f, 25f), C(.75f, .15f, .15f));
        BuildQueueBelt(new Vector3(-2.6f, 0f, 22f), new Vector3(-1.8f, 0f, 22f), C(.75f, .15f, .15f));
        BuildQueueBelt(new Vector3(1.8f, 0f, 22f), new Vector3(2.6f, 0f, 22f), C(.75f, .15f, .15f));

        Box("ScannerPostL", new Vector3(-1.2f, 1.2f, 27f), new Vector3(.3f, 2.4f, .3f), C(.2f, .2f, .25f), props, true);
        Box("ScannerPostR", new Vector3(1.2f, 1.2f, 27f), new Vector3(.3f, 2.4f, .3f), C(.2f, .2f, .25f), props, true);
        Box("ScannerBar", new Vector3(0f, 2.55f, 27f), new Vector3(2.7f, .3f, .3f), C(.2f, .2f, .25f), props, false);
        Box("ScannerLight", new Vector3(0f, 2.55f, 26.82f), new Vector3(0.8f, 0.12f, 0.05f), C(.1f, .9f, .3f), props, false);
        Box("ConveyorL", new Vector3(-2.4f, .45f, 27f), new Vector3(1.2f, .9f, 3.2f), C(.15f, .15f, .15f), props, true);
        Box("ConveyorR", new Vector3(2.4f, .45f, 27f), new Vector3(1.2f, .9f, 3.2f), C(.15f, .15f, .15f), props, true);
        Box("XRayTunnel", new Vector3(-2.4f, 1.25f, 27f), new Vector3(1.1f, 0.8f, 1.4f), C(.28f, .30f, .35f), props, false);
        Box("SecurityTray1", new Vector3(2.4f, 0.95f, 26.2f), new Vector3(0.5f, 0.1f, 0.4f), C(.35f, .35f, .4f), props, false);
        Box("SecurityTray2", new Vector3(2.4f, 0.95f, 27.8f), new Vector3(0.5f, 0.1f, 0.4f), C(.35f, .35f, .4f), props, false);

        // =========================================================================
        // 6. INTERACTIVE KIOSKS
        // =========================================================================
        foreach (var k in AirportData.Kiosks)
        {
            Box("KioskPillar", k + new Vector3(0f, .65f, 0f), new Vector3(.6f, 1.3f, .6f), C(0f, .70f, .85f), props, true);
            Box("KioskScreen", k + new Vector3(0f, 1.15f, 0f), new Vector3(.68f, .45f, .68f), C(.05f, .15f, .25f), props, false);
        }

        // =========================================================================
        // 7. BAGGAGE CLAIM MOTORIZED MOVING CAROUSELS (WEST WING)
        // =========================================================================
        Color[] suitcaseColors =
        {
            C(.75f, .18f, .18f), C(.18f, .35f, .75f), C(.15f, .65f, .3f),
            C(.85f, .75f, .2f), C(.25f, .25f, .28f), C(.65f, .25f, .7f),
            C(.85f, .45f, .15f), C(.45f, .55f, .65f)
        };

        for (int i = 0; i < 3; i++)
        {
            float cz = 36f + 7f * i;
            var cRoot = new GameObject("Carousel_" + (i + 1)).transform;
            cRoot.SetParent(props, false);

            Box("CarouselBase", new Vector3(-42f, .4f, cz), new Vector3(8f, .8f, 2.4f), C(.18f, .18f, .20f), cRoot, true);
            Box("CarouselCenter", new Vector3(-42f, .65f, cz), new Vector3(6.2f, .35f, 1.0f), C(.28f, .28f, .32f), cRoot, false);
            Box("CarouselBumper", new Vector3(-42f, .42f, cz), new Vector3(8.15f, 0.12f, 2.55f), C(.10f, .10f, .12f), cRoot, false);

            var conveyor = cRoot.gameObject.AddComponent<CarouselConveyor>();
            conveyor.center = new Vector3(-42f, 0f, cz);
            conveyor.halfX = 3.6f;
            conveyor.halfZ = 0.9f;
            conveyor.speed = 0.85f + i * 0.1f;
            conveyor.elevation = 0.88f;

            for (int s = 0; s < 8; s++)
            {
                Color sCol = (i == 2 && s == 3) ? C(.15f, .65f, .3f) : suitcaseColors[(s + i * 2) % suitcaseColors.Length];
                var bag = Box("MovingSuitcase_" + i + "_" + s, Vector3.zero, new Vector3(0.68f, 0.24f, 0.44f), sCol, cRoot, false);
                LocalBox("Handle", new Vector3(0f, 0.55f, 0f), new Vector3(0.35f, 0.2f, 0.14f), C(.15f, .15f, .15f), bag.transform);
                LocalBox("Tag", new Vector3(0.38f, 0.52f, 0f), new Vector3(0.12f, 0.08f, 0.25f), Color.white, bag.transform);

                conveyor.AddLuggage(bag.transform, s / 8f);
            }
        }

        // =========================================================================
        // 8. ARRIVALS EXIT & GROUND TRANSPORTATION CONCOURSE (x: -68..-50, z: 34..52)
        // =========================================================================
        // Customs Checkpoint Desks positioned to the sides of the z = 44..50 exit walkway
        Box("CustomsDesk1", new Vector3(-53.5f, 0.5f, 39.5f), new Vector3(1.2f, 1.0f, 2.8f), C(.18f, .22f, .32f), props, true);
        Box("CustomsDesk2", new Vector3(-53.5f, 0.5f, 50.5f), new Vector3(1.2f, 1.0f, 2.0f), C(.18f, .22f, .32f), props, true);

        // Public Arrivals Greeting Barriers with wide open central walkthrough (z = 43..49 open)
        BuildQueueBelt(new Vector3(-58.5f, 0f, 36f), new Vector3(-58.5f, 0f, 42.5f), C(.85f, .75f, .2f));
        BuildQueueBelt(new Vector3(-58.5f, 0f, 49.5f), new Vector3(-58.5f, 0f, 51.5f), C(.85f, .75f, .2f));

        // Car Rental Counters (Hertz, Avis, Enterprise)
        Box("CarRentalCounter", new Vector3(-64f, 0.5f, 50f), new Vector3(4.5f, 1.0f, 1.2f), C(.85f, .75f, .2f), props, true);
        Sign("HERTZ & AVIS CAR RENTALS", V(-64f, 2.4f, 51.6f), 0, 4.4f, 0.8f, false, C(.15f, .15f, .18f), Color.yellow);

        // Taxi Rank & Public Transportation Dispatch Kiosk
        Box("TaxiKiosk", new Vector3(-64f, 0.5f, 36f), new Vector3(4.5f, 1.0f, 1.2f), C(.18f, .35f, .65f), props, true);
        Sign("TAXI & GROUND TRANSPORTATION", V(-64f, 2.4f, 34.4f), 180, 4.6f, 0.8f, false, signBlue, Color.white);

        // Metro & Express Train Portal on far west wall
        Box("MetroPortal", new Vector3(-67.4f, 1.2f, 43f), new Vector3(0.6f, 2.4f, 4.0f), C(.12f, .14f, .18f), props, true);
        Box("EscalatorRailL", new Vector3(-66.8f, 0.5f, 41.5f), new Vector3(1.2f, 0.9f, 0.15f), C(.7f, .7f, .75f), props, false);
        Box("EscalatorRailR", new Vector3(-66.8f, 0.5f, 44.5f), new Vector3(1.2f, 0.9f, 0.15f), C(.7f, .7f, .75f), props, false);

        // =========================================================================
        // 9. EAST WING: GATES A1 & A2 LOUNGES + REALISTIC RUNWAY AIRLINER
        // =========================================================================
        foreach (float x in new[] { 21f, 25f, 29f })
        {
            Box("BenchNorth", new Vector3(x, .25f, 41.2f), new Vector3(2.2f, .5f, .6f), C(.25f, .27f, .32f), props, true);
            Box("BenchSouth", new Vector3(x, .25f, 38.8f), new Vector3(2.2f, .5f, .6f), C(.25f, .27f, .32f), props, true);
        }
        Box("GateA1Desk", new Vector3(29f, .5f, 50.5f), new Vector3(2.5f, 1f, .8f), C(.15f, .45f, .30f), props, true);
        Box("GateA2Desk", new Vector3(29f, .5f, 35.2f), new Vector3(2.5f, 1f, .8f), C(.45f, .28f, .22f), props, true);

        // REALISTIC SCALE COMMERCIAL AIRLINER PARKED OUTSIDE ON THE TARMAC (x = 54, z = 44)
        // Uses LocalBox so NO parts ever spawn at (0,0,0) above the player's respawn point!
        BuildAirliner(new Vector3(54f, 0f, 44f));

        // Ground Support Equipment on the Tarmac
        BuildGroundSupportEquipment(new Vector3(54f, 0f, 44f));

        // =========================================================================
        // 10. EXPANDED ZONE PROPS: CENTRAL DEPARTURE PLAZA (z = 60..86)
        // =========================================================================
        // Grand 4-Sided Flight Information Display (FIDS) Totem Pillar at (0, 2.2, 75)
        Box("FidsTotemCore", new Vector3(0f, 2.0f, 75f), new Vector3(1.6f, 4.0f, 1.6f), C(.12f, .14f, .18f), props, true);
        Sign(AirportData.DeparturesBoard, V(0f, 2.6f, 74.12f), 0, 3.8f, 2.2f, false, fidsDark, amber);
        Sign(AirportData.DeparturesBoard, V(0f, 2.6f, 75.88f), 180, 3.8f, 2.2f, false, fidsDark, amber);
        Sign("DEPARTURES & GATES\nGATE A1 - A2 << EAST\nGATES B1 - B2 >> NORTH\nVIP LOUNGE >> EAST", V(-0.88f, 2.6f, 75f), 270, 3.2f, 2.2f, false, signBlue, Color.white);
        Sign("TERMINAL SERVICES\n<< DUTY FREE GALLERIA\nFOOD COURT & CAFE >>\nATMs & RESTROOMS ^", V(0.88f, 2.6f, 75f), 90, 3.2f, 2.2f, false, signBlue, Color.white);

        // Atrium Planter Islands & Lounge Benches
        float[] planX = { -5.5f, 5.5f };
        float[] planZ = { 68f, 82f };
        foreach (float px in planX)
        {
            foreach (float pz in planZ)
            {
                Box("PlanterBox", new Vector3(px, 0.35f, pz), new Vector3(2.2f, 0.7f, 2.2f), C(.20f, .22f, .25f), props, true);
                Box("PlanterSoil", new Vector3(px, 0.72f, pz), new Vector3(2.0f, 0.08f, 2.0f), C(.15f, .12f, .08f), props, false);
                Prim(PrimitiveType.Cylinder, "PlanterTreeTrunk", new Vector3(px, 1.5f, pz), new Vector3(0.28f, 0.8f, 0.28f), C(.32f, .22f, .14f), props);
                Prim(PrimitiveType.Sphere, "PlanterCanopy", new Vector3(px, 2.5f, pz), new Vector3(1.6f, 1.4f, 1.6f), C(.14f, .50f, .24f), props);
                Box("BenchP1", new Vector3(px, 0.22f, pz - 1.5f), new Vector3(2.2f, 0.44f, 0.55f), C(.30f, .32f, .36f), props, true);
                Box("BenchP2", new Vector3(px, 0.22f, pz + 1.5f), new Vector3(2.2f, 0.44f, 0.55f), C(.30f, .32f, .36f), props, true);
            }
        }

        // =========================================================================
        // 11. DUTY FREE & EXPANDED STORE SECTIONS (WEST WING: x = -34..-10, z = 64..86)
        // =========================================================================
        // Store 1: "Luxe Perfumes & Cosmetics" (x = -28, z = 67..72)
        Box("StoreCounter1", new Vector3(-28f, 0.5f, 69.5f), new Vector3(1.2f, 1.0f, 3.5f), C(.15f, .16f, .22f), props, true);
        Box("Register1", new Vector3(-28f, 1.08f, 69.5f), new Vector3(0.5f, 0.25f, 0.4f), C(.1f, .1f, .12f), props, false);
        for (int i = 0; i < 3; i++)
        {
            float sz = 67f + i * 2.2f;
            Box("PerfumeShelf_" + i, new Vector3(-33.2f, 1.2f, sz), new Vector3(0.8f, 2.4f, 1.8f), C(.22f, .18f, .26f), props, true);
            Box("PerfumeBoxes_" + i, new Vector3(-32.75f, 1.4f, sz), new Vector3(0.3f, 0.45f, 1.5f), C(.85f, .75f, .35f), props, false);
        }

        // Store 2: "Grand Swiss Watchmaker & Chronographs" (x = -27, z = 74..78)
        Box("WatchShowcase1", new Vector3(-26f, 0.5f, 76f), new Vector3(1.2f, 1.0f, 2.2f), C(.12f, .14f, .18f), props, true);
        Box("WatchDisplayPad1", new Vector3(-26f, 1.04f, 76f), new Vector3(0.8f, 0.08f, 1.8f), C(.85f, .72f, .25f), props, false);
        Box("WatchShowcase2", new Vector3(-30f, 0.5f, 76f), new Vector3(1.2f, 1.0f, 2.2f), C(.12f, .14f, .18f), props, true);
        Box("WatchDisplayPad2", new Vector3(-30f, 1.04f, 76f), new Vector3(0.8f, 0.08f, 1.8f), C(.85f, .72f, .25f), props, false);

        // Store 3: "Relay Newsstand, Books & Travel Necessities"
        Box("Bookstand1", new Vector3(-16f, 0.6f, 82.5f), new Vector3(2.6f, 1.2f, 0.8f), C(.25f, .28f, .35f), props, true);
        Box("MagazineRack", new Vector3(-13.5f, 0.7f, 82.5f), new Vector3(0.5f, 1.4f, 2.2f), C(.65f, .15f, .15f), props, true);

        // Store 4: "TechZone Electronics & Sound Lounge" (x = -28, z = 80..85)
        Box("TechDisplay1", new Vector3(-24f, 0.45f, 82f), new Vector3(2.2f, 0.9f, 1.4f), C(.12f, .14f, .18f), props, true);
        Box("LaptopDemo1", new Vector3(-24f, 0.95f, 82f), new Vector3(0.5f, 0.08f, 0.4f), C(.8f, .8f, .85f), props, false);
        Box("TechDisplay2", new Vector3(-28f, 0.45f, 82f), new Vector3(2.2f, 0.9f, 1.4f), C(.12f, .14f, .18f), props, true);

        // Central Promotion Podium (positioned clearly in Duty Free atrium)
        Box("PromoPodium", new Vector3(-19f, 0.35f, 72f), new Vector3(2.4f, 0.7f, 2.4f), C(.45f, .15f, .25f), props, true);
        Box("PromoDisplay", new Vector3(-19f, 0.95f, 72f), new Vector3(1.1f, 0.5f, 1.1f), C(.9f, .8f, .2f), props, false);

        // =========================================================================
        // 12. FOOD COURT & SKYLINE CAFE (EAST WING: x = 10..34, z = 64..86)
        // =========================================================================
        Box("CafeCounter", new Vector3(26f, 0.5f, 83.5f), new Vector3(10f, 1.0f, 1.2f), C(.35f, .25f, .18f), props, true);
        Box("EspressoMachine", new Vector3(22.5f, 1.25f, 83.5f), new Vector3(1.1f, 0.5f, 0.65f), C(.75f, .75f, .8f), props, false);
        Box("PastryTray", new Vector3(26f, 1.06f, 83.5f), new Vector3(2.0f, 0.12f, 0.7f), C(.85f, .68f, .38f), props, false);
        Box("CashRegisterCafe", new Vector3(29.5f, 1.15f, 83.5f), new Vector3(0.6f, 0.3f, 0.4f), C(.1f, .1f, .12f), props, false);

        // Gelato & Artisan Snack Bar
        Box("GelatoCounter", new Vector3(14f, 0.5f, 83.5f), new Vector3(4.0f, 1.0f, 1.2f), C(.45f, .30f, .22f), props, true);

        // Cafe Dining Tables and Chairs
        float[] tableX = { 16f, 22f, 28f };
        float[] tableZ = { 68f, 75f };
        foreach (float tx in tableX)
        {
            foreach (float tz in tableZ)
            {
                Box("CafeTable", new Vector3(tx, 0.4f, tz), new Vector3(1.4f, 0.8f, 1.4f), C(.42f, .30f, .22f), props, true);
                Box("ChairNorth", new Vector3(tx, 0.25f, tz + 0.9f), new Vector3(0.5f, 0.5f, 0.5f), C(.20f, .22f, .26f), props, true);
                Box("ChairSouth", new Vector3(tx, 0.25f, tz - 0.9f), new Vector3(0.5f, 0.5f, 0.5f), C(.20f, .22f, .26f), props, true);
                Box("ChairEast", new Vector3(tx + 0.9f, 0.25f, tz), new Vector3(0.5f, 0.5f, 0.5f), C(.20f, .22f, .26f), props, true);
                Box("ChairWest", new Vector3(tx - 0.9f, 0.25f, tz), new Vector3(0.5f, 0.5f, 0.5f), C(.20f, .22f, .26f), props, true);
            }
        }

        // =========================================================================
        // 13. AMENITIES & RESTROOMS HUB (z = 86..92, x = -8..8)
        // =========================================================================
        // Positioned flush against the side walls (x = ±6.8) so center walkway (x = -4..4) is 100% clear!
        Box("ForexDesk", new Vector3(-6.6f, 0.55f, 89f), new Vector3(1.4f, 1.1f, 2.8f), C(.12f, .24f, .48f), props, true);
        for (int i = 0; i < 2; i++)
        {
            float az = 88f + i * 2.0f;
            Box("ATM_" + i, new Vector3(6.8f, 1.0f, az), new Vector3(0.8f, 2.0f, 0.8f), C(.15f, .35f, .25f), props, true);
            Box("ATMScreen_" + i, new Vector3(6.35f, 1.25f, az), new Vector3(0.06f, 0.45f, 0.5f), C(.1f, .85f, .4f), props, false);
        }

        // =========================================================================
        // 14. CONCOURSE B (GATES B1 & B2: z = 92..110, x = -34..5)
        // =========================================================================
        Box("GateB1Desk", new Vector3(-24f, 0.5f, 106.5f), new Vector3(2.8f, 1.0f, 0.9f), C(.15f, .42f, .30f), props, true);
        Box("GateB1Door", new Vector3(-28f, 1.2f, 109.8f), new Vector3(2.6f, 2.4f, 0.1f), C(.20f, .55f, .35f), props, false);
        BuildQueueBelt(new Vector3(-26.5f, 0f, 104f), new Vector3(-21.5f, 0f, 104f), C(.15f, .45f, .30f));

        Box("GateB2Desk", new Vector3(-8f, 0.5f, 106.5f), new Vector3(2.8f, 1.0f, 0.9f), C(.18f, .32f, .55f), props, true);
        Box("GateB2Door", new Vector3(-4f, 1.2f, 109.8f), new Vector3(2.6f, 2.4f, 0.1f), C(.22f, .38f, .65f), props, false);
        BuildQueueBelt(new Vector3(-10.5f, 0f, 104f), new Vector3(-5.5f, 0f, 104f), C(.18f, .32f, .55f));

        float[] seatX = { -30f, -20f, -12f, -2f };
        float[] seatZ = { 96f, 99f };
        foreach (float sx in seatX)
            foreach (float sz in seatZ)
                Box("LoungeSeat", new Vector3(sx, 0.28f, sz), new Vector3(2.2f, 0.55f, 0.65f), C(.22f, .28f, .32f), props, true);

        // =========================================================================
        // 15. VIP SKYCLUB EXECUTIVE LOUNGE (z = 92..110, x = 6..34)
        // =========================================================================
        Box("LoungeReception", new Vector3(12f, 0.55f, 94f), new Vector3(2.8f, 1.1f, 0.9f), C(.18f, .16f, .24f), props, true);
        Box("LoungeWelcomeMonitor", new Vector3(12f, 1.25f, 94f), new Vector3(0.6f, 0.4f, 0.1f), C(.85f, .75f, .35f), props, false);

        float[] lArmX = { 18f, 24f, 30f };
        float[] lArmZ = { 96f, 102f, 106f };
        foreach (float lx in lArmX)
            foreach (float lz in lArmZ)
            {
                Box("Armchair", new Vector3(lx, 0.35f, lz), new Vector3(1.0f, 0.7f, 1.0f), C(.16f, .20f, .32f), props, true);
                Box("CoffeeTable", new Vector3(lx, 0.22f, lz + 1.2f), new Vector3(0.9f, 0.44f, 0.9f), C(.35f, .28f, .20f), props, true);
            }

        Box("BuffetBar", new Vector3(22f, 0.5f, 109f), new Vector3(14f, 1.0f, 1.0f), C(.25f, .20f, .18f), props, true);
        Box("BeverageUrn1", new Vector3(18f, 1.18f, 109f), new Vector3(0.4f, 0.4f, 0.4f), C(.8f, .8f, .85f), props, false);
        Box("BeverageUrn2", new Vector3(22f, 1.18f, 109f), new Vector3(0.4f, 0.4f, 0.4f), C(.8f, .8f, .85f), props, false);

        // =========================================================================
        // 16. WAYFINDING, AIRLINE, STORE & COMMERCIAL ADVERTISEMENT SIGNAGE
        // =========================================================================
        Sign("CHECK-IN HALL\nCheck-in desks ahead  ^\nSecurity & gates  ^", V(0, 3.2f, 9), 0, 5.5f, 1.5f, false, signBlue, Color.white);
        Sign("CHECK-IN DESKS 1-2\nAI 202 DELHI", V(-10, 3.2f, 17.3f), 0, 4.5f, 1.1f, false, signBlue, Color.white);
        Sign("CHECK-IN DESKS 3-4\n6E 517 CHENNAI / UK 815", V(10, 3.2f, 17.3f), 0, 4.5f, 1.1f, false, signBlue, Color.white);
        Sign("SECURITY  ^\nDEPARTURES  ^", V(0, 3.4f, 19.6f), 0, 4.5f, 1.1f, false, signGreen, Color.white);
        Sign(AirportData.DeparturesBoard, V(-14.78f, 2.5f, 10), 270, 5.8f, 2.3f, false, fidsDark, amber);
        Sign("SECURITY CHECKPOINT\nFollow queue lane - Boarding pass ready", V(0, 3.3f, 23), 0, 4.8f, 1.1f, false, signGreen, Color.white);
        Sign("DEPARTURES / ALL GATES  ^", V(0, 3.3f, 30.5f), 0, 4.8f, .9f, false, signGreen, Color.white);
        Sign(AirportData.DeparturesBoard, V(2.78f, 2.8f, 36), 90, 4.0f, 1.7f, false, fidsDark, amber);
        Sign("INFO KIOSK (Press E)", V(2.4f, 1.85f, 36), 90, 1.4f, .42f, false, signGreen, Color.white);
        Sign("INFO KIOSK (Press E)", V(13.5f, 1.95f, 5), 0, 1.6f, .42f, false, signGreen, Color.white);
        Sign("INFO KIOSK (Press E)", V(13.5f, 1.95f, 64f), 90, 1.6f, .42f, false, signGreen, Color.white);
        Sign("INFO KIOSK (Press E)", V(-31.5f, 1.95f, 54f), 0, 1.6f, .42f, false, signGreen, Color.white);

        // Key decision-point signs
        Sign("<< BAGGAGE CLAIM & ARRIVALS EXIT\nGATES A1 - A2 >>\nNORTH TERMINAL / GATES B ^", V(0, 3.3f, 38.5f), 0, 5.8f, 1.5f, true, signBlue, Color.white);
        Sign("GATES A1 - A2 <<\nBAGGAGE CLAIM & EXIT >>\nCHECK-IN / ENTRANCE ^", V(0, 3.3f, 47.5f), 180, 5.8f, 1.5f, true, signBlue, Color.white);
        Sign("GATES A1 - A2  ^\nPANORAMIC RUNWAY VIEW", V(9, 3.3f, 43), 90, 4.4f, 1.2f, true, signBlue, Color.white);
        Sign("BAGGAGE CLAIM & EXIT  ^", V(-9, 3.3f, 43), 270, 4.4f, 1f, true, signBlue, Color.white);
        Sign("ARRIVALS / BAGGAGE CLAIM  ^\nFollow corridor to carousels & exit", V(0, 3.3f, 54.5f), 180, 5.4f, 1.2f, true, signBlue, Color.white);

        // Gate Lounge A signs (mounted at y = 3.4f so the airplane view out the east side is 100% clear)
        Sign("<< GATE A1 (AI 202 DELHI)\nGATE A2 (6E 517 CHENNAI) >>", V(20.5f, 3.3f, 43), 90, 5.2f, 1.2f, false, signBlue, Color.white);
        Sign("GATE A1\nAI 202 DELHI - BOARDING NOW", V(33.5f, 3.4f, 47.5f), 90, 4.8f, 1.1f, false, signGreen, Color.white);
        Sign("GATE A2\n6E 517 CHENNAI - ON TIME", V(33.5f, 3.4f, 36.5f), 90, 4.8f, 1.1f, false, C(.55f, .25f, .1f), Color.white);

        // Baggage Hall signs & Carousel labels
        Sign("BAGGAGE CLAIM HALL\nCheck Arrivals FIDS for Carousel #\nPASSENGER EXIT AHEAD <<", V(-49.75f, 3.3f, 38), 270, 6.2f, 1.4f, false, signBlue, Color.white);
        Sign(AirportData.ArrivalsBoard, V(-40, 2.6f, 55.75f), 0, 6.2f, 2.3f, false, fidsDark, amber);
        string[] carouselLabels = { "CAROUSEL 1 (MOVING)\n6E 622 BENGALURU", "CAROUSEL 2 (MOVING)\nUK 811 KOLKATA", "CAROUSEL 3 (MOVING)\nAI 305 MUMBAI" };
        for (int i = 0; i < 3; i++)
            Sign(carouselLabels[i], V(-42, 2.3f, 36f + 7f * i), 270, 3.6f, .95f, false, signGreen, Color.white);

        // Arrivals Exit Portal Signage
        Sign("<< PASSENGER ARRIVALS EXIT\nCUSTOMS & GROUND TRANSPORT", V(-49.85f, 3.4f, 47f), 270, 5.6f, 1.2f, false, signGreen, Color.white);
        Sign("EXIT & GROUND TRANSPORTATION >>\nTAXIS | METRO EXPRESS | CAR RENTAL", V(-67.6f, 3.2f, 43f), 270, 6.2f, 1.3f, false, signBlue, Color.white);

        // Commercial Advertisement Billboards
        AdBillboard("FLY EMIRATES\nFLY BETTER TO OVER 150 DESTINATIONS", V(-14.78f, 2.4f, 4f), 270, 5.0f, 2.0f, C(.75f, .12f, .15f), Color.white);
        AdBillboard("ROLEX OYSTER PERPETUAL\nA CROWN FOR EVERY ACHIEVEMENT", V(14.78f, 2.4f, 12f), 90, 5.0f, 2.0f, C(.1f, .28f, .18f), amber);
        AdBillboard("AMERICAN EXPRESS PLATINUM\nGLOBAL AIRPORT LOUNGE ACCESS WORLDWIDE", V(-2.82f, 2.5f, 52f), 270, 4.6f, 1.8f, C(.15f, .3f, .6f), Color.white);
        AdBillboard("SONY 1000XM5 NOISE-CANCELING HEADPHONES\nSILENCE THE WORLD ON YOUR FLIGHT", V(2.82f, 2.5f, 52f), 90, 4.6f, 1.8f, C(.08f, .12f, .18f), Color.white);

        // North Terminal Wayfinding
        Sign("WELCOME TO NORTH TERMINAL\n<< DUTY FREE GALLERIA | FOOD COURT & CAFE >>\nGATES B1 - B2 ^  |  VIP LOUNGE >>", V(0, 3.4f, 61.5f), 0, 7.5f, 1.3f, false, signBlue, Color.white);
        Sign("<< GATES A1 - A2 | BAGGAGE CLAIM & EXIT >>\nSECURITY & CHECK-IN ^", V(0, 3.4f, 61.5f), 180, 6.5f, 1.2f, false, signBlue, Color.white);

        // Store headers & ads in Duty Free
        Sign("<< DUTY FREE GRAND GALLERIA\nPERFUMES | WATCHES | TECH | GIFTS", V(-10.5f, 3.3f, 75f), 270, 5.2f, 1.3f, false, signBlue, Color.white);
        Sign("LUXE PERFUMES & COSMETICS\nTAX FREE SAVINGS", V(-33.7f, 3.1f, 69.5f), 270, 5.0f, 1.0f, false, C(.45f, .15f, .35f), Color.white);
        Sign("SWISS CHRONOGRAPHS & LUXURY WATCHES\nOMEGA - BREITLING - TAG HEUER", V(-33.7f, 3.1f, 76f), 270, 5.4f, 1.0f, false, C(.12f, .18f, .25f), amber);
        Sign("RELAY BOOKS, MAGAZINES & ACCESSORIES\nTRAVEL BESTSELLERS & SNACKS", V(-24f, 3.2f, 85.7f), 0, 5.2f, 1.0f, false, C(.75f, .15f, .18f), Color.white);

        // Food Court & Cafe Signs
        Sign("FOOD COURT & SKYLINE CAFE >>\nDINING & ARTISAN COFFEE", V(10.5f, 3.3f, 75f), 90, 5.0f, 1.2f, false, signBlue, Color.white);
        Sign("SKYLINE CAFE & BISTRO\nFRESH ESPRESSO & ARTISAN PASTRIES", V(26f, 3.2f, 85.7f), 0, 5.8f, 1.0f, false, C(.48f, .25f, .12f), Color.white);

        // Restroom & Amenities Hub Signs
        Sign("AMENITIES & RESTROOMS  ^\nRESTROOMS | FOREX | ATMs", V(0f, 3.3f, 85.5f), 0, 4.8f, 1.1f, false, signBlue, Color.white);
        Sign("CURRENCY EXCHANGE  <<", V(-7.7f, 2.7f, 89f), 270, 2.8f, 0.8f, false, C(.1f, .3f, .6f), Color.white);
        Sign(">> GLOBAL ATMs", V(7.7f, 2.7f, 89f), 90, 2.8f, 0.8f, false, signGreen, Color.white);

        // Concourse B & VIP signs
        Sign("CONCOURSE B  ^\n<< GATE B1 (DUBAI)  |  GATE B2 (GOA) >>", V(-15f, 3.3f, 92f), 0, 6.2f, 1.2f, false, signBlue, Color.white);
        Sign("GATE B1\nEMIRATES EK 501 DUBAI - BOARDING NOW", V(-24f, 3.3f, 109.7f), 0, 5.8f, 1.3f, false, signGreen, Color.white);
        Sign("GATE B2\nINDIGO 6E 244 GOA - ON TIME", V(-8f, 3.3f, 109.7f), 0, 5.2f, 1.2f, false, signBlue, Color.white);
        Sign("VIP SKYCLUB LOUNGE >>\nFIRST & BUSINESS CLASS ONLY", V(6.3f, 3.3f, 98.5f), 90, 4.8f, 1.2f, false, C(.12f, .18f, .38f), amber);

        // Clutter Promotional Banners
        ClutterSign("DUTY FREE MEGA SALE 50% OFF!\n<< PERFUMES | ELECTRONICS >>", V(0, 2.4f, 37.8f), 0, 5.2f, 1.0f, clutterPink);
        ClutterSign("VIP EXECUTIVE LOUNGE ^\nFAST-TRACK UPGRADE DESK >>", V(-1.6f, 2.6f, 41.2f), 45, 3.8f, 1.1f, clutterPurple);
        ClutterSign("STAR CAFE & BAKERY <<\nCURRENCY EXCHANGE / ATM >>", V(1.6f, 2.6f, 41.2f), 315, 3.8f, 1.1f, clutterOrange);
        ClutterSign("TERMINAL TRANSFER B1-B12 ^\nSHUTTLE BUS LEVEL 0 <<", V(0, 2.4f, 48.2f), 180, 5.2f, 1.0f, clutterPurple);
        ClutterSign("LUXURY WATCHES & GIFTS\nLIMITED TIME OFFER", V(5.5f, 2.5f, 43f), 90, 3.5f, 1.1f, clutterPink);
        ClutterSign("TAX REFUND COUNTER <<\nHOTEL SHUTTLES & TAXI >>", V(-5.5f, 2.5f, 43f), 270, 3.5f, 1.1f, clutterOrange);

        SetSignCondition(SignCondition.Clear);
    }

    // =========================================================================
    // 17. COMPLETE ROOF & CEILING BUILDER (EXACT ROOM FOOTPRINTS, ZERO GAPS)
    // =========================================================================
    void BuildRoof()
    {
        const float rh = 4.08f;
        const float th = 0.16f;

        // Exact-fit roof slabs over every single wing — no gaps inside and no overhanging blocks outside
        RoofSlab("Roof_CheckIn", new Vector3(0f, rh, 10f), new Vector3(30.4f, th, 20.4f));
        RoofSlab("Roof_SecurityAndConcourse", new Vector3(0f, rh, 40f), new Vector3(6.4f, th, 40.4f));
        RoofSlab("Roof_EastCorridor", new Vector3(10.5f, rh, 43f), new Vector3(15.4f, th, 6.4f));
        RoofSlab("Roof_EastGateLounges", new Vector3(26f, rh, 43f), new Vector3(16.4f, th, 18.4f));
        RoofSlab("Roof_WestCorridor", new Vector3(-16.5f, rh, 43f), new Vector3(27.4f, th, 6.4f));
        RoofSlab("Roof_WestBaggage", new Vector3(-40f, rh, 43f), new Vector3(20.4f, th, 26.4f));
        RoofSlab("Roof_ArrivalsExit", new Vector3(-59f, rh, 43f), new Vector3(18.4f, th, 18.4f));
        RoofSlab("Roof_NorthTerminal", new Vector3(0f, rh, 85f), new Vector3(68.4f, th, 50.4f));

        // Structural Steel Crossbeams spanning the exact width of each room
        Truss(new Vector3(0f, 3.92f, 5f), new Vector3(29.8f, 0.18f, 0.22f));
        Truss(new Vector3(0f, 3.92f, 15f), new Vector3(29.8f, 0.18f, 0.22f));
        Truss(new Vector3(26f, 3.92f, 43f), new Vector3(15.8f, 0.18f, 0.22f));
        Truss(new Vector3(-40f, 3.92f, 43f), new Vector3(19.8f, 0.18f, 0.22f));
        Truss(new Vector3(-59f, 3.92f, 43f), new Vector3(17.8f, 0.18f, 0.22f));
        foreach (float bz in new[] { 68f, 78f, 92f, 102f })
            Truss(new Vector3(0f, 3.92f, bz), new Vector3(67.8f, 0.18f, 0.22f));

        // Recessed Ceiling LED Downlight panels + real indoor Point Lights
        float[] allLightZ = { 5f, 14f, 24f, 35f, 43f, 53f, 66f, 75f, 84f, 96f, 104f };
        foreach (float lz in allLightZ)
        {
            Box("CeilingLightC", new Vector3(0f, 3.96f, lz), new Vector3(1.8f, 0.05f, 0.5f), C(.98f, .98f, 1f), roof, false);
            if (lz >= 65f)
            {
                Box("CeilingLightW", new Vector3(-20f, 3.96f, lz), new Vector3(2.2f, 0.05f, 0.5f), C(.98f, .98f, 1f), roof, false);
                Box("CeilingLightE", new Vector3(20f, 3.96f, lz), new Vector3(2.2f, 0.05f, 0.5f), C(1f, .96f, .88f), roof, false);
            }
        }

        Vector3[] lightPts =
        {
            new Vector3(0f, 3.6f, 10f), new Vector3(0f, 3.6f, 28f), new Vector3(0f, 3.6f, 45f),
            new Vector3(26f, 3.6f, 43f), new Vector3(-40f, 3.6f, 43f), new Vector3(-59f, 3.6f, 43f),
            new Vector3(0f, 3.6f, 75f), new Vector3(-22f, 3.6f, 75f), new Vector3(22f, 3.6f, 75f),
            new Vector3(-16f, 3.6f, 100f), new Vector3(20f, 3.6f, 100f)
        };
        foreach (var lp in lightPts)
        {
            var lg = new GameObject("IndoorPointLight");
            lg.transform.SetParent(roof, false);
            lg.transform.position = lp;
            var pt = lg.AddComponent<Light>();
            pt.type = LightType.Point;
            pt.range = 28f;
            pt.intensity = 1.6f;
            pt.color = C(1f, .98f, .94f);
        }
    }

    void RoofSlab(string name, Vector3 pos, Vector3 size)
    {
        Box(name, pos, size, roofCol, roof, true);
    }

    void Truss(Vector3 pos, Vector3 size)
    {
        Box("RoofTruss", pos, size, roofTrussCol, roof, false);
    }

    // =========================================================================
    // 18. OPEN OBSERVATION RAILING AT GATE A1 & GATE A2 (NO GLASS PANEL!)
    // =========================================================================
    void BuildOpenObservationRailing(float x, float z1, float z2)
    {
        float len = Mathf.Abs(z2 - z1);
        float midZ = (z1 + z2) * 0.5f;

        // Waist-high chrome handrail & posts along x = 34 so the view above y = 1.0m is 100% open to the plane!
        Box("HandrailTop", new Vector3(x - 0.15f, 0.95f, midZ), new Vector3(0.08f, 0.08f, len), C(.8f, .82f, .86f), walls, true);
        Box("HandrailMid", new Vector3(x - 0.15f, 0.50f, midZ), new Vector3(0.06f, 0.06f, len), C(.6f, .62f, .66f), walls, false);
        Box("EdgeCurb", new Vector3(x, 0.12f, midZ), new Vector3(0.3f, 0.24f, len), wallTrimCol, walls, false);

        for (float z = z1; z <= z2 + 0.1f; z += 3.0f)
        {
            Box("HandrailPost", new Vector3(x - 0.15f, 0.48f, z), new Vector3(0.08f, 0.96f, 0.08f), C(.4f, .42f, .46f), walls, false);
        }
    }

    // =========================================================================
    // 19. REALISTIC AIRLINER ON RUNWAY APRON (ALL PARTS USE LocalBox -> ZERO BLOCKS AT SPAWN!)
    // =========================================================================
    void BuildAirliner(Vector3 center)
    {
        var plane = new GameObject("Airliner_AI202").transform;
        plane.SetParent(props, false);
        plane.position = center;
        // 0 deg Y rotation: Fuselage runs North-South along the runway, facing Gate A1 & Gate A2!
        plane.rotation = Quaternion.identity;

        Color fuselageWhite = C(.95f, .96f, .98f);
        Color liveryNavy = C(.08f, .20f, .42f);
        Color cockpitTint = C(.10f, .12f, .18f);
        Color engineChrome = C(.70f, .72f, .76f);

        // 1. Fuselage (All parts use LocalBox relative to plane at (54, 0, 44)!)
        LocalBox("FuselageBody", new Vector3(0f, 3.2f, 0f), new Vector3(3.6f, 3.8f, 24f), fuselageWhite, plane);
        LocalBox("LiveryStripe", new Vector3(0f, 2.8f, 0f), new Vector3(3.66f, 0.5f, 23.8f), liveryNavy, plane);
        LocalBox("NoseCone", new Vector3(0f, 3.0f, 13.5f), new Vector3(3.0f, 3.2f, 4.0f), fuselageWhite, plane);
        LocalBox("NoseTip", new Vector3(0f, 2.8f, 16.0f), new Vector3(1.8f, 2.0f, 1.8f), fuselageWhite, plane);
        LocalBox("CockpitGlass", new Vector3(0f, 3.8f, 13.2f), new Vector3(2.4f, 0.8f, 1.2f), cockpitTint, plane);
        LocalBox("CabinWindowsPort", new Vector3(-1.84f, 3.4f, 0f), new Vector3(0.04f, 0.4f, 20f), cockpitTint, plane);
        LocalBox("CabinWindowsStbd", new Vector3(1.84f, 3.4f, 0f), new Vector3(0.04f, 0.4f, 20f), cockpitTint, plane);

        // 2. Main Wings (32m wingspan)
        LocalBox("WingLeft", new Vector3(-9.5f, 2.4f, -1.5f), new Vector3(16f, 0.45f, 4.8f), fuselageWhite, plane);
        LocalBox("WingRight", new Vector3(9.5f, 2.4f, -1.5f), new Vector3(16f, 0.45f, 4.8f), fuselageWhite, plane);
        LocalBox("WingletLeft", new Vector3(-17.4f, 3.4f, -3.2f), new Vector3(0.2f, 2.0f, 1.2f), liveryNavy, plane);
        LocalBox("NavLightRed", new Vector3(-17.5f, 4.3f, -3.2f), new Vector3(0.25f, 0.25f, 0.25f), C(1f, 0f, 0f), plane);
        LocalBox("WingletRight", new Vector3(17.4f, 3.4f, -3.2f), new Vector3(0.2f, 2.0f, 1.2f), liveryNavy, plane);
        LocalBox("NavLightGreen", new Vector3(17.5f, 4.3f, -3.2f), new Vector3(0.25f, 0.25f, 0.25f), C(0f, 1f, .2f), plane);

        // 3. Twin High-Bypass Turbofan Jet Engines
        foreach (float ex in new[] { -5.5f, 5.5f })
        {
            LocalBox("EnginePylon", new Vector3(ex, 2.0f, 1.0f), new Vector3(0.4f, 1.2f, 2.2f), C(.4f, .4f, .45f), plane);
            LocalBox("EngineNacelle", new Vector3(ex, 1.35f, 1.2f), new Vector3(2.2f, 2.2f, 4.5f), fuselageWhite, plane);
            LocalBox("EngineIntake", new Vector3(ex, 1.35f, 3.5f), new Vector3(1.9f, 1.9f, 0.2f), C(.15f, .16f, .20f), plane);
            LocalBox("EngineSpinner", new Vector3(ex, 1.35f, 3.3f), new Vector3(0.5f, 0.5f, 0.5f), engineChrome, plane);
            LocalBox("EngineExhaust", new Vector3(ex, 1.35f, -1.1f), new Vector3(1.6f, 1.6f, 0.4f), engineChrome, plane);
        }

        // 4. Tail Empennage (Vertical stabilizer fin & horizontal stabilizers)
        LocalBox("TailCone", new Vector3(0f, 3.6f, -14.5f), new Vector3(2.4f, 2.6f, 5.5f), fuselageWhite, plane);
        LocalBox("VerticalStabilizer", new Vector3(0f, 7.5f, -15.5f), new Vector3(0.45f, 6.5f, 4.8f), liveryNavy, plane);
        LocalBox("TailFinLogo", new Vector3(0f, 8.2f, -15.5f), new Vector3(0.5f, 2.0f, 2.5f), amber, plane);
        LocalBox("HorizTailL", new Vector3(-4.8f, 4.8f, -16.5f), new Vector3(7.5f, 0.3f, 2.6f), fuselageWhite, plane);
        LocalBox("HorizTailR", new Vector3(4.8f, 4.8f, -16.5f), new Vector3(7.5f, 0.3f, 2.6f), fuselageWhite, plane);

        // 5. Heavy Aircraft Tricycle Landing Gear
        LocalBox("NoseGearStrut", new Vector3(0f, 0.85f, 11f), new Vector3(0.35f, 1.7f, 0.35f), C(.3f, .32f, .36f), plane);
        LocalBox("NoseTireL", new Vector3(-0.35f, 0.45f, 11f), new Vector3(0.28f, 0.9f, 0.9f), C(.12f, .12f, .14f), plane);
        LocalBox("NoseTireR", new Vector3(0.35f, 0.45f, 11f), new Vector3(0.28f, 0.9f, 0.9f), C(.12f, .12f, .14f), plane);
        foreach (float mx in new[] { -3.2f, 3.2f })
        {
            LocalBox("MainGearStrut", new Vector3(mx, 1.0f, -1.8f), new Vector3(0.5f, 2.0f, 0.5f), C(.3f, .32f, .36f), plane);
            LocalBox("MainTire1", new Vector3(mx + 0.45f, 0.55f, -1.4f), new Vector3(0.35f, 1.1f, 1.1f), C(.12f, .12f, .14f), plane);
            LocalBox("MainTire2", new Vector3(mx - 0.45f, 0.55f, -1.4f), new Vector3(0.35f, 1.1f, 1.1f), C(.12f, .12f, .14f), plane);
            LocalBox("MainTire3", new Vector3(mx + 0.45f, 0.55f, -2.4f), new Vector3(0.35f, 1.1f, 1.1f), C(.12f, .12f, .14f), plane);
            LocalBox("MainTire4", new Vector3(mx - 0.45f, 0.55f, -2.4f), new Vector3(0.35f, 1.1f, 1.1f), C(.12f, .12f, .14f), plane);
        }
    }

    void BuildGroundSupportEquipment(Vector3 planePos)
    {
        var gse = new GameObject("GroundSupportEquipment").transform;
        gse.SetParent(props, false);

        // Aircraft Pushback Tug Tractor parked ahead of the nose
        Vector3 tugPos = planePos + new Vector3(-3f, 0.6f, 20f);
        Box("PushbackTug", tugPos, new Vector3(2.2f, 1.2f, 3.5f), C(.95f, .75f, .15f), gse, false);
        Box("TugCabin", tugPos + new Vector3(0f, 0.7f, 0.8f), new Vector3(1.8f, 0.8f, 1.2f), C(.15f, .2f, .25f), gse, false);

        // Baggage Trolley Train (Tug + 2 luggage wagons)
        Vector3 bagTrain = planePos + new Vector3(-8f, 0.4f, -10f);
        Box("BagTractor", bagTrain, new Vector3(1.2f, 0.9f, 2.0f), C(.85f, .35f, .15f), gse, false);
        Box("BagCart1", bagTrain + new Vector3(0f, 0f, -2.8f), new Vector3(1.3f, 0.8f, 2.2f), C(.35f, .38f, .42f), gse, false);
        Box("CartLuggage1", bagTrain + new Vector3(0f, 0.5f, -2.8f), new Vector3(1.0f, 0.6f, 1.8f), C(.7f, .2f, .2f), gse, false);
        Box("BagCart2", bagTrain + new Vector3(0f, 0f, -5.6f), new Vector3(1.3f, 0.8f, 2.2f), C(.35f, .38f, .42f), gse, false);
        Box("CartLuggage2", bagTrain + new Vector3(0f, 0.5f, -5.6f), new Vector3(1.0f, 0.6f, 1.8f), C(.2f, .35f, .7f), gse, false);

        // Ground Power Unit (GPU) cart
        Box("GPU_Cart", planePos + new Vector3(-4.5f, 0.5f, 8f), new Vector3(1.2f, 1.0f, 1.6f), C(.9f, .85f, .2f), gse, false);

        // Taxiway edge blue beacon light cones
        for (float z = 18f; z <= 72f; z += 9f)
        {
            Box("TaxiLightCone", new Vector3(37f, 0.15f, z), new Vector3(0.2f, 0.3f, 0.2f), C(.8f, .8f, .2f), gse, false);
            Box("TaxiLightBlue", new Vector3(37f, 0.35f, z), new Vector3(0.12f, 0.12f, 0.12f), C(.1f, .4f, 1f), gse, false);
        }
    }

    // =========================================================================
    // 20. ADVERTISING BILLBOARDS & HELPERS
    // =========================================================================
    void AdBillboard(string text, Vector3 pos, float yaw, float w, float h, Color bg, Color fg)
    {
        var r = new GameObject("Ad_" + text.Split('\n')[0]).transform;
        r.SetParent(signsRoot, false);
        r.position = pos;
        r.rotation = Quaternion.Euler(0f, yaw, 0f);

        LocalBox("Frame", Vector3.zero, new Vector3(w + 0.15f, h + 0.15f, 0.08f), C(.15f, .16f, .20f), r);
        LocalBox("AdBoard", Vector3.zero, new Vector3(w, h, 0.1f), bg, r);

        MakeText(r, text, new Vector3(0f, 0f, -0.07f), new Vector2(w * 0.94f, h * 0.9f), fg);
    }

    static Vector3 V(float x, float y, float z) { return new Vector3(x, y, z); }

    Transform Sub(Transform parent, string n)
    {
        var g = new GameObject(n); g.transform.SetParent(parent, false); return g.transform;
    }

    Material Mat(Color c) { var m = new Material(baseMat); m.color = c; return m; }

    GameObject Prim(PrimitiveType type, string name, Vector3 worldPos, Vector3 scale, Color c, Transform parent)
    {
        var g = GameObject.CreatePrimitive(type);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.position = worldPos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = Mat(c);
        Destroy(g.GetComponent<Collider>());
        return g;
    }

    GameObject LocalBox(string name, Vector3 localCenter, Vector3 scale, Color c, Transform parent, bool collider = false)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localCenter;
        g.transform.localRotation = Quaternion.identity;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = Mat(c);
        if (!collider) Destroy(g.GetComponent<Collider>());
        return g;
    }

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
        Box("StanchionPost", p1 + new Vector3(0f, 0.45f, 0f), new Vector3(0.12f, 0.9f, 0.12f), C(.15f, .16f, .18f), props, false);
        Box("StanchionPost", p2 + new Vector3(0f, 0.45f, 0f), new Vector3(0.12f, 0.9f, 0.12f), C(.15f, .16f, .18f), props, false);
        Vector3 mid = (p1 + p2) * 0.5f + new Vector3(0f, 0.78f, 0f);
        float len = Vector3.Distance(p1, p2);
        var belt = Box("QueueBelt", mid, new Vector3(0.05f, 0.1f, len), beltCol, props, false);
        belt.transform.LookAt(p2 + new Vector3(0f, 0.78f, 0f));
    }

    void Wall(float x1, float z1, float x2, float z2, Color? customCol = null)
    {
        const float t = 0.3f, h = 4.0f, pad = 0.15f;
        Color c = customCol ?? wallCol;
        Vector3 center, scale;

        if (Mathf.Approximately(x1, x2))
        {
            float len = Mathf.Abs(z2 - z1) + 2 * pad;
            center = new Vector3(x1, h / 2f, (z1 + z2) / 2f);
            scale = new Vector3(t, h, len);
            Box("Wall", center, scale, c, walls, true);
            Box("WallTrim", new Vector3(x1, 0.15f, (z1 + z2) / 2f), new Vector3(t + 0.05f, 0.3f, len), wallTrimCol, walls, false);
        }
        else
        {
            float len = Mathf.Abs(x2 - x1) + 2 * pad;
            center = new Vector3((x1 + x2) / 2f, h / 2f, z1);
            scale = new Vector3(len, h, t);
            Box("Wall", center, scale, c, walls, true);
            Box("WallTrim", new Vector3((x1 + x2) / 2f, 0.15f, z1), new Vector3(len, 0.3f, t + 0.05f), wallTrimCol, walls, false);
        }
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

        LocalBox("Board", Vector3.zero, new Vector3(w, h, 0.1f), bg, r);
        MakeText(r, text, new Vector3(0f, 0f, -0.07f), new Vector2(w * 0.95f, h * 0.9f), fg);

        var info = new SignInfo { key = key };
        if (key)
        {
            var bl = new GameObject("Blocker");
            bl.transform.SetParent(r, false);
            LocalBox("Panel", new Vector3(0f, 0f, -0.14f), new Vector3(w + 0.1f, h + 0.1f, 0.06f), blockerCol, bl.transform);
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

        LocalBox("PromoBoard", Vector3.zero, new Vector3(w, h, 0.08f), bg, r);
        MakeText(r, text, new Vector3(0f, 0f, -0.06f), new Vector2(w * 0.92f, h * 0.88f), Color.yellow);
        r.gameObject.SetActive(false);
        clutterSigns.Add(r.gameObject);
    }
}
