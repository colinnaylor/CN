﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SwiftImporterUI.Properties {
    
    
    [CompilerGenerated()]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("08:30:00")]
        public TimeSpan ScheduledRunTime {
            get {
                return ((TimeSpan)(this["ScheduledRunTime"]));
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("\\\\swiftprod\\SWIFT_FILES\\")]
        public string SwiftFilesPath {
            get {
                return ((string)(this["SwiftFilesPath"]));
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("Reconciliation")]
        public string DSN_LIVE {
            get {
                return ((string)(this["DSN_LIVE"]));
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("Reconciliation_Test")]
        public string DSN_TEST {
            get {
                return ((string)(this["DSN_TEST"]));
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [SpecialSetting(SpecialSetting.ConnectionString)]
        [DefaultSettingValue("Data Source=MINKY;Initial Catalog=Reconciliation;Integrated Security=True")]
        public string ReconciliationConnectionString1 {
            get {
                return ((string)(this["ReconciliationConnectionString1"]));
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [SpecialSetting(SpecialSetting.ConnectionString)]
        [DefaultSettingValue("Data Source=uatMinky;Initial Catalog=reconciliation;Integrated Security=True")]
        public string reconciliationConnectionString {
            get {
                return ((string)(this["reconciliationConnectionString"]));
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("Reconciliation_Test_UAT")]
        public string DSN_TEST_UAT {
            get {
                return ((string)(this["DSN_TEST_UAT"]));
            }
        }
        
        [ApplicationScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("C:\\Temp\\SWIFT_FILES\\")]
        public string SwiftFilesPath_TEST {
            get {
                return ((string)(this["SwiftFilesPath_TEST"]));
            }
        }
    }
}