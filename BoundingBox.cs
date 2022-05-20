using Godot;
using System;
using System.Collections.Generic;

public class BoundingBox : Spatial
{
    public string TopLabel {get; set;} = "Top";
    public string BottomLabel {get; set;} = "Bottom";
    public string LeftLabel {get; set;} = "Left";
    public string RightLabel {get; set;} = "Right";
    public string FrontLabel {get; set;} = "Front";
    public string BackLabel {get; set;} = "Back";

    public override void _Ready()
    {
        GetNode<Label>("Top/Label").Text = TopLabel;
        GetNode<Label>("Bottom/Label").Text = BottomLabel;
        GetNode<Label>("Left/Label").Text = LeftLabel;
        GetNode<Label>("Right/Label").Text = RightLabel;
        GetNode<Label>("Front/Label").Text = FrontLabel;
        GetNode<Label>("Back/Label").Text = BackLabel;
    }

    public override void _Process(float delta)
    {
        DebugDraw.DrawBox(GlobalTransform.origin, new Vector3(2, 2, 2));
    //     var point = get_viewport().get_camera().unproject_position($CarryPoint.global_transform.origin)
	// $Label.rect_position = point
        foreach (string pos in new List<string> {"Top", "Bottom", "Left", "Right", "Front", "Back"}) {
            Spatial node = GetNode<Spatial>(pos);
            Label label = node.GetNode<Label>("Label");
            Vector2 point = GetViewport().GetCamera().UnprojectPosition(node.GlobalTransform.origin);
            label.RectPosition = point;
        }
    }
}
