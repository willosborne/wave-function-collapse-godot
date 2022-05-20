using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ModuleGallery : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    Dictionary<string, Module> modules;

    [Export]
    public int RowLength = 7;

    [Export] 
    public int Offset = 30;

    private PackedScene boundingBox = ResourceLoader.Load("res://BoundingBox.tscn") as PackedScene;

    private ModuleSet moduleSet;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        string absFilename = Godot.ProjectSettings.GlobalizePath("res://blender/prototypes.json");
        GD.Print("Loading prototypes from " + absFilename);
        // LoadPrototypes(absFilename);
        moduleSet = GetNode<ModuleSet>("ModuleSet");
        moduleSet.LoadModules(absFilename);
        setupGallery();
    }

    public void LoadPrototypes(string filename)
    {
        string json = System.IO.File.ReadAllText(filename);
        Dictionary<String, Module> modules = JsonConvert.DeserializeObject<Dictionary<String, Module>>(json);

        Console.WriteLine(modules.Keys);
        int x = 0;
        int z = 0;
        int i = 0;
        foreach (string module in modules.Keys) {
            createModulePreview(modules[module], x, z);
            i++;
            x += Offset;
            if (i > RowLength) {
                i = 0;
                x = 0;
                z += Offset;
            }
            Console.WriteLine($"i: {i}, x: {x}, z: {z}");
        }
    }

    private void setupGallery() {
        int x = 0;
        int z = 0;
        int i = 0;
        foreach (Module module in moduleSet.modules.Values) {
            createModulePreview(module, x, z);
            i++;
            x += Offset;
            if (i > RowLength) {
                i = 0;
                x = 0;
                z += Offset;
            }
            Console.WriteLine($"i: {i}, x: {x}, z: {z}");
        }
    }

    private PackedScene loadScene(string meshName) {
        string filename = $"res://blender/{meshName}.scn";
        return ResourceLoader.Load(filename) as PackedScene;
    }

    private void createModulePreview(Module module, int x, int z)
    {
        if (module.MeshName != "") {
            if (module.Scene == null) {
                module.Scene = loadScene(module.MeshName);
            }
            Spatial instance = module.Scene.Instance() as Spatial;
            Vector3 rot = instance.Rotation;
            instance.Rotation = Vector3.Zero;
            instance.Translate(new Vector3(x, 0, z));
            instance.Rotation = rot;
            // instance.RotateX(Godot.Mathf.Deg2Rad(90));
            instance.RotateY(Godot.Mathf.Deg2Rad(module.Rotation * 90));

            AddChild(instance);
        }

        BoundingBox box = boundingBox.Instance() as BoundingBox;
        box.Translate(new Vector3(x, 0, z));
        box.TopLabel = module.Sockets["py"];
        box.BottomLabel = module.Sockets["ny"];
        box.RightLabel = module.Sockets["px"];
        box.LeftLabel = module.Sockets["nx"];
        box.FrontLabel = module.Sockets["nz"];
        box.BackLabel = module.Sockets["pz"];
        AddChild(box);
        // instance.AddChild(box);

    }
}
