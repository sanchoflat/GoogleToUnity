using System.Text;
using UnityEngine;


namespace GoogleSheetIntergation {
    public class ConstantsClassBuilder {
      
        public void GenerateConstantClass() {
            G2UConfig.Instance.PathManager.CreateConstantClassFolder();
            var file = new StringBuilder();
            file.AppendLine(string.Format("internal class {0} {{",
                G2UConfig.Instance.ConstantClassName));
//            file.AppendLine(string.Format("{0}public const string path = \"\"", FileBuilder.GetTabulator(1)));
            file.Append("}");
//            SaveLoadManager.SaveFile(G2UConfig.Instance.ParameterClassFullName, file.ToString());
//            Debug.Log(string.Format("Param class <b>{0}</b> was successful generated",
//                G2UConfig.Instance.ConstantClassName));
        }

        public void UpdateDataLocation(string path) {}
    }
}