using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour {
    private GameObject hunter, wolf, red;
    private GameObject[] path;
	// Use this for initialization
	void Start () {
        path = GameObject.FindGameObjectsWithTag("wayPoints");
        hunter = GameObject.Find("Hunter");
        wolf = GameObject.Find("Wolf");
        red = GameObject.Find("Red");
     //   red.SetActive(false);
        hunter.SetActive(false);
        wolf.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {

	    if (!wolf.activeSelf && !hunter.activeSelf && !red.activeSelf)
        {
            red.SetActive(true);
          //  red.transform.position = path[path.Length-1].transform.position;
            Debug.Log(red.transform.position);
        }
        // red.GetComponent<AI>().Pathfollow();
        red.GetComponent<AI>().state = AI.State.PATHFOLLOWING;

    }
}
