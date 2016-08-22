using System;
using System.Collections.Generic;
using System.Text;


namespace ClassGenerator {
    public enum Access {
        @public,
        @private,
        @protected
    }

    public enum ClassModifier {
        @static,
        @abstract,
        @sealed
    }

    public class AbstractGenerator {
        public Access Access { get; protected set; }
        public string Name { get; protected set; }
    }

    public class ClassGenerator : AbstractGenerator {
        public string Namespace { get; private set; }
        public ClassModifier ClassModifier { get; private set; }
        public string[] Extends { get; private set; }
        private List<MethodGenerator> _methods = new List<MethodGenerator>();
 
        public ClassGenerator(string className, string ns, params string[] extends) {
            Name = className;
            Namespace = ns;
            Extends = extends;
        }

        public ClassGenerator(string className, string ns, Access access, params string[] extends) {
            Name = className;
            Namespace = ns;
            Access = access;
            Extends = extends;
        }

        public ClassGenerator(string className, string ns, Access access, ClassModifier classModifier,
            params string[] extends) {
            Name = className;
            Namespace = ns;
            Access = access;
            ClassModifier = classModifier;
            Extends = extends;
        }

        public ClassGenerator(string className, string ns, ClassModifier classModifier, params string[] extends) {
            Name = className;
            Namespace = ns;
            ClassModifier = classModifier;
            Extends = extends;
        }

        public string GenerateEmptyClass() {
            return "";
        }

        public void AddMethod(MethodGenerator mg) {
           _methods.Add(mg);
        }

        public static string GetUsing(params string[] includes) {
            var sb = new StringBuilder();
            for(var i = 0; i < includes.Length; i++) {
                sb.Append(string.Format("using {0};\n", includes[i]));
            }
            return sb.ToString();
        }

        public static string GetClassStart(Access access, ClassModifier classModifier, string className,
            string namespaceName, string extendClass, params string[] extendInterfaces) {
            var sb = new StringBuilder();
            sb.Append(string.Format("namespace {0} {{\n", @namespaceName));
            sb.Append(string.Format("{0} {1} class {2} : {3}, {4} {{\n", access.ToString().Replace("@", ""),
                classModifier.ToString().Replace("@", ""), className, extendClass, Concat(extendInterfaces, ", ")));
            return sb.ToString();
        }

        public static string GetEndClassBraces(bool withNamespace) {
            var sb = new StringBuilder();
            sb.AppendLine(GetTabulator(1) + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string Concat(string[] data, string separator) {
            var sb = new StringBuilder();
            for(var i = 0;
                i < data.Length
                ;
                i++) {
                sb.Append(data[i] + separator);
            }
            return sb.ToString();
        }

        public static string GetTabulator(byte count) {
            var sb = new StringBuilder();
            for(var i = 0; i < count; i++) {
                sb.Append("\t");
            }
            return sb.ToString();
        }
    }

    public class MethodGenerator : AbstractGenerator {
        public Dictionary<Type, string> InputParams { get; private set; }
        public Type OutputType { get; private set; }

        public bool @static { get; private set; }

        public MethodGenerator(string methodName, Access access) {
            Name = methodName;
            Access = access;
        }

        public MethodGenerator(string methodName, Dictionary<Type, string> inputParams, Access access) {
            Name = methodName;
            InputParams = inputParams;
            Access = access;
        }

        public MethodGenerator(Type outputType, string methodName, Access access) {
            OutputType = outputType;
            Name = methodName;
            Access = access;
        }

        public MethodGenerator(string methodName, Dictionary<Type, string> inputParams, Type outputType, Access access) {
            Name = methodName;
            InputParams = inputParams;
            OutputType = outputType;
            Access = access;
        }

        public string CreateMethod() {
            return string.Format("{0} {1} {2} ({3}) {{}}", Access, OutputType, Name, GetInputTypes(InputParams));
        }

        public string CreateEmptyMethod() {
            return string.Format("{0} {1} {2} ({3}) {{}}", Access, OutputType, Name, GetInputTypes(InputParams));
        }

        private string GetInputTypes(Dictionary<Type, string> input) {
            var sb = new StringBuilder();
            foreach(var d in input) {
                sb.Append(string.Format("{0} {1}, ", d.Key, d.Value));
            }
            return sb.ToString();
        }
    }
}