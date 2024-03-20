using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;


public class CreateTexture : MonoBehaviour
{
    [Range(1, 8)]
    public int Multiplier = 1;

    public bool ShowRays = true;
    public bool ShowRaysPersistent = false;
    public bool ShowHits = true;

    public List<GameObject> Shperes = new List<GameObject>();

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(Camera.main.transform.position, new Vector3(0.1f, 0.1f, 0.1f));
    }

    [ContextMenu("Create Pixels")]
    private void Create()
    {
        Debug.Log("Create Pixels");
        StopAllCoroutines();
        StartCoroutine(CreatePixels());
    }

    private IEnumerator CreatePixels()
    {
        int width = 16 * Multiplier;
        int height = 9 * Multiplier;

        var t = new Texture2D(width, height);
        t.filterMode = FilterMode.Point;
        GetComponent<Renderer>().sharedMaterial.mainTexture = t;

        float sizeX = (1f / (width / 16f)) * 0.5f;
        float sizeY = (1f / (height / 9f)) * 0.5f;

        float closest_distance = float.MaxValue;

        Vector3 startingPosition = transform.position - new Vector3((height - 1) * (sizeY) / 2f, (width - 1) * (sizeX) / 2f, 0f) + new Vector3(-1.75f, 1.75f, 0.0f);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Debug.Log("Row: " + y + " Col: " + x);

                Vector3 position = startingPosition + new Vector3((x) * (sizeX), (y) * (sizeY), 0f);

                closest_distance = float.MaxValue;

                bool hit = false;
                GameObject s = null;

                foreach (var sphere in Shperes)
                {
                    bool hitResult = Hit(Camera.main.transform.position, (position - Camera.main.transform.position) * 10f, sphere, ref closest_distance);
                    
                    if (hitResult)
                    {
                        hit = true;
                        s = sphere;
                    }
                }

                Color pixelColor = hit ? s.GetComponent<Renderer>().sharedMaterial.color : Color.cyan;

                t.SetPixel(x, y, pixelColor);

                if (ShowRays)
                {
                    if (ShowRaysPersistent)
                        Debug.DrawRay(Camera.main.transform.position, (position - Camera.main.transform.position) * 10f, pixelColor, 0.1f * width, true);
                    else
                        Debug.DrawRay(Camera.main.transform.position, (position - Camera.main.transform.position) * 10f, pixelColor, 0.1f, true);
                }

                t.Apply();

                yield return new WaitForSeconds(0.1f);
            }
        }

    }

    Vector3 GetPixelCenter(int x, int y)
    {
        int width = 16 * Multiplier;
        int height = 9 * Multiplier;        

        var res = transform.position - transform.TransformDirection(new Vector3(x - width/2, y - height/2, 0));
        return res;
    }

    bool Hit(Vector3 origin, Vector3 direction, GameObject sphere, ref float closest_distance)
    {
        Vector3 oc = origin - sphere.transform.position;
        float a = Vector3.Dot(direction, direction);
        float b = 2.0f * Vector3.Dot(oc, direction);
        float c = Vector3.Dot(oc, oc) - (sphere.GetComponent<SphereCollider>().radius * sphere.transform.lossyScale.x) * (sphere.GetComponent<SphereCollider>().radius * sphere.transform.lossyScale.x);

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return false;
        }

        float solution = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

        if (solution > 0.0 && solution < closest_distance)
        {
            closest_distance = solution;
            return true;
        }

        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        Create();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
