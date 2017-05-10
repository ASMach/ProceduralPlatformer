using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Released under Creative Commons License by Alonzo Machiraju, 2017

namespace HUD
{
	[RequireComponent(typeof (Text))]
	public class ScoreCounter : MonoBehaviour {
		
		const string display = "Score: {0}";
		private Text text;


		private void Start()
		{
			text = GetComponent<Text>();
		}

		public void UpdateScoreDisplay(int score)
		{
			text.text = string.Format(display, score);
		}
	}
}
