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

			// Add a potential hazard

			if (Random.value <= hazardRisk)
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

	int RandomInsidePlatformCountBounds()
	{
		return Random.Range(-maxPlatforms, maxPlatforms);
	}

	int RandomPlatformArrayIndex()
	{
		return Random.Range(0, maxPlatforms - 1);
	}

	float RandomPlatformSpacing()
	{
		return Random.Range(minPlatformOffset, maxPlatformOffset);
	}

	// Lifecycle

	void Awake () {

		// We generate our world before doing anything else

		Stack<GameObject> platformsInMap = new Stack<GameObject>(); // We will need to track all of our platforms here

		Stack<GameObject> platformsBeingAdded = new Stack<GameObject>(); // Secondary tracking stack

		platformsBeingAdded.Push(MakePlatform(startingPosition, false)); // Create our first platform and add it to the stack, don't add any additional features

		Vector3 endingPosition = new Vector3(RandomInsidePlatformCountBounds(), RandomInsidePlatformCountBounds(), RandomInsidePlatformCountBounds());

		GameObject endPlatform = MakePlatform(endingPosition, false); // We don't want any additional things on the final platform either

		// Now create the end, with the goal located at it.

		platformsBeingAdded.Push(endPlatform);

		GameObject endGoal = Instantiate(goal, endPlatform.transform.position + new Vector3(0.0f, 2.0f, 0.0f), endPlatform.transform.rotation);
		endGoal.transform.parent = endPlatform.transform; // We need to be able to move both of them at the same time if a viable solution is not possible.

		// Create the rest of the platforms at random

		float distanceToGoal = Vector3.Distance(startingPosition, endingPosition); // Start by getting distance to goal

		Debug.Log("We are " + distanceToGoal + " units from the goal");
		Debug.Log("X Offset: " + (startingPosition.x - endingPosition.x));
		Debug.Log("Y Offset: " + (startingPosition.y - endingPosition.y));
		Debug.Log("Z Offset: " + (startingPosition.z - endingPosition.z));

		// Use a 3d array to store positions of new platforms
		Vector3[,,] platformPoints = new Vector3[maxPlatforms, maxPlatforms, maxPlatforms];

		// The actual position vector is not a valid index
		Vector3 startingPositionIndexInMatrix = new Vector3(RandomPlatformArrayIndex(), RandomPlatformArrayIndex(), RandomPlatformArrayIndex());

		platformPoints[(int)startingPositionIndexInMatrix.x, (int)startingPositionIndexInMatrix.y, (int)startingPositionIndexInMatrix.z] = startingPosition;

		// See how many platforms we need until the end

		int directPathPlatforms = (int)(distanceToGoal / maxPlatformOffset);

		// Generate an index within the location matrix for the endpoint

		Vector3 endingPositionIndexInMatrix = new Vector3(Mathf.Clamp(startingPositionIndexInMatrix.x + directPathPlatforms, 0, maxPlatforms -1), Mathf.Clamp(startingPositionIndexInMatrix.y + directPathPlatforms, 0, maxPlatforms -1), Mathf.Clamp(startingPositionIndexInMatrix.z + directPathPlatforms, 0, maxPlatforms -1));

		// Now store the ending position

		platformPoints[(int)endingPositionIndexInMatrix.x, (int)endingPositionIndexInMatrix.y, (int)endingPositionIndexInMatrix.z] = endingPosition;

		// Populate the matrix

		int xStart, yStart, zStart, xEnd, yEnd, zEnd; // These iterators and end indices will have to be set based on conditions

		float xPosSeed, yPosSeed, zPosSeed; // We need these to avoid calculating them at every step of the for loop

		if (endingPositionIndexInMatrix.x > startingPositionIndexInMatrix.x)
		{
			xStart = (int)startingPositionIndexInMatrix.x;
			xEnd = (int)endingPositionIndexInMatrix.x;
			xPosSeed = startingPosition.x - endingPosition.x;
		}
		else
		{
			xStart = (int)endingPositionIndexInMatrix.x;
			xEnd = (int)startingPositionIndexInMatrix.x;
			xPosSeed = endingPosition.x - startingPosition.x;
		}
		if (endingPositionIndexInMatrix.y > startingPositionIndexInMatrix.y)
		{
			yStart = (int)startingPositionIndexInMatrix.y;
			yEnd = (int)endingPositionIndexInMatrix.y;
			yPosSeed = startingPosition.y - endingPosition.y;
		}
		else
		{
			yStart = (int)endingPositionIndexInMatrix.y;
			yEnd = (int)startingPositionIndexInMatrix.y;
			yPosSeed = endingPosition.y - startingPosition.y;

		}
		if (endingPositionIndexInMatrix.z > startingPositionIndexInMatrix.z)
		{
			zStart = (int)startingPositionIndexInMatrix.z;
			zEnd = (int)endingPositionIndexInMatrix.z;
			zPosSeed = startingPosition.z - endingPosition.z;
		}
		else
		{
			zStart = (int)endingPositionIndexInMatrix.z;
			zEnd = (int)startingPositionIndexInMatrix.z;
			zPosSeed = endingPosition.z - startingPosition.z;
		}

		// Get the number of platforms to use
		int platformCount;

		if (directPathPlatforms < minPlatforms) platformCount = Random.Range(minPlatforms, maxPlatforms);
		else platformCount = Random.Range(directPathPlatforms, maxPlatforms);
			
		for (int i = xStart; i < xEnd; i++)
		{
			for (int j = yStart; j < yEnd; j++)
			{
				for (int k = zStart; k < zEnd; k++)
				{
					if (platformCount <= 0) break; // Bail if we have run out of platforms to add.

					// We generate a cadidate location for the next platform

					Vector3 newPosition = new Vector3(xPosSeed / i , yPosSeed / j , xPosSeed / k);

					// Try to get the last one we added

					GameObject previousPlatform = null;

					// Only get the last platform if it exists
					if (platformsBeingAdded.Count > 0) previousPlatform = platformsBeingAdded.Pop();

					if (previousPlatform != null)
					{
						platformsInMap.Push(previousPlatform); // We need to track what we have previously added.

						// Move our new platform if its location is not far enough from the last one
						if (Mathf.Abs(newPosition.magnitude - previousPlatform.transform.position.magnitude) < minPlatformOffset)
						{
							// Randomize the horizontal axis to move along
							switch (Random.Range(1, 2))
							{
								case 1: // Move along x axis
									// Randomize sign
									if (Random.value >= 0.5) newPosition[0] = newPosition[0] + RandomPlatformSpacing();
									else newPosition[0] = newPosition[0] - RandomPlatformSpacing();
									break;
								case 2: // Move along z axis
									// Randomize sign
									if (Random.value >= 0.5) newPosition[2] = newPosition[2] + RandomPlatformSpacing();
									else newPosition[2] = newPosition[2] - RandomPlatformSpacing();
									break;
							}
						}

						// Randomize y position at random

						if (Random.value >= 0.5)
						{
							// Randomize sign
							if (Random.value >= 0.5) newPosition[1] = newPosition[1] + RandomPlatformSpacing();
							else newPosition[1] = newPosition[1] - RandomPlatformSpacing();
						}
					}

					Debug.Log("Platform added at (" + newPosition.x + ", " + newPosition.y + "," + newPosition.z + ")");
					// Only make a new platform if we need one
					GameObject newPlatform = MakePlatform(newPosition, true);

					platformsBeingAdded.Push(newPlatform);
					platformCount--;
				}
			}
		}

		// Old Algorithm
		/*
		// See how many platforms we need until the end

		int directPathPlatforms = (int)(distanceToGoal / minPlatformOffset);

		// Get the number of platforms to use
		int platformCount;

		if (directPathPlatforms < minPlatforms) platformCount = Random.Range(minPlatforms, maxPlatforms);
		else platformCount = Random.Range(directPathPlatforms, maxPlatforms);

		// Lay out basic path
		for (int i = 0; i < directPathPlatforms; i++)
		{
			GameObject previousPlatform = platformsBeingAdded.Pop();

			platformsInMap.Push(previousPlatform); // We need to track what we have previously added.

			Vector3 newPosition = (startingPosition - previousPlatform.transform.position) / 2;

			// We don't want to have compressed platform spaces, so we shift platforms that are too close to the previous one
			if (newPosition.magnitude >= minPlatformOffset)
			{
				float decompressionOffset = minPlatformOffset;
				//
				if (Random.value >= 0.5) decompressionOffset = -decompressionOffset;
					
				// Actually offset horizontally on either x or z axis, y being vertical in Unity, as conterintuitive as it is
				if (Random.value >= 0.5) newPosition[0] += decompressionOffset;
				if (Random.value >= 0.5) newPosition[2] += decompressionOffset;
			}

			GameObject newPlatform = MakePlatform(newPosition, true); // Add a platform and possibly add additional features to it

			// Add to our linked inst, maintaining a sequential order of platforms
			platformsBeingAdded.Push(newPlatform);
		}

		// We have already added some platforms, so we will calculate and store the remaining ones
		platformCount -= directPathPlatforms;

		// Insert any additional platforms needed for successful player traversal and/or play value, but only if we need to do so.

		if (platformCount > 0)
		{
			for (int i = 0; i < platformCount; i++)
			{
				// Make a new platform in a random location and potentially spawn extras on it
				MakePlatform(new Vector3(Random.Range(-maxPlatforms, maxPlatforms), Random.Range(-maxPlatforms, maxPlatforms), Random.Range(-maxPlatforms, maxPlatforms)), true);
				platformCount--; // Because we need this for our remaining
			}
		}
		*/

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

	// Use this for initialization
	void Start () {
		// Spawn the player at the origin
		Instantiate(player, startingPosition, Quaternion.identity);
	}
}
