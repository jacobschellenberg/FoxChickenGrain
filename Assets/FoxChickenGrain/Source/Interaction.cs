using UnityEngine;
using System.Collections;

public class Interaction : MonoBehaviour {

	float checkDistance = 3.5f;
	Vector3 heldObjectSize = new Vector3(0.5f, 0.5f, 0.5f);

	GameObject playerHeldObject;
	GameObject playerHoldPoint;
	GameObject boatHoldPoint;
	GameObject boatRoot;
	GameObject fox;
	GameObject chicken;
	GameObject grain;
	GameObject foxRespawn;
	GameObject chickenRespawn;
	GameObject grainRespawn;
	GameObject playerRespawn;
	GameObject boatRespawn;
	GameObject riddleObjectsRoot;
	GameObject water;
	GameObject fade;
	GameObject winCameraPosition;
	GameObject crosshair;
	UILabel lookingAt;
	UILabel interactMessage;

	AudioClip pickUpSound;
	AudioClip putDownSound;
	AudioClip exitBoatSound;
	AudioClip enterBoatSound;
	AudioClip drown;
	AudioClip cluck;
	
	bool playerHoldSomething;
	bool playerInBoat;
	bool onStartingIsland = true;
	bool isFoxOnIsland1 = true;
	bool isChickenOnIsland1 = true;
	bool isGrainOnIsland1 = true;
	bool didWin;

	void Start(){
		fox = GameObject.FindGameObjectWithTag("Fox");
		chicken = GameObject.FindGameObjectWithTag("Chicken");
		grain = GameObject.FindGameObjectWithTag("Grain");
		foxRespawn = GameObject.FindGameObjectWithTag("Fox Respawn");
		chickenRespawn = GameObject.FindGameObjectWithTag("Chicken Respawn");
		grainRespawn = GameObject.FindGameObjectWithTag("Grain Respawn");
		playerRespawn = GameObject.FindGameObjectWithTag("Respawn");
		playerHoldPoint = GameObject.FindGameObjectWithTag("Hold Point");
		boatHoldPoint = GameObject.FindGameObjectWithTag("Boat Hold Point");
		boatRoot = GameObject.FindGameObjectWithTag("Boat");
		boatRespawn = GameObject.FindGameObjectWithTag("Boat Respawn");
		riddleObjectsRoot = GameObject.FindGameObjectWithTag("Riddle Objects");
		water = GameObject.FindGameObjectWithTag("Water");
		winCameraPosition = GameObject.FindGameObjectWithTag("Winning Camera Position");
		fade = GameObject.FindGameObjectWithTag("Fade");
		crosshair = GameObject.FindGameObjectWithTag("Crosshair");
		pickUpSound = (AudioClip)Resources.Load("pickup");
		putDownSound = (AudioClip)Resources.Load("putdown");
		exitBoatSound = (AudioClip)Resources.Load("exitboat");
		enterBoatSound = (AudioClip)Resources.Load("enterboat");
		drown = (AudioClip)Resources.Load("drown");
		cluck = (AudioClip)Resources.Load("cluck");
		lookingAt = GameObject.FindGameObjectWithTag("Looking At").GetComponent<UILabel>();
		interactMessage = GameObject.FindGameObjectWithTag("Interact Message").GetComponent<UILabel>();
	}

	void Update () {
		LookingAt();

		if(Input.GetKeyDown(KeyCode.R))
			Application.LoadLevel("Game"); //Reload Game

		if(Input.GetButtonDown("Fire1"))
			Interact();

		if(playerInBoat && Input.GetButtonDown("Jump"))
			ExitBoat();
	}

	void LookingAt(){
		StartCoroutine(_LookingAt());
	}

	IEnumerator _LookingAt(){
		lookingAt.text = string.Empty;
		interactMessage.text = string.Empty;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Update Raycast info
		RaycastHit hit;
		
		if (Physics.Raycast(ray, out hit, checkDistance)){
			if(!playerHoldSomething){
				if(hit.collider.CompareTag("Fox") || hit.collider.CompareTag("Chicken") || hit.collider.CompareTag("Grain")){
					lookingAt.text = hit.collider.name;
					interactMessage.text = "Left Click to Pick Up";
				}
			}
			else if(hit.collider.CompareTag("Terrain") && !playerInBoat){
				lookingAt.text = "Ground";
				interactMessage.text = "Left Click to Put Down";
			}

			if(hit.collider.CompareTag("Boat") && playerInBoat){
				lookingAt.text = hit.collider.name;
				interactMessage.text = "Space(Jump) to Exit";
			}
			else if(hit.collider.CompareTag("Boat")){
				lookingAt.text = hit.collider.name;
				interactMessage.text = "Left Click to Enter";
			}
		}

		yield return null;
	}

