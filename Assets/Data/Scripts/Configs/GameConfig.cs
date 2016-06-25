
namespace EternalMaze {
	public class GameConfig {

		public bool Debug { get; private set; }

		/// <summary>
		/// размер динамического окна по X
		/// </summary>
		public int RepaintXSize { get; private set; }

		/// <summary>
		/// размер динамического окна по Y
		/// </summary>
		public int RepaintYSize { get; private set; }

		public float UnitYShift { get; private set; }

		public float BonusesYShift { get; private set; }

		public float FloatHorizontalYShift { get; private set; }

		public float FloatVerticalYShift { get; private set; }

		public float HoleYShift { get; private set; }

		public float TabletYShift { get; private set; }

		/// <summary>
		/// задержка, через которую появляется 1 буква в диалоговом окне (настраиваемо)
		/// </summary>
		public float TextLetterAppearDelay { get; private set; }

		/// <summary>
		/// время появления диалогового окна
		/// </summary>
		public float DialogTimeAppear { get; private set; }

		public string Name { get; private set; }

		public int ExtraScoreMultiplyer { get; private set; }

		public int TreasureCoinsCount { get; private set; }

		public int UndyingCoinsCount { get; private set; }

		public int BagWithCoinsCount { get; private set; }


	}
}

