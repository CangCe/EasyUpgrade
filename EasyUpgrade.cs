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
            //int magnification = this.Config.magnification;
            //KeybindList toggleKey = this.Config.ToggleKey;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            //意思是将 OnButtonPressed 方法绑定到 SMAPI 的 ButtonPressed 按钮按下事件
            //this 表示本对象，也就是当前的 ModEntry 类
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        }

        /*********
        ** 私有方法
        *********/
        /// <summary>在玩家按下键盘、控制器或鼠标上的按钮后引发</summary>
        /// <param name="sender">对象 sender 表示调用此方法的对象</param>
        /// <param name="e">对象 e 表示事件数据</param>

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
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "提升星级所需金钱倍率",
                tooltip: () => "提升星级所需金钱倍率",
                getValue: () => this.Config.magnification,
                setValue: value => this.Config.magnification = value
                );
            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => "提升星级热键",
                tooltip: () => "提升星级热键",
                getValue: () => this.Config.ToggleKey,
                setValue: value => this.Config.ToggleKey = value
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
                this.Monitor.Log($"玩家[{Game1.player.Name}]持有着 [{Game1.player.CurrentItem.Name}]共[{Game1.player.CurrentItem.Stack}]个,品质为[{Game1.player.CurrentItem.Quality}].", LogLevel.Debug);
                Item nowItem = Game1.player.CurrentItem;
                //判断物品是否是蔬菜、水果、花卉、鱼类、蛋类、奶类、采集品

                if (this.CanUpgradeItem(nowItem))
                {
                    this.DeductMoneyForUpgrade(nowItem);
                }

            }
            // 向控制台输出按下了什么按钮
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
                    return false;
            }
            else
                return false;
        }

        private void UpgradeItemQuality(Item item)
        {
            // 提升物品星级
            if (item is StardewValley.Object)
            {
                StardewValley.Object obj = (StardewValley.Object)item;
                if(obj.Quality != 2)
                    obj.Quality++;
                else
                    obj.Quality = 4;

            }   
        }

        private void DeductMoneyForUpgrade(Item item)
        {
            // 计算升级所需的金钱
            int upgradeCost = this.CalculateUpgradeCost(item);
            this.Monitor.Log($"需扣除{upgradeCost}G", LogLevel.Info);

            // 检查玩家是否有足够的金钱
            if (Game1.player.Money >= upgradeCost)
            {
                // 扣除金钱
                Game1.player.Money -= upgradeCost;
                this.UpgradeItemQuality(item);
                //this.Monitor.Log($"{item.Name} 的星级已提升，扣除了 {upgradeCost} 金钱。", LogLevel.Info);
            }
            //else this.Monitor.Log("金钱不足，无法提升星级。", LogLevel.Info);
        }

        private int CalculateUpgradeCost(Item item)
        {
            // 这里可以根据物品的属性计算升级所需的金钱
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
                //this.Monitor.Log($"目前星级倍率为{coe}", LogLevel.Info);

                return (int)(price * coe ) * obj.Stack * this.Config.magnification;

            }
            else
            {
                return 0;
            }
        }
    }

}


