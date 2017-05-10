using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Released under Creative Commons License by Alonzo Machiraju, 2017

public class WorldGenerator : MonoBehaviour {

	public GameObject[] platforms;

	public GameObject[] gems;

	public GameObject player;

	public GameObject[] hazards;

	public GameObject goal;

	public Vector3 startingPosition = new Vector3(0.0f, 0.0f, 0.0f);

	public int minPlatforms = 15;

	public int maxPlatforms = 100;

	public int maxGemsPerPlatform = 8;

	public float platformOffset = 4.0f;

	public float hazardRisk = 0.1f;

	// Helper

	private static Vector3 RandomPointInBox(Vector3 center, Vector3 size) {

		return center + new Vector3(
			(Random.value - 0.5f) * size.x,
			(Random.value - 0.5f) * size.y,
			(Random.value - 0.5f) * size.z
		);
	}

	void MakeGem(Vector3 position)
	{
		// Get a random gem
		int gemIndex = Random.Range(0, gems.Length);

		// Now we spawn a gem
		GameObject gem = Instantiate(gems[gemIndex]);
		gem.transform.position = position;
	}

	void MakeHazard(Vector3 position)
	{
		// Get a random hazard
		int hazardIndex = Random.Range(0, hazards.Length);

		// Now we spawn a gem
		GameObject hazard = Instantiate(hazards[hazardIndex]);
		hazard.transform.position = position;
	}

	GameObject MakePlatform(Vector3 position)
	{
		int platformIndex = Random.Range(0, platforms.Length); // We will use this to spawn a base platform

		GameObject platform = Instantiate(platforms[platformIndex]);
		platform.transform.position = position;

		return platform;
	}

	void AddGemDeposit(Vector3 position, BoxCollider collider)
	{
		int gemCount = Random.Range(1, maxGemsPerPlatform);

		for (int i = 0; i < gemCount; i++)
		{
			MakeGem(RandomPointInBox(position + new Vector3(0.0f, collider.size.y, 0.0f), collider.size));
		}
	}

	void AddDeathBox()
	{

		// We need a way to kill the player on a fallout

		GameObject deathBox = new GameObject();
		deathBox.AddComponent<BoxCollider>();
		deathBox.GetComponent<BoxCollider>().isTrigger = true; // Otherwise it won't be able to kill us!

		float deathBoxSide = Mathf.Pow(maxPlatforms * platformOffset, 2); // Cannot possibly fall beyond this size!
		deathBox.GetComponent<BoxCollider>().size = new Vector3(deathBoxSide, 10.0f, deathBoxSide);

		// Add script to kill us
		deathBox.AddComponent<KillPlayer>();

		// Actually add the deathbox to our map
		Instantiate(deathBox, new Vector3(0.0f, -deathBoxSide, 0.0f), Quaternion.identity);
	}

	// Lifecycle

	void Awake () {

		// We generate our world before doing anything else

		Stack<GameObject> platformsInMap = new Stack<GameObject>(); // We will need to track all of our platforms here

		platformsInMap.Push(MakePlatform(startingPosition)); // Create our first platform and add it to the stack

		Vector3 endingPosition = new Vector3(Random.Range(-maxPlatforms, maxPlatforms), Random.Range(-maxPlatforms, maxPlatforms), Random.Range(-maxPlatforms, maxPlatforms));

		GameObject endPlatform = MakePlatform(endingPosition);

		// Now create the end, with the goal located at it.

		platformsInMap.Push(endPlatform);

		GameObject endGoal = Instantiate(goal, endPlatform.transform.position + new Vector3(0.0f, 2.0f, 0.0f), endPlatform.transform.rotation);
		endGoal.transform.parent = endPlatform.transform; // We need to be able to move both of them at the same time if a viable solution is not possible.

		// Create the rest of the platforms at random

		float distanceToGoal = Vector3.Distance(startingPosition, endingPosition); // Start by getting distance to goal

		Debug.Log("We are " + distanceToGoal + " units from the goal");
		Debug.Log("X Offset: " + (startingPosition.x - endingPosition.x));
		Debug.Log("Y Offset: " + (startingPosition.y - endingPosition.y));
		Debug.Log("Z Offset: " + (startingPosition.z - endingPosition.z));

		// See how many platforms we need until the end

		int directPathPlatforms = (int)(distanceToGoal / platformOffset);

		// Get the number of platforms to use
		int platformCount;

		if (directPathPlatforms < minPlatforms) platformCount = Random.Range(minPlatforms, maxPlatforms);
		else platformCount = Random.Range(directPathPlatforms, maxPlatforms);

		// Lay out basic path
		for (int i = 0; i < directPathPlatforms; i++)
		{
			GameObject previousPlatform = platformsInMap.Pop();

			Vector3 newLocation = (startingPosition - previousPlatform.transform.position) / 2;

			GameObject newPlatform;

			// We don't want to have compressed platform spaces
			if (newLocation.magnitude >= platformOffset) newPlatform = MakePlatform(newLocation);
			else newPlatform = MakePlatform(newLocation * 2);

			platformsInMap.Push(newPlatform);

			// Box for spawning any extras

			BoxCollider collider = newPlatform.GetComponent<BoxCollider>();

			// Add a potential hazard

			if (Random.value >= hazardRisk)
			{
				MakeHazard(RandomPointInBox(newPlatform.transform.position + new Vector3(0.0f, collider.size.y, 0.0f), collider.size));
			}

			// We will create gems at random points within the top of a platform

			// Yes or no for gem creation
			if (Random.value >= 0.5)
			{
				continue; // We will not create gems
			}

			AddGemDeposit(newPlatform.transform.position, collider);
		}

		// TODO: Make a new algorithm to distribute any excess platforms around the level.

		// Old generation algorithm
		// Now make platforms
		/*
		for (int i = 0; i < platformCount; i++) {

			// We need data from last platform

			GameObject lastPlatform = platformsInMap.Pop();

			// Now determine the position of the next one

			float xOffset = Random.Range(-platformOffset, platformOffset);

			float yOffset; // This can only be one of two values

			if (Random.value >= 0.5)
			{
				yOffset = platformOffset/2;
			}
			else
			{
				yOffset = -platformOffset/2;
			}

			// Double the offset of the second platform to make it easier to see our player when we start
			if (i == 0) yOffset = yOffset * 2;

			float zOffset = Random.Range(-platformOffset, platformOffset);

			Vector3 position = lastPlatform.transform.position + new Vector3(xOffset, yOffset, zOffset); 

			platformsInMap.Push(MakePlatform(position)); // Add the next platform

			// Box for spawning any extras

			BoxCollider collider = lastPlatform.GetComponent<BoxCollider>();

			// Add a potential hazard

			if (Random.value >= 0.5)
			{
				MakeHazard(RandomPointInBox(position + new Vector3(0.0f, collider.size.y, 0.0f), collider.size));
			}

			// We will create gems at random points within the top of a platform

			// Yes or no for gem creation
			if (Random.value >= 0.5)
			{
				continue; // We will not create gems
			}

			AddGemDeposit(position, collider);
		}
	*/
		//AddDeathBox();
	}

	// Use this for initialization
	void Start () {
		// Spawn the player at the origin
		Instantiate(player, startingPosition, Quaternion.identity);
	}
}
