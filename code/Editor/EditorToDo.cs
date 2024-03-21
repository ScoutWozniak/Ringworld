using Sandbox;

namespace Editor;

[Dock("Editor", "To Do")]
public partial class ToDoList : Widget
{
	public ToDoList(Widget parent) : base(parent) {
		ReBuild();
	}

	public void ReBuild()
	{
		Layout?.Clear( true );
		WindowTitle = "ToDo";
		
		var textEditor = new TextEdit( this );
		textEditor.MinimumSize = Vector2.One * 512.0f;
		textEditor.TextChanged = Test;
		if (ProjectCookie.TryGetString("ToDo", out var val) )
		{
			textEditor.AppendPlainText( val );
		}

	}

	protected void Test(string value)
	{
		ProjectCookie.SetString( "ToDo", value );
	}
}
