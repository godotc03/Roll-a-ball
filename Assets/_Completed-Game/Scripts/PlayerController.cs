using UnityEngine;

// Include the namespace required to use Unity UI
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    //DLC Test
    private string CDN_URL = "http://localhost:3000";
    private string Version = "v1.1.1";
    private string Local_CDN_URL = "";
    //private string[] fileList = { "dlc_v1.ab"};
    private AssetBundle scene_bundle = null;

    //end of DLC Test
    // Create public variables for player speed, and for the Text UI game objects
    public float speed;
    public Text countText;
    public Text winText;

    // Create private references to the rigidbody component on the player, and the count of pick up objects picked up so far
    private Rigidbody rb;
    private int count;

    // At the start of the game..
    void Start()
    {
        // Assign the Rigidbody component to our private rb variable
        rb = GetComponent<Rigidbody>();

        // Set the count to zero 
        count = 0;

        // Run the SetCountText function to update the UI (see below)
        SetCountText();

        // Set the text property of our Win Text UI to an empty string, making the 'You Win' (game over message) blank
        winText.text = "";

#if UNITY_EDITOR_WIN
        Local_CDN_URL = "file:///" + Application.dataPath + "/../FakeServer/";
#elif UNITY_EDITOR_OSX
        Local_CDN_URL = "file://" + Application.dataPath + "/../FakeServer/";
#endif
        StartCoroutine(DLC_Instance());
    }

    // Each physics step..
    void FixedUpdate ()
	{
		// Set some local float variables equal to the value of our Horizontal and Vertical Inputs
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		// Create a Vector3 variable, and assign X and Z to feature our horizontal and vertical float variables above
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		// Add a physical force to our Player rigidbody using our 'movement' Vector3 above, 
		// multiplying it by 'speed' - our public player speed that appears in the inspector
		rb.AddForce (movement * speed);
	}

	// When this game object intersects a collider with 'is trigger' checked, 
	// store a reference to that collider in a variable named 'other'..
	void OnTriggerEnter(Collider other) 
	{
		// ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
		if (other.gameObject.CompareTag ("Pick Up"))
		{
			// Make the other game object (the pick up) inactive, to make it disappear
			other.gameObject.SetActive (false);

			// Add one to the score variable 'count'
			count = count + 1;

			// Run the 'SetCountText()' function (see below)
			SetCountText ();
		}
	}

	// Create a standalone function that can update the 'countText' UI and check if the required amount to win has been achieved
	void SetCountText()
	{
		// Update the text field of our 'countText' variable
		countText.text = "Count: " + count.ToString ();

		// Check if our 'count' is equal to or exceeded 12
		if (count >= 12) 
		{
			// Set the text value of our 'winText'
			winText.text = "You Win!";
            if(scene_bundle != null)
            {
                string[] scenePaths = scene_bundle.GetAllScenePaths();
                if(scenePaths.Length > 0){
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePaths[0]);
                    SceneManager.LoadScene(sceneName);
                }
            }
        }
	}

    private IEnumerator DLC_Instance()
    {
        //string CDN = CDN_URL + "/";      //use http://v
        string CDN = Local_CDN_URL + "/";  //use local fake server
        string url = CDN + Version + "/dlc_v1.ab";

        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
        yield return request.SendWebRequest();

        if (!request.isHttpError)
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            GameObject cube = bundle.LoadAsset<GameObject>("Cube");
            Instantiate(cube,new Vector3(2f,0.5f,0f),Quaternion.identity);
        }
        else{
            Debug.Log("can't load assetbundle: dlc_v1.ab");
        }

        url = CDN + Version + "/dlc_v1_scene.ab";
        request = UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
        yield return request.SendWebRequest();
        if(!request.isHttpError)
        {
            scene_bundle = DownloadHandlerAssetBundle.GetContent(request);
        }
        else
        {
            Debug.Log("can't load assetbundle: dlc_v1_scene.ab");
            Debug.Log("error:"+ request.error);
        }
    }
}