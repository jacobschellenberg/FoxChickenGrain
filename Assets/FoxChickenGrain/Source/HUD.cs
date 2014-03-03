using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

	void Update(){
		if(Input.GetKeyDown(KeyCode.Escape)){
			Screen.lockCursor = !Screen.lockCursor;
		}
	}

	IEnumerator FocusMouse(bool focusStatus){
		if(focusStatus){
			Debug.Log("FOCUSED");
			yield return new WaitForSeconds(0.1f); //Give enough time to allow cursor to settle
			Screen.lockCursor = true;
		}
		else{
			Debug.Log("LOST FOCUS");
			Screen.lockCursor = false;
		}

		yield return null;
	}

	void OnApplicationFocus(bool focusStatus){
		StartCoroutine("FocusMouse", focusStatus);
	}
}
