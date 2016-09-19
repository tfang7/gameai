/*
 * TOMMY FANG
 * GAME AI HW 1
 * STATES: PATHFOLLOWING, PURSUING, EVADING, WANDERING
 * The scene is a little bit buggy 
 * When the game is played, the hunter and wolf start off in the wander state
 * Little RED starts her path using PATHFOLLOWING to Grandma's house.
 * When they wander next to each other, the HUNTER goes into PURSUING state 
 * the WOLF goes into the evading state.
 * The code for avoiding the target seems to be working and the prediction for pursue seek commands.
 * I wasn't sure what to do when the fleeing target left the bounds of the view, so I just resset the position to 0,0, which makes it buggy.
 * arrive code is implemented exactly as instructed, but can't find error in it.
  */

  /* ** IMPORTANT **
   * You can modify the AI state on play using the State field in the AI script inside the INSPECTOR
   * Other values like max speed/acceleration/timeToTarget and target transforms can be modified there.
   * */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class AI : MonoBehaviour {
    private bool alive;
    public Vector3 linearVelocity, linearAcceleration;
    public float angularAcceleration, angularVelocity;

    public float maxSpeed, maxAcceleration;
    public float maxRotationSpeed = 10.0f;
    private GameObject circle;

    public float targetDistance;
    public float distance;
    public float time;
    public float timeToTarget;
    public Vector3 dest;

    public Transform target;
    public GameObject wanderTarget;
    public Camera cam;
    private float radius;

    private int pathIndex;
    private GameObject[] pathPoints;

    public enum State
    {
        WANDERING,
        EVADING,
        PURSUING,
        PATHFOLLOWING
    }
    public State state;

    // Use this for initialization
    void Start () {
        
        alive = true;
        radius = this.gameObject.GetComponent<SphereCollider>().radius / 2f;
        GameObject.FindWithTag("wolfTarget").transform.position = GameObject.Find("Wolf").transform.position;
        GameObject.FindWithTag("hunterTarget").transform.position = GameObject.Find("Hunter").transform.position;

        if (state == State.WANDERING) setWanderTarget();
        cam = Camera.main;
        pathPoints = GameObject.FindGameObjectsWithTag("wayPoints");
        pathPoints = pathPoints.OrderBy(path => path.name).ToArray();
        pathIndex = 0;// pathPoints.Length -1;
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
                case State.EVADING:
                    Flee();
                    break;
                case State.PATHFOLLOWING:
                    Pathfollow();
                    break;
                case State.PURSUING:
                    Pursue();
                    break;
            }
            yield return null;
        }
    }
	// Update is called once per frame
	void Update () {
        displayState();
        FSM();
	}
    public void Pathfollow()
    {
        if (pathIndex > pathPoints.Length - 1) pathIndex = 0;
        target = pathPoints[pathIndex].transform;
        distance = Vector3.Distance(target.position, transform.position);
        seekTarget(target.position);
        align();

        if (distance < 1f)
        {
            pathIndex++;
        }
    }
    void Flee()
    {
        //Avoid pursuer
        linearAcceleration = transform.position - target.position;
        linearVelocity += linearAcceleration * Time.deltaTime;
        clipAccelVeloc();
        angularAcceleration = 0;
        align();
        //check if fleeing within bounds
        if (checkBounds(transform.position + linearVelocity * Time.deltaTime))
        {
            transform.position += linearVelocity * Time.deltaTime;
        }
        else
        {
            transform.position = new Vector3(transform.position.x / -2, 0);
        }
        distance = Vector3.Distance(transform.position, dest);
    }
    void seekTarget(Vector3 target)
    {
        //Move towards target at full acceleration
        linearAcceleration = target - transform.position;
        linearAcceleration.Normalize();
        linearAcceleration *= maxAcceleration;
        //clip velocity
        linearVelocity += linearAcceleration;
        linearVelocity.Normalize();
        linearVelocity *= maxSpeed;
        //move ai
        transform.position += linearVelocity * Time.deltaTime;
    }
    //if the AI is a hunter or a wolf in WANDERING state
    //create a new wander target
    void setWanderTarget()
    {
        if (state == State.WANDERING)
        {

            if (this.gameObject.name.ToLower() == "hunter")
            {
                wanderTarget = GameObject.FindWithTag("hunterTarget");
            }
            if (this.gameObject.name.ToLower() == "wolf")
            {
                wanderTarget = GameObject.FindWithTag("wolfTarget");
            }
            //calculate the new position inside the unit circle and in front of AI
            circle = wanderTarget;
            target = wanderTarget.transform;
            Vector3 circlePos = calculateTargetPosition(radius);
            dest = target.transform.position + circlePos;
            circle.transform.position = transform.position + (transform.right * targetDistance);

        }
    }
    void Pursue()
    {
        //Find the circle that gets drawn upon arrival.
        drawCircle circ = GameObject.Find("arriveRadius").GetComponent<drawCircle>();
        //Check Scene view for this line that is drawn when an AI is in pursuit, it draws a line from
        //current AI position to target AI position
        Debug.DrawLine(target.position + (target.right * 1), transform.position, Color.green, 2f);
        //predict target future movement
        Vector3 predictedTarget = target.position + (target.right * 1);
        //calculate steering towards future position
        seekTarget(predictedTarget);
        
        distance = Vector3.Distance(transform.position, target.position);
        align();

        //This checks if current is within the radius of the arrival circle
        if (distance < circ.xrad / 2)
        {
            Arrive(distance);
            distance = Vector3.Distance(transform.position, predictedTarget);
            if (distance < 0.2f)
            {
                Debug.Log("arrived");
            }
        }
        else
        {
            LineRenderer rad = GameObject.Find("arriveRadius").GetComponent<LineRenderer>();
            rad.enabled = false;

        }
    }
    void Arrive(float dist)
    {
        //Find circle object to draw when arriving
        LineRenderer rad = GameObject.Find("arriveRadius").GetComponent<LineRenderer>();
        rad.enabled = true;
        rad.transform.position = target.position;

        //calculate target speed
        float targetSpeed = maxSpeed * (dist / 1.25f);
        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 targetVelocity = dir * targetSpeed;
        linearVelocity = targetVelocity - linearVelocity;
        time += Time.deltaTime;
        if (time > timeToTarget) time = 0f;

        //Acceleration tries to get to the target velocity
        linearVelocity += linearAcceleration;
        //clip velocity
        if (linearVelocity.magnitude > maxSpeed)
        {
            linearVelocity.Normalize();
            linearVelocity *= maxSpeed;
        }
        //clip acceleration
        if (linearAcceleration.magnitude > maxAcceleration)
        {
            linearAcceleration.Normalize();
            linearAcceleration *= maxAcceleration;
        }
        //move based on time
        //not sure why this doesn't work, followed slides exactly
        linearVelocity = linearVelocity / timeToTarget;
        transform.position += linearVelocity * Time.deltaTime;

    }
    void Evade() { }
    void Wander()
    {
     //   dest = target.position;
        linearAcceleration = (dest - transform.position);

        linearVelocity = linearAcceleration;
        //clip accel/velocity to max accel/speed
        clipAccelVeloc();

        distance = Vector3.Distance(transform.position, dest);

        transform.position += linearVelocity * Time.deltaTime;

        if (linearVelocity != Vector3.zero) align();
        
        if (distance < 0.5f)
        {
            Vector3 circlePos = calculateTargetPosition(radius);
            circle.transform.position = transform.transform.position + (transform.right * targetDistance);
            Vector2 temp = (Vector2)circle.transform.position + (Vector2)circlePos;

            bool check = checkBounds(temp);
            if (check)
            {
                circle.transform.position = transform.transform.position + (transform.right * targetDistance);
                dest = (Vector2)circle.transform.position + (Vector2)circlePos;
            }
        }
        if (this.gameObject.name.ToLower() == "wolf")
        {
            GameObject hunter = GameObject.Find("Hunter");
            float distToHunter = Vector3.Distance(hunter.transform.position, transform.position);
            //when the wolf runs into the hunter while wandering
            if (distToHunter < 2f)
            {
                //set the hunter to purse the wolf
                AI HunterAI = hunter.GetComponent<AI>();
                HunterAI.state = State.PURSUING;
                HunterAI.target = transform;

                state = State.EVADING;
                target = HunterAI.transform;
            }
            Debug.DrawLine(hunter.transform.position, transform.position, Color.green);
        }
    }
    //generate point on unit circle
    Vector3 calculateTargetPosition(float radius)
    {
        Vector3 randomPointOnCircle = Random.insideUnitCircle;
        randomPointOnCircle.Normalize();
        randomPointOnCircle *= radius;
        return randomPointOnCircle;
    }
    //clip velocity and acceleration to max values
    void clipAccelVeloc()
    {
            linearVelocity = linearVelocity.normalized;
            linearVelocity *= maxSpeed;
            linearAcceleration = linearAcceleration.normalized;
            linearAcceleration *= maxAcceleration;

    }
    void displayState()
    {
        string name = this.gameObject.name.ToLower();
        TextMesh mesh;
        switch (name)
        {
            case ("wolf"):
                mesh = GameObject.Find("WolfState").GetComponent<TextMesh>();
                mesh.text = state.ToString();
                break;
            case ("red"):
                mesh = GameObject.Find("RedState").GetComponent<TextMesh>();
                mesh.text = state.ToString();
                break;
            case ("hunter"):
                mesh = GameObject.Find("HunterState").GetComponent<TextMesh>();
                mesh.text = state.ToString();
                break;
        }

        //  Debug.Log(name);
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
    void align()
    {
   //     angularVelocity += 1;
        float angle = Mathf.Atan2(linearVelocity.y, linearVelocity.x) * Mathf.Rad2Deg;
        //rotate towards target
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = q;
        Quaternion.RotateTowards(transform.rotation, q, 60f * Time.deltaTime);
    }
}
   
