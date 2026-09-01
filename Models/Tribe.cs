using System.Collections.Generic;

namespace TribeGameUI.Models;

public class Tribe
{
    public int Food { get; set; }
    public int Wood { get; set; }
    public int People { get; set; }
    public int CampLevel { get; set; }
    public int Morale { get; set; } = 100; // 100% морали на старте
    
    public Dictionary<string, int> Trust { get; private set; } = new();
    public HashSet<string> Flags { get; private set; } = new();

    public Tribe(int startFood, int startWood, int startPeople)
    {
        Food = startFood;
        Wood = startWood;
        People = startPeople;
        CampLevel = 0;
    }

    public void ApplyChanges(int food, int wood, int people, string newFlags = null)
    {
        Food += food;
        Wood += wood;
        People += people;
        
        // Разбираем входящие флаги по запятой
        if (!string.IsNullOrEmpty(newFlags))
        {
            var flagList = newFlags.Split(',');
            foreach (var flag in flagList)
            {
                Flags.Add(flag.Trim()); // Убираем лишние пробелы
            }
        }
    }

    public bool IsDead()
    {
        return People <= 0;
    }
}