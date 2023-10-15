﻿using Sandbox.Utility;
using System;
using System.Threading.Tasks;

namespace Editor.EntityPrefabEditor;

/// <summary>
/// A popup dialog to select an entity type
/// </summary>
internal partial class ComponentTypeSelector : PopupWidget
{
	public Action<TypeDescription> OnSelect { get; set; }
	List<ComponentSelection> Selections { get; set; } = new();

	int CurrentSelectionIndex { get; set; } = 0;
	Widget Main { get; set; }

	string searchString;
	const string NoCategoryName = "Uncategorized";

	void PushSelection( ComponentSelection selection )
	{
		CurrentSelectionIndex++;

		// Do we have something at our new index, if so, kill it
		if ( Selections.Count > CurrentSelectionIndex && Selections.ElementAt( CurrentSelectionIndex ) is var existingObj ) existingObj.Destroy();

		Selections.Insert( CurrentSelectionIndex, selection );
		Main.Layout.Add( selection, 1 );

		UpdateSelection( selection );
		AnimateSelection( true, Selections[CurrentSelectionIndex - 1], selection );
	}

	internal void PopSelection()
	{
		// Don't pop while empty
		if ( CurrentSelectionIndex == 0 ) return;

		var currentIdx = Selections[CurrentSelectionIndex];
		CurrentSelectionIndex--;

		AnimateSelection( false, currentIdx, Selections[CurrentSelectionIndex] );
	}

	/// <summary>
	/// Runs an animation on the last selection, and the current selection.
	/// I kinda hate this. A lot. But it's pretty.
	/// </summary>
	/// <param name="forward"></param>
	/// <param name="prev"></param>
	/// <param name="selection"></param>
	void AnimateSelection( bool forward, ComponentSelection prev, ComponentSelection selection )
	{
		const string easing = "ease-out";
		const float speed = 0.3f;

		var distance = Width;

		var prevFrom = prev.Position.x;
		var prevTo = forward ? prev.Position.x - distance : prev.Position.x + distance;

		var selectionFrom = forward ? selection.Position.x + distance : selection.Position.x;
		var selectionTo = forward ? selection.Position.x : selection.Position.x + distance;

		var func = ( ComponentSelection a, float x ) =>
		{
			a.Position = a.Position.WithX( x );
			OnMoved();
		};

		Animate.Add( prev, speed, prevFrom, prevTo, x => func( prev, x ), easing );
		Animate.Add( selection, speed, selectionFrom, selectionTo, x => func( selection, x ), easing );
	}

	public ComponentTypeSelector( Widget parent ) : base( parent )
	{
		Layout = Layout.Column();

		var head = Layout.Row();
		head.Margin = 6;

		Layout.Add( head );

		Main = new Widget( this );
		Main.Layout = Layout.Row();
		Layout.Add( Main );

		FixedWidth = 260;
		MaximumHeight = 300;
		DeleteOnClose = true;

		var search = new LineEdit( this );
		search.MinimumHeight = 22;
		search.PlaceholderText = "Search..";
		search.TextEdited += ( t ) =>
		{
			searchString = t;
			UpdateSelection( Selections.First() );
		};

		head.Add( search );

		var selection = new ComponentSelection( this, this );
		UpdateSelection( selection );
		Selections.Add( selection );
		Main.Layout.Add( selection );

		search.Focus();
	}

	protected override void OnPaint()
	{
		Paint.Antialiasing = true;
		Paint.SetPen( Theme.WidgetBackground.Darken( 0.4f ), 1 );
		Paint.SetBrush( Theme.WidgetBackground );
		Paint.DrawRect( LocalRect.Shrink( 1 ), 3 );
	}

	void OnCategorySelected( string category )
	{
		// Push this as a new selection
		PushSelection( new ComponentSelection( this, this, category ) );
	}

	void OnComponentSelected( TypeDescription type )
	{
		OnSelect( type );
		Destroy();
	}

	void OnNewComponentSelected()
	{
		_ = CreateNewComponent();
		Destroy();
	}

	void UpdateSelection( ComponentSelection selection )
	{
		selection.Clear();

		// entity components
		var types = EditorTypeLibrary.GetTypes<BaseComponent>().Where( x => !x.IsAbstract );

		if ( !string.IsNullOrWhiteSpace( searchString ) )
		{
			var query = types.Where( x => x.Title.Contains( searchString, StringComparison.OrdinalIgnoreCase ) );
			foreach ( var type in query )
			{
				selection.AddEntry( new ComponentEntry( selection, type ) { MouseClick = () => OnComponentSelected( type ) } );
			}
			selection.AddStretchCell();
			return;
		}

		if ( selection.Category == null )
		{
			var categories = types.Select( x => string.IsNullOrWhiteSpace( x.Group ) ? NoCategoryName : x.Group ).Distinct().OrderBy( x => x ).ToArray();
			if ( categories.Length > 1 )
			{
				foreach ( var category in categories )
				{
					selection.AddEntry( new ComponentCategory( selection )
					{
						Category = category,
						MouseClick = () => OnCategorySelected( category ),
					} );
				}

				selection.AddEntry( new ComponentEntry( selection ) { Text = "New Component...", MouseClick = OnNewComponentSelected } );
				selection.AddStretchCell();

				return;
			}
		}
		else
		{
			types = types.Where( x => selection.Category == NoCategoryName ? x.Group == null : x.Group == selection.Category ).OrderBy( x => x.Title );

			foreach ( var type in types )
			{
				selection.AddEntry( new ComponentEntry( selection, type ) { MouseClick = () => OnComponentSelected( type ) } );
			}
			selection.AddStretchCell();
		}
	}

	/// <summary>
	/// We're creating a new component..
	/// </summary>
	async Task CreateNewComponent()
	{
		var codePath = LocalProject.CurrentGame.GetCodePath();

		var fd = new FileDialog( null );
		fd.Title = "Create new component..";
		fd.Directory = codePath;
		fd.DefaultSuffix = ".cs";
		fd.SelectFile( $"MyComponent.cs" );
		fd.SetFindFile();
		fd.SetModeSave();
		fd.SetNameFilter( "Cs File (*.cs)" );

		if ( !fd.Execute() )
			return;

		var componentName = System.IO.Path.GetFileNameWithoutExtension( fd.SelectedFile );

		if ( !System.IO.File.Exists( fd.SelectedFile ) )
		{
			var defaultComponent = $$"""
				using Sandbox;

				public sealed class {{componentName}} : BaseComponent
				{
					public override void Update()
					{
						
					}
				}

				""";

			System.IO.File.WriteAllText( fd.SelectedFile, defaultComponent );
		}

		// give it half a second, should do it
		await Task.Delay( 500 );

		// open it in the code editor
		CodeEditor.OpenFile( fd.SelectedFile );

		// we just wrote a file, lets wait until its compiled and loaded
		await EditorUtility.Projects.WaitForCompiles();

		var componentType = EditorTypeLibrary.GetType<BaseComponent>( componentName );
		if ( componentType is null )
		{
			Log.Warning( $"Couldn't find target component type {componentName}" );

			componentType = EditorTypeLibrary.GetType( componentName );
			Log.Warning( $"Couldn't find target component type {componentType}" );

			foreach ( var t in EditorTypeLibrary.GetTypes<BaseComponent>() )
			{
				Log.Info( $"{t}" );
			}
		}
		else
		{
			OnSelect( componentType );
		}

		Destroy();
	}


	partial class ComponentSelection : Widget
	{
		internal string Category { get; init; }
		Widget CategoryHeader { get; set; }
		ScrollArea Scroller { get; set; }
		ComponentTypeSelector Selector { get; set; }

		internal ComponentSelection( Widget parent, ComponentTypeSelector selector, string categoryName = null ) : base( parent )
		{
			Selector = selector;
			Category = categoryName;
			FixedWidth = 300;
			MaximumHeight = 220;

			Layout = Layout.Column();

			CategoryHeader = new Widget( this );
			CategoryHeader.FixedHeight = 24;
			CategoryHeader.OnPaintOverride = PaintHeader;
			CategoryHeader.MouseClick = Selector.PopSelection;
			Layout.Add( CategoryHeader );

			Scroller = new ScrollArea( this );
			Scroller.Layout = Layout.Column();
			Layout.Add( Scroller, 1 );

			Scroller.Canvas = new Widget( Scroller );
			Scroller.Canvas.Layout = Layout.Column();
		}

