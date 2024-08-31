using Editor;
using QuickSwitcher;

public static class QuickSwitcherMenu
{
    [Menu( "Editor", "Tools/Open Quick Switcher", "code" )]
    [Shortcut( "quick-switcher.toggle", "CTRL+K", ShortcutType.Widget )]
    public static void ToggleQuickSwitcher()
    {
        Log.Trace( "ToggleQuickSwitcher" );
        QuickSwitcherWindow.Instance?.Destroy();

        QuickSwitcherWindow.Instance = new QuickSwitcherWindow();
        QuickSwitcherWindow.Instance.Show();
    }
}