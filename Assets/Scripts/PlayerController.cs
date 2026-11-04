using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform cameraPivot;
    public float walkSpeed = 3.8f, sprintSpeed = 6.5f, mouseSensitivity = 2f, gravity = -20f;
    public float maxTeleportDist = 14f;

    public bool MovementLocked { get; set; }
    public bool CursorFree { get; private set; }
    public bool TeleportModeOn { get; set; }
    public float Speed { get; private set; }
    public int TeleportCount { get; set; }

    CharacterController cc;
    float pitch, vy;

    // VR / Teleportation visuals
    LineRenderer teleportArc;
    GameObject teleportReticle;
    bool aimingTeleport;
    bool validTeleportTarget;
    Vector3 teleportTargetPos;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        SetupTeleportVisuals();
        SetCursorFree(false);
    }

    void SetupTeleportVisuals()
    {
        var arcGo = new GameObject("TeleportArc");
        arcGo.transform.SetParent(transform, false);
        teleportArc = arcGo.AddComponent<LineRenderer>();
        teleportArc.positionCount = 20;
        teleportArc.startWidth = 0.035f;
        teleportArc.endWidth = 0.015f;
        var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mat = new Material(tmp.GetComponent<Renderer>().sharedMaterial);
        Destroy(tmp);
        mat.color = new Color(0f, 0.95f, 1f, 1f);
        teleportArc.sharedMaterial = mat;
        teleportArc.enabled = false;

        teleportReticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        teleportReticle.name = "TeleportReticle";
        Destroy(teleportReticle.GetComponent<Collider>());
        teleportReticle.transform.localScale = new Vector3(0.9f, 0.02f, 0.9f);
        teleportReticle.GetComponent<Renderer>().sharedMaterial = mat;
        teleportReticle.SetActive(false);
    }

    public void SetCursorFree(bool free)
    {
        CursorFree = free;
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
        if (free) CancelTeleportAim();
    }

    public void Teleport(Vector3 pos, float yaw)
    {
        CancelTeleportAim();
        cc.enabled = false;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitch = 0f; vy = 0f;
        if (cameraPivot != null) cameraPivot.localRotation = Quaternion.identity;
        cc.enabled = true;
    }

    public void ShortHopTeleport(Vector3 targetFloorPos)
    {
        cc.enabled = false;
        transform.position = new Vector3(targetFloorPos.x, 0.05f, targetFloorPos.z);
        vy = 0f;
        cc.enabled = true;
        TeleportCount++;
    }

    void CancelTeleportAim()
    {
        aimingTeleport = false;
        validTeleportTarget = false;
        if (teleportArc != null) teleportArc.enabled = false;
        if (teleportReticle != null) teleportReticle.SetActive(false);
    }

    void Update()
    {
        if (AirportInput.GetKeyDown(KeyCode.Tab)) SetCursorFree(!CursorFree);
        if (AirportInput.GetKeyDown(KeyCode.Escape)) SetCursorFree(true);

        bool frozen = MovementLocked || CursorFree;
        Vector3 move = Vector3.zero;

        if (!frozen)
        {
            // Mouse look when OpenXR headset is not overriding camera pose
            bool xrActive = XRSettings.isDeviceActive;
            Vector2 look = AirportInput.GetLookDelta();
            transform.Rotate(0f, look.x * mouseSensitivity, 0f);
            if (!xrActive)
            {
                pitch = Mathf.Clamp(pitch - look.y * mouseSensitivity, -80f, 80f);
                if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            // VR Point-and-Teleport (Right Mouse Button or W key when TeleportModeOn is active)
            HandleTeleportLocomotion();

            // Continuous WASD / Thumbstick locomotion
            Vector2 axes = AirportInput.GetMoveAxes();
            move = transform.right * axes.x + transform.forward * axes.y;
            if (move.sqrMagnitude > 1f) move.Normalize();
            move *= AirportInput.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        }
        else
        {
            CancelTeleportAim();
        }

        if (cc.isGrounded && vy < 0f) vy = -2f;
        vy += gravity * Time.deltaTime;

        Vector3 before = transform.position;
        cc.Move((move + Vector3.up * vy) * Time.deltaTime);
        Vector3 d = transform.position - before; d.y = 0f;
        Speed = Time.deltaTime > 0f ? d.magnitude / Time.deltaTime : 0f;
    }

    void HandleTeleportLocomotion()
    {
        if (cameraPivot == null) return;

        bool holdAim = AirportInput.GetRightMouseHeld();
        bool releaseAim = AirportInput.GetRightMouseUp();

        if (holdAim)
        {
            aimingTeleport = true;
            Vector3 origin = cameraPivot.position + cameraPivot.forward * 0.3f - Vector3.up * 0.2f;
            Vector3 dir = cameraPivot.forward;

            // Cast ray to find floor or wall obstacle
            validTeleportTarget = false;
            RaycastHit hit;
            if (Physics.Raycast(cameraPivot.position, dir, out hit, maxTeleportDist))
            {
                // Valid if surface normal points mostly upward (floor)
                if (hit.normal.y > 0.7f)
                {
                    validTeleportTarget = true;
                    teleportTargetPos = hit.point;
                }
            }
            else
            {
                // Parabolic drop to floor y=0
                Vector3 end = cameraPivot.position + new Vector3(dir.x, 0f, dir.z).normalized * 7.5f;
                end.y = 0.02f;
                // Ensure line of sight isn't through a wall
                Vector3 chest = transform.position + Vector3.up * 1.0f;
                Vector3 toTarget = new Vector3(end.x, 1.0f, end.z) - chest;
                if (!Physics.Raycast(chest, toTarget.normalized, toTarget.magnitude))
                {
                    validTeleportTarget = true;
                    teleportTargetPos = end;
                }
            }

            if (teleportArc != null)
            {
                teleportArc.enabled = true;
                Vector3 endPt = validTeleportTarget
                    ? teleportTargetPos + Vector3.up * 0.03f
                    : cameraPivot.position + dir * 4f;
                for (int i = 0; i < teleportArc.positionCount; i++)
                {
                    float t = i / (float)(teleportArc.positionCount - 1);
                    Vector3 pt = Vector3.Lerp(origin, endPt, t);
                    pt.y += Mathf.Sin(t * Mathf.PI) * 0.6f;
                    teleportArc.SetPosition(i, pt);
                }
                teleportArc.sharedMaterial.color = validTeleportTarget
                    ? new Color(0f, 0.95f, 1f, 1f)
                    : new Color(1f, 0.3f, 0.2f, 1f);
            }

            if (teleportReticle != null)
            {
                teleportReticle.SetActive(validTeleportTarget);
                if (validTeleportTarget)
                    teleportReticle.transform.position = teleportTargetPos + Vector3.up * 0.03f;
            }
        }
        else if (releaseAim && aimingTeleport)
        {
            if (validTeleportTarget)
                ShortHopTeleport(teleportTargetPos);
            CancelTeleportAim();
        }
        else if (!holdAim && aimingTeleport)
        {
            CancelTeleportAim();
        }
    }
}