		internal bool PaintHeader()
		{
			var c = CategoryHeader;

			Paint.ClearPen();
			Paint.SetBrush( Theme.WidgetBackground.WithAlpha( c.IsUnderMouse ? 0.7f : 0.4f ) );
			Paint.DrawRect( c.LocalRect );

			var r = c.LocalRect.Shrink( 12, 2 );
			Paint.SetPen( Theme.ControlText );

			if ( Selector.CurrentSelectionIndex > 0 )
			{
				Paint.DrawIcon( r, "arrow_back", 14, TextFlag.LeftCenter );
			}

			Paint.SetDefaultFont( 8 );
			Paint.DrawText( r, string.IsNullOrEmpty( Category ) ? "Component" : Category, TextFlag.Center );

			return true;
		}

		internal void AddEntry( ComponentBaseEntry entry )
		{
			Scroller.Canvas.Layout.Add( entry );
			entry.Selector = this;
		}

		internal void AddStretchCell()
		{
			Scroller.Canvas.Layout.AddStretchCell( 1 );
			Update();
		}

		internal void Clear()
		{
			Scroller.Canvas.Layout.Clear( true );
		}

		protected override void OnPaint()
		{
			Paint.Antialiasing = true;
			Paint.SetPen( Theme.WidgetBackground.Darken( 0.8f ), 1 );
			Paint.SetBrush( Theme.WidgetBackground.Darken( 0.2f ) );
			Paint.DrawRect( LocalRect.Shrink( 0 ), 3 );
		}
	}

	partial class ComponentBaseEntry : Widget
	{
		internal ComponentSelection Selector { get; set; }

		internal ComponentBaseEntry( Widget parent ) : base( parent )
		{
			FixedHeight = 24;
		}
	}

	partial class ComponentEntry : ComponentBaseEntry
	{
		public string Text { get; set; } = "My Component";
		public string Icon { get; set; } = "note_add";

		public TypeDescription Type { get; init; }

		internal ComponentEntry( Widget parent, TypeDescription type = null ) : base( parent )
		{
			Type = type;

			if ( type is not null )
			{
				Text = type.Title;
				Icon = type.Icon;
			}
		}

		protected override void OnPaint()
		{
			var r = LocalRect.Shrink( 12, 2 );
			var opacity = IsUnderMouse ? 1.0f : 0.7f;

			if ( Type is not null && !string.IsNullOrEmpty( Type.Icon ) )
			{
				Helpers.PaintComponentIcon( Type, new Rect( r.Position, r.Height ).Shrink( 2 ), opacity );
			}
			else
			{
				Paint.SetPen( Theme.Green.WithAlpha( opacity ) );
				Paint.DrawIcon( new Rect( r.Position, r.Height ).Shrink( 2 ), "note_add", r.Height, TextFlag.Center );
			}

			r.Left += r.Height + 6;

			Paint.SetDefaultFont( 8 );
			Paint.SetPen( Theme.ControlText.WithAlpha( IsUnderMouse ? 1.0f : 0.5f ) );
			Paint.DrawText( r, Text, TextFlag.LeftCenter );
		}
	}

	partial class ComponentCategory : ComponentBaseEntry
	{
		public string Category { get; set; }
		public ComponentCategory( Widget parent ) : base( parent ) { }

		protected override void OnPaint()
		{
			Paint.SetPen( Theme.ControlText.WithAlpha( IsUnderMouse ? 1.0f : 0.5f ) );

			var r = LocalRect.Shrink( 12, 2 );
			Paint.SetDefaultFont( 8 );
			Paint.DrawText( r, Category, TextFlag.LeftCenter );
			Paint.DrawIcon( r, "arrow_forward", 14, TextFlag.RightCenter );
		}
	}
}
