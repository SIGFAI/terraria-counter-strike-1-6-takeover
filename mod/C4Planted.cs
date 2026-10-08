using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class C4Planted : ModProjectile
{
    const int Fuse = 600;   // 10 s
    int beepAt;

    public override void SetDefaults()
    {
        Projectile.width = 30; Projectile.height = 40; Projectile.friendly = false; Projectile.hostile = false;
        Projectile.tileCollide = true; Projectile.timeLeft = Fuse; Projectile.penetrate = -1;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.velocity = new Vector2(0, 4f);
        CsHud.BombUntil = Main.GameUpdateCount + Fuse;
        Mix.Title("THE BOMB HAS BEEN PLANTED", "Clear the area!", 3);
        Mix.Say("The bomb has been planted.", new Color(255, 80, 60));
    }

    public override bool OnTileCollide(Vector2 oldVelocity) { Projectile.velocity = Vector2.Zero; return false; }

    public override void AI()
    {
        Projectile.velocity.X = 0;
        int left = Projectile.timeLeft;
        // beeps get faster as the timer runs out
        if (--beepAt <= 0)
        {
            beepAt = (int)MathHelper.Lerp(8, 60, left / (float)Fuse);
            Mix.Sound("c4_beep", Projectile.Center, 0.8f);
            Mix.Burst(Projectile.Top, DustID.RedTorch, 4, 6f, 2f, null, 1.5f);
            Mix.Popup(Projectile.Top - new Vector2(0, 8), (left / 60 + 1).ToString(), Color.Red);
        }
        Mix.Light(Projectile.Center, left % 20 < 10 ? Color.Red : new Color(90, 0, 0));
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // pulsing red glow behind the bomb
        var tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
        float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * (0.2f + 0.8f * (1f - Projectile.timeLeft / (float)Fuse)));
        Vector2 pos = Projectile.Center - Main.screenPosition;
        var px = Terraria.GameContent.TextureAssets.MagicPixel.Value;
        Main.spriteBatch.Draw(px, pos, new Rectangle(0, 0, 1, 1), Color.Red * (0.15f + 0.3f * pulse), 0f, new Vector2(0.5f), 130f * (1 + pulse * 0.3f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(tex, pos, null, Color.White, 0f, tex.Size() / 2f, 2.2f, SpriteEffects.None, 0f);
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        if (timeLeft > 0) return;
        Vector2 c = Projectile.Center;
        Mix.Sound("c4_boom", c, 1f);
        Mix.Sound(SoundID.Item14, c);
        Mix.Shake(22, 1.2);
        CsHud.FlashUntil = Main.GameUpdateCount + 30;
        for (int r = 0; r < 4; r++)
            for (int k = 0; k < 40; k++)
            {
                var d = Dust.NewDustPerfect(c, r % 2 == 0 ? DustID.Torch : DustID.Flare, Vector2.UnitX.RotatedBy(k / 40f * MathHelper.TwoPi) * (6 + r * 4), 0, default, 4f);
                d.noGravity = true;
            }
        Mix.Burst(c, DustID.Torch, 90, 60f, 9f, null, 3.2f);
        Mix.Burst(c, DustID.Smoke, 50, 70f, 5f, null, 2.8f);
        Mix.Burst(c, DustID.Flare, 30, 40f, 8f, null, 2f);
        foreach (var n in Mix.NpcsNear(c, 14, Mix.IsHostile))
        {
            int dir = n.Center.X > c.X ? 1 : -1;
            n.SimpleStrikeNPC(220, dir, false, 12f, DamageClass.Generic);
            n.velocity += new Vector2(dir * 9f, -7f);
            n.AddBuff(BuffID.OnFire, 240);
        }
        for (int x = -7; x <= 7; x++)
            for (int y = -7; y <= 7; y++)
                if (x * x + y * y <= 40 && (x * x + y * y > 12 || Main.rand.NextBool(2)))
                    Mix.KillTile(c + new Vector2(x * 16, y * 16));
    }
}
