using System;
using UnityEngine;
namespace EternalMaze.Configs {
	public class GameConfig {

		public int d_LevelToLoad { get; private set; }

		public int d_PartToLoad { get; private set; }

		public int d_ChapterToLoad { get; private set; }

		public bool d_Debug { get; private set; }

		public bool d_KanobuVersion { get; private set; }

		public bool ShowAllLevelsInMenu { get; private set; }

		public bool CreateDefaultSavedData { get; private set; }

		/// <summary>
		/// количество ячеек, которое будет отрисовываться при постройке уровня
		/// </summary>
		public int CreateCellsCount { get; private set; }

		/// <summary>
		/// время, через которое стартует игра и можно управлять игроком
		/// </summary>
		public float GameStartDelay { get; private set; }

		/// <summary>
		/// отдаление камеры
		/// </summary>
		public float CameraSize { get; private set; }

		/// <summary>
		/// время лерпа декораторов (ускорениезамедление)
		/// </summary>
		public float LerpTime { get; private set; }

		/// <summary>
		/// расстояние, на которое поднимаются использованные бонусы
		/// </summary>
		public float BonusYShift { get; private set; }

		/// <summary>
		/// время, за которое эти бонусы поднимаются
		/// </summary>
		public float BonusYShiftTime { get; private set; }

		/// <summary>
		/// время задержки перед рестартом после смерти
		/// </summary>
		public int RestartAfterDeathDelay { get; private set; }

		/// <summary>
		/// скорость проползания через трубу
		/// </summary>
		public int TubeCrawlSpeed { get; private set; }

		/// <summary>
		/// задержка перед выполнением ИИ собак (1/10 c)
		/// </summary>
		public float CoroutineReloadTime { get; private set; }

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

		/// <summary>
		/// множитель очков
		/// </summary>
		public int ExtraScoreMultiplyer { get; private set; }

		public int TreasureCoinsCount { get; private set; }

		public int UndyingCoinsCount { get; private set; }

		public int BagWithCoinsCount { get; private set; }

		public int NegativEffectCoinsCount { get; private set; }

		public int StaminaSpendCoinsCount { get; private set; }

		public int BatterySpendCoinsCount { get; private set; }

		public int GrasscutterCoinsCount { get; private set; }

		public int FloatCoinsCount { get; private set; }

		public int TubeCoinsCount { get; private set; }

		public int HoleCoinsCount { get; private set; }

		public int LeavesCoinsCount { get; private set; }

		/// <summary>
		/// время задержки между концом игры и появлением меню финиша
		/// </summary>
		public float AppearFinishMenuDelay { get; private set; }

		/// <summary>
		/// время задержки перед включением счётчика очков в finishScreen
		/// </summary>
		public float ScoreCounterAppearDelay { get; private set; }

		/// <summary>
		/// время, за которое очки набираются от 0 до n
		/// </summary>
		public int ScoreUpdateTime { get; private set; }

		/// <summary>
		/// Скорость затемнения чёрного экрана
		/// </summary>
		public int BlackScreenFadeDuration { get; private set; }

		/// <summary>
		/// длительность мигания игрока на карте
		/// </summary>
		public int PlayerBlinkDuration { get; private set; }

		/// <summary>
		/// длительность быстрого мигания игрока на карте (dblClick)
		/// </summary>
		public float PlayerFastBlinkDuration { get; private set; }

		/// <summary>
		/// количество миганий игрока при dblClick
		/// </summary>
		public int PlayerFastBlinkCount { get; private set; }

		/// <summary>
		/// максимальный оступ карты от края
		/// </summary>
		public int MaxIndent { get; private set; }

		/// <summary>
		/// максимальное видимое количество тайлов карты
		/// </summary>
		public int MinScaleTiles { get; private set; }

		/// <summary>
		/// минимальное видимое количество тайлов карты
		/// </summary>
		public int MaxScaleTiles { get; private set; }

		/// <summary>
		/// если расстояние между положением игрока относительно центра экрана < MinDistanceToMove, то переместить карту моментально
		/// </summary>
		public int MinDistanceToMove { get; private set; }

		/// <summary>
		/// время центрирования карты по dblClick
		/// </summary>
		public float MapMovementDuration { get; private set; }

		/// <summary>
		/// время появление / исчезновения карты
		/// </summary>
		public float MapFadeDuration { get; private set; }

		public float UnPauseMapDelay { get; private set; }

		/// <summary>
		/// максимальная задержка для doubleClick
		/// </summary>
		public float DelayDoubleClick { get; private set; }

		/// <summary>
		/// чувствительность пермещения для тача
		/// </summary>
		public float MoveSensitivity { get; private set; }

		/// <summary>
		/// чувствительность скейла для тача
		/// </summary>
		public float ScaleSensitivitySensor { get; private set; }

		/// <summary>
		/// шаг скейла карты на PC (колёсиком)
		/// </summary>
		public float ScaleStepPC { get; private set; }

		/// <summary>
		/// количество секунд, за которое громкость фоновой музыки нарастает от 0 до 1.  Если конечная громкость 0.5, то время будет поделено пополам
		/// </summary>
		public int BgMusicStartDelay { get; private set; }

		/// <summary>
		/// количество секунд, за которое громкость фоновой музыки нарастает от 0 до 1. 
		/// </summary>
		public int[] MenuBgMusicStartDelay { get; private set; }

		public GameConfig Load(string fileName) {
			var dataType = Resources.Load("Resources\\Configs\\" + fileName) as TextAsset;
			var d = new GameConfig();
			if(dataType == null || string.IsNullOrEmpty(dataType.text)) return null;
			d = d.LoadJSONFromString(dataType.text);
			return d;
		}

	}
}
