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
         100000u,   //8
         200000u,   //7
         300000u,   //6
         400000u,   //5
         550000u,   //4
         700000u,   //3
         850000u,   //2
        1000000u,   //1
        1200000u,   //S1
        1400000u,   //S2
        1600000u,   //S3
        1800000u,   //S4
        2000000u,   //S5
        2250000u,   //S6
        2500000u,   //S7
        2750000u,   //S8
        3000000u    //S9
    };



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
    private AudioManager aManager;

    public const string HIGH_SCORES_FILE_PATH = "user://highscore.save";
    public readonly uint[] defaulttop5 = { 250000, 20000, 15000, 10000, 5000 };

    public override void _Ready()
    {
        aManager = GetNode<AudioManager>("/root/AudioManager");
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

        playing = true;
        aManager.Play("bgm", 0);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionJustPressed("reset"))
        {
            GetTree().ChangeScene("res://Scenes/Title.tscn");
            playing = false;
            aManager.StopAll();
        }
        if (!playing) return;
        if (boss != null)
        {
            if (boss.hp <= 0 || boss.LeaveCheck())
            {
                boss = null;
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
        for (int i = 0; i <= 5; i++)
        {
            pos = i;
            if (pos == 5) break;
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
        aManager.StopAll();
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
        rtnValue += loop + (clearTimer <= 0 ? 1 : 0);
        rtnValue = Math.Min(rtnValue, 19);
        if (rtnValue == 19 && p1Score < 3000000u) rtnValue--;

        return rtnValue;
    }
}
