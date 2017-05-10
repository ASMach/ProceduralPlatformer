using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Released under Creative Commons License by Alonzo Machiraju, 2017

public class KillPlayer : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Player") // Only the player can be killed
		{	
			PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
			playerStatus.KillPlayer();
		}
	}
}
