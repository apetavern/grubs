using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor;
using Sandbox;
using Sandbox.Diagnostics;

namespace QuickSwitcher;

public class QuickSwitcherWindow : PopupWidget
{
	public QuickSwitcherWindow( Widget parent = null ) : base( parent )
	{
		Instance = this;

		Layout = Layout.Column();
		Size = new Vector2( 500, 400 );

		Layout.Margin = 8;
		Layout.Spacing = 4;

		SearchBar = Layout.Add( new SearchBar(), 1 );
		SearchBar.PlaceholderText = "Search...";
		SearchBar.FocusMode = FocusMode.Click;
		SearchBar.FixedHeight = 32;
		SearchBar.Focus();

		SearchBar.ReturnPressed += () => SearchBar.Blur();
		SearchBar.SearchBlurred += SearchBarOnSearchBlurred;
		SearchBar.TextChanged += _ => SetListItems();

		Bind( nameof( Filter ) ).ReadOnly().From( SearchBar, x => x.Text );

		ToolBar = Layout.Add( new SegmentedControl( this ) );
		ToolBar.AddOption( "All", "list" );
		ToolBar.AddOption( "Asset", "description" );
		ToolBar.AddOption( "Action", "directions_run" );
		ToolBar.OnSelectedChanged += OnToolBarOptionChanged;

		Layout.Add( BuildOptions(), 1 );
	}

	private static Logger Log { get; } = new( "QuickSwitcher" );

	public string Filter { get; set; }
	private OptionType FilterType { get; set; } = OptionType.All;
	private Option Selected { get; set; }
	private int SelectedIndex { get; }

	private SearchBar SearchBar { get; }
	private ListView List { get; set; }
	private SegmentedControl ToolBar { get; }

	public static QuickSwitcherWindow Instance { get; set; }

	private void SearchBarOnSearchBlurred()
	{
		List.Focus();
		if ( !List.Items.Any() ) return;
		var selection = new SelectionSystem();
		selection.Add( List.Items.First() );
		List.Selection = selection;
	}

	private void SetListItems()
	{
		List<Option> options = new();

		Log.Trace( $"Setting list items with filter type: {FilterType}" );

		//
		// Assets
		//
		if ( FilterType is OptionType.Asset or OptionType.All )
		{
			var assets = AssetSystem.All.OrderBy( x => x.Name ).ToList();
			options.AddRange( assets.Select( x =>
				new AssetOption( OptionType.Asset, x.Name, $"{x.AssetType.FriendlyName} Asset", x.AssetType.Icon64,
					x ) ) );
		}

		//
		// Actions
		//
		if ( FilterType is OptionType.Action or OptionType.All )
		{
			options.AddRange( ActionOption.All() );
		}

		if ( !string.IsNullOrEmpty( Filter ) )
			options = options.Where( x => x.Name.Contains( Filter, StringComparison.OrdinalIgnoreCase ) ).ToList();

		options = options.OrderByDescending( x => x.Type ).ToList();

		List.SetItems( options );
	}

	private Widget BuildOptions()
	{
		var canvas = new Widget( null );
		canvas.Layout = Layout.Row();

		List = new ListView( canvas );
		SetListItems();
		List.Margin = 0;
		List.ItemSize = new Vector2( 0, 24 );
		List.ItemPaint = PaintListMode;
		List.ItemSpacing = 0;
		List.ItemClicked = o =>
		{
			if ( o is Option opt )
			{
				HandleOption( opt );
			}
		};
		List.ItemActivated = o =>
		{
			if ( o is Option opt )
			{
				HandleOption( opt );
			}
		};

		canvas.Layout.Add( List );

		return canvas;
	}

	private void PaintListMode( VirtualWidget item )
	{
		if ( item.Object is not Option option )
			return;

		var itemRect = item.Rect;

		if ( Paint.HasSelected || Paint.HasPressed )
		{
			Selected = option;

			Paint.SetPen( Paint.HasPressed ? Theme.Green : Theme.Primary, 2, PenStyle.Dash );
			Paint.ClearBrush();
			Paint.DrawRect( itemRect.Shrink( 1 ), 3 );

			Paint.ClearPen();
			Paint.SetBrush( Paint.HasPressed ? Theme.Green.WithAlpha( 0.4f ) : Theme.Primary.WithAlpha( 0.4f ) );
			Paint.DrawRect( itemRect.Shrink( 0 ), 3 );

			Paint.SetPen( Theme.White );
		}
		else if ( Paint.HasMouseOver )
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.Blue.Darken( 0.7f ).Desaturate( 0.3f ).WithAlpha( 0.5f ) );
			Paint.DrawRect( itemRect );
			Paint.SetPen( Theme.White );
		}
		else
		{
			Paint.ClearPen();
			Paint.SetBrush( option.Type.GetColor().WithAlpha( 0.05f ) );
			Paint.DrawRect( itemRect );
			Paint.SetPen( Theme.White );
		}

		itemRect = itemRect.Shrink( 0, 0, 16, 0 );

		if ( item.Object is DirectoryInfo dirInfo )
		{
			Paint.SetDefaultFont();
			Paint.DrawText( itemRect.Shrink( 42, 0 ), dirInfo.Name, TextFlag.LeftCenter | TextFlag.SingleLine );

			Paint.SetPen( Theme.Yellow );
			Paint.DrawIcon( itemRect.Shrink( 15, 3 ), "folder", 18, TextFlag.LeftCenter );
			return;
		}

		var name = option.Name;
		var icon = option.Icon;
		var actionText = option.ActionText;
		var rect = itemRect;

		{
			Paint.SetDefaultFont( 7 );
			Paint.SetPen( Theme.ControlText.WithAlpha( 0.5f ) );
			Paint.DrawText( rect, actionText, TextFlag.RightCenter | TextFlag.SingleLine );
		}

		Paint.SetPen( Theme.ControlText );
		Paint.SetDefaultFont();
		Paint.DrawText( rect.Shrink( 24, 0 ), name, TextFlag.LeftCenter | TextFlag.SingleLine );

		if ( icon == null )
			return;

		var ir = itemRect.Shrink( 3 );
		ir.Size = 16;
		Paint.Draw( ir, icon );
	}

	private void OnToolBarOptionChanged( string selected )
	{
		if ( Enum.TryParse( selected, true, out OptionType type ) )
		{
			FilterType = type;
			SetListItems();
		}
	}

	public override void Show()
	{
		base.Show();
		// Show the window in the center of the screen
		Position = ScreenGeometry.Contain( Size ).Position;
		Position -= new Vector2( 0, 200 );
	}

	public override void Hide()
	{
		base.Hide();

		WindowOpacity = 0;
	}

	protected override void OnKeyPress( KeyEvent e )
	{
		if ( e.KeyboardModifiers.HasFlag( KeyboardModifiers.Ctrl ) && e.Key is KeyCode.K )
			SearchBar.Focus();
	}

	private void HandleOption( Option option )
	{
		switch ( option.Type )
		{
			case OptionType.Asset:
				if ( option is not AssetOption assetOption )
					break;
				assetOption.asset.OpenInEditor();
				Instance?.Destroy();
				break;
			case OptionType.Action:
				if ( option is not ActionOption actionOption )
					break;
				actionOption.action.Invoke();
				break;
			default:
				Log.Warning( $"Unhandled option type: {option.Type}" );
				break;
		}

		Close();
	}
}
