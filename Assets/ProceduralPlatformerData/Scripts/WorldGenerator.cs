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

	public float minPlatformOffset = 2.0f;

	public float maxPlatformOffset = 6.0f;

	public float hazardRisk = 0.1f;

	public float gemOdds = 0.5f;

	public float verticalOffsetOdds = 0.5f;

	public float inversionOdds = 0.5f;

	public ParticleSystem spawnEffect;

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

	GameObject MakePlatform(Vector3 position, bool trySpawningFeatures)
	{
		int platformIndex = Random.Range(0, platforms.Length); // We will use this to spawn a base platform

		GameObject platform = Instantiate(platforms[platformIndex]);
		platform.transform.position = position;

		if (trySpawningFeatures)
		{
			// Box for spawning any extras

			BoxCollider collider = platform.GetComponent<BoxCollider>();

			// Add a potential hazard - but only if the platform is large enough.

			if (Random.value <= hazardRisk && (collider.size.x / 2 > minPlatformOffset || collider.size.z / 2 > minPlatformOffset))
			{
				MakeHazard(RandomPointInBox(platform.transform.position + new Vector3(0.0f, collider.size.y, 0.0f), collider.size));
			}

			// We will create gems at random points within the top of a platform

			// Yes or no for gem creation
			if (Random.value <= gemOdds)
			{
				AddGemDeposit(platform.transform.position, collider);
			}
		}

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

	void AddDeathBox(Vector3 position)
	{

		// We need a way to kill the player on a fallout

		GameObject deathBox = new GameObject();
		deathBox.transform.position = position;
		deathBox.name = "DeathBox";
		deathBox.AddComponent<BoxCollider>();
		deathBox.GetComponent<BoxCollider>().isTrigger = true; // Otherwise it won't be able to kill us!

		float deathBoxSide = Mathf.Pow(maxPlatforms * maxPlatformOffset, 2); // Cannot possibly fall beyond this size!
		deathBox.GetComponent<BoxCollider>().size = new Vector3(deathBoxSide, 10.0f, deathBoxSide);

		// Add script to kill us
		deathBox.AddComponent<KillPlayer>();
	}

	float RandomPlatformSpacing()
	{
		return Random.Range(minPlatformOffset, maxPlatformOffset);
	}

	float RandomInvert(float value)
	{
		if (Random.value >= inversionOdds) return -value;
		else return value;
	}

	void MakeWorld()
	{
		// We generate our world before doing anything else

		Stack<GameObject> platformsInMap = new Stack<GameObject>(); // We will need to track all of our platforms here

		Stack<GameObject> platformsBeingAdded = new Stack<GameObject>(); // Secondary tracking stack

		// Create our first platform and add it to the stack, don't add any additional features

		GameObject startingPlatform = MakePlatform(startingPosition, false);

		// If our starting platform is not the right size, try again

		if (startingPlatform.GetComponent<BoxCollider>().size.x < maxPlatformOffset || startingPlatform.GetComponent<BoxCollider>().size.z < maxPlatformOffset)
		{
			MakeWorld();
			return; // Because we don't want to make multiple layouts and deathboxes!
		}

		platformsInMap.Push(startingPlatform);
		platformsBeingAdded.Push(startingPlatform);
		// Determine how many platforms to make, accounting for our starting platform

		int platformsRemaining = Random.Range(minPlatforms, maxPlatforms) - 1;

		for (int i = 0; i < platformsRemaining; i++)
		{
			// We need data from the last one to be added

			GameObject lastPlatform = platformsBeingAdded.Pop();

			// We have to offset from the last platform in at least one horizontal direction, 

			float xOffset = RandomInvert(RandomPlatformSpacing());
			float zOffset = RandomInvert(RandomPlatformSpacing());

			// We will only do a horizontal offset some of the time

			float yOffset;

			if (Random.value >= verticalOffsetOdds) yOffset = RandomInvert(RandomPlatformSpacing());
			else yOffset = 0.0f;

			// Offset from the boundaries of the last platform's bounding box instead of its origin.

			Vector3 newPosition = lastPlatform.transform.position + lastPlatform.GetComponent<BoxCollider>().size + new Vector3(xOffset, yOffset, zOffset);

			GameObject newPlatform = MakePlatform(newPosition, true); // Also try to spawn extras

			platformsInMap.Push(newPlatform);
			platformsBeingAdded.Push(newPlatform);

			// Spawn the goal on the last platform

			if (i + 1 == platformsRemaining)
			{
				GameObject endGoal = Instantiate(goal, newPlatform.transform.position + new Vector3(0.0f, 2.0f, 0.0f), newPlatform.transform.rotation);
			}
		}

		// Find the platform at the lowest level

		float lowestPosition = 0.0f;

		foreach (GameObject platform in platformsInMap)
		{
			// We are looking for the lowest position, and it must always be a negative number
			if (platform.transform.position.y < lowestPosition) lowestPosition = -Mathf.Abs(platform.transform.position.y);

			// Now rearrange all platforms so that they
		}

		AddDeathBox(new Vector3(0.0f, lowestPosition - 20.0f, 0.0f));
	}

	// Lifecycle

	void Awake () {

		MakeWorld();
	}

	// Use this for initialization
	void Start () {
		// Spawn effect
		if (spawnEffect) Instantiate(spawnEffect, startingPosition, Quaternion.identity);
		// Spawn the player at the origin
		Instantiate(player, startingPosition, Quaternion.identity);
	}
}
