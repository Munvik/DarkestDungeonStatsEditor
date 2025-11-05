using System;
using System.Collections.Generic;
using System.Linq;
using DarkestDungeonEditor.Models;

namespace DarkestDungeonEditor.Services
{
    public static class StatModifier
    {
        public static void BoostWeaponsRelative(List<Weapon> current, List<Weapon> original, double dmgPercent)
        {
            for (int i = 0; i < current.Count; i++)
            {
                current[i].DmgMin = (int)Math.Round(original[i].DmgMin * (1 + dmgPercent / 100.0));
                current[i].DmgMax = (int)Math.Round(original[i].DmgMax * (1 + dmgPercent / 100.0));
            }
        }

        public static void BoostArmoursRelative(List<Armour> current, List<Armour> original, double hpPercent)
        {
            for (int i = 0; i < current.Count; i++)
            {
                current[i].Hp = (int)Math.Round(original[i].Hp * (1 + hpPercent / 100.0));
            }
        }
    }
}
