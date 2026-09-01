namespace TribeGameUI.Models;

public class Choice
{
    public string Text { get; set; }
    public string NextNodeId { get; set; }
    public string LoadFile { get; set; } 

    public int FoodDelta { get; set; }
    public int WoodDelta { get; set; }
    public int PeopleDelta { get; set; }
    
    public string RequiredFlag { get; set; } 
    public string AddFlag { get; set; } 
    public string HideIfFlag { get; set; } 
    
    public string SfxPath { get; set; } 
    
    // --- УВЕДОМЛЕНИЕ О ДОВЕРИИ ---
    public string TrustNotification { get; set; } 

    public int MoraleDelta { get; set; }
    
    public bool IsEndOfDay { get; set; } // Если true, игра спишет еду и дрова
}