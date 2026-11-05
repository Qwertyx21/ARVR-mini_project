using System.Collections.Generic;
using UnityEngine;

public class CarouselConveyor : MonoBehaviour
{
    public Vector3 center;
    public float halfX = 3.6f;
    public float halfZ = 0.9f;
    public float speed = 0.9f; // m/s belt speed
    public float elevation = 0.88f;

    class Item
    {
        public Transform t;
        public float progress; // 0..1
    }

    readonly List<Item> items = new List<Item>();
    float totalPerimeter;
    float straightLen;
    float semiCircumference;

    void Awake()
    {
        straightLen = 2f * halfX;
        semiCircumference = Mathf.PI * halfZ;
        totalPerimeter = 2f * straightLen + 2f * semiCircumference;
    }

    public void AddLuggage(Transform luggage, float initialNormalizedPos)
    {
        if (luggage == null) return;
        items.Add(new Item { t = luggage, progress = Mathf.Repeat(initialNormalizedPos, 1f) });
        UpdateItemTransform(items[items.Count - 1]);
    }

    void Update()
    {
        if (items.Count == 0 || totalPerimeter <= 0.001f) return;
        float dt = Time.deltaTime;
        float deltaNorm = (speed * dt) / totalPerimeter;

        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (it.t == null) continue;
            it.progress = Mathf.Repeat(it.progress + deltaNorm, 1f);
            UpdateItemTransform(it);
        }
    }

    void UpdateItemTransform(Item it)
    {
        float d = it.progress * totalPerimeter;
        Vector3 pos;
        Vector3 fwd;

        // Stage 1: North straight (going East, x: -halfX -> +halfX, z = +halfZ)
        if (d < straightLen)
        {
            float frac = d / straightLen;
            pos = new Vector3(Mathf.Lerp(-halfX, halfX, frac), elevation, halfZ);
            fwd = Vector3.right;
        }
        // Stage 2: East turn (semicircle around (+halfX, 0) from +90 deg to -90 deg)
        else if (d < straightLen + semiCircumference)
        {
            float arcD = d - straightLen;
            float angle = Mathf.PI * 0.5f - (arcD / semiCircumference) * Mathf.PI;
            pos = new Vector3(halfX + Mathf.Cos(angle) * halfZ, elevation, Mathf.Sin(angle) * halfZ);
            fwd = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
        }
        // Stage 3: South straight (going West, x: +halfX -> -halfX, z = -halfZ)
        else if (d < 2f * straightLen + semiCircumference)
        {
            float frac = (d - straightLen - semiCircumference) / straightLen;
            pos = new Vector3(Mathf.Lerp(halfX, -halfX, frac), elevation, -halfZ);
            fwd = Vector3.left;
        }
        // Stage 4: West turn (semicircle around (-halfX, 0) from -90 deg to -270 deg)
        else
        {
            float arcD = d - 2f * straightLen - semiCircumference;
            float angle = -Mathf.PI * 0.5f - (arcD / semiCircumference) * Mathf.PI;
            pos = new Vector3(-halfX + Mathf.Cos(angle) * halfZ, elevation, Mathf.Sin(angle) * halfZ);
            fwd = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
        }

        it.t.position = center + pos;
        if (fwd.sqrMagnitude > 0.001f)
            it.t.rotation = Quaternion.LookRotation(fwd, Vector3.up);
    }
}
