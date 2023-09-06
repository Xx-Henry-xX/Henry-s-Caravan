using Godot;
using System;

public class AudioManager : Node
{
    [Export]
    public Godot.Collections.Dictionary<String, AudioStream> audios;

    private AudioStreamPlayer[] channels;
    private int[] currentPriority = { 0, 0, 0, 0 };

    public override void _Ready()
    {
        channels = new AudioStreamPlayer[] {
            GetChild<AudioStreamPlayer>(0),
            GetChild<AudioStreamPlayer>(1),
            GetChild<AudioStreamPlayer>(2),
            GetChild<AudioStreamPlayer>(3)
        };
    }

    public void StopAll()
    {
        foreach (AudioStreamPlayer player in channels)
        {
            player.Stop();
        }
    }

    public void Stop(int channel)
    {
        channels[channel].Stop();
    }

    public void Play(String name, int channel, int priority = 0)
    {
        if (!audios.ContainsKey(name)) return;
        if (channels[channel].Playing && currentPriority[channel] > priority) return;
        currentPriority[channel] = priority;
        //channels[channel].Stop();
        channels[channel].Stream = audios[name];
        channels[channel].Play();
    }
}
