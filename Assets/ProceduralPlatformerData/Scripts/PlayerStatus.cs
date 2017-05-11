using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Released under Creative Commons License by Alonzo Machiraju, 2017

public class PlayerStatus : MonoBehaviour {

	public GameObject deathEffect;

	public Canvas gameoverScreen;

	public float maxHealth = 100.0f;

	private float health;
	private int score = 0;

	public float Health {
		get { return health; }
		set {
			if (value <= 0.0f) KillPlayer();
			else if (health > maxHealth) Health = maxHealth;
			else health = value; }
	}


	public int Score {
		get { return score; }
		set { 
			if (value < 0) score = 0;
			else score = value; 

			//Debug.Log("Score: " + score);

			GameObject scoreLabel = GameObject.FindGameObjectWithTag("ScoreLabel");
			scoreLabel.GetComponent<HUD.ScoreCounter>().UpdateScoreDisplay(score);
		}
	}


	void Awake() {
		health = maxHealth;
	}
	// Use this for initialization
	void Start () {
		//Debug.Log("Current Score: " + Score);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void KillPlayer() {
		health = 0.0f;

		if (deathEffect) Instantiate(deathEffect, transform.position, transform.rotation);
		Destroy(gameObject);

		if (gameoverScreen) Instantiate(gameoverScreen);
	}
}
