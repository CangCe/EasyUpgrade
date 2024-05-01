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
        public int magnification { get; set; } = 5;
        //设置触发热键
        public KeybindList ToggleKey { get; set; } = KeybindList.Parse("J");

        public bool UpgradeWhenPicked { get; set; } = false;

        public bool DeductMoneyWhenPicked { get; set; } = false;
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
            // 这个事件会在玩家背包发生任何变化时触发
            // e.Added 是一个包含所有新添加物品的列表
            // e.Removed 是一个包含所有移除物品的列表

            // 遍历新添加的物品
            foreach (Item item in e.Added)
            {
                // 这里可以检查物品是否是您希望改变品质的物品
                // 如果是，将物品的品质设置为铱星品质
                if (item is StardewValley.Object)
                {
                    //当物品品质不为4时，升级为铱星

                    if (item.Quality != 4 && this.Config.UpgradeWhenPicked && this.CanUpgradeItem(item))
                    {
                        StardewValley.Object obj = (StardewValley.Object)item;

                        int upgradeCost = 2 * obj.Stack * obj.Price * this.Config.magnification;
                        if (Game1.player.Money >= upgradeCost)
                        {
                            if (this.Config.DeductMoneyWhenPicked)
                                Game1.player.Money -= upgradeCost;
                            item.Quality = 4;

                        }
                        else;
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

            // add some config options
            configMenu.AddSectionTitle(
                mod: this.ModManifest, 
                text :() => "EasyUpgrade"
                );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Penalty coefficient",
                tooltip: () => "Penalty coefficient",
                getValue: () => this.Config.magnification,
                setValue: value => this.Config.magnification = value
                );
            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => "Toggle Key",
                tooltip: () => "Toggle Key",
                getValue: () => this.Config.ToggleKey,
                setValue: value => this.Config.ToggleKey = value
                );
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "EasyUpgrade"
                );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Auto Upgrade When Pick",
                tooltip: () => "Auto Upgrade When Pick",
                getValue: () => this.Config.UpgradeWhenPicked,
                setValue: value => this.Config.UpgradeWhenPicked = value
                );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Deduct Money When Auto Upgrade",
                tooltip: () => "Deduct Money When Auto Upgrade",
                getValue: () => this.Config.DeductMoneyWhenPicked,
                setValue: value => this.Config.DeductMoneyWhenPicked = value
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
                Game1.addHUDMessage(new HUDMessage("提升星级", 2));
                Item nowItem = Game1.player.CurrentItem;

                if (this.CanUpgradeItem(nowItem))
                {
                    this.DeductMoneyForUpgrade(nowItem);
                }

            }
        }
        private bool CanUpgradeItem(Item item)
        {

            if(item is StardewValley.Object)
            {
                StardewValley.Object obj = (StardewValley.Object)item;
                if (obj.Category == StardewValley.Object.VegetableCategory 
                    | obj.Category == StardewValley.Object.FruitsCategory 
                    | obj.Category == StardewValley.Object.flowersCategory 
                    | obj.Category == StardewValley.Object.FishCategory 
                    | obj.Category == StardewValley.Object.EggCategory 
                    | obj.Category == StardewValley.Object.MilkCategory 
                    | obj.Category == StardewValley.Object.GreensCategory 
                    | obj.Category == StardewValley.Object.GreensCategory
                    | obj.Category == StardewValley.Object.artisanGoodsCategory)
                    return obj.Quality != 4;
                else
                {
                    Game1.addHUDMessage(new HUDMessage("该物品不可升级", 2));
                    return false;
                }
            }
            else
            {
                Game1.addHUDMessage(new HUDMessage("该物品不可升级", 2));
                return false;
            }
        }

        private void UpgradeItemQuality(Item item)
        {
            // 提升物品星级
            if (item is StardewValley.Object)
            {
                StardewValley.Object obj = (StardewValley.Object)item;
                if(obj.Quality != 2){
                    obj.Quality++;}
                else
                    obj.Quality = 4;
                Game1.addHUDMessage(new HUDMessage($"成功提升物品星级为{obj.Quality}", 1));
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
                Game1.addHUDMessage(new HUDMessage($"共花费{upgradeCost}G", 1));

            }
            else;
                Game1.addHUDMessage(new HUDMessage($"余额不足，需{upgradeCost}G", 2));

        }

        private int CalculateUpgradeCost(Item item)
        {
            if (item is StardewValley.Object)
            {
                StardewValley.Object obj = (StardewValley.Object)item;
                int Q = obj.Quality+1;

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

                return (int)(price * coe ) * obj.Stack * this.Config.magnification;

            }
            else
            {
                return 0;
            }
        }
    }

}


