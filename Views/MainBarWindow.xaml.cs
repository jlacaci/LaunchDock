using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LaunchDock.Helpers;
using LaunchDock.Models;
using Orientation = System.Windows.Controls.Orientation;

namespace LaunchDock.Views;

public partial class MainBarWindow : Window
{
    public bool IsEditMode { get; private set; } = false;
    public bool IsPrimary { get; private set; } = true;

    private readonly List<CategoryControl> _categoryControls = new();
    private DispatcherTimer? _hideTimer;
    private bool _isHidden = false;
    private readonly DispatcherTimer _systemCloseTimer;
    private readonly ConfigManager _configManager;

    // Acceso cómodo a la config de esta barra
    private AppConfig Cfg => _configManager.InstanceConfig;

    public MainBarWindow() : this(ConfigManager.Instance, isPrimary: true) { }

    public MainBarWindow(ConfigManager configManager, bool isPrimary)
    {
        InitializeComponent();
        DataContext = this;
        _configManager = configManager;
        IsPrimary = isPrimary;
        _systemCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _systemCloseTimer.Tick += (s, e) => { _systemCloseTimer.Stop(); SystemPopup.IsOpen = false; };
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BuildCategories();
        ApplyConfig();
        SetupAutoHide();

        // Ocultar panel sistema y botón nueva barra en barras secundarias
        if (!IsPrimary)
        {
            if (SystemPanelGroup != null) SystemPanelGroup.Visibility = Visibility.Collapsed;
            if (AddBarBtn != null) AddBarBtn.Visibility = Visibility.Collapsed;
        }

        // Permitir scroll horizontal con la rueda del ratón sobre las categorías
        var scrollViewer = FindScrollViewer(CategoriesItemsControl);
        if (scrollViewer != null)
        {
            scrollViewer.PreviewMouseWheel += (s, ev) =>
            {
                scrollViewer.ScrollToHorizontalOffset(
                    scrollViewer.HorizontalOffset + (ev.Delta > 0 ? -40 : 40));
                ev.Handled = true;
            };
        }
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject element)
    {
        if (element is ScrollViewer sv) return sv;
        var parent = VisualTreeHelper.GetParent(element);
        return parent == null ? null : FindScrollViewer(parent);
    }

    // ─── LAYOUT & POSITIONING ─────────────────────────────────────────────────

    private void ApplyConfig()
    {
        var cfg = Cfg;

        // Apply orientation (forzar vertical si es Left o Right)
        var effectiveOrientation = (cfg.Position == "Left" || cfg.Position == "Right") 
            ? "Vertical" 
            : cfg.Orientation;
        ApplyOrientation(effectiveOrientation);

        // Apply customization (después de tener las categorías)
        ApplyCustomization();

        // Position the window
        PositionWindow(cfg.Position, cfg.FloatX, cfg.FloatY);

        // Apply corner radius based on position
        var cr = cfg.CornerRadiusValue;
        MainBorder.CornerRadius = cfg.Position switch
        {
            "Top"    => new CornerRadius(0, 0, cr, cr),
            "Bottom" => new CornerRadius(cr, cr, 0, 0),
            "Left"   => new CornerRadius(0, cr, cr, 0),
            "Right"  => new CornerRadius(cr, 0, 0, cr),
            _        => new CornerRadius(cr)
        };
    }

    private void ApplyCustomization()
    {
        var cfg = Cfg;

        // Aplicar color de fondo
        try
        {
            var bgColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(cfg.BackgroundColor);
            MainBorder.Background = new SolidColorBrush(bgColor);
        }
        catch { }

        // Aplicar opacidad
        this.Opacity = cfg.Opacity / 100.0;

        // Reemplazar el AccentBrush global para que {DynamicResource AccentBrush} en los
        // triggers de los estilos refleje el color de acento configurado por el usuario
        try
        {
            var accentColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                .ConvertFromString(cfg.AccentColor);
            System.Windows.Application.Current.Resources["AccentBrush"] =
                new SolidColorBrush(accentColor);
        }
        catch { }

        // Aplicar estilos a todos los controles de categoría
        foreach (var ctrl in _categoryControls)
        {
            ctrl.ApplyCustomization(cfg.AccentColor, cfg.TextColor, cfg.FontFamily, cfg.FontSize);
        }
    }

    private void ApplyOrientation(string orientation)
    {
        bool isVertical = orientation == "Vertical";

        if (isVertical)
        {
            var itemsPanelTemplate = new ItemsPanelTemplate();
            var factory = new FrameworkElementFactory(typeof(StackPanel));
            factory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);
            itemsPanelTemplate.VisualTree = factory;
            CategoriesItemsControl.ItemsPanel = itemsPanelTemplate;
            SizeToContent = SizeToContent.WidthAndHeight;
            Width = double.NaN;
            Height = double.NaN;
        }
        else
        {
            var itemsPanelTemplate = new ItemsPanelTemplate();
            var factory = new FrameworkElementFactory(typeof(StackPanel));
            factory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            itemsPanelTemplate.VisualTree = factory;
            CategoriesItemsControl.ItemsPanel = itemsPanelTemplate;
            Width = double.NaN;
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        // Mostrar botones en el panel correcto según orientación
        AddCategoryBtn.Visibility  = Visibility.Collapsed;
        ExitEditBtn.Visibility     = Visibility.Collapsed;
        AddCategoryBtnV.Visibility = Visibility.Collapsed;
        ExitEditBtnV.Visibility    = Visibility.Collapsed;

        if (isVertical)
        {
            EditModeBtn.Visibility           = Visibility.Collapsed;
            SettingsBtn.Visibility           = Visibility.Collapsed;
            AddBarBtn.Visibility             = Visibility.Collapsed;
            SystemPanelGroup.Visibility      = Visibility.Collapsed;
            VerticalActionButtons.Visibility = Visibility.Visible;
            EditModeBtnV.Visibility          = Visibility.Visible;
            SettingsBtnV.Visibility          = Visibility.Visible;
            AddBarBtnV.Visibility            = IsPrimary ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            VerticalActionButtons.Visibility = Visibility.Collapsed;
            EditModeBtn.Visibility           = Visibility.Visible;
            SettingsBtn.Visibility           = Visibility.Visible;
            AddBarBtn.Visibility             = IsPrimary ? Visibility.Visible : Visibility.Collapsed;
            SystemPanelGroup.Visibility      = IsPrimary ? Visibility.Visible : Visibility.Collapsed;
        }

        foreach (var ctrl in _categoryControls)
            ctrl.SetOrientation(orientation);

        // Forzar actualización del layout
        UpdateLayout();
        InvalidateMeasure();
        InvalidateArrange();
    }

    private void PositionWindow(string position, double floatX, double floatY)
    {
        var screen = System.Windows.SystemParameters.WorkArea;
        var orientation = Cfg.Orientation ?? "Horizontal";

        switch (position)
        {
            case "Top":
                Left = 0;
                Top = 0;
                if (orientation == "Horizontal")
                {
                    Width = screen.Width;
                    SizeToContent = SizeToContent.Height;
                }
                else
                {
                    Width = double.NaN; // Auto
                    SizeToContent = SizeToContent.WidthAndHeight;
                }
                break;
            case "Bottom":
                Left = 0;
                if (orientation == "Horizontal")
                {
                    Width = screen.Width;
                    SizeToContent = SizeToContent.Height;
                    Dispatcher.InvokeAsync(() =>
                    {
                        Top = screen.Bottom - ActualHeight;
                    }, DispatcherPriority.Loaded);
                }
                else
                {
                    Width = double.NaN; // Auto
                    SizeToContent = SizeToContent.WidthAndHeight;
                    Dispatcher.InvokeAsync(() =>
                    {
                        Top = screen.Bottom - ActualHeight;
                    }, DispatcherPriority.Loaded);
                }
                break;
            case "Left":
                // Siempre vertical para izquierda
                Left = 0;
                Top = 0;
                Height = screen.Height;
                SizeToContent = SizeToContent.Width;
                break;
            case "Right":
                // Siempre vertical para derecha
                Top = 0;
                Height = screen.Height;
                SizeToContent = SizeToContent.Width;
                Dispatcher.InvokeAsync(() =>
                {
                    Left = screen.Right - ActualWidth;
                }, DispatcherPriority.Loaded);
                break;
            case "Floating":
            default:
                Left = floatX;
                Top = floatY;
                SizeToContent = SizeToContent.WidthAndHeight;
                break;
        }
    }

    // ─── CATEGORY BUILDING ────────────────────────────────────────────────────

    private void BuildCategories()
    {
        CategoriesItemsControl.Items.Clear();
        _categoryControls.Clear();

        var cfg = Cfg;

        foreach (var cat in cfg.Categories)
        {
            var ctrl = new CategoryControl(cat, this);
            ctrl.ApplyCustomization(cfg.AccentColor, cfg.TextColor, cfg.FontFamily, cfg.FontSize);
            _categoryControls.Add(ctrl);
            CategoriesItemsControl.Items.Add(ctrl);
        }
    }

    // ─── EDIT MODE ────────────────────────────────────────────────────────────

    public void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
        OnPropertyChanged(nameof(IsEditMode));

        bool isVertical = Cfg.Orientation == "Vertical" ||
                          Cfg.Position == "Left" || Cfg.Position == "Right";

        // Botones panel horizontal
        AddCategoryBtn.Visibility  = (!isVertical && IsEditMode) ? Visibility.Visible : Visibility.Collapsed;
        ExitEditBtn.Visibility     = (!isVertical && IsEditMode) ? Visibility.Visible : Visibility.Collapsed;
        // Botones panel vertical
        AddCategoryBtnV.Visibility = (isVertical && IsEditMode)  ? Visibility.Visible : Visibility.Collapsed;
        ExitEditBtnV.Visibility    = (isVertical && IsEditMode)   ? Visibility.Visible : Visibility.Collapsed;

        foreach (var ctrl in _categoryControls)
            ctrl.SetEditMode(IsEditMode);

