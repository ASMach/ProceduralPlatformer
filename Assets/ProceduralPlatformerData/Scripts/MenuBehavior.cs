using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBehavior : MonoBehaviour {

	public string playMapName;

	// Use this for initialization
	void Start () {
		
	}

	public void StartGame () {
		Application.LoadLevel(playMapName);
	}

	public void ReloadLevel () {
		Application.LoadLevel(Application.loadedLevel);
	}
	
	public void QuitGame () {
		Application.Quit();
	}
}
