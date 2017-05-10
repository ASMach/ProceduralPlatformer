using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Released under Creative Commons License by Alonzo Machiraju, 2017

public class WinMap : MonoBehaviour {

	public ParticleSystem winEffect;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Player") // Only the player can pick something up and score!
		{	
			// Play effect on win
			if (winEffect) Instantiate<ParticleSystem>(winEffect, transform.position, transform.rotation * Quaternion.Euler(-90.0f, 0.0f, 0.0f));

			// Update player score here

			PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();

			// TODO: Calculate the player's final score based on gems.

			// Remove so that we don't pick it up twice!
			Destroy(gameObject);
		}
	}
}
