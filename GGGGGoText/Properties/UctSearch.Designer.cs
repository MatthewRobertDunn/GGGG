//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GGGGGoText.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class UctSearch : global::System.Configuration.ApplicationSettingsBase {
        
        private static UctSearch defaultInstance = ((UctSearch)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new UctSearch())));
        
        public static UctSearch Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:00:12")]
        public global::System.TimeSpan TimeToThink {
            get {
                return ((global::System.TimeSpan)(this["TimeToThink"]));
            }
        }
    }
}