	void Interact(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Update Raycast info
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, checkDistance)){
			if(!playerHoldSomething){
				if(hit.collider.CompareTag("Fox") || hit.collider.CompareTag("Chicken") || hit.collider.CompareTag("Grain"))
					PickUp(hit.transform.gameObject);
			}
			
			if(!playerInBoat && hit.collider.CompareTag("Boat"))
				EnterBoat();
			
			if(playerHoldSomething && hit.collider.CompareTag("Terrain")){
				if(!playerInBoat)
					PutDown(hit.point);
			}
		}
	}
	
	void PickUp(GameObject gameObject){
		PlaySound(pickUpSound);

		if(gameObject.collider.CompareTag("Chicken"))
		   PlaySound(cluck);

		gameObject.transform.parent = playerHoldPoint.transform;
		gameObject.transform.position = playerHoldPoint.transform.position;
		gameObject.transform.localScale = heldObjectSize;
		playerHeldObject = gameObject.transform.gameObject;
		playerHeldObject.transform.collider.enabled = false;
		playerHoldSomething = true;
	}

	void PutDown(Vector3 point){
		PlaySound(putDownSound);
		playerHeldObject.transform.position = new Vector3(point.x, point.y + 0.5f, point.z);//
		playerHeldObject.transform.localScale = new Vector3(1,1,1);
		playerHeldObject.transform.parent = riddleObjectsRoot.transform;
		playerHeldObject.collider.enabled = true;
		playerHeldObject = null;
		playerHoldSomething = false;

		if(!isFoxOnIsland1 && !isChickenOnIsland1 && !isGrainOnIsland1){
			Win ();
		}
	}

	void EnterBoat(){
		PlaySound(enterBoatSound);

		if(playerHoldSomething){
			if(playerHeldObject.CompareTag("Fox"))
				isFoxOnIsland1 = !onStartingIsland;
			if(playerHeldObject.CompareTag("Chicken"))
				isChickenOnIsland1 = !onStartingIsland;
			if(playerHeldObject.CompareTag("Grain"))
				isGrainOnIsland1 = !onStartingIsland;
		}
		Verify();

		this.GetComponent<FirstPersonHeadBob>().enabled = false;
		gameObject.transform.position = boatHoldPoint.transform.position;
		gameObject.transform.rotation = boatHoldPoint.transform.rotation;
		boatRoot.transform.parent = this.transform;
		playerInBoat = true;

		water.collider.isTrigger = false;
	}

	void ExitBoat(){
		PlaySound(exitBoatSound);

		if(playerHoldSomething){
			if(playerHeldObject.CompareTag("Fox"))
				isFoxOnIsland1 = onStartingIsland;
			if(playerHeldObject.CompareTag("Chicken"))
				isChickenOnIsland1 = onStartingIsland;
			if(playerHeldObject.CompareTag("Grain"))
				isGrainOnIsland1 = onStartingIsland;
		}

		this.GetComponent<FirstPersonHeadBob>().enabled = true;

		boatRoot.transform.parent = null;

		playerInBoat = false;
		water.collider.isTrigger = true;
	}

	void Verify(){
		if(playerHoldSomething){
			if(isFoxOnIsland1 && isChickenOnIsland1 && !isGrainOnIsland1 && !playerHeldObject.CompareTag("Fox") && !playerHeldObject.CompareTag("Chicken"))
				ResetGame();
			else if(!isFoxOnIsland1 && isChickenOnIsland1 && isGrainOnIsland1 && !playerHeldObject.CompareTag("Chicken") && !playerHeldObject.CompareTag("Grain"))
				ResetGame();
			else if(!isFoxOnIsland1 && !isChickenOnIsland1 && isGrainOnIsland1 && !playerHeldObject.CompareTag("Fox") && !playerHeldObject.CompareTag("Chicken"))
				ResetGame();
			else if(isFoxOnIsland1 && !isChickenOnIsland1 && !isGrainOnIsland1 && !playerHeldObject.CompareTag("Chicken") && !playerHeldObject.CompareTag("Grain"))
				ResetGame();
		}
		else{
			if(isFoxOnIsland1 && isChickenOnIsland1 && !isGrainOnIsland1)
				ResetGame();
			else if(!isFoxOnIsland1 && isChickenOnIsland1 && isGrainOnIsland1)
				ResetGame();
			else if(!isFoxOnIsland1 && !isChickenOnIsland1 && isGrainOnIsland1)
				ResetGame();
			else if(isFoxOnIsland1 && !isChickenOnIsland1 && !isGrainOnIsland1)
				ResetGame();
		}
	}

	void Win(){
		didWin = true;

		lookingAt.text = string.Empty;
		interactMessage.text = string.Empty;

		crosshair.gameObject.SetActive(false);
		Camera.main.transform.parent = null;
		Camera.main.GetComponent<SimpleMouseRotator>().enabled = false;
		Camera.main.GetComponent<TweenPosition>().from = Camera.main.transform.position;
		Camera.main.GetComponent<TweenPosition>().to = winCameraPosition.transform.position;
		Camera.main.GetComponent<TweenRotation>().from = Camera.main.transform.rotation.eulerAngles;
		Camera.main.GetComponent<TweenRotation>().to = winCameraPosition.transform.rotation.eulerAngles;
		Camera.main.GetComponent<TweenPosition>().enabled = true;
		Camera.main.GetComponent<TweenRotation>().enabled = true;
		Camera.main.GetComponent<TweenPosition>().PlayForward();
		Camera.main.GetComponent<TweenRotation>().PlayForward();
		this.gameObject.SetActive(false);
	}
	
	void ResetGame(){
		StartCoroutine(_ResetGame());
	}

	IEnumerator _ResetGame(){
		fade.GetComponent<TweenAlpha>().PlayReverse();

		yield return new WaitForSeconds(fade.GetComponent<TweenAlpha>().duration);

		gameObject.transform.position = playerRespawn.transform.position;
		boatRoot.transform.parent = null;
		boatRoot.transform.position = boatRespawn.transform.position;
		boatRoot.transform.rotation = boatRespawn.transform.rotation;
		fox.transform.parent = riddleObjectsRoot.transform;
		fox.transform.position = foxRespawn.transform.position;
		fox.transform.localScale = new Vector3(1,1,1);
		fox.collider.enabled = true;
		chicken.transform.parent = riddleObjectsRoot.transform;
		chicken.transform.position = chickenRespawn.transform.position;
		chicken.collider.enabled = true;
		chicken.transform.localScale = new Vector3(1,1,1);
		grain.transform.parent = riddleObjectsRoot.transform;
		grain.transform.position = grainRespawn.transform.position;
		grain.transform.localScale = new Vector3(1,1,1);
		grain.collider.enabled = true;
		water.collider.enabled = true;

		playerHoldSomething = false;
		playerInBoat = false;
		onStartingIsland = true;
		isFoxOnIsland1 = true;
		isChickenOnIsland1 = true;
		isGrainOnIsland1 = true;
		didWin = false;

		fade.GetComponent<TweenAlpha>().PlayForward();
	}

	void OnTriggerEnter(Collider trigger){
		if(!playerInBoat && trigger.collider.CompareTag("Water")){
			PlaySound(drown);
			ResetGame();
		}

		if(trigger.CompareTag("Boat Exit Trigger 1"))
			onStartingIsland = true;

		if(trigger.CompareTag("Boat Exit Trigger 2"))
			onStartingIsland = false;
	}

	void PlaySound(AudioClip clip){
		StartCoroutine(_PlaySound(clip));
	}

	IEnumerator _PlaySound(AudioClip clip){
		GameObject audioSource = new GameObject("Audio_" + clip.name);
		audioSource.AddComponent<AudioSource>();
		audioSource.GetComponent<AudioSource>().clip = clip;
		audioSource.GetComponent<AudioSource>().Play();

		while(audioSource.GetComponent<AudioSource>().isPlaying)
			yield return null;

		Destroy(audioSource);
	}
}






