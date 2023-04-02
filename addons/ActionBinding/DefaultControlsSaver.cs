using Godot;
using System.Collections.Generic;
using System;

public class DefaultControlsSaver : Control
{
    public Dictionary<string, List<InputEvent>> InitialBindings = new Dictionary<string, List<InputEvent>>();
    public float InitialDeadzone = 0.5f;
    public bool defined = false;
}
