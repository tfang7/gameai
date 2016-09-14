using UnityEngine;
using System.Collections;
[RequireComponent(typeof(LineRenderer))]
public class drawCircle : MonoBehaviour {
    public float xrad, yrad;
    public int segments;
    public Vector3[] points;
    LineRenderer line;

	// Use this for initialization
	void Start () {
        line = gameObject.GetComponent<LineRenderer>();
        line.SetVertexCount(segments + 1);
        line.useWorldSpace = false;
        line.SetWidth(0.05f, 0.05f);
        generatePoints();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void generatePoints()
    {
        float x, y;
        float z = 0f;
        float angle = 20f;

        for (int i = 0; i < segments + 1; i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xrad;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * yrad;
            line.SetPosition(i, new Vector3(x, y, z));
            angle += (360f / segments);
        }
    }
}
