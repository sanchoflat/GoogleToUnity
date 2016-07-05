using UnityEngine;


namespace Assets.GoogleToUnityIntegration.Scripts {
    public class ColorManager {

        public static void Reset() {
            _colorCounter = 0;
        }

        private static int _colorCounter;

        public static Color GetColor() {
            _colorCounter++;
            if(_colorCounter % 2 == 0) {
                return new Color(.7f, 0, 0, 0.3f);
            }
            return new Color(0, .7f, 0, 0.3f);
        }
    }
}