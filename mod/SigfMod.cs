using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class SigfMod : ModSystem
{
    static int roundKills;

    static void StartRound()
    {
        roundKills = 0;
        Mix.Title("ROUND " + CsHud.Round, "Fire in the hole!", 3);
        Mix.Sound("fire_in_the_hole", Mix.Host.Center, 0.9f);
        CsHud.Ammo = 30;
    }

    public static void KillScored()
    {
        if (++roundKills < 8) return;
        roundKills = 0;
        CsHud.RoundsWon++; CsHud.Round++; CsHud.Money += 1000;
        Mix.Title("CTs WIN", "Counter-Terrorists win the round  +$1000", 3);
        Mix.Sound("ct_win", Mix.Host.Center, 0.9f);
    }

    // always several soldiers close to the player, in the middle of the screen
    static void Reinforce()
    {
        var host = Mix.Host;
        if (host == null || !host.active) return;
        int n = Main.npc.Count(x => x.active && (x.ModNPC is Terrorist || x.ModNPC is CounterTerrorist));
        for (; n < 5; n++)
        {
            int side = Main.rand.NextBool() ? 1 : -1;
            float d = Main.rand.Next(6, 10);
            var p = Mix.Ahead(side * d);
            if (Main.rand.NextBool()) Mix.Spawn<Terrorist>(p); else Mix.Spawn<CounterTerrorist>(p);
            Mix.Burst(p - new Vector2(0, 40), DustID.Smoke, 8);
        }
    }

    public override void OnWorldLoad()
    {
        Mix.Every(2, Reinforce);
        Mix.After(1, () => { Mix.Arm<Ak47>(); StartRound(); });

        // demo: the clip moments, in order
        Mix.Demo(1, () => Mix.Title("COUNTER-STRIKE 1.6", "Terraria takeover", 3));
        Mix.Demo(4, () =>
        {
            // a crate stack from the dust maps, next to the bomb site
            var b = Mix.Ahead(7);
            for (int x = 0; x < 3; x++) for (int y = 1; y <= 3 - x; y++) Mix.PlaceTile(b + new Vector2(x * 16, -y * 16 + 8), TileID.WoodBlock);
        });
        Mix.Demo(6, () =>
        {
            Mix.Give<C4>();
            var p = Mix.Ahead(5);
            Mix.Shoot<C4Planted>(p - new Vector2(0, 40), Vector2.Zero, 0);
            Mix.Sound("fire_in_the_hole", p, 0.9f);
        });
    }
}
