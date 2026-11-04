using System.IO;
using UnityEngine;

public class AirportBootstrap : MonoBehaviour
{
    public string participantId = "P01";
    public bool seedPilotDataIfEmpty = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureBootstrap()
    {
        if (FindFirstObjectByType<AirportBootstrap>() == null)
        {
            new GameObject("Bootstrap").AddComponent<AirportBootstrap>();
        }
    }

    void Start()
    {
        var airport = new GameObject("AirportBuilder").AddComponent<AirportBuilder>();
        airport.Build();

        var pgo = new GameObject("Player");
        pgo.transform.position = AirportData.DepartureSpawn;
        var cc = pgo.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.stepOffset = 0.3f;
        var pc = pgo.AddComponent<PlayerController>();

        Camera cam = Camera.main;
        if (cam == null)
        {
            var cg = new GameObject("Main Camera");
            cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
            cg.AddComponent<AudioListener>();
        }
        cam.transform.SetParent(pgo.transform, false);
        cam.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        cam.transform.localRotation = Quaternion.identity;
        cam.nearClipPlane = 0.05f;
        pc.cameraPivot = cam.transform;

        var nav = gameObject.AddComponent<NavigationManager>();
        nav.Init(pgo.transform, cam);

        var ui = gameObject.AddComponent<UIManager>();
        var exp = gameObject.AddComponent<ExperimentManager>();
        exp.Init(pc, nav, airport, ui, participantId);
        ui.Init(exp, nav, pc);

        if (seedPilotDataIfEmpty && !File.Exists(exp.CsvPath))
        {
            exp.GeneratePilotStudyData();
        }
    }
}
