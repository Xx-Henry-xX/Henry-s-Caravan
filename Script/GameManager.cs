using Godot;
using System;
using Newtonsoft.Json;

public class GameManager : Node2D
{
    public uint p1Score = 0;
    public int p1Lives = 2;
    public int p1Bombs = 3;
    public int framesLeft = 7200;
    public bool timerOn = true;
    public int startTimer = 0;
    public uint hiScore = 1000000;
    public uint[] top5 = { 250000, 20000, 15000, 10000, 5000 };
    public int pos = -1;
    public readonly string[] posString = { "1ST", "2ND", "3RD", "4TH", "5TH", "NOWHERE" };
    public readonly uint[] gradeCutoffs =
    {
        50000u,     //8
        100000u,    //7
        150000u,    //6
        200000u,    //5
        300000u,    //4
        500000u,    //3
        650000u,    //2
        800000u,    //1
        1000000u,   //S1
        1250000u,   //S2
        1500000u,   //S3
        1750000u,   //S4
        2000000u,   //S5
        2500000u,   //S6
        3000000u    //S7
    };
    public int twoUpCount = 0;



    public bool playing = false;

    public int rank = 200000;
    public int rankPerFrame = 10;
    public int loop = 0;

    public readonly uint[] medalScore = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000 };
    public readonly int[] medalRank = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 150, 200, 250, 300, 350, 400, 450, 500, 1000 };
    public int currentMedal = 0;
    public int medalDropCounter = 100;
    public int waitingExtends = 0;
    public uint nextExtend = 1000000;

    public int clearTimer = 300;
    private bool bonusgiven = false;

    public EnemyBase boss = null; //for hp bar

    public const string HIGH_SCORES_FILE_PATH = "user://highscore.save";
    public readonly uint[] defaulttop5 = { 250000, 20000, 15000, 10000, 5000 };

    public override void _Ready()
    {
        
    }

    public void Reset()
    {
        p1Score = 0;
        p1Lives = 2;
        p1Bombs = 3;
        framesLeft = 7200;
        timerOn = true;
        rank = 200000;
        rankPerFrame = 10;
        currentMedal = 0;
        medalDropCounter = 100;
        waitingExtends = 0;
        nextExtend = 1000000;
        hiScore = top5[0];
        startTimer = 179;
        pos = -1;
        loop = 0;
        clearTimer = 300;
        bonusgiven = false;
        boss = null;
        twoUpCount = 0;

        playing = true;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            GetTree().ChangeScene("res://Scenes/Title.tscn");
            playing = false;
        }
        if (!playing) return;
        if (boss != null)
        {
            if (boss.hp <= 0)
            {
                boss = null;
                GD.Print("rip boss");
            }
            if (boss.LeaveCheck())
            {
                boss = null;
                GD.Print("boss left");
            }
        }

        if (startTimer > 0)
        {
            startTimer--;
            if (startTimer <= 0) GetTree().Paused = false;
            return;
        }
        if (framesLeft <= 0 || clearTimer <= 0)
        {
            if (clearTimer <= 0 && !bonusgiven)
            {
                GD.Print("bonk");
                p1Score += (uint)(framesLeft % 6 * 1000);
                bonusgiven = true;
            }
            GameOver();
            return;
        }
        if (timerOn) framesLeft--;
        else clearTimer--;
        rank += rankPerFrame;
        rank = Math.Min(rank, 999999);
        if (p1Score >= nextExtend)
        {
            nextExtend += 1000000;
            waitingExtends++;
        }
        hiScore = Math.Max(hiScore, p1Score);
    }

    public void GameOver()
    {
        hiScore = Math.Max(hiScore, p1Score);
        GetTree().Paused = true;
        playing = false;
        for (int i = 0; i < 5; i++)
        {
            pos = i;
            if (p1Score > top5[i]) break;
        }
        if (pos < 5)
        {
            for (int i = 4; i > pos; i--)
            {
                top5[i] = top5[i - 1];
            }
            top5[pos] = p1Score;
        }
        SaveScores();
    }

    public void ResetScores()
    {
        top5 = defaulttop5;
        var dir = new Godot.Directory();
        dir.Remove(HIGH_SCORES_FILE_PATH);
    }

    public void SaveScores()
    {
        var dir = new Godot.Directory();
        dir.Remove(HIGH_SCORES_FILE_PATH);

        var savedControlsFile = new Godot.File();
        savedControlsFile.Open(HIGH_SCORES_FILE_PATH, Godot.File.ModeFlags.Write);
        savedControlsFile.StoreLine(JsonConvert.SerializeObject(top5));
        savedControlsFile.Close();
    }

    public void LoadScores()
    {
        var savedScoresFile = new Godot.File();
        if (!savedScoresFile.FileExists(HIGH_SCORES_FILE_PATH))
        {
            GD.Print("Saved high score file could not be found; falling back to default scores");
            ResetScores();
            return;
        }

        savedScoresFile.Open(HIGH_SCORES_FILE_PATH, Godot.File.ModeFlags.Read);
        string line = savedScoresFile.GetLine();
        uint[] scoreData = JsonConvert.DeserializeObject<uint[]>(line);
        top5 = scoreData;
        savedScoresFile.Close();
    }

    public int Grade()
    {
        int rtnValue = 0;
        foreach (uint n in gradeCutoffs)
        {
            if (p1Score < n) break;
            rtnValue++;
        }
        rtnValue += loop + twoUpCount + (clearTimer <= 0 ? 1 : 0);
        rtnValue = Math.Min(rtnValue, 19);
        if (rtnValue == 19 && p1Score < 3000000u) rtnValue--;

        return rtnValue;
    }
}