        if (IsEditMode)
        {
            ConstrainToCurrentMonitor();
        }
        else
        {
            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;
            ApplyConfig();
            _configManager.SaveInstance();
        }
    }

    /// <summary>
    /// Limita la ventana al área de trabajo del monitor en que se encuentra,
    /// para que en modo edición nunca se salga de pantalla.
    /// </summary>
    private void ConstrainToCurrentMonitor()
    {
        // Obtener el área de trabajo del monitor donde está la ventana
        var workArea = GetCurrentMonitorWorkArea();

        var cfg = Cfg;
        var position = cfg.Position ?? "Top";

        if (position == "Top" || position == "Bottom")
        {
            // En Top/Bottom la ventana ya ocupa el ancho completo del monitor.
            // Solo asegurar que MaxWidth esté limitado; el ScrollViewer absorbe el exceso.
            MaxWidth = workArea.Width;
        }
        else if (position == "Left" || position == "Right")
        {
            MaxHeight = workArea.Height;
            Top = workArea.Top;
            Height = workArea.Height;
            SizeToContent = SizeToContent.Width;
        }
        else // Floating
        {
            // Limitar al monitor actual y reposicionar si se sale por la derecha/abajo
            MaxWidth = workArea.Width;
            MaxHeight = workArea.Height;
            SizeToContent = SizeToContent.WidthAndHeight;

            Dispatcher.InvokeAsync(() =>
            {
                if (Left + ActualWidth > workArea.Right)
                    Left = workArea.Right - ActualWidth;
                if (Left < workArea.Left)
                    Left = workArea.Left;
                if (Top + ActualHeight > workArea.Bottom)
                    Top = workArea.Bottom - ActualHeight;
                if (Top < workArea.Top)
                    Top = workArea.Top;
            }, DispatcherPriority.Loaded);
        }
    }

    /// <summary>
    /// Devuelve el WorkArea del monitor en el que está actualmente la ventana.
    /// Usa WinForms Screen para soporte multi-monitor.
    /// </summary>
    private System.Windows.Rect GetCurrentMonitorWorkArea()
    {
        // Convertir posición WPF (DIPs) a píxeles para WinForms
        var source = PresentationSource.FromVisual(this);
        double dpiX = 1.0, dpiY = 1.0;
        if (source?.CompositionTarget != null)
        {
            dpiX = source.CompositionTarget.TransformToDevice.M11;
            dpiY = source.CompositionTarget.TransformToDevice.M22;
        }

        int cx = (int)((Left + ActualWidth / 2) * dpiX);
        int cy = (int)((Top + ActualHeight / 2) * dpiY);

        var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point(cx, cy));
        var wa = screen.WorkingArea;

        // Devolver en DIPs
        return new System.Windows.Rect(
            wa.Left   / dpiX,
            wa.Top    / dpiY,
            wa.Width  / dpiX,
            wa.Height / dpiY);
    }

    private void ExitEditMode_Click(object sender, RoutedEventArgs e)
    {
        if (IsEditMode)
        {
            ToggleEditMode();
        }
    }

    // ─── AUTO HIDE ────────────────────────────────────────────────────────────

    private void SetupAutoHide()
    {
        if (!Cfg.AutoHide) return;

        MouseEnter += (s, e) => ShowBar();
        MouseLeave += (s, e) => StartHideTimer();

        _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _hideTimer.Tick += (s, e) => { _hideTimer.Stop(); HideBar(); };

        Dispatcher.InvokeAsync(HideBar, DispatcherPriority.Loaded);
    }

    private void ShowBar()
    {
        _hideTimer?.Stop();
        if (!_isHidden) return;
        _isHidden = false;

        var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(180));
        var tt = (TranslateTransform)RenderTransform;
        tt.BeginAnimation(TranslateTransform.YProperty, anim);
        Opacity = 1;
    }

    private void HideBar()
    {
        if (_isHidden) return;
        _isHidden = true;

        double offset = Cfg.Position == "Top" ? -(ActualHeight - 4) : (ActualHeight - 4);

        if (RenderTransform is not TranslateTransform)
            RenderTransform = new TranslateTransform();

        var anim = new DoubleAnimation(offset, TimeSpan.FromMilliseconds(250));
        ((TranslateTransform)RenderTransform).BeginAnimation(TranslateTransform.YProperty, anim);
    }

    private void StartHideTimer()
    {
        _hideTimer?.Stop();
        _hideTimer?.Start();
    }

    // ─── DRAG ─────────────────────────────────────────────────────────────────

    private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
        Cfg.Position = "Floating";
        Cfg.FloatX = Left;
        Cfg.FloatY = Top;
    }

    // ─── EVENT HANDLERS ───────────────────────────────────────────────────────

    private void AddCategory_Click(object sender, RoutedEventArgs e)
    {
        var cfg = Cfg;
        var cat = new CategoryModel { Name = "NUEVA" };
        cfg.Categories.Add(cat);
        var ctrl = new CategoryControl(cat, this);
        ctrl.ApplyCustomization(cfg.AccentColor, cfg.TextColor, cfg.FontFamily, cfg.FontSize);
        _categoryControls.Add(ctrl);
        CategoriesItemsControl.Items.Add(ctrl);
        ctrl.SetEditMode(true);
        ctrl.StartRenaming();
    }

    private void SystemPanel_Click(object sender, RoutedEventArgs e)
        => SystemPopup.IsOpen = !SystemPopup.IsOpen;

    private void SystemPanelBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _systemCloseTimer.Stop();
        SystemPopup.IsOpen = true;
    }

    private void SystemPanelBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        => _systemCloseTimer.Start();

    private void SystemPopup_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        => _systemCloseTimer.Stop();

    private void SystemPopup_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        => _systemCloseTimer.Start();

    private void SystemItem_Click(object sender, RoutedEventArgs e)
    {
        SystemPopup.IsOpen = false;
        if (sender is not System.Windows.Controls.Button btn) return;
        string tag = btn.Tag?.ToString() ?? "";
        if (string.IsNullOrEmpty(tag)) return;

        try
        {
            if (tag == "run")
            {
                // Abrir diálogo Ejecutar via ShellExecute
                Process.Start(new ProcessStartInfo("rundll32.exe", "shell32.dll,#61") { UseShellExecute = true });
                return;
            }
            if (tag == "search")
            {
                Process.Start(new ProcessStartInfo("explorer.exe", "search-ms:") { UseShellExecute = true });
                return;
            }
            if (tag.StartsWith("shutdown") || tag.StartsWith("rundll32"))
            {
                // Comandos con argumentos
                var parts = tag.Split(' ', 2);
                var psi = new ProcessStartInfo(parts[0], parts.Length > 1 ? parts[1] : "")
                    { UseShellExecute = true };
                Process.Start(psi);
                return;
            }
            // Resto: control, ms-settings:, taskmgr, services.msc, cmd, powershell
            Process.Start(new ProcessStartInfo(tag) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"No se pudo abrir: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void EditMode_Click(object sender, RoutedEventArgs e)
        => ToggleEditMode();

    private void Settings_Click(object sender, RoutedEventArgs e)
        => OpenSettings();

    private void AddBar_Click(object sender, RoutedEventArgs e)
    {
        string barId = BarManager.AddBar();
        var mgr = new ConfigManager(barId);
        mgr.LoadInstance();
        // Desplazar la nueva barra 40px hacia abajo para que no tape la principal
        mgr.InstanceConfig.Position = "Floating";
        mgr.InstanceConfig.FloatX = Left + 40;
        mgr.InstanceConfig.FloatY = Top + 40;
        mgr.SaveInstance();
        var bar = new MainBarWindow(mgr, isPrimary: false);
        bar.Show();
    }

    public void CloseThisBar()
    {
        BarManager.RemoveBar(_configManager.BarId);
        Close();
    }

    public void OpenSettings()
    {
        var win = new SettingsWindow(_configManager);
        win.Owner = this;
        if (win.ShowDialog() == true)
        {
            ApplyConfig();
            SetupAutoHide();
        }
    }

    public void RemoveCategory(CategoryControl ctrl, CategoryModel model)
    {
        _categoryControls.Remove(ctrl);
        CategoriesItemsControl.Items.Remove(ctrl);
        Cfg.Categories.Remove(model);
    }

    public void MoveCategoryUp(CategoryControl ctrl)
    {
        int idx = _categoryControls.IndexOf(ctrl);
        if (idx <= 0) return;
        _categoryControls.RemoveAt(idx);
        _categoryControls.Insert(idx - 1, ctrl);
        CategoriesItemsControl.Items.Remove(ctrl);
        CategoriesItemsControl.Items.Insert(idx - 1, ctrl);
        Cfg.Categories.RemoveAt(idx);
        Cfg.Categories.Insert(idx - 1, ctrl.Model);
    }

    public void MoveCategoryDown(CategoryControl ctrl)
    {
        int idx = _categoryControls.IndexOf(ctrl);
        if (idx < 0 || idx >= _categoryControls.Count - 1) return;
        _categoryControls.RemoveAt(idx);
        _categoryControls.Insert(idx + 1, ctrl);
        CategoriesItemsControl.Items.Remove(ctrl);
        CategoriesItemsControl.Items.Insert(idx + 1, ctrl);
        Cfg.Categories.RemoveAt(idx);
        Cfg.Categories.Insert(idx + 1, ctrl.Model);
    }

    // INotifyPropertyChanged stub
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
}
