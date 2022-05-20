using Godot;
using System;

public class Demo : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private WFC wfc;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        wfc = GetNode<WFC>("WFC");   
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("ui_accept")) {
            wfc.Iterate();
        }
    }
}
