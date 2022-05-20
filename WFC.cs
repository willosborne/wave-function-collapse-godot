using Godot;
using System.Linq;
using System.Collections.Generic;
using System;

public class WFC : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    ModuleSet moduleSet;

    List<string>[,,] grid;

    [Export]
    int gridWidth = 10;
    [Export]
    int gridHeight = 10;
    [Export]
    int gridDepth = 5;

    [Export]
    public bool ShowDebugSpheres = false;

    DebugSphere[,,] debugSpheres;

    Stack<Vector3> propagateStack = new Stack<Vector3>();

    Random random = new Random();

    Vector3 gridSize = new Vector3(2, 2, 2);

    PackedScene debugSphereScene = Godot.ResourceLoader.Load("res://DebugSphere.tscn") as PackedScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        moduleSet = GetNode<ModuleSet>("ModuleSet");
        string absFilename = Godot.ProjectSettings.GlobalizePath("res://blender/prototypes.json");
        moduleSet = GetNode<ModuleSet>("ModuleSet");
        moduleSet.LoadModules(absFilename);

        Console.WriteLine(string.Join(",", moduleSet.modules.Keys));
        initialiseGrid();
    }

    void initialiseGrid()
    {
        grid = new List<string>[gridWidth, gridHeight, gridDepth];
        if (ShowDebugSpheres)
        {
            debugSpheres = new DebugSphere[gridWidth, gridHeight, gridDepth];
        }
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    grid[x, y, z] = new List<string>(moduleSet.modules.Keys);
                    // todo eliminate incompatible tiles at the border
                    if (ShowDebugSpheres)
                    {
                        DebugSphere instance = debugSphereScene.Instance() as DebugSphere;
                        instance.SetMaxPossibilities(moduleSet.modules.Count);
                        instance.Translate(gridSize * new Vector3(x, y, z));
                        AddChild(instance);
                        debugSpheres[x, y, z] = instance;
                    }
                }
            }
        }
        eliminateBorders();
    }

    void eliminateBorders()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1 || z == 0 || z == gridDepth - 1)
                    {
                        grid[x, y, z].Clear();
                        grid[x, y, z].Add("empty");
                    }
                    propagate(new Vector3(x, y, z));
                }
            }
        }
    }

    public bool IsCollapsed()
    {
        bool isCollapsed = true;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    if (grid[x, y, z].Count > 1)
                    {
                        isCollapsed = false;
                    }
                }
            }
        }
        return isCollapsed;
    }

    public void Iterate()
    {
        if (IsCollapsed()) {
            return;
        }
        Vector3 lowestPoint = findLowestEntropy();
        collapse(lowestPoint);

        moduleSet.InstanceModule(getModules(lowestPoint)[0], lowestPoint * gridSize);

        propagate(lowestPoint);
    }

    void collapse(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        Console.WriteLine($"Collapsing tile at [{x}, {y}, {z}]...");
        Console.WriteLine(string.Join(",", grid[x, y, z]));

        string elem = grid[x, y, z][random.Next(grid[x, y, z].Count)];
        grid[x, y, z].Clear();
        grid[x, y, z].Add(elem);
        Console.WriteLine($"Collapsed tile at [{x}, {y}, {z}] to module {elem}");

        if (ShowDebugSpheres)
        {
            RemoveChild(debugSpheres[x, y, z]);
        }
    }

    Vector3 findLowestEntropy()
    {
        Console.WriteLine("Finding lowest entropy...");
        List<Vector3> lowest = new List<Vector3>();
        int currentLowest = int.MaxValue;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int z = 0; z < gridDepth; z++)
                {
                    int count = grid[x, y, z].Count;
                    switch (grid[x, y, z].Count)
                    {
                        case var _ when count <= 1:
                            continue;
                        case var _ when count < currentLowest:
                            currentLowest = grid[x, y, z].Count;
                            lowest.Clear();
                            lowest.Add(new Vector3(x, y, z));
                            break;
                        case var _ when count == currentLowest:
                            lowest.Add(new Vector3(x, y, z));
                            break;

                    }
                }
            }
        }
        return lowest[random.Next(lowest.Count)];
    }

    void propagate(Vector3 pos)
    {
        Console.WriteLine("yeet");
        // bool modified = false;
        propagateStack.Push(pos);

        while (propagateStack.Count != 0)
        {
            Vector3 val = propagateStack.Pop();
            Console.WriteLine($"Propagating tile at {val}, stack size {propagateStack.Count}");
            foreach (Vector3 direction in getPossibleDirections(pos))
            {
                Vector3 otherCoords = pos + direction;

                Console.WriteLine($"Constraining {otherCoords}");
                List<string> other = getModules(otherCoords);

                List<string> validModules = getValidModules(pos, direction);

                // Console.WriteLine($"Socket A")

                if (other.Any(v => !validModules.Contains(v)))
                {
                    // we have illegal modules; remove them
                    other.RemoveAll(v => !validModules.Contains(v));

                    if (ShowDebugSpheres)
                    {
                        updateDebugSphere(otherCoords);
                    }

                    if (!propagateStack.Contains(otherCoords))
                    {
                        propagateStack.Push(otherCoords);
                    }
                }
            }
        }

    }

    private void updateDebugSphere(Vector3 pos)
    {
        (int x, int y, int z) = unpackVector(pos);
        int count = grid[x, y, z].Count;
        debugSpheres[x, y, z].UpdateSize(count);
    }

    (int, int, int) unpackVector(Vector3 v)
    {
        return ((int)v.x, (int)v.y, (int)v.z);
    }

    List<string> getValidModules(Vector3 pos, Vector3 direction)
    {
        List<string> modulesAt = getModules(pos);

        // from moduleName in modulesAt 
        // join module in moduleSet.modules.Keys on moduleName == module
        // select moduleSet[moduleName]
        string socketName = getSocketName(direction);

        // for each value in modulesAt, look up the corresponding entry in moduleSet.modules
        // flatten the results to a list
        // return modulesAt.stream()
        //     .flatMap(moduleName -> moduleSet.modules[moduleName].Adjacency[socketName]
        //         .stream())
        //         .collect(Collectors.toList());

        // return modulesAt.Join(moduleSet.modules,
        //     moduleName => moduleName,
        //     moduleEntry => moduleEntry.Key,
        //     (moduleName, moduleEntry) => moduleEntry.Value.Adjacency[socketName])
        //         .SelectMany(a => a)
        //         .ToList();

        // return (from moduleName in modulesAt
        //     join module in moduleSet.modules.Keys on moduleName equals module
        //     selectmany moduleSet.modules[module].Adjacency[socketName]);

        return (from moduleName in modulesAt
                from mod in moduleSet.modules[moduleName].Adjacency[socketName]
                select mod).ToList();
    }

    string getSocketName(Vector3 position)
    {
        if (position == Vector3.Left) return "nx";
        if (position == Vector3.Right) return "px";
        if (position == Vector3.Up) return "py";
        if (position == Vector3.Down) return "ny";
        if (position == Vector3.Forward) return "pz";
        if (position == Vector3.Back) return "nz";
        throw new ArgumentException("Vector must be a unit direction");
    }

    List<string> getModules(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        return grid[x, y, z];
    }

    List<Vector3> getPossibleDirections(Vector3 pos)
    {
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        var output = new List<Vector3>();

        if (x > 0) output.Add(new Vector3(-1, 0, 0));
        if (x < gridWidth - 1) output.Add(new Vector3(1, 0, 0));
        if (y > 0) output.Add(new Vector3(0, -1, 0));
        if (y < gridHeight - 1) output.Add(new Vector3(0, 1, 0));
        if (z > 0) output.Add(new Vector3(0, 0, -1));
        if (z < gridDepth - 1) output.Add(new Vector3(0, 0, 1));

        return output;
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
