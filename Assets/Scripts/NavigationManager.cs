using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public bool AssistOn = true;
    public Dest Current { get; private set; } = Dest.None;
    public string Instruction { get; private set; } = "";
    public float ArrowAngle { get; private set; }

    Transform player;
    Camera cam;

    static readonly Vector2[] NodePos =
    {
        new Vector2(0, 3),      // 0 Entrance
        new Vector2(0, 15.5f),  // 1 Check-in desks
        new Vector2(0, 21),     // 2 Security queue entrance
        new Vector2(0, 27),     // 3 Security screening arch
        new Vector2(0, 35),     // 4 Concourse
        new Vector2(0, 43),     // 5 Main concourse junction
        new Vector2(12, 43),    // 6 East corridor
        new Vector2(27, 47.5f), // 7 Gate A1 boarding area
        new Vector2(-14, 43),   // 8 West corridor
        new Vector2(-33, 43),   // 9 Baggage hall entrance
        new Vector2(-35, 50),   // 10 Carousel 3 (AI 305)
        new Vector2(0, 56)      // 11 Arrival gate
    };
    static readonly string[] NodeName =
    {
        "Entrance", "Check-in desks", "Security queue", "Security screening", "Concourse",
        "Main junction", "East corridor", "Gate A1", "West corridor", "Baggage hall", "Carousel 3", "Arrival gate"
    };
    static readonly int[][] Edges =
    {
        new[]{0,1}, new[]{1,2}, new[]{2,3}, new[]{3,4}, new[]{4,5}, new[]{5,6},
        new[]{6,7}, new[]{5,8}, new[]{8,9}, new[]{9,10}, new[]{5,11}
    };

    List<int>[] adj;
    readonly List<Transform> crumbs = new List<Transform>();
    GameObject beacon;
    const int MaxCrumbs = 80;

    public void Init(Transform playerT, Camera camera)
    {
        player = playerT; cam = camera;
        adj = new List<int>[NodePos.Length];
        for (int i = 0; i < adj.Length; i++) adj[i] = new List<int>();
        foreach (var e in Edges) { adj[e[0]].Add(e[1]); adj[e[1]].Add(e[0]); }

        var root = new GameObject("Guidance").transform;
        for (int i = 0; i < MaxCrumbs; i++)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            s.name = "Crumb";
            Destroy(s.GetComponent<Collider>());
            s.transform.SetParent(root, false);
            s.transform.localScale = new Vector3(.3f, .08f, .3f);
            s.GetComponent<Renderer>().material.color = Color.cyan;
            s.SetActive(false);
            crumbs.Add(s.transform);
        }
        beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beacon.name = "DestinationBeacon";
        Destroy(beacon.GetComponent<Collider>());
        beacon.transform.SetParent(root, false);
        beacon.transform.localScale = new Vector3(.5f, 4f, .5f);
        beacon.GetComponent<Renderer>().material.color = Color.yellow;
        beacon.SetActive(false);
    }

    public void SetDestination(Dest d)
    {
        Current = d;
        if (d != Dest.None && beacon != null)
        {
            var p = NodePos[DestNode(d)];
            beacon.transform.position = new Vector3(p.x, 4f, p.y);
        }
    }

    static int DestNode(Dest d)
    {
        switch (d)
        {
            case Dest.CheckIn: return 1;
            case Dest.Security: return 3;
            case Dest.GateA1: return 7;
            default: return 10;
        }
    }

    int Nearest(Vector2 p)
    {
        int best = 0; float bd = float.MaxValue;
        for (int i = 0; i < NodePos.Length; i++)
        {
            float d = Vector2.Distance(p, NodePos[i]);
            if (d < bd) { bd = d; best = i; }
        }
        return best;
    }

    List<int> FindPath(int s, int t)
    {
        int n = NodePos.Length;
        var prev = new int[n];
        for (int i = 0; i < n; i++) prev[i] = -2;
        prev[s] = -1;
        var q = new Queue<int>(); q.Enqueue(s);
        while (q.Count > 0)
        {
            int u = q.Dequeue();
            if (u == t) break;
            foreach (int v in adj[u]) if (prev[v] == -2) { prev[v] = u; q.Enqueue(v); }
        }
        var path = new List<int>();
        for (int c = t; c >= 0; c = prev[c]) path.Add(c);
        path.Reverse();
        return path;
    }

    void HideGuidance()
    {
        if (beacon != null) beacon.SetActive(false);
        foreach (var c in crumbs) if (c != null) c.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;
        if (Current == Dest.None)
        {
            HideGuidance();
            Instruction = "Select a destination: keys 1-6 (or press Tab and click a button)";
            return;
        }

        Vector3 p = player.position;
        Vector2 pf = new Vector2(p.x, p.z);
        List<int> idx = FindPath(Nearest(pf), DestNode(Current));
        var pts = new List<Vector2>();
        foreach (int n in idx) pts.Add(NodePos[n]);

        while (pts.Count > 1 &&
               (Vector2.Distance(pf, pts[1]) < Vector2.Distance(pts[0], pts[1]) || Vector2.Distance(pf, pts[0]) < 2.5f))
        {
            pts.RemoveAt(0); idx.RemoveAt(0);
        }

        float dist = Vector2.Distance(pf, pts[0]);
        for (int i = 1; i < pts.Count; i++) dist += Vector2.Distance(pts[i - 1], pts[i]);

        Vector3 dir = new Vector3(pts[0].x - p.x, 0f, pts[0].y - p.z);
        Vector3 f = cam != null ? cam.transform.forward : player.forward;
        f.y = 0f;
        if (f.sqrMagnitude < 0.001f) f = player.forward;
        f.Normalize();
        float a = dir.sqrMagnitude > 0.0001f ? Vector3.SignedAngle(f, dir, Vector3.up) : 0f;
        ArrowAngle = a;

        string turn = Mathf.Abs(a) < 20f ? "Go straight" : Mathf.Abs(a) > 150f ? "Turn around" : (a > 0f ? "Turn right" : "Turn left");
        Instruction = AssistOn
            ? turn + "   |   next: " + NodeName[idx[0]] + "   |   " + dist.ToString("0") + " m to destination"
            : "Guidance OFF - read overhead signage, FIDS boards & Info Kiosks (E) to navigate";

        if (!AssistOn) { HideGuidance(); return; }

        if (beacon != null) beacon.SetActive(true);
        int ci = 0; float carry = 1.5f;
        Vector3 prevP = new Vector3(p.x, 0.05f, p.z);
        foreach (var q in pts)
        {
            Vector3 cur = new Vector3(q.x, 0.05f, q.y);
            float seg = Vector3.Distance(prevP, cur);
            float d = carry;
            while (d <= seg && ci < crumbs.Count)
            {
                crumbs[ci].position = Vector3.Lerp(prevP, cur, d / seg);
                crumbs[ci].gameObject.SetActive(true);
                ci++; d += 2f;
            }
            carry = d - seg;
            prevP = cur;
        }
        for (; ci < crumbs.Count; ci++) crumbs[ci].gameObject.SetActive(false);
    }
}
