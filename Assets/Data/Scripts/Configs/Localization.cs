using System;
using UnityEngine;
namespace EternalMaze.Configs {
	public class Localization {

		public string[] arrayTest { get; private set; }

		public string StartNewGame { get; private set; }

		public string Options { get; private set; }

		public string ReferenceBook { get; private set; }

		public string Score { get; private set; }

		public string Time { get; private set; }

		public string GameScore { get; private set; }

		public string ExtraScore { get; private set; }

		public string Undying { get; private set; }

		public string Treasure { get; private set; }

		public string TotalScore { get; private set; }

		public string BestScore { get; private set; }

		public string BestTime { get; private set; }

		public string ReferenceItems { get; private set; }

		public string ReferenceEnemies { get; private set; }

		public string ReferenceObjects { get; private set; }

		public string ReferenceAchieve { get; private set; }

		public string ReferenceStats { get; private set; }

		public string Tractor { get; private set; }

		public string Mole { get; private set; }

		public string Camp { get; private set; }

		public string DogMutation { get; private set; }

		public string ReadTheBoard { get; private set; }

		public string TakeCoinsBag { get; private set; }

		public string MakeStepsWithAlcohol { get; private set; }

		public string CompleteLevelWithoutDeath { get; private set; }

		public string FindTreasure { get; private set; }

		public string UseTubeWithPoison { get; private set; }

		public string Lure3EnemiesToRopeWithBanks { get; private set; }

		public string content_speedDown { get; private set; }

		public string content_speedUp { get; private set; }

		public string content_alco { get; private set; }

		public string content_poison { get; private set; }

		public string content_coins { get; private set; }

		public string content_battery { get; private set; }

		public string content_health { get; private set; }

		public string pnl_dogX { get; private set; }

		public string pnl_dog { get; private set; }

		public string pnl_mapTreasure { get; private set; }

		public string pnl_tube { get; private set; }

		public string pnl_hole { get; private set; }

		public string pnl_grassCutter { get; private set; }

		public string pnl_banki { get; private set; }

		public string pnl_float { get; private set; }

		public string pnl_teleportStone { get; private set; }

		public string pnl_table { get; private set; }

		public string pnl_blind { get; private set; }

		public string pnl_safeZone { get; private set; }

		public string pnl_leaves { get; private set; }

		public string[] Tips { get; private set; }

		public string[] Intro { get; private set; }

		public Localization Load(string fileName) {
			var dataType = Resources.Load("Resources\\Configs\\" + fileName) as TextAsset;
			var d = new Localization();
			if(dataType == null || string.IsNullOrEmpty(dataType.text)) return null;
			d = d.LoadJSONFromString(dataType.text);
			return d;
		}

	}
}
