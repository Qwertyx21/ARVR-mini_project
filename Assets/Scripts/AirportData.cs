using UnityEngine;
using UnityEngine.EventSystems;

public enum Dest { None, CheckIn, Security, GateA1, Baggage }
public enum ZoneId { None, CheckIn, Security, Concourse, EastWing, WestWing, ArrivalGate, GateA2Wrong }
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
        new Vector3(13.5f, 0f, 5f),
        new Vector3(2.3f, 0f, 36f)
    };

    public const string DeparturesBoard =
        "DEPARTURES\n" +
        "AI 202  DELHI      DESK 1-2  GATE A1  BOARDING\n" +
        "6E 517  CHENNAI    DESK 3-4  GATE A2  ON TIME\n" +
        "UK 815  MUMBAI     DESK 3-4  GATE A1  DELAYED";

    public const string ArrivalsBoard =
        "ARRIVALS - BAGGAGE CLAIM\n" +
        "AI 305  MUMBAI     LANDED    CAROUSEL 3\n" +
        "6E 622  BENGALURU  LANDED    CAROUSEL 1\n" +
        "UK 811  KOLKATA    EXPECTED  CAROUSEL 2";

    public const string KioskText =
        "<b>DEPARTURES (FLIGHT INFORMATION DISPLAY)</b>\n" +
        "AI 202   DELHI      DESK 1-2   GATE A1   <color=#66FF88>BOARDING</color>\n" +
        "6E 517   CHENNAI    DESK 3-4   GATE A2   ON TIME\n" +
        "UK 815   MUMBAI     DESK 3-4   GATE A1   <color=#FFAA55>DELAYED</color>\n\n" +
        "<b>ARRIVALS (BAGGAGE RECLAIM)</b>\n" +
        "AI 305   MUMBAI     LANDED     <color=#66FF88>CAROUSEL 3</color>\n" +
        "6E 622   BENGALURU  LANDED     CAROUSEL 1\n" +
        "UK 811   KOLKATA    EXPECTED   CAROUSEL 2\n\n" +
        "<b>TERMINAL WAYFINDING DIRECTORY</b>\n" +
        "• <b>Check-In (Flight AI 202):</b> Straight ahead in the Check-In Hall (Desks 1-2 on the left, or Desks 3-4 on the right).\n" +
        "• <b>Security Checkpoint:</b> Walk north through the central hall opening, follow the queue lanes through the scanner arch.\n" +
        "• <b>Gate A1 (Flight AI 202 to Delhi):</b> After security, walk north to the Main Concourse Junction, turn RIGHT (East Wing), and proceed to the north-east boarding desk (avoid Gate A2 on the south side).\n" +
        "• <b>Baggage Claim (Flight AI 305 from Mumbai):</b> At the Main Concourse Junction, turn LEFT (West Wing) into the Baggage Hall and proceed to <b>Carousel 3</b> (north end).";

    public static string Label(Dest d, bool fullJourney = false)
    {
        if (fullJourney) return "Full Departure (Check-In -> Security -> Gate A1)";
        switch (d)
        {
            case Dest.CheckIn: return "Check-In";
            case Dest.Security: return "Security";
            case Dest.GateA1: return "Gate A1";
            case Dest.Baggage: return "Baggage Claim";
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
            case Dest.GateA1: return "Board flight AI 202 to Delhi at Gate A1 (not Gate A2)";
            case Dest.Baggage:
                return arrival
                    ? "Arrival: collect bags for flight AI 305 (find Carousel 3)"
                    : "Collect bags for flight AI 305 (find Carousel 3 via FIDS)";
            default: return "Select a destination or scenario (Keys 1-6)";
        }
    }

    public static ZoneId ZoneAt(Vector3 p)
    {
        float x = p.x, z = p.z;
        // East wing check: distinguish Gate A2 wrong area (south-east corner of gate room)
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
                return z == ZoneId.WestWing || z == ZoneId.ArrivalGate || z == ZoneId.GateA2Wrong;
            case Dest.Baggage:
                if (z == ZoneId.EastWing || z == ZoneId.GateA2Wrong) return true;
                if (arrival && (z == ZoneId.CheckIn || z == ZoneId.Security)) return true;
                return false;
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
        }
        return false;
    }
}
