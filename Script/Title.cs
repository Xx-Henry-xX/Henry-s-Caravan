using Godot;
using System;

public class Title : Node2D
{
    private readonly char[] letterScoreList = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private GameManager gManager;

    public override void _Ready()
    {
        gManager = GetNode<GameManager>("/root/GameManager");
        gManager.LoadScores();
    }

    public override void _Process(float delta)
    {
        if (gManager.top5[0] >= 1000000000)
        {
            GetNode("1st").Call("set", "bbcode_text", "[color=#ff0][i][b]1st[/b][/i][right][valign px=8][i]" + letterScoreList[gManager.top5[0] / 100000000 - 10] + (gManager.top5[0] / 1000000 % 100).ToString() + "[/i][/valign]" + (gManager.top5[0] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else if (gManager.top5[0] >= 1000000)
        {
            GetNode("1st").Call("set", "bbcode_text", "[color=#ff0][i][b]1st[/b][/i][right][valign px=8][i]" + (gManager.top5[0] / 1000000).ToString() + "[/i][/valign]" + (gManager.top5[0] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else
        {
            GetNode("1st").Call("set", "bbcode_text", "[color=#ff0][i][b]1st[/b][/i][right][valign px=8][i][/i][/valign]" + (gManager.top5[0] % 1000000).ToString() + "[/right][/color]");
        }



        if (gManager.top5[1] >= 1000000000)
        {
            GetNode("2nd").Call("set", "bbcode_text", "[color=#aaa][i][b]2nd[/b][/i][right][valign px=8][i]" + letterScoreList[gManager.top5[1] / 100000000 - 10] + (gManager.top5[1] / 1000000 % 100).ToString() + "[/i][/valign]" + (gManager.top5[1] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else if (gManager.top5[1] >= 1000000)
        {
            GetNode("2nd").Call("set", "bbcode_text", "[color=#aaa][i][b]2nd[/b][/i][right][valign px=8][i]" + (gManager.top5[1] / 1000000).ToString() + "[/i][/valign]" + (gManager.top5[1] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else
        {
            GetNode("2nd").Call("set", "bbcode_text", "[color=#aaa][i][b]2nd[/b][/i][right][valign px=8][i][/i][/valign]" + (gManager.top5[1] % 1000000).ToString() + "[/right][/color]");
        }



        if (gManager.top5[2] >= 1000000000)
        {
            GetNode("3rd").Call("set", "bbcode_text", "[color=#840][i][b]3rd[/b][/i][right][valign px=8][i]" + letterScoreList[gManager.top5[2] / 100000000 - 10] + (gManager.top5[2] / 1000000 % 100).ToString() + "[/i][/valign]" + (gManager.top5[2] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else if (gManager.top5[2] >= 1000000)
        {
            GetNode("3rd").Call("set", "bbcode_text", "[color=#840][i][b]3rd[/b][/i][right][valign px=8][i]" + (gManager.top5[2] / 1000000).ToString() + "[/i][/valign]" + (gManager.top5[2] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else
        {
            GetNode("3rd").Call("set", "bbcode_text", "[color=#840][i][b]3rd[/b][/i][right][valign px=8][i][/i][/valign]" + (gManager.top5[2] % 1000000).ToString() + "[/right][/color]");
        }



        if (gManager.top5[3] >= 1000000000)
        {
            GetNode("4th").Call("set", "bbcode_text", "[color=#008][i][b]4th[/b][/i][right][valign px=8][i]" + letterScoreList[gManager.top5[3] / 100000000 - 10] + (gManager.top5[3] / 1000000 % 100).ToString() + "[/i][/valign]" + (gManager.top5[3] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else if (gManager.top5[3] >= 1000000)
        {
            GetNode("4th").Call("set", "bbcode_text", "[color=#008][i][b]4th[/b][/i][right][valign px=8][i]" + (gManager.top5[3] / 1000000).ToString() + "[/i][/valign]" + (gManager.top5[3] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else
        {
            GetNode("4th").Call("set", "bbcode_text", "[color=#008][i][b]4th[/b][/i][right][valign px=8][i][/i][/valign]" + (gManager.top5[3] % 1000000).ToString() + "[/right][/color]");
        }



        if (gManager.top5[4] >= 1000000000)
        {
            GetNode("5th").Call("set", "bbcode_text", "[color=#400][i][b]5th[/b][/i][right][valign px=8][i]" + letterScoreList[gManager.top5[4] / 100000000 - 10] + (gManager.top5[4] / 1000000 % 100).ToString() + "[/i][/valign]" + (gManager.top5[4] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else if (gManager.top5[4] >= 1000000)
        {
            GetNode("5th").Call("set", "bbcode_text", "[color=#400][i][b]5th[/b][/i][right][valign px=8][i]" + (gManager.top5[4] / 1000000).ToString() + "[/i][/valign]" + (gManager.top5[4] % 1000000).ToString("D6") + "[/right][/color]");
        }
        else
        {
            GetNode("5th").Call("set", "bbcode_text", "[color=#400][i][b]5th[/b][/i][right][valign px=8][i][/i][/valign]" + (gManager.top5[4] % 1000000).ToString() + "[/right][/color]");
        }

        if (Input.IsActionJustPressed("ui_select"))
        {
            GetTree().Paused = true;
            GetTree().ChangeScene("res://Scenes/InGame.tscn");
            gManager.Reset();
        }
        //rebind

        if (Input.IsActionJustPressed("rebind"))
        {
            GetTree().ChangeScene("res://Scenes/KeySettings.tscn");
        }

        if (Input.IsActionJustPressed("scorereset"))
        {
            gManager.ResetScores();
        }
    }
}
