using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

public class ModuleSet : Spatial {
    public Dictionary<string, Module> modules = new Dictionary<string, Module>();
    public Dictionary<string, PackedScene> moduleScenes = new Dictionary<string, PackedScene>();

    public void LoadModules(string filename) 
    {
        string json = System.IO.File.ReadAllText(filename);
        Dictionary<string, Module> modules = JsonConvert.DeserializeObject<Dictionary<string, Module>>(json);
        this.modules = modules;

        foreach (KeyValuePair<string, Module> entry in modules) {
            swizzleSockets(entry.Value);
            if (entry.Value.MeshName != "") {
                moduleScenes[entry.Key] = loadScene(entry.Value.MeshName);
            }
        }
    }

    private void swizzleSockets(Module module) 
    {
        Dictionary<string, string> socketsCopy = new Dictionary<string, string>(module.Sockets);
        Dictionary<string, List<string>> adjacencyCopy = new Dictionary<string, List<string>>(module.Adjacency);

        // simply swapping z and y
        module.Sockets["py"] = socketsCopy["pz"];
        module.Sockets["pz"] = socketsCopy["py"];
        module.Sockets["ny"] = socketsCopy["nz"];
        module.Sockets["nz"] = socketsCopy["ny"];
        
        module.Adjacency["py"] = adjacencyCopy["pz"];
        module.Adjacency["pz"] = adjacencyCopy["py"];
        module.Adjacency["ny"] = adjacencyCopy["nz"];
        module.Adjacency["nz"] = adjacencyCopy["ny"];

        // rotate
        // module.Sockets["py"] = socketsCopy["nz"];
        // module.Sockets["pz"] = socketsCopy["py"];
        // module.Sockets["ny"] = socketsCopy["pz"];
        // module.Sockets["nz"] = socketsCopy["ny"];
        
        // module.Adjacency["py"] = adjacencyCopy["nz"];
        // module.Adjacency["pz"] = adjacencyCopy["py"];
        // module.Adjacency["ny"] = adjacencyCopy["pz"];
        // module.Adjacency["nz"] = adjacencyCopy["ny"];
    }

    private PackedScene loadScene(string meshName)
    {
        string filename = $"res://blender/{meshName}.scn";
        return ResourceLoader.Load(filename) as PackedScene;
    }

    public Spatial InstanceModule(string moduleName, Vector3 position)
    {
        if (moduleName == "empty") {
            return null;
        }
        Module module = modules[moduleName];
        PackedScene scene = moduleScenes[moduleName];
        Spatial instance = scene.Instance() as Spatial;
        Vector3 rot = instance.Rotation;
        instance.Rotation = Vector3.Zero;
        instance.Translate(position);
        instance.Rotation = rot;
        // instance.RotateX(Godot.Mathf.Deg2Rad(90));
        instance.RotateY(Godot.Mathf.Deg2Rad(module.Rotation * 90));

        AddChild(instance);
        return instance;
    }
}