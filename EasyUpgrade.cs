using System;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace EasyUpgrade
{
    /// <summary>模组入口点</summary>
    public sealed class ModConfig
    {
        //惩罚系数
        public int magnification { get; set; } = 5;
        //设置触发热键
        public KeybindList ToggleKey { get; set; } = KeybindList.Parse("J");
        //设置拾取物品时是否自动升级
        public bool UpgradeWhenPicked { get; set; } = false;
        //设置拾取物品时是否扣除金钱
        public bool DeductMoneyWhenPicked { get; set; } = false;
        //设置是否显示HUD信息
        public bool HUDMessage { get; set; } = false;
        //设置物品类别
        public bool Mineral { get; set; } = false; //矿石
        public bool Fish { get; set; } = false; //鱼类
        public bool AnimalProduct { get; set; } = false; //动物产品
        public bool Cooking { get; set; } = false; //烹饪
        public bool Resource { get; set; } = false; //资源
        public bool Fertilizer { get; set; } = false; //肥料
        public bool Trash { get; set; } = false; //垃圾
        public bool Bait { get; set; } = false; //鱼饵
        public bool FishingTackle { get; set; } = false; //鱼竿配件
        public bool ArtisanGoods { get; set; } = false; //工艺品
        public bool MonsterLoot { get; set; } = false; //怪物掉落
        public bool Seed { get; set; } = false; //种子
        public bool Vegetable { get; set; } = false; //蔬菜
        public bool Fruit { get; set; } = false; //水果
        public bool Flower { get; set; } = false; //花卉
        public bool Forage { get; set; } = false; //采集物

    }
    public class ModEntry : Mod
    {
        private ModConfig Config;
        /*********
        ** 公共方法
        *********/
        /// <summary>模组的入口点，在首次加载模组后自动调用</summary>
        /// <param name="helper">对象 helper 提供用于编写模组的简化接口</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        }

        /*********
        ** 私有方法
        *********/
        /// <summary>在玩家按下键盘、控制器或鼠标上的按钮后引发</summary>
        /// <param name="sender">对象 sender 表示调用此方法的对象</param>
        /// <param name="e">对象 e 表示事件数据</param>

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // 遍历新添加的物品
            foreach (Item item in e.Added)
            {
                // 在控制台输出调试信息
                // 这里可以检查物品是否是您希望改变品质的物品
                // 如果是，将物品的品质设置为铱星品质
                if (item is StardewValley.Object)
                {
                    //当物品品质不为4时，升级为铱星
                    if (item.Quality != 4 && this.Config.UpgradeWhenPicked && this.CanUpgradeItem(item))
                    {
                        StardewValley.Object obj = (StardewValley.Object)item;
                        int upgradeCost = 2 * obj.Stack * obj.Price * this.Config.magnification;
                        if (Game1.player.Money >= upgradeCost && this.Config.DeductMoneyWhenPicked)
                        {
                            Game1.player.Money -= upgradeCost;
                        }
                        Game1.player.removeItemFromInventory(item);
                        Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(obj.ItemId, obj.Stack, false, -1, 4));
                    }
                }
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
                );

            //Section 1
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Easy Upgrade"
                );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Penalty coefficient",
                tooltip: () => "The coefficient of money deducted for upgrading items.",
                getValue: () => this.Config.magnification,
                setValue: value => this.Config.magnification = value
                );
            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => "Toggle Key",
                tooltip: () => "The key to be pressed for an upgrade.",
                getValue: () => this.Config.ToggleKey,
                setValue: value => this.Config.ToggleKey = value
                );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "HUD Message",
                tooltip: () => "If enabled, there will be several message if you press \"ToggleKey\".",
                getValue: () => this.Config.HUDMessage,
                setValue: value => this.Config.HUDMessage = value
                );
            //Section 2
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Auto Upgrade"
                );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Auto Upgrade When Pick",
                tooltip: () => "If enabled, picked-up items will automatically upgrade.",
                getValue: () => this.Config.UpgradeWhenPicked,
                setValue: value => this.Config.UpgradeWhenPicked = value
                );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Deduct Money When Auto Upgrade",
                tooltip: () => "If enabled, picked-up items will deduct money.",
                getValue: () => this.Config.DeductMoneyWhenPicked,
                setValue: value => this.Config.DeductMoneyWhenPicked = value
                );
            //Section 3
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Category Options"
                );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Mineral",
                tooltip: () => "Enable or disable Mineral options",
                getValue: () => this.Config.Mineral,
                setValue: value => this.Config.Mineral = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fish",
                tooltip: () => "Enable or disable Fish options",
                getValue: () => this.Config.Fish,
                setValue: value => this.Config.Fish = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Animal Product",
                tooltip: () => "Enable or disable Animal Product options",
                getValue: () => this.Config.AnimalProduct,
                setValue: value => this.Config.AnimalProduct = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Cooking",
                tooltip: () => "Enable or disable Cooking options",
                getValue: () => this.Config.Cooking,
                setValue: value => this.Config.Cooking = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Resource",
                tooltip: () => "Enable or disable Resource options",
                getValue: () => this.Config.Resource,
                setValue: value => this.Config.Resource = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fertilizer",
                tooltip: () => "Enable or disable Fertilizer options",
                getValue: () => this.Config.Fertilizer,
                setValue: value => this.Config.Fertilizer = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Trash",
                tooltip: () => "Enable or disable Trash options",
                getValue: () => this.Config.Trash,
                setValue: value => this.Config.Trash = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Bait",
                tooltip: () => "Enable or disable Bait options",
                getValue: () => this.Config.Bait,
                setValue: value => this.Config.Bait = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fishing Tackle",
                tooltip: () => "Enable or disable Fishing Tackle options",
                getValue: () => this.Config.FishingTackle,
                setValue: value => this.Config.FishingTackle = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "ArtisanGoods",
                tooltip: () => "Enable or disable Artisan Goods options",
                getValue: () => this.Config.ArtisanGoods,
                setValue: value => this.Config.ArtisanGoods = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "MonsterLoot",
                tooltip: () => "Enable or disable Monster Loot options",
                getValue: () => this.Config.MonsterLoot,
                setValue: value => this.Config.MonsterLoot = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Seed",
                tooltip: () => "Enable or disable Seed options",
                getValue: () => this.Config.Seed,
                setValue: value => this.Config.Seed = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Vegetable",
                tooltip: () => "Enable or disable Vegetable options",
                getValue: () => this.Config.Vegetable,
                setValue: value => this.Config.Vegetable = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Fruit",
                tooltip: () => "Enable or disable Fruit options",
                getValue: () => this.Config.Fruit,
                setValue: value => this.Config.Fruit = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Flower",
                tooltip: () => "Enable or disable Flower options",
                getValue: () => this.Config.Flower,
                setValue: value => this.Config.Flower = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Forage",
                tooltip: () => "Enable or disable Forage options",
                getValue: () => this.Config.Forage,
                setValue: value => this.Config.Forage = value
                );

        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // 如果玩家还没有进入存档，则取消执行
            if (!Context.IsWorldReady)
                return;
            //读取热键
            if (this.Config.ToggleKey.JustPressed() && Game1.player != null && Game1.player.CurrentItem != null)
            {
                Item nowItem = Game1.player.CurrentItem;
                if (this.CanUpgradeItem(nowItem))
                {
                    this.DeductMoneyForUpgrade(nowItem);
                }
            }
        }
        private bool CanUpgradeItem(Item item)
        {
            if (item is StardewValley.Object && item.Quality < 4)
            {
                StardewValley.Object obj = (StardewValley.Object)item;
                int cat = obj.Category;
                if (this.Config.Mineral && (cat == -2 || cat == -12))
                    return obj.Quality != 4;
                else if (this.Config.Fish && (cat ==  -4))
                    return obj.Quality != 4;
                else if (this.Config.AnimalProduct && (cat == -5 || cat == -6 || cat == -18 ))
                    return obj.Quality != 4;
                else if (this.Config.Cooking && (cat == -7  || cat == -25 ))
                    return obj.Quality != 4;
                else if (this.Config.Resource && (cat == -15 || cat == -16))
                    return obj.Quality != 4;
                else if (this.Config.Fertilizer && (cat ==  -19 ))
                    return obj.Quality != 4;
                else if (this.Config.Trash && (cat == -20 ))
                    return obj.Quality != 4;
                else if (this.Config.Bait &&( cat == -21))
                    return obj.Quality != 4;
                else if (this.Config.FishingTackle && (cat == -22))
                    return obj.Quality != 4;
                else if (this.Config.ArtisanGoods && (cat == -26 || cat == -27))
                    return obj.Quality != 4;
                else if (this.Config.MonsterLoot && (cat == -28))
                    return obj.Quality != 4;
                else if (this.Config.Seed && (cat == -74))
                    return obj.Quality != 4;
                else if (this.Config.Vegetable && (cat == -75))
                    return obj.Quality != 4;
                else if (this.Config.Fruit && (cat == -79))
                    return obj.Quality != 4;
                else if (this.Config.Flower && (cat == -80))
                    return obj.Quality != 4;
                else if (this.Config.Forage && (cat == -81))
                    return obj.Quality != 4;
                else return false;
            }
            else
            {
                if (this.Config.HUDMessage)  Game1.addHUDMessage(new HUDMessage("This item cannot be upgraded.", 2));
                return false;
            }
        }

        private void UpgradeItemQuality(Item item)
        {
            // 提升物品星级
            if (item is StardewValley.Object)
            {
                StardewValley.Object obj = (StardewValley.Object)item;
                if (obj.Quality != 2 && obj.Quality < 4)
                {
                    obj.Quality++;
                }
                else if (obj.Quality == 2)
                {
                    obj.Quality = 4;
                }
                else
                {
                    if (this.Config.HUDMessage)  Game1.addHUDMessage(new HUDMessage("The item has achieved the highest quality.", 2));
                }

            }
        }

        private void DeductMoneyForUpgrade(Item item)
        {
            // 计算升级所需的金钱
            int upgradeCost = this.CalculateUpgradeCost(item);

            // 检查玩家是否有足够的金钱
            if (Game1.player.Money >= upgradeCost)
            {
                // 扣除金钱
                Game1.player.Money -= upgradeCost;
                this.UpgradeItemQuality(item);
                if (this.Config.HUDMessage)  Game1.addHUDMessage(new HUDMessage($"Successfully upgraded the item to {item.Quality}, at a cost of {upgradeCost}G.", 1));

            }
            else
                if (this.Config.HUDMessage)  Game1.addHUDMessage(new HUDMessage($"Insufficient balance, {upgradeCost}G required.", 2));

        }

        private int CalculateUpgradeCost(Item item)
        {
            if (item is StardewValley.Object)
            {
                StardewValley.Object obj = (StardewValley.Object)item;
                int Q = obj.Quality + 1;

                float coe;
                int price = obj.Price;
                Dictionary<int, float> coeLookup = new Dictionary<int, float>
                {
                    {1, 1.25f},
                    {2, 1.5f},
                    {3, 2.0f}
                };
                if
                    (coeLookup.TryGetValue(Q, out float tempCoe)) coe = tempCoe;
                else
                    coe = 1.0f;

                return (int)(price * coe) * obj.Stack * this.Config.magnification;

            }
            else
            {
                return 0;
            }
        }
        
        private void ShowHUDMessage(string message, int duration = 1)
        {
            if (this.Config.HUDMessage)
                Game1.addHUDMessage(new HUDMessage(message, duration));
        }
    }

}


