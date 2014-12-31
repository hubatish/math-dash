using UnityEngine;
using System.Collections;

public class AndroidBridge : MonoBehaviour
{

	AndroidJavaClass androidClass;

    void Start()
    {
		AndroidJNI.AttachCurrentThread();
		androidClass = new AndroidJavaClass("com.DrexelResearch.MathDash.MathDashBridge");
    }

	public void checkMyIntOnJava(string message){
		if (message == "READY") {
			int myInt = androidClass.CallStatic<int>("getMyInt");
			Debug.Log("My Int: " + myInt);
		}
	}

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Pressed left click.");
			object[] args = new object[] { Random.Range(1,100), this.gameObject.name, "checkMyIntOnJava" };
			androidClass.CallStatic("callbackToUnityMethod", args);

        }
    }
}