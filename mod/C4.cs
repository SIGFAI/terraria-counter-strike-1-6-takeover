using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class C4 : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 24; Item.height = 32;
        Item.damage = 0; Item.useTime = 30; Item.useAnimation = 30; Item.useStyle = ItemUseStyleID.Thrust;
        Item.noMelee = true; Item.consumable = false; Item.maxStack = 1;
        Item.shoot = ModContent.ProjectileType<C4Planted>(); Item.shootSpeed = 0f;
        Item.rare = ItemRarityID.Red;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;
}
