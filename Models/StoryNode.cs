using System.Collections.Generic;

namespace TribeGameUI.Models;

public class StoryNode
{
    public string Id { get; set; } 
    public string SpeakerName { get; set; } 
    public string Text { get; set; }
    public string Question { get; set; } 

    public string ImagePath { get; set; } 
    public string CharacterImage { get; set; } 
    
    public string BgmPath { get; set; } 
    public string AmbientPath { get; set; } 
    public string MusicPath { get; set; } 
    public string VoiceSfx { get; set; } 
    
    // --- СИСТЕМА ТАЙМЕРА ---
    public int? TimerSeconds { get; set; } 
    public string TimeoutNodeId { get; set; } 

    // --- СИСТЕМА ТРЯСКИ ---

    public bool Shake { get; set; }
    
    public List<Choice> Choices { get; set; } = new List<Choice>();
}