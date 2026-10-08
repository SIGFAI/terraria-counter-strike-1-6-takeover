using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace Sigf.Content;

/// <summary>The classic amber Counter-Strike HUD: health, ammo, money, radar, round timer, kill feed.</summary>
public class CsHud : ModSystem
{
    public static int Ammo = 30, Money = 800, Kills, RoundsWon, Round = 1;
    public static ulong BombUntil, FlashUntil;
    public static double RoundEnds;
    static readonly Color Amber = new Color(255, 176, 0);

    public static void Kill(string victim, bool head)
    {
        Kills++; Money += 300;
        string weapon = Main.LocalPlayer.HeldItem.type == ModContent.ItemType<Ak47>() ? "AK-47" : "grenade";
        if (head) Mix.Popup(Main.LocalPlayer.Top - new Vector2(0, 60), "HEADSHOT!", Color.Red);
        Mix.Popup(Main.LocalPlayer.Top - new Vector2(0, 30), "+$300", new Color(120, 255, 90));
    }

    // put the ground in the lower third so the world fills the view
    public override void ModifyScreenPosition()
    {
        if (!Main.gameMenu) Main.screenPosition.Y -= Main.screenHeight * 0.27f;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int i = layers.FindIndex(l => l.Name.Equals("Vanilla: Mouse Text"));
        if (i < 0) i = layers.Count;
        layers.Insert(i, new LegacyGameInterfaceLayer("Sigf: CS HUD", () => { Draw(); return true; }, InterfaceScaleType.None));
    }

    static float K = 1f;   // logical-to-screen scale so the HUD stays readable on any window size

    static void Rect(Rectangle r, Color c) =>
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(r.X * K), (int)(r.Y * K), (int)(r.Width * K), (int)(r.Height * K)), new Rectangle(0, 0, 1, 1), c);

    static void Text(string s, Vector2 pos, Color c, float scale = 1f, float ax = 0f)
    {
        var f = FontAssets.MouseText.Value;
        var size = f.MeasureString(s) * scale;
        Utils.DrawBorderString(Main.spriteBatch, s, (pos - new Vector2(size.X * ax, 0)) * K, c, scale * K);
    }

    static void Draw()
    {
        if (Main.gameMenu) return;
        var p = Main.LocalPlayer;
        K = Main.screenWidth / 960f * 1.25f;
        float W = Main.screenWidth / K, H = Main.screenHeight / K;

        if (FlashUntil > Main.GameUpdateCount)
            Rect(new Rectangle(0, 0, (int)W + 2, (int)H + 2), Color.White * ((FlashUntil - Main.GameUpdateCount) / 30f));

        // bottom bar: health and armor, ammo
        Rect(new Rectangle(14, (int)H - 66, 250, 48), new Color(0, 0, 0, 120));
        Text("+", new Vector2(24, H - 64), Amber, 2f);
        Text("100", new Vector2(56, H - 64), Amber, 1.9f);
        Text("[]", new Vector2(138, H - 62), Amber, 1.7f);
        Text("100", new Vector2(170, H - 64), Amber, 1.9f);

        string weapon = p.HeldItem.type == ModContent.ItemType<Ak47>() ? "AK-47" : p.HeldItem.Name;
        Rect(new Rectangle((int)W - 250, (int)H - 66, 236, 48), new Color(0, 0, 0, 120));
        Text(weapon, new Vector2(W - 242, H - 56), Amber, 0.9f);
        Text($"{Ammo} | 90", new Vector2(W - 24, H - 64), Amber, 1.9f, 1f);

        // money, top right
        Text($"$ {Money}", new Vector2(W - 24, H - 110), Amber, 1.8f, 1f);

        // round timer and score, top centre
        if (BombUntil <= Main.GameUpdateCount) {
        Rect(new Rectangle((int)(W * 0.6f) - 110, 8, 220, 52), new Color(0, 0, 0, 130));
        double left = Math.Max(0, 105 - (Main.GameUpdateCount / 60.0) % 105);
        string timer = BombUntil > Main.GameUpdateCount
            ? $"C4  0:{(int)((BombUntil - Main.GameUpdateCount) / 60):00}"
            : $"{(int)left / 60}:{(int)left % 60:00}";
        Text(timer, new Vector2(W * 0.6f, 8), BombUntil > Main.GameUpdateCount ? Color.Red : Amber, 1.8f, 0.5f);
        Text($"CT {RoundsWon}   Round {Round}   T {Math.Max(0, Round - 1 - RoundsWon)}", new Vector2(W * 0.6f, 40), Color.White, 0.8f, 0.5f);

        }
        if (BombUntil > Main.GameUpdateCount)
        {
            int sec = (int)((BombUntil - Main.GameUpdateCount) / 60) + 1;
            Rect(new Rectangle((int)(W / 2) - 150, (int)(H * 0.1f), 300, 70), new Color(0, 0, 0, 150));
            Utils.DrawBorderStringBig(Main.spriteBatch, "C4  0:" + sec.ToString("00"), new Vector2(W / 2, H * 0.1f + 35) * K, Main.GameUpdateCount % 30 < 15 ? Color.Red : Color.White, 1.2f * K, 0.5f, 0.5f);
        }

        // radar, top left
        var radar = new Rectangle(14, 70, 130, 130);
        Rect(radar, new Color(0, 40, 0, 150));
        Rect(new Rectangle(radar.X, radar.Y + 64, 130, 2), new Color(0, 120, 0, 120));
        Rect(new Rectangle(radar.X + 64, radar.Y, 2, 130), new Color(0, 120, 0, 120));
        Vector2 c = new Vector2(radar.Center.X, radar.Center.Y);
        foreach (var n in Main.npc)
        {
            if (!n.active || !n.CanBeChasedBy()) continue;
            Vector2 d = (n.Center - p.Center) / 12f;
            if (Math.Abs(d.X) > 60 || Math.Abs(d.Y) > 60) continue;
            bool t = n.ModNPC is Terrorist;
            bool ct = n.ModNPC is CounterTerrorist;
            Rect(new Rectangle((int)(c.X + d.X) - 3, (int)(c.Y + d.Y) - 3, 6, 6), t ? Color.Red : ct ? new Color(80, 140, 255) : Color.Orange);
        }
        Rect(new Rectangle((int)c.X - 3, (int)c.Y - 3, 7, 7), new Color(90, 255, 90));
        foreach (var pr in Main.projectile)
            if (pr.active && pr.ModProjectile is C4Planted)
            {
                Vector2 d = (pr.Center - p.Center) / 12f;
                if (Math.Abs(d.X) < 62 && Math.Abs(d.Y) < 62 && Main.GameUpdateCount % 30 < 15)
                    Rect(new Rectangle((int)(c.X + d.X) - 4, (int)(c.Y + d.Y) - 4, 8, 8), Color.Red);
            }
    }
}
