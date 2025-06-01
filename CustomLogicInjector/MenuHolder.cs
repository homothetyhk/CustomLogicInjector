using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using Modding;

namespace CustomLogicInjector
{
    public class MenuHolder
    {
        public static MenuHolder Instance { get; private set; }

        public MenuPage ConnectionsPage;
        public MenuPage MainPage;
        public MenuPage SettingsPage;
        public SmallButton JumpButton;
        public MultiGridItemPanel Panel;
        public OrderedItemViewer SettingsViewer;
        public Dictionary<string, ToggleButton> PackToggleLookup = new();
        public Dictionary<string, ToggleButton> SettingToggleLookup = new();
        public SmallButton? RestoreLocalPacks;

        public static void OnExitMenu()
        {
            Instance = null;
        }

        public static void ConstructMenu(MenuPage connectionsPage)
        {
            Instance ??= new();
            Instance.OnConstructMenuFirstTime(connectionsPage);
            Instance.OnMenuConstruction();
        }

        public void ReconstructMenu()
        {
            JumpButton.ClearOnClick();
            RestoreLocalPacks = null;
            UnityEngine.Object.Destroy(MainPage.self);
            UnityEngine.Object.Destroy(SettingsPage.self);
            OnMenuConstruction();
        }

        public void OnConstructMenuFirstTime(MenuPage connectionsPage)
        {
            ConnectionsPage = connectionsPage;
            JumpButton = new(ConnectionsPage, "Custom Logic Injection");
            connectionsPage.BeforeShow += () => JumpButton.Text.color = CustomLogicInjectorMod.GS.ActivePacks.Count != 0 ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
        }

        public void OnMenuConstruction()
        {
            MainPage = new("Custom Logic Injector Main Menu", ConnectionsPage);
            SettingsPage = new("Custom Logic Injector Settings Page", MainPage);
            JumpButton.AddHideAndShowEvent(MainPage);
            PackToggleLookup.Clear();
            SettingToggleLookup.Clear();
            List<Subpage> subpages = new();
            List<SmallButton> pageButtons = new();
            foreach (var pack in CustomLogicInjectorMod.Packs) CreatePackSubpage(pack, subpages, pageButtons);
            Panel = new(MainPage, 5, 3, 60f, 650f, new(0, 300), pageButtons.ToArray());
            SettingsViewer = new(SettingsPage, subpages.ToArray());
        }

        public void CreateRestoreLocalPacksButton()
        {
            RestoreLocalPacks = new SmallButton(MainPage, "Restore Local Packs");
            RestoreLocalPacks.OnClick += () =>
            {
                MainPage.Hide();
                CustomLogicInjectorMod.LoadFiles();
                ReconstructMenu();
                MainPage.Show();
            };

            RestoreLocalPacks.MoveTo(new(0f, -300f));
            RestoreLocalPacks.SymSetNeighbor(Neighbor.Up, Panel);
            RestoreLocalPacks.SymSetNeighbor(Neighbor.Down, MainPage.backButton);
        }

        public void ToggleAllOff()
        {
            foreach (ToggleButton b in PackToggleLookup.Values)
            {
                if (b.Value)
                {
                    b.SetValue(false);
                }
            }

            foreach (ToggleButton b in SettingToggleLookup.Values) if (b.Value) b.SetValue(false);
        }

        public void CreatePackSubpage(LogicPack pack, List<Subpage> subpages, List<SmallButton> pageButtons)
        {
            Subpage page = new(SettingsPage, pack.Name);
            List<IMenuElement> pageElements = new()
            {
                CreatePackToggle(SettingsPage, pack)
            };
            if (pack.Settings != null && pack.Settings.Count != 0)
            {
                pageElements.Add(new MenuLabel(SettingsPage, "Settings"));
                foreach (LogicSetting setting in pack.Settings)
                {
                    pageElements.Add(CreatePackSettingToggle(SettingsPage, pack, setting));
                }
            }
            VerticalItemPanel panel = new(SettingsPage, new(0f, 325f), 60f, false, pageElements.ToArray());
            page.Add(panel);
            subpages.Add(page);

            SmallButton jump = new(MainPage, pack.Name);
            jump.OnClick += () =>
            {
                MainPage.TransitionTo(SettingsPage);
                jump.Button.ForceDeselect();
                SettingsViewer.JumpTo(page);
                SettingsPage.nav.SelectDefault();
            };
            MainPage.BeforeShow += () => jump.Text.color = CustomLogicInjectorMod.GS.IsPackActive(pack.Name) ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;

            pageButtons.Add(jump);
        }


        public ToggleButton CreatePackToggle(MenuPage page, LogicPack pack)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            if (pack == null) throw new ArgumentNullException(nameof(pack));

            ToggleButton button = new(page, "Enabled");
            button.ValueChanged += pack.Toggle;
            button.SetValue(CustomLogicInjectorMod.GS.IsPackActive(pack.Name));
            PackToggleLookup.Add(pack.Name, button);
            return button;
        }

        public ToggleButton CreatePackSettingToggle(MenuPage page, LogicPack pack, LogicSetting setting)
        {
            ToggleButton button = new(page, setting.MenuName);
            button.ValueChanged += setting.Toggle;
            SettingToggleLookup.Add(setting.LogicName, button);
            return button;
        }
        

        public static bool TryGetMenuButton(MenuPage connectionsPage, out SmallButton button)
        {
            return Instance.TryGetJumpButton(connectionsPage, out button);
        }

        public bool TryGetJumpButton(MenuPage connectionsPage, out SmallButton button)
        {
            button = JumpButton;
            return true;
        }

    }
}
