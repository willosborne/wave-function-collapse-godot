using Godot;
using System;

public class DebugSphere : MeshInstance
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    int maxPossibilites;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public void SetMaxPossibilities(int max) {
        maxPossibilites = max;
    }

    public void UpdateSize(int current) {
        float ratio = (float) current / (float) maxPossibilites;
        Scale = Vector3.One * ratio;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
