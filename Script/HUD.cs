using Godot;
using System;

public class HUD : Node2D
{
    private readonly char[] letterScoreList = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private readonly string[] gradeText =
    {
        "[b][i]9[/i][/b]\n\nAND YOU SUCK",
        "[b][i]8[/i][/b]\n\nAND YOU SUCK",
        "[b][i]7[/i][/b]\n\nAND YOU SUCK",
        "[b][i]6[/i][/b]\n\nAND YOU SUCK A BIT",
        "[b][i]5[/i][/b]\n\nAND YOU SUCK A BIT",
        "[b][i]4[/i][/b]\n\nAND YOU ARE BARELY AVERAGE",
        "[b][i]3[/i][/b]\n\nAND YOU ARE BARELY AVERAGE",
        "[b][i]2[/i][/b]\n\nAND YOU ARE AVERAGE",
        "[b][i]1[/i][/b]\n\nAND YOU ARE AVERAGE",
        " [b][i]S[/i][/b][valign px=-8]1[/valign]\n\nAND YOU DID WELL",
        " [b][i]S[/i][/b][valign px=-8]2[/valign]\n\nAND YOU DID WELL",
        " [b][i]S[/i][/b][valign px=-8]3[/valign]\n\nAND YOU DID WELL",
        " [b][i]S[/i][/b][valign px=-8]4[/valign]\n\nAND YOU DID WELL",
        " [b][i]S[/i][/b][valign px=-8]5[/valign]\n\nAND YOU PLAYED VERY WELL",
        " [b][i]S[/i][/b][valign px=-8]6[/valign]\n\nAND YOU PLAYED VERY WELL",
        " [b][i]S[/i][/b][valign px=-8]7[/valign]\n\nAND YOU PLAYED VERY WELL",
        " [b][i]S[/i][/b][valign px=-8]8[/valign]\n\nAND YOU PLAYED VERY WELL",
        " [b][i]S[/i][/b][valign px=-8]9[/valign]\n\nAND YOU ARE ALMOST THERE",
        " [b][i]M[/i][/b][valign px=-8]ASTER[/valign]\n\nAND YOU ARE ALMOST THERE",
        "[b][i]G[/i][/b][valign px=-8]RAND[/valign] [b][i]M[/i][/b][valign px=-8]ASTER[/valign]\n\nTHANK YOU FOR PLAYING -XX_HENRY_XX"
    };
    private GameManager gManager;

    public override void _Ready()
    {
        gManager = GetNode<GameManager>("/root/GameManager");
    }

    public override void _Process(float delta)
    {
        if (gManager.startTimer > 0)
        {
            GetNode("Countdown").Call("set", "visible", true);
            GetNode("Countdown").Call("set", "bbcode_text", "[b][i]" + (gManager.startTimer / 60 + 1).ToString("D1"));
        }
        else GetNode("Countdown").Call("set", "visible", false);
        //GetNode("Timer").Call("set", "bbcode_text", "[right]" + gManager.framesLeft + "[/right]");
        GetNode("Timer").Call("set", "bbcode_text", "[right]" + (gManager.framesLeft / 3600).ToString("D1") + ";" + (gManager.framesLeft % 3600 / 60).ToString("D2") + "," + (gManager.framesLeft % 60 / 6).ToString("D1") + "[/right]");
        GetNode("1PLives").Call("set", "rect_size", new Vector2(8 * gManager.p1Lives, 16));
        GetNode("1PBombs").Call("set", "rect_size", new Vector2(8 * gManager.p1Bombs, 16));
        GetNode("LastLifeText").Call("set", "visible", gManager.p1Lives == 0);

        GetNode("Rank").Call("set", "bbcode_text", "[right]" + gManager.rankPerFrame.ToString("D3")  + " " + gManager.rank.ToString("D6") + "[/right]");

        if (gManager.p1Score >= 3600000000) gManager.p1Score = gManager.p1Score % 10 + 3599999990;
        if (gManager.p1Score >= 1000000000)
        {
            GetNode("1PScore").Call("set", "bbcode_text", "[right][i]" + letterScoreList[gManager.p1Score / 100000000 - 10] + (gManager.p1Score / 1000000 % 100).ToString() + "[/i][valign px=-8]" + (gManager.p1Score % 1000000).ToString("D6") + "[/valign][/right]");
        }
        else if (gManager.p1Score >= 1000000)
        {
            GetNode("1PScore").Call("set", "bbcode_text", "[right][i]" + (gManager.p1Score / 1000000).ToString() + "[/i][valign px=-8]" + (gManager.p1Score % 1000000).ToString("D6") + "[/valign][/right]");
        }
        else
        {
            GetNode("1PScore").Call("set", "bbcode_text", "[right][valign px=-8]" + (gManager.p1Score % 1000000).ToString() + "[/valign][/right]");
        }

        if (gManager.hiScore >= 3600000000) gManager.hiScore = gManager.hiScore % 10 + 3599999990;
        if (gManager.hiScore >= 1000000000)
        {
            GetNode("HiScore").Call("set", "bbcode_text", "[right][i]" + letterScoreList[gManager.hiScore / 100000000 - 10] + (gManager.hiScore / 1000000 % 100).ToString() + "[/i][valign px=-8]" + (gManager.hiScore % 1000000).ToString("D6") + "[/valign][/right]");
        }
        else if (gManager.hiScore >= 1000000)
        {
            GetNode("HiScore").Call("set", "bbcode_text", "[right][i]" + (gManager.hiScore / 1000000).ToString() + "[/i][valign px=-8]" + (gManager.hiScore % 1000000).ToString("D6") + "[/valign][/right]");
        }
        else
        {
            GetNode("HiScore").Call("set", "bbcode_text", "[right][valign px=-8]" + (gManager.hiScore % 1000000).ToString() + "[/valign][/right]");
        }

        if (gManager.pos != -1)
        {
            GetNode("GameOver").Call("set", "visible", true);
            GetNode("GameOver").Call("set", "bbcode_text", "[center][b][i]" + (gManager.clearTimer <= 0 ? "COMPLETE" : gManager.framesLeft == 0 ? "TIME OVER" : "GAME OVER") + "[/i][/b]\n\nYOU PLACED " + gManager.posString[gManager.pos] + "\n\n\nYOUR MASTER GRADE IS\n" + gradeText[gManager.Grade()]);
        }
        else GetNode("GameOver").Call("set", "visible", false);
    }
}
