using System.Collections.Generic;
using Godot;

namespace TinyTourney.Core;

public class NodePool<T> where T : CanvasItem
{
    private readonly PackedScene _template;
    private readonly Node _parent;
    private readonly Queue<T> _available = new();

    public NodePool(PackedScene template, Node parent, int prewarmCount = 0)
    {
        _template = template;
        _parent = parent;

        for (int i = 0; i < prewarmCount; i++)
        {
            _available.Enqueue(Spawn());
        }
    }

    public T Get()
    {
        T instance = _available.Count > 0 ? _available.Dequeue() : Spawn();
        instance.Visible = true;
        instance.ProcessMode = Node.ProcessModeEnum.Inherit;
        return instance;
    }

    public void Return(T instance)
    {
        instance.Visible = false;
        instance.ProcessMode = Node.ProcessModeEnum.Disabled;
        _available.Enqueue(instance);
    }

    private T Spawn()
    {
        var instance = _template.Instantiate<T>();
        _parent.AddChild(instance);
        instance.Visible = false;
        instance.ProcessMode = Node.ProcessModeEnum.Disabled;
        return instance;
    }
}
