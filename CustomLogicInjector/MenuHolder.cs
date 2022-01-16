using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;

namespace CustomLogicInjector
{
    public class MenuHolder
    {
        public static MenuHolder Instance { get; private set; }

        public MenuPage MainPage;
        public MenuPage SettingsPage;
        public SmallButton JumpButton;
        public MultiGridItemPanel Panel;
        public OrderedItemViewer SettingsViewer;

        public static void OnExitMenu()
        {
             Instance = null;
        }

        public static void ConstructMenu(MenuPage connectionsPage)
        {
            Instance ??= new();
            Instance.OnMenuConstruction(connectionsPage);
        }

        public void OnMenuConstruction(MenuPage connectionsPage)
        {
            MainPage = new("Custom Logic Injector Main Menu", connectionsPage);
            SettingsPage = new("Custom Logic Injector Settings Page", MainPage);
            JumpButton = new(connectionsPage, "Custom Logic Injection");
            JumpButton.AddHideAndShowEvent(MainPage);

            List<Subpage> subpages = new();
            List<SmallButton> pageButtons = new();
            foreach (var pack in CustomLogicInjectorMod.Packs) CreatePackSubpage(pack, subpages, pageButtons);
            Panel = new(MainPage, 5, 3, 60f, 650f, new(0, 300), pageButtons.ToArray());
            SettingsViewer = new(SettingsPage, subpages.ToArray());
        }

        public void CreatePackSubpage(LogicPack pack, List<Subpage> subpages, List<SmallButton> pageButtons)
        {
            Subpage page = new(SettingsPage, pack.Name);
            List<IMenuElement> pageElements = new();
            pageElements.Add(CreatePackToggle(SettingsPage, pack));
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
            pageButtons.Add(jump);
        }


        public ToggleButton CreatePackToggle(MenuPage page, LogicPack pack)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            if (pack == null) throw new ArgumentNullException(nameof(pack));

            ToggleButton button = new(page, "Enabled");
            button.ValueChanged += pack.Toggle;
            button.SetValue(CustomLogicInjectorMod.GS.ActivePacks.TryGetValue(pack.Name, out bool val) && val);
            return button;
        }

        public ToggleButton CreatePackSettingToggle(MenuPage page, LogicPack pack, LogicSetting setting)
        {
            ToggleButton button = new(page, setting.MenuName);
            button.ValueChanged += setting.Toggle;
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
