using System.Collections.Generic;

namespace OpenGL {
    public static class Features {
        internal static HashSet<string> ExtensionsGPU = new HashSet<string>();
        
        public static bool IsExtensionSupported(string ExtensionName) => ExtensionsGPU.Contains(ExtensionName);
        
        public static bool GL_VERSION_1_0 {get; internal set;} = false;
        public static bool GL_VERSION_1_1 {get; internal set;} = false;
        public static bool GL_VERSION_1_2 {get; internal set;} = false;
        public static bool GL_VERSION_1_3 {get; internal set;} = false;
        public static bool GL_VERSION_1_4 {get; internal set;} = false;
        public static bool GL_VERSION_1_5 {get; internal set;} = false;
        public static bool GL_VERSION_2_0 {get; internal set;} = false;
        public static bool GL_VERSION_2_1 {get; internal set;} = false;
        public static bool GL_VERSION_3_0 {get; internal set;} = false;
        public static bool GL_VERSION_3_1 {get; internal set;} = false;
        public static bool GL_VERSION_3_2 {get; internal set;} = false;
        public static bool GL_VERSION_3_3 {get; internal set;} = false;
        public static bool GL_VERSION_4_0 {get; internal set;} = false;
        public static bool GL_VERSION_4_1 {get; internal set;} = false;
        public static bool GL_VERSION_4_2 {get; internal set;} = false;
        public static bool GL_VERSION_4_3 {get; internal set;} = false;
        public static bool GL_VERSION_4_4 {get; internal set;} = false;
        public static bool GL_VERSION_4_5 {get; internal set;} = false;
        public static bool GL_VERSION_4_6 {get; internal set;} = false;
        public static bool GL_VERSION_ES_CM_1_0 {get; internal set;} = false;
        public static bool GL_ES_VERSION_2_0 {get; internal set;} = false;
        public static bool GL_ES_VERSION_3_0 {get; internal set;} = false;
        public static bool GL_ES_VERSION_3_1 {get; internal set;} = false;
        public static bool GL_ES_VERSION_3_2 {get; internal set;} = false;
        public static bool GL_SC_VERSION_2_0 {get; internal set;} = false;
    }
}
