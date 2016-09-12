using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class AI : MonoBehaviour {
    private bool alive;
    public Vector3 linearVelocity, linearAcceleration,
                   angularVelocity, angularRotation;
    public float maxSpeed;
    public float distance;
    private Transform currentTransform;
    public Transform target;
    public GameObject[] path;
    public Camera cam;
    private float height;
    private float width;
    public GameObject label;
    public enum State
    {
        WANDERING,
        HUNTING,
        PURSUING,
        PATHFOLLOWING
    }
    public State state;

    // Use this for initialization
    void Start () {
        alive = true;
        target = GameObject.Find("wanderTarget").transform;
        // currentTransform = GameObject.Find("Hunter").transform;
        // Debug.Log(currentTransform);
        cam = Camera.main;
        StartCoroutine("FSM");
	}
	//Finite State machine to hold logic for switching AI states.
    IEnumerator FSM()
    {
        while (alive)
        {
            switch (state)
            {
                case State.WANDERING:
                    Wander();
                    break;
            }
            yield return null;
        }
    }
	// Update is called once per frame
	void Update () {
        FSM();
	}
    void Pathfollow()
    {

    }
    void Wander()
    {
        currentTransform = this.gameObject.transform;
        linearVelocity = linearAcceleration = (target.position - currentTransform.position);
        distance = linearVelocity.magnitude;
        currentTransform.position += linearAcceleration * Time.deltaTime;

        float radius = this.gameObject.GetComponent<SphereCollider>().radius;
        
        if (distance < 0.5)
        {
            Vector3 pos = target.position;
            bool check = checkBounds(pos);
            if (check)
            {
                target.position += calculateTargetPosition(radius);
            } else
            {
                target.position = new Vector3(0, 0);
            }
        }
    }

    Vector3 calculateTargetPosition(float radius)
    {
        Vector3 randomPointOnCircle = Random.insideUnitCircle;
        randomPointOnCircle.Normalize();
        randomPointOnCircle *= radius;
        Debug.Log(randomPointOnCircle);
        return randomPointOnCircle;
    }
    bool checkBounds(Vector3 pos)
    {

        Vector3 p1 = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 p2 = cam.ViewportToWorldPoint(new Vector3(1, 0, cam.nearClipPlane));
        Vector3 p3 = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
        Vector3 p4 = cam.ViewportToWorldPoint(new Vector3(0, 1, cam.nearClipPlane));

        if (pos.x < p1.x || pos.x > p2.x 
         || pos.y < p1.y || pos.y > p3.y)
        {
            return false;
        }

        float width = (p2 - p1).magnitude;
        float height = (p3 - p2).magnitude;

        return true;
    }
    void align(Quaternion target, Quaternion character)
    {
        this.gameObject.transform.rotation = target * Quaternion.Inverse(character);

    }
}
   
