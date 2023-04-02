using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;

public class CaravanSpawner : Node2D
{
    private GameManager gManager;
    private EnemyManager eManager;
    private PlayerController player;

    private RandomNumberGenerator rng;
    private int currentWave = -1;
    private int spawnDelay = 0;
    private int spawnTimer = 0;
    private int spawnCount = 0;
    private bool leftSide = true;
    private bool bossSpawned = false;

    private void SetCount()
    {
        rng = new RandomNumberGenerator
        {
            Seed = (ulong)(gManager.p1Score + gManager.rank)
        };
        player = GetNode<PlayerController>("/root/Main/Player");
        leftSide = player.GetSFPosition().x < (sfloat)120;
        switch (currentWave)
        {
            case 3:
            case 7:
            case 12:
            case 14:
            case 16:
            case 19:
            case 21:
            case 22:
            case 25:
            case 26:
            case 27:
            case 29:
                spawnCount = 1;
                spawnDelay = 0;
                break;
            case 0:
            case 1:
            case 2:
            case 8:
            case 9:
            case 10:
            case 11:
            case 15:
            case 17:
            case 20:
            case 24:
                spawnCount = 6 + gManager.rank / 50000;
                spawnDelay = 10;
                break;
            case 4:
                spawnCount = 2 + gManager.rank / 50000;
                spawnDelay = 10;
                break;
            case 5:
            case 6:
                spawnCount = 3 + gManager.rank / 50000;
                spawnDelay = 5;
                break;
            case 18:
                spawnCount = 6 + gManager.rank / 50000;
                spawnDelay = 5;
                break;
            case 23:
                spawnCount = 10 + gManager.rank / 50000;
                spawnDelay = 5;
                break;
            case 13:
            case 28:
                spawnCount = 10 + gManager.rank / 50000;
                spawnDelay = 5;
                break;
        }
    }

