using Godot;
using System;

public class FirstPersonCamera : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    [Export]
    public float MoveSpeed = 10.0f;
    [Export]
    public float Sensitivity = 0.002f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseMotion motionEvent) {
            // Rotation = new Vector3(-motionEvent.Relative.y * Sensitivity, -motionEvent.Relative.x * Sensitivity, 0);
            // Rotation = new Vector3(Mathf.Clamp(Rotation.x, -1.2f, 1.2f), Rotation.y, Rotation.z);
            Camera child = GetNode<Camera>("Camera");
            child.RotateX(-motionEvent.Relative.y * Sensitivity);
            child.Rotation = new Vector3(Mathf.Clamp(child.Rotation.x, -1.2f, 1.2f), child.Rotation.y, child.Rotation.z);
            RotateY(-motionEvent.Relative.x * Sensitivity);
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Vector3 moveDir = getMoveDir();
        Vector3 velocity = moveDir * MoveSpeed;

        Translate(velocity * delta);

        if (Input.IsActionJustReleased("exit")) {
            GetTree().Quit(0);
        }
    }

    Vector3 getMoveDir() {
        Vector3 moveDir = new Vector3();
        if (Input.IsActionPressed("move_forward")) {
            // moveDir += -GlobalTransform.basis.z;
            moveDir += Vector3.Forward;
        }   
        if (Input.IsActionPressed("move_backward")) {
            // moveDir += GlobalTransform.basis.z;
            moveDir += Vector3.Back;
        }   
        if (Input.IsActionPressed("strafe_left")) {
            // moveDir += -GlobalTransform.basis.x;
            moveDir += Vector3.Left;
        }   
        if (Input.IsActionPressed("strafe_right")) {
            // moveDir += GlobalTransform.basis.x;
            moveDir += Vector3.Right;
        }   
        if (Input.IsActionPressed("fly_up")) {
            // moveDir += GlobalTransform.basis.x;
            moveDir += Vector3.Up;
        }   
        if (Input.IsActionPressed("fly_down")) {
            // moveDir += GlobalTransform.basis.x;
            moveDir += Vector3.Down;
        }   
        return moveDir.Normalized();
    }
}
