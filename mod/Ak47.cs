using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class Ak47 : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 44; Item.height = 14;
        Item.damage = 15; Item.DamageType = DamageClass.Ranged; Item.knockBack = 2.5f;
        Item.useTime = 8; Item.useAnimation = 8; Item.useStyle = ItemUseStyleID.Shoot;
        Item.autoReuse = true; Item.noMelee = true;
        Item.shoot = ProjectileID.BulletHighVelocity; Item.shootSpeed = 16f;
        Item.UseSound = Mix.Style("ak_shot", 0.55f);
        Item.rare = ItemRarityID.Orange; Item.value = Item.buyPrice(gold: 25);
    }

    public override Vector2? HoldoutOffset() => new Vector2(-6, 0);

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        position += Vector2.Normalize(velocity) * 34f;
        velocity = velocity.RotatedByRandom(0.05f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // muzzle flash: bright flare, sparks and a puff of smoke
        Mix.Burst(position, DustID.Torch, 6, 6f, 4f, null, 1.8f);
        Mix.Burst(position, DustID.YellowStarDust, 4, 4f, 5f, null, 1.1f);
        Mix.Burst(position, DustID.Smoke, 2, 6f, 1.5f, null, 1.2f);
        Mix.Light(position, new Color(255, 190, 80));
        CsHud.Ammo = CsHud.Ammo <= 1 ? 30 : CsHud.Ammo - 1;
        return true;
    }
}
