using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public abstract class Soldier : ModNPC
{
    int fire;
    protected abstract bool IsT { get; }

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 2;

    public override void SetDefaults()
    {
        NPC.width = 64; NPC.height = 160; NPC.scale = 1.65f;
        NPC.lifeMax = IsT ? 240 : 280; NPC.damage = 14; NPC.defense = 2; NPC.knockBackResist = 0.45f; NPC.value = 200;
        NPC.aiStyle = NPCAIStyleID.Fighter; AIType = NPCID.Zombie;
        NPC.HitSound = SoundID.NPCHit1; NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.ZoneForest ? 0.35f : 0f;

    public override void FindFrame(int frameHeight)
    {
        bool walking = System.Math.Abs(NPC.velocity.X) > 0.3f && NPC.velocity.Y == 0;
        if (walking) NPC.frameCounter++;
        NPC.frame.Y = (int)(NPC.frameCounter / 8 % 2) * frameHeight;
    }

    public override void AI()
    {
        var p = Main.player[NPC.target];
        if (!p.active || p.dead) return;
        float dist = Vector2.Distance(NPC.Center, p.Center);
        if (dist < 16 * 16 && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, p.position, p.width, p.height) && ++fire > (IsT ? 100 : 80))
        {
            fire = Main.rand.Next(0, 20);
            Vector2 muzzle = NPC.Center + new Vector2(NPC.direction * 80, -16);
            Mix.Shoot(ProjectileID.BulletDeadeye, muzzle, Mix.Aim(muzzle, p.Center, 9f), 9, 2f, true);
            Mix.Burst(muzzle, DustID.Torch, 5, 4f, 3f, null, 1.5f);
            Mix.Burst(muzzle, DustID.Smoke, 2, 4f, 1f, null, 1.1f);
            Mix.Sound("ak_shot", NPC.Center, 0.35f, 0.15f);
            Mix.Light(muzzle, new Color(255, 190, 80));
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        Mix.Burst(NPC.Center, DustID.Blood, 10, 14f, 4f, null, 1.4f);
        if (NPC.life <= 0)
        {
            Mix.Burst(NPC.Center, DustID.Blood, 40, 20f, 7f, null, 2f);
            Mix.Burst(NPC.Center, DustID.Smoke, 12, 20f, 2f, null, 1.5f);
            SigfMod.KillScored(); CsHud.Kill(IsT ? "Terrorist" : "Counter-Terrorist", hit.Crit);
        }
    }

    public override void OnKill()
    {
        Mix.Drop(ItemID.SilverCoin, NPC.Center, 3);
        if (Main.rand.NextBool(6)) Mix.Drop(ItemID.Grenade, NPC.Center, 2);
        if (Main.rand.NextBool(8)) Mix.Drop<Ak47>(NPC.Center);
        if (Main.rand.NextBool(12)) Mix.Drop<C4>(NPC.Center);
    }
}
