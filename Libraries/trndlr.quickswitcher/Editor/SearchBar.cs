using System;
using System.Collections.Generic;
using Editor;

namespace QuickSwitcher;

public class SearchBar : LineEdit
{
	private readonly List<KeyCode> BlurKeys = new()
	{
		KeyCode.Escape,
		KeyCode.Tab,
		KeyCode.Enter,
		KeyCode.Up,
		KeyCode.Down
	};

	public event Action SearchBlurred;

	protected override void OnKeyPress( KeyEvent e )
	{
		if ( BlurKeys.Contains( e.Key ) )
		{
			Blur();
		}
	}

	protected override void OnBlur( FocusChangeReason _ )
	{
		SearchBlurred?.Invoke();
	}
}
