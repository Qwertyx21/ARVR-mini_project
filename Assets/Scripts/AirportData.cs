using UnityEngine;
using UnityEngine.EventSystems;

public enum Dest
{
    None,
    CheckIn,
    Security,
    GateA1,
    Baggage,
    Exit,
    GateB1,
    GateB2,
    DutyFree,
    FoodCourt,
    VIPLounge,
    Restrooms
}

public enum ZoneId
{
    None,
    CheckIn,
    Security,
    Concourse,
    EastWing,
    WestWing,
    ArrivalGate,
    GateA2Wrong,
    ArrivalsExit,
    NorthPlaza,
    DutyFree,
    FoodCourt,
    GateBWing,
    VIPLounge,
    Restrooms
}

public enum SignCondition { Clear = 0, Obstructed = 1, VisualClutter = 2 }

public static class AirportInput
{
    public static bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public static bool GetKey(KeyCode key)
    {
        return Input.GetKey(key);
    }

    public static bool GetRightMouseDown()
    {
        return Input.GetMouseButtonDown(1);
    }

    public static bool GetRightMouseHeld()
    {
        return Input.GetMouseButton(1);
    }

    public static bool GetRightMouseUp()
    {
        return Input.GetMouseButtonUp(1);
    }

    public static Vector2 GetMoveAxes()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    public static Vector2 GetLookDelta()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    public static void AttachInputModule(GameObject esGo)
    {
        esGo.AddComponent<StandaloneInputModule>();
    }
}

public static class AirportData
{
    public static readonly Vector3 DepartureSpawn = new Vector3(0f, 0.05f, 3f);
    public const float DepartureYaw = 0f;
    public static readonly Vector3 ArrivalSpawn = new Vector3(0f, 0.05f, 57f);
    public const float ArrivalYaw = 180f;

    public static readonly Vector3[] Kiosks =
    {
        new Vector3(13.5f, 0f, 5f),      // Check-In Hall Kiosk (against east wall)
        new Vector3(2.3f, 0f, 36f),      // Concourse Kiosk (against east wall)
        new Vector3(13.5f, 0f, 64f),     // North Plaza Kiosk (tucked by east wall)
        new Vector3(-31.5f, 0f, 54f)     // Baggage Claim Kiosk (tucked by north-east wall)
    };

    public const string DeparturesBoard =
        "DEPARTURES (EXPANDED TERMINAL)\n" +
        "AI 202   DELHI      DESK 1-2   GATE A1   BOARDING\n" +
        "6E 517   CHENNAI    DESK 3-4   GATE A2   ON TIME\n" +
        "EK 501   DUBAI      DESK 1-2   GATE B1   BOARDING\n" +
        "6E 244   GOA        DESK 3-4   GATE B2   ON TIME\n" +
        "UK 815   MUMBAI     DESK 3-4   GATE A1   DELAYED";

    public const string ArrivalsBoard =
        "ARRIVALS - BAGGAGE CLAIM\n" +
        "AI 305  MUMBAI     LANDED    CAROUSEL 3 (MOVING)\n" +
        "6E 622  BENGALURU  LANDED    CAROUSEL 1 (MOVING)\n" +
        "UK 811  KOLKATA    EXPECTED  CAROUSEL 2 (MOVING)";

    public const string KioskText =
        "<b>DEPARTURES (FLIGHT INFORMATION DISPLAY)</b>\n" +
        "AI 202   DELHI      DESK 1-2   GATE A1   <color=#66FF88>BOARDING (Runway View)</color>\n" +
        "6E 517   CHENNAI    DESK 3-4   GATE A2   ON TIME\n" +
        "EK 501   DUBAI      DESK 1-2   GATE B1   <color=#66FF88>BOARDING</color>\n" +
        "6E 244   GOA        DESK 3-4   GATE B2   ON TIME\n" +
        "UK 815   MUMBAI     DESK 3-4   GATE A1   <color=#FFAA55>DELAYED</color>\n\n" +
        "<b>ARRIVALS & BAGGAGE CLAIM</b>\n" +
        "AI 305   MUMBAI     LANDED     <color=#66FF88>CAROUSEL 3</color> -> Exit West\n" +
        "6E 622   BENGALURU  LANDED     CAROUSEL 1 -> Exit West\n" +
        "UK 811   KOLKATA    EXPECTED   CAROUSEL 2 -> Exit West\n\n" +
        "<b>EXPANDED TERMINAL DIRECTORY</b>\n" +
        "• <b>Check-In:</b> South Hall (Desks 1-2 Left, Desks 3-4 Right).\n" +
        "• <b>Security Checkpoint:</b> North of Check-In, queue through walk-through arch.\n" +
        "• <b>Gate Concourse A (A1 & A2):</b> East Wing. Features panoramic glass observation windows overlooking the Boeing 737 aircraft on the apron and runway.\n" +
        "• <b>Baggage Claim Hall:</b> West Wing. 3 active motorized carousels.\n" +
        "• <b>Arrivals Exit & Ground Transportation:</b> West of Carousel 3 through Customs sliding doors (Taxis, Metro, Car Rentals).\n" +
        "• <b>Central Plaza & Skylight Atrium:</b> Continue North past Arrival Portal (360 FIDS Totem).\n" +
        "• <b>Duty Free Galleria:</b> West Promenade (Luxe Perfumes, TechZone, Swiss Chocolates, Watches).\n" +
        "• <b>Food Court & Skyline Cafe:</b> East Promenade (Dining, Coffee Roasters, Gelato).\n" +
        "• <b>Concourse B (Gates B1 & B2):</b> North Promenade past Restrooms Hub.\n" +
        "  - <i>Gate B1:</i> EK 501 to Dubai (Jetway Boarding)\n" +
        "  - <i>Gate B2:</i> 6E 244 to Goa (Jetway Boarding)\n" +
        "• <b>SkyClub VIP Executive Lounge:</b> North-East Wing (Business & First Class).";

    public static string Label(Dest d, bool fullJourney = false)
    {
        if (fullJourney) return "Full Departure (Check-In -> Security -> Gate A1)";
        switch (d)
        {
            case Dest.CheckIn: return "Check-In";
            case Dest.Security: return "Security";
            case Dest.GateA1: return "Gate A1 (Runway Aircraft View)";
            case Dest.Baggage: return "Baggage Claim (Carousel 3)";
            case Dest.Exit: return "Arrivals Exit (Ground Transport)";
            case Dest.GateB1: return "Gate B1 (EK 501 Dubai)";
            case Dest.GateB2: return "Gate B2 (6E 244 Goa)";
            case Dest.DutyFree: return "Duty Free Galleria";
            case Dest.FoodCourt: return "Food Court & Cafe";
            case Dest.VIPLounge: return "VIP SkyClub Lounge";
            case Dest.Restrooms: return "Restrooms & ATMs";
            default: return "None";
        }
    }

    public static string SignConditionLabel(SignCondition c)
    {
        switch (c)
        {
            case SignCondition.Obstructed: return "Obstructed";
            case SignCondition.VisualClutter: return "VisualClutter";
            default: return "Clear";
        }
    }

    public static string Task(Dest d, bool arrival, bool fullJourney, int stageIndex)
    {
        if (fullJourney)
        {
            return $"Stage {stageIndex}/3: " + Task(d, false, false, 0);
        }
        switch (d)
        {
            case Dest.CheckIn: return "Check in for flight AI 202 (Check-In Desks)";
            case Dest.Security: return "Pass through the Security Checkpoint queue & scanner";
            case Dest.GateA1: return "Board flight AI 202 to Delhi at Gate A1 (view airplane outside glass window)";
            case Dest.Baggage:
                return arrival
                    ? "Arrival: collect bags from moving Carousel 3 for flight AI 305"
                    : "Collect bags from moving Carousel 3 for flight AI 305";
            case Dest.Exit: return "Proceed to Arrivals Exit / Ground Transportation / Taxis";
            case Dest.GateB1: return "Proceed to Gate B1 for Emirates flight EK 501 to Dubai";
            case Dest.GateB2: return "Proceed to Gate B2 for IndiGo flight 6E 244 to Goa";
            case Dest.DutyFree: return "Explore the Duty Free Galleria & Luxury Boutiques";
            case Dest.FoodCourt: return "Head to the Food Court & Skyline Cafe Dining Area";
            case Dest.VIPLounge: return "Access the SkyClub VIP Executive Lounge (North-East)";
            case Dest.Restrooms: return "Locate the Central Restrooms & ATM Amenities Hub";
            default: return "Select a destination or scenario";
        }
    }

    public static ZoneId ZoneAt(Vector3 p)
    {
        float x = p.x, z = p.z;

        // Arrivals Exit West zone
        if (x <= -50f && x >= -68f && z >= 34f && z <= 55f)
            return ZoneId.ArrivalsExit;

        // Expanded North Terminal Zones (z >= 60)
        if (z >= 92f && z <= 112f)
        {
            if (x >= 6f && x <= 35f) return ZoneId.VIPLounge;
            if (x >= -35f && x <= 5f) return ZoneId.GateBWing;
        }

        if (z >= 86f && z < 92f && Mathf.Abs(x) <= 8f)
            return ZoneId.Restrooms;

        if (z >= 60f && z < 92f)
        {
            if (x < -8f && x >= -35f) return ZoneId.DutyFree;
            if (x > 8f && x <= 35f) return ZoneId.FoodCourt;
            if (Mathf.Abs(x) <= 8f) return ZoneId.NorthPlaza;
        }

        // Original South & Central Zones
        if (x >= 20f && x <= 33f && z >= 34f && z <= 38.2f)
            return ZoneId.GateA2Wrong;

        if (z >= 34f && z <= 52f)
        {
            if (x > 4f && x <= 34f) return ZoneId.EastWing;
            if (x < -4f && x >= -50f) return ZoneId.WestWing;
        }
        if (Mathf.Abs(x) <= 3f)
        {
            if (z >= 52f && z <= 60f) return ZoneId.ArrivalGate;
            if (z >= 32f && z < 52f) return ZoneId.Concourse;
            if (z >= 20f && z < 32f) return ZoneId.Security;
        }
        if (x >= -15f && x <= 15f && z >= 0f && z < 20f) return ZoneId.CheckIn;

        return ZoneId.None;
    }

    // Wrong turn = ENTERING a zone that is not on the correct route for the selected destination.
    public static bool IsWrong(Dest d, ZoneId z, bool arrival)
    {
        switch (d)
        {
            case Dest.CheckIn:
                return z != ZoneId.CheckIn;
            case Dest.Security:
                return z != ZoneId.CheckIn && z != ZoneId.Security;
            case Dest.GateA1:
                return z == ZoneId.WestWing || z == ZoneId.ArrivalGate || z == ZoneId.GateA2Wrong ||
                       z == ZoneId.DutyFree || z == ZoneId.FoodCourt || z == ZoneId.GateBWing || z == ZoneId.VIPLounge || z == ZoneId.ArrivalsExit;
            case Dest.Baggage:
                if (z == ZoneId.EastWing || z == ZoneId.GateA2Wrong || z == ZoneId.DutyFree ||
                    z == ZoneId.FoodCourt || z == ZoneId.GateBWing || z == ZoneId.VIPLounge || z == ZoneId.ArrivalsExit) return true;
                if (arrival && (z == ZoneId.CheckIn || z == ZoneId.Security)) return true;
                return false;
            case Dest.Exit:
                return z == ZoneId.EastWing || z == ZoneId.GateA2Wrong || z == ZoneId.DutyFree ||
                       z == ZoneId.FoodCourt || z == ZoneId.GateBWing || z == ZoneId.VIPLounge;
            case Dest.GateB1:
                return z == ZoneId.WestWing || z == ZoneId.EastWing || z == ZoneId.GateA2Wrong || z == ZoneId.VIPLounge || z == ZoneId.ArrivalsExit;
            case Dest.GateB2:
                return z == ZoneId.WestWing || z == ZoneId.EastWing || z == ZoneId.GateA2Wrong || z == ZoneId.VIPLounge || z == ZoneId.ArrivalsExit;
            case Dest.DutyFree:
                return z == ZoneId.EastWing || z == ZoneId.GateA2Wrong || z == ZoneId.FoodCourt || z == ZoneId.ArrivalsExit;
            case Dest.FoodCourt:
                return z == ZoneId.WestWing || z == ZoneId.GateA2Wrong || z == ZoneId.DutyFree || z == ZoneId.ArrivalsExit;
            case Dest.VIPLounge:
                return z == ZoneId.WestWing || z == ZoneId.GateA2Wrong || z == ZoneId.GateBWing || z == ZoneId.ArrivalsExit;
            case Dest.Restrooms:
                return z == ZoneId.WestWing || z == ZoneId.EastWing || z == ZoneId.GateA2Wrong || z == ZoneId.ArrivalsExit;
        }
        return false;
    }

    // Returns 1, 2, or 3 when the user stands in the claim zone of Carousel 1, 2, or 3; otherwise 0.
    public static int CarouselAt(Vector3 p)
    {
        if (p.x >= -46f && p.x <= -31f)
        {
            if (p.z >= 34f && p.z <= 38f) return 1; // Carousel 1 (6E 622 Bengaluru - WRONG for AI 305)
            if (p.z >= 41f && p.z <= 45f) return 2; // Carousel 2 (UK 811 Kolkata - WRONG for AI 305)
            if (p.z >= 48f && p.z <= 52.5f) return 3; // Carousel 3 (AI 305 Mumbai - CORRECT)
        }
        return 0;
    }

    public static bool InDestination(Dest d, Vector3 p)
    {
        switch (d)
        {
            case Dest.CheckIn:
                return p.x >= -13f && p.x <= 13f && p.z >= 13.5f && p.z <= 19f;
            case Dest.Security:
                return Mathf.Abs(p.x) <= 2.8f && p.z >= 25.5f && p.z <= 29.2f;
            case Dest.GateA1:
                return p.x >= 22f && p.x <= 33.5f && p.z >= 42f && p.z <= 51.5f;
            case Dest.Baggage:
                return CarouselAt(p) == 3;
            case Dest.Exit:
                return p.x <= -53f && p.x >= -66f && p.z >= 36f && p.z <= 54f;
            case Dest.GateB1:
                return p.x >= -28f && p.x <= -18f && p.z >= 98f && p.z <= 109f;
            case Dest.GateB2:
                return p.x >= -14f && p.x <= -2f && p.z >= 98f && p.z <= 109f;
            case Dest.DutyFree:
                return p.x >= -30f && p.x <= -12f && p.z >= 68f && p.z <= 84f;
            case Dest.FoodCourt:
                return p.x >= 12f && p.x <= 30f && p.z >= 68f && p.z <= 84f;
            case Dest.VIPLounge:
                return p.x >= 12f && p.x <= 32f && p.z >= 95f && p.z <= 109f;
            case Dest.Restrooms:
                return Mathf.Abs(p.x) <= 6f && p.z >= 86f && p.z <= 92f;
        }
        return false;
    }
}