    private void Spawn()
    {
        switch (currentWave)
        {
            case 0:
                eManager.enemies.Add(new Bouncy(gManager, new SFPoint((sfloat)60, (sfloat)(-8))));
                eManager.enemies.Add(new Bouncy(gManager, new SFPoint((sfloat)180, (sfloat)(-8))));
                break;
            case 1:
                eManager.enemies.Add(new Sine(gManager, player, new SFPoint((sfloat)(!leftSide ? 20 : 220), (sfloat)(-8)), true));
                break;
            case 2:
            case 15:
                eManager.enemies.Add(new Search(gManager, new SFPoint((sfloat)40, (sfloat)(-8))));
                eManager.enemies.Add(new Search(gManager, new SFPoint((sfloat)200, (sfloat)(-8))));
                break;
            case 3:
                eManager.enemies.Add(new BronzeMedium(gManager, new SFPoint((sfloat)(!leftSide ? 60 : 180), (sfloat)(-8))));
                break;
            case 4:
                for (int i = 30; i < 240; i += 30) eManager.enemies.Add(new Aimed(gManager, player, new SFPoint((sfloat)i, (sfloat)(-8))));
                break;
            case 5:
            case 6:
                if (!leftSide) eManager.enemies.Add(new Chicken(gManager, player, new SFPoint((sfloat)rng.RandiRange(20, 100), (sfloat)(-9))));
                else eManager.enemies.Add(new Chicken(gManager, player, new SFPoint((sfloat)rng.RandiRange(140, 220), (sfloat)(-9))));
                break;
            case 7:
                eManager.enemies.Add(new PurpleMedium(gManager, new SFPoint(player.GetSFPosition().x * (sfloat)(-1) + (sfloat)240, (sfloat)(-8)), 0));
                break;
            case 8:
            case 18:
            case 23:
                eManager.enemies.Add(new Sideroll(gManager, new SFPoint((sfloat)(-8), (sfloat)rng.RandiRange(40, 140))));
                eManager.enemies.Add(new Sideroll(gManager, new SFPoint((sfloat)248, (sfloat)rng.RandiRange(40, 140))));
                break;
            case 9:
            case 24:
                eManager.enemies.Add(new Sine(gManager, player, new SFPoint((sfloat)(20), (sfloat)(-8)), true));
                eManager.enemies.Add(new Sine(gManager, player, new SFPoint((sfloat)(220), (sfloat)(-8)), true));
                break;
            case 10:
                eManager.enemies.Add(new Bouncy(gManager, new SFPoint((sfloat)(!leftSide ? 60 : 180), (sfloat)(-8))));
                break;
            case 11:
            case 17:
                eManager.enemies.Add(new Sine(gManager, player, new SFPoint((sfloat)(!leftSide ? 110 : 130), (sfloat)(-8)), false));
                break;
            case 12:
            case 25:
            case 26:
            case 27:
                eManager.enemies.Add(new WaveGunMedium(gManager, new SFPoint(player.GetSFPosition().x * (sfloat)(-1) + (sfloat)240, (sfloat)(-8))));
                break;
            case 13:
            case 28:
                eManager.enemies.Add(new StraightDown(gManager, new SFPoint((sfloat)rng.RandiRange(20, 220), (sfloat)(-8))));
                break;
            case 14:
                Midboss1 boss1 = new Midboss1(gManager);
                eManager.enemies.Add(boss1);
                gManager.boss = boss1;
                break;
            case 29:
                Midboss2 boss2 = new Midboss2(gManager);
                eManager.enemies.Add(boss2);
                gManager.boss = boss2;
                break;
            case 16:
                eManager.enemies.Add(new BronzeMedium(gManager, new SFPoint((sfloat)(!leftSide ? 60 : 180), (sfloat)(328))));
                eManager.enemies.Add(new BronzeMedium(gManager, new SFPoint((sfloat)(!leftSide ? 180 : 60), (sfloat)(-8))));
                break;
            case 19:
                eManager.enemies.Add(new PurpleMedium(gManager, new SFPoint(player.GetSFPosition().x * (sfloat)(-1) + (sfloat)240, (sfloat)(-8)), 1));
                break;
            case 20:
                eManager.enemies.Add(new Sine(gManager, player, new SFPoint((sfloat)20, (sfloat)(-8)), true));
                eManager.enemies.Add(new Sine(gManager, player, new SFPoint((sfloat)220, (sfloat)(-8)), true));
                break;
            case 21:
            case 22:
                for (int i = 40; i < 240; i += 40) eManager.enemies.Add(new Aimed(gManager, player, new SFPoint((sfloat)i, (sfloat)(-8))));
                for (int i = 40; i < 240; i += 40) eManager.enemies.Add(new Aimed(gManager, player, new SFPoint((sfloat)i, (sfloat)(328))));
                for (int i = 40; i < 320; i += 40) eManager.enemies.Add(new Aimed(gManager, player, new SFPoint((sfloat)(-8), (sfloat)i)));
                for (int i = 40; i < 320; i += 40) eManager.enemies.Add(new Aimed(gManager, player, new SFPoint((sfloat)248, (sfloat)i)));
                break;
        }
    }

    public override void _Ready()
    {
        gManager = GetNode<GameManager>("/root/GameManager");
        eManager = GetNode<EnemyManager>("/root/Main/EnemyManager");
    }


    public override void _PhysicsProcess(float delta)
    {
        if (gManager.framesLeft > 1800)
        {
            spawnTimer--;
            if (eManager.enemies.Count == 0 && spawnCount == 0)
            {
                currentWave++;
                if (currentWave >= 30)
                {
                    currentWave %= 30;
                    gManager.loop++;
                }
                SetCount();
            }
            else if (spawnCount > 0 && spawnTimer <= 0)
            {
                Spawn();
                spawnCount--;
                spawnTimer = spawnDelay;
            }
        }
        else if (eManager.enemies.Count == 0 && !bossSpawned)
        {
            BossGreatBummer boss = new BossGreatBummer(gManager);
            eManager.enemies.Add(boss);
            gManager.boss = boss;
            bossSpawned = true;
        }
    }
}
