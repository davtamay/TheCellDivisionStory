namespace TriLibEditor
{
    public static class TriLibSettings 
    {
        public static bool DisableNativePluginsChecking
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_DISABLE_PLUGINS_CHECK"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_DISABLE_PLUGINS_CHECK", value); }
        }

        public static bool DisableOldVersionsChecking
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_DISABLE_OLD_VER_CHECK"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_DISABLE_OLD_VER_CHECK", value); }
        }

        public static bool DisableEditorAutomaticImporting
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_DISABLE_AUTO_IMPORT"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_DISABLE_AUTO_IMPORT", value); }
        }

        public static bool EnableZipLoading
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_USE_ZIP"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_USE_ZIP", value); }
        }

        public static bool EnableOutputMessages
        {
            get { return TriLibDefineSymbolsHelper.IsSymbolDefined("TRILIB_OUTPUT_MESSAGES"); }
            set { TriLibDefineSymbolsHelper.UpdateSymbol("TRILIB_OUTPUT_MESSAGES", value); }
        }
    }
}