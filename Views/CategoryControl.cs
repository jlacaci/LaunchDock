using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LaunchDock.Helpers;
using LaunchDock.Models;
using Button = System.Windows.Controls.Button;
using UserControl = System.Windows.Controls.UserControl;
using TextBox = System.Windows.Controls.TextBox;
using DragEventArgs = System.Windows.DragEventArgs;
using DragDropEffects = System.Windows.DragDropEffects;
using DataFormats = System.Windows.DataFormats;
using DataObject = System.Windows.DataObject;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using FontFamily = System.Windows.Media.FontFamily;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Setter = System.Windows.Setter;
using Trigger = System.Windows.Trigger;
using ControlTemplate = System.Windows.Controls.ControlTemplate;
using FrameworkElementFactory = System.Windows.FrameworkElementFactory;
using TemplateBindingExtension = System.Windows.TemplateBindingExtension;

namespace LaunchDock.Views;

public class CategoryControl : UserControl
{
    public CategoryModel Model { get; }
    private readonly MainBarWindow _mainWindow;

    private Button _headerBtn = null!;
    private Popup _popup = null!;
    private Border _popupBorder = null!;
    private StackPanel _itemsPanel = null!;
    private TextBox? _renameBox;
    private bool _editMode = false;
    private string _orientation = "Horizontal";
    private DispatcherTimer _closeTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    private bool _hasOpenContextMenu = false;

    public CategoryControl(CategoryModel model, MainBarWindow mainWindow)
    {
        Model = model;
        _mainWindow = mainWindow;
        _orientation = ConfigManager.Config.Orientation ?? "Horizontal";
        Build();
        _closeTimer.Tick += (s, e) => 
        { 
            _closeTimer.Stop();
            // No cerrar si hay un menú contextual abierto
            if (!_hasOpenContextMenu)
            {
                _popup.IsOpen = false;
            }
        };
    }

    private void Build()
    {
        var root = new Grid();

        // ── Header Button ──
        _headerBtn = new Button
        {
            Style = (Style)Application.Current.Resources["CategoryButtonStyle"],
            Height = 42,
            MinWidth = 90,
        };
        UpdateHeaderContent();

        _headerBtn.MouseEnter += (s, e) => { _closeTimer.Stop(); OpenPopup(); };
        _headerBtn.MouseLeave += (s, e) => StartCloseTimer();

        // ── Popup ──
        _popup = new Popup
        {
            PlacementTarget = _headerBtn,
            Placement = _orientation == "Vertical" ? PlacementMode.Right : PlacementMode.Bottom,
            StaysOpen = true,
            AllowsTransparency = true,
            PopupAnimation = PopupAnimation.Fade,
        };

        var popupBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0xEE, 0x1E, 0x1E, 0x35)),
            CornerRadius = new CornerRadius(0, 0, 10, 10),
            MinWidth = 220,
            MaxHeight = 500,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 20,
                ShadowDepth = 6,
                Opacity = 0.7
            }
        };
        // Recortar el Border para que las esquinas redondeadas sean realmente transparentes
        popupBorder.Loaded += (s, e) => ClipBorderToCornerRadius(popupBorder);
        _popupBorder = popupBorder;

        popupBorder.MouseEnter += (s, e) => _closeTimer.Stop();
        popupBorder.MouseLeave += (s, e) => StartCloseTimer();

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };

        _itemsPanel = new StackPanel { Margin = new Thickness(6, 8, 6, 8) };
        scrollViewer.Content = _itemsPanel;

        // Drop zone
        _itemsPanel.AllowDrop = true;
        _itemsPanel.DragEnter += ItemsPanel_DragEnter;
        _itemsPanel.DragOver += ItemsPanel_DragOver;
        _itemsPanel.Drop += ItemsPanel_Drop;

        popupBorder.Child = scrollViewer;
        _popup.Child = popupBorder;

        root.Children.Add(_headerBtn);
        root.Children.Add(_popup);

        // Also allow drop on the header button
        _headerBtn.AllowDrop = true;
        _headerBtn.DragEnter += (s, e) => { OpenPopup(); e.Effects = DragDropEffects.Link; };
        _headerBtn.Drop += ItemsPanel_Drop;

        Content = root;
        BuildItems();
    }

    private void UpdateHeaderContent()
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        if (_editMode)
        {
            _renameBox = new TextBox
            {
                Text = Model.Name,
                Style = (Style)Application.Current.Resources["EditTextBoxStyle"],
                MinWidth = 40,
                MaxWidth = 80,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
            };
            _renameBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape) CommitRename();
            };
            _renameBox.LostFocus += (s, e) => CommitRename();
            panel.Children.Add(_renameBox);

            // Delete button
            var del = new Button
            {
                Content = "✕",
                Style = (Style)Application.Current.Resources["IconButtonStyle"],
                Width = 22, Height = 22, FontSize = 10,
                Margin = new Thickness(2, 0, 0, 0),
                ToolTip = "Eliminar categoría",
                Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(ConfigManager.Config.AccentColor))
            };
            del.Click += (s, e) =>
            {
                if (MessageBox.Show($"¿Eliminar la categoría «{Model.Name}»?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    _mainWindow.RemoveCategory(this, Model);
            };
            panel.Children.Add(del);

            // Move left/right
            var left = new Button { Content = "◀", Style = (Style)Application.Current.Resources["IconButtonStyle"], Width = 22, Height = 22, FontSize = 10, ToolTip = "Mover izquierda" };
            left.Click += (s, e) => _mainWindow.MoveCategoryUp(this);
            var right = new Button { Content = "▶", Style = (Style)Application.Current.Resources["IconButtonStyle"], Width = 22, Height = 22, FontSize = 10, ToolTip = "Mover derecha", Margin = new Thickness(1, 0, 0, 0) };
            right.Click += (s, e) => _mainWindow.MoveCategoryDown(this);
            panel.Children.Add(left);
            panel.Children.Add(right);
        }
        else
        {
            var cfg = ConfigManager.Config;
            var tb = new TextBlock
            {
                Text = Model.Name,
                Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(cfg.TextColor)),
                FontFamily = new FontFamily(cfg.FontFamily),
                FontSize = cfg.FontSize,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
            };
            panel.Children.Add(tb);
            var arrow = new TextBlock
            {
                Text = " ▾",
                Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(cfg.AccentColor)),
                FontSize = cfg.FontSize * 0.8,
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(arrow);
        }

        _headerBtn.Content = panel;
    }

    private void BuildItems()
    {
        _itemsPanel.Children.Clear();

        if (_editMode)
        {
            // Drop hint
            var hint = new TextBlock
            {
                Text = "Arrastra accesos directos aquí",
                Foreground = new SolidColorBrush(Color.FromArgb(0x88, 0x9A, 0x9A, 0xB0)),
                FontSize = 11,
                Margin = new Thickness(12, 4, 12, 8),
                FontStyle = FontStyles.Italic,
            };
            _itemsPanel.Children.Add(hint);
        }

        foreach (var sc in Model.Shortcuts)
        {
            _itemsPanel.Children.Add(BuildShortcutItem(sc));
        }

        if (_editMode)
        {
            var addBtn = new Button
            {
                Content = "＋  Añadir acceso directo",
                Style = (Style)Application.Current.Resources["ShortcutItemStyle"],
                Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(ConfigManager.Config.AccentColor)),
                FontSize = ConfigManager.Config.FontSize,
                FontFamily = new FontFamily(ConfigManager.Config.FontFamily),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            addBtn.Click += AddShortcut_Click;
            _itemsPanel.Children.Add(addBtn);
        }
    }

    private UIElement BuildShortcutItem(ShortcutModel sc)
    {
        // Usar IconPath si está disponible (PNG sin flecha), sino usar el icono del sistema
        var icon = !string.IsNullOrEmpty(sc.IconPath) && File.Exists(sc.IconPath)
            ? IconHelper.GetIconForPath(sc.IconPath, ConfigManager.Config.IconSize)
            : IconHelper.GetIconForPath(sc.TargetPath, ConfigManager.Config.IconSize);

        if (_editMode)
        {
            // Editable row
            var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            if (icon != null)
            {
                var img = new Image { Source = icon, Width = 20, Height = 20, Margin = new Thickness(4, 0, 6, 0) };
                Grid.SetColumn(img, 0);
                row.Children.Add(img);
            }

            // Name
            var tb = new TextBox
            {
                Text = sc.Name,
                Style = (Style)Application.Current.Resources["EditTextBoxStyle"],
                FontSize = 12,
                Margin = new Thickness(0, 0, 4, 0),
            };
            tb.TextChanged += (s, e) => sc.Name = tb.Text;
            Grid.SetColumn(tb, 1);
            row.Children.Add(tb);

            // Move Up
            var up = new Button { Content = "▲", Style = (Style)Application.Current.Resources["IconButtonStyle"], ToolTip = "Subir" };
            up.Click += (s, e) => MoveShortcut(sc, -1);
            Grid.SetColumn(up, 2);
            row.Children.Add(up);

            // Move Down
            var dn = new Button { Content = "▼", Style = (Style)Application.Current.Resources["IconButtonStyle"], ToolTip = "Bajar", Margin = new Thickness(2, 0, 2, 0) };
            dn.Click += (s, e) => MoveShortcut(sc, +1);
            Grid.SetColumn(dn, 3);
            row.Children.Add(dn);

            // Delete
            var del = new Button
            {
                Content = "✕",
                Style = (Style)Application.Current.Resources["IconButtonStyle"],
                Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(ConfigManager.Config.AccentColor)),
                ToolTip = "Eliminar"
            };
            del.Click += (s, e) =>
            {
                Model.Shortcuts.Remove(sc);
                BuildItems();
            };
            Grid.SetColumn(del, 4);
            row.Children.Add(del);

            // Drag handle
            row.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragDrop.DoDragDrop(row, new DataObject("ShortcutReorder", sc), DragDropEffects.Move);
            };

            return row;
        }
        else
        {
            // Normal clickable item
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            if (icon != null)
            {
                var size = ConfigManager.Config.IconSize;
                panel.Children.Add(new Image
                {
                    Source = icon,
                    Width = size,
                    Height = size,
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }

            panel.Children.Add(new TextBlock
            {
                Text = sc.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(ConfigManager.Config.TextColor)),
                FontFamily = new FontFamily(ConfigManager.Config.FontFamily),
                FontSize = ConfigManager.Config.FontSize,
            });

            var btn = new Button
            {
                Content = panel,
                Style = (Style)Application.Current.Resources["ShortcutItemStyle"],
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Cursor = System.Windows.Input.Cursors.Hand,
            };

            // Click izquierdo: Lanzar aplicación
            btn.Click += (s, e) => LaunchShortcut(sc);

            // ═══ MENÚ CONTEXTUAL (Clic Derecho) ═══
            var contextMenu = new ContextMenu
            {
                Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#18192B")),
                BorderBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#18192B")),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                HasDropShadow = true,
            };

            // Estilo para el ContextMenu completo
            var contextMenuStyle = new Style(typeof(ContextMenu));
            var contextMenuTemplate = new ControlTemplate(typeof(ContextMenu));

            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#18192B")));
            borderFactory.SetValue(Border.BorderBrushProperty, new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#444466")));
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(0));

            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.IsItemsHostProperty, true);
            stackPanelFactory.SetValue(StackPanel.BackgroundProperty, new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#18192B")));

            borderFactory.AppendChild(stackPanelFactory);
            contextMenuTemplate.VisualTree = borderFactory;
            contextMenuStyle.Setters.Add(new Setter(ContextMenu.TemplateProperty, contextMenuTemplate));

            contextMenu.Style = contextMenuStyle;

            // Estilo completo para los MenuItems con ControlTemplate personalizado
            var menuItemStyle = new Style(typeof(MenuItem));

            // Template personalizado para control total del diseño
            var template = new ControlTemplate(typeof(MenuItem));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.Name = "Border";
            factory.SetValue(Border.BackgroundProperty, new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#2A2A4A")));
            factory.SetValue(Border.BorderThicknessProperty, new Thickness(0));
            factory.SetValue(Border.PaddingProperty, new Thickness(12, 8, 12, 8));
            factory.SetValue(Border.MarginProperty, new Thickness(0));

            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(MenuItem.HeaderProperty));
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(contentFactory);

            template.VisualTree = factory;

            // Trigger para hover
            var hoverTrigger = new Trigger { Property = MenuItem.IsHighlightedProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E94560")), "Border"));
            template.Triggers.Add(hoverTrigger);

            menuItemStyle.Setters.Add(new Setter(MenuItem.TemplateProperty, template));
            menuItemStyle.Setters.Add(new Setter(MenuItem.ForegroundProperty, System.Windows.Media.Brushes.White));
            menuItemStyle.Setters.Add(new Setter(MenuItem.FontFamilyProperty, new FontFamily("Segoe UI")));
            menuItemStyle.Setters.Add(new Setter(MenuItem.FontSizeProperty, 13.0));
            menuItemStyle.Setters.Add(new Setter(MenuItem.MarginProperty, new Thickness(0)));
            menuItemStyle.Setters.Add(new Setter(MenuItem.PaddingProperty, new Thickness(0)));

            var deleteMenuItem = new MenuItem
            {
                Header = "🗑 Eliminar acceso directo",
                Style = menuItemStyle,
            };
            deleteMenuItem.Click += (s, e) =>
            {
                Model.Shortcuts.Remove(sc);
                BuildItems();
                ConfigManager.Save();
            };
            contextMenu.Items.Add(deleteMenuItem);

            var moveUpMenuItem = new MenuItem
            {
                Header = "⬆ Mover arriba",
                Style = menuItemStyle,
            };
            moveUpMenuItem.Click += (s, e) =>
            {
                MoveShortcut(sc, -1);
                ConfigManager.Save();
            };
            contextMenu.Items.Add(moveUpMenuItem);

            var moveDownMenuItem = new MenuItem
            {
                Header = "⬇ Mover abajo",
                Style = menuItemStyle,
            };
            moveDownMenuItem.Click += (s, e) =>
            {
                MoveShortcut(sc, +1);
                ConfigManager.Save();
            };
            contextMenu.Items.Add(moveDownMenuItem);

            // Controlar el estado del menú contextual
            contextMenu.Opened += (s, e) =>
            {
                _hasOpenContextMenu = true;
                _closeTimer.Stop(); // Detener el timer cuando se abre el menú
            };

            contextMenu.Closed += (s, e) =>
            {
                _hasOpenContextMenu = false;
                // Reiniciar el timer para cerrar el popup después de que se cierra el menú
                StartCloseTimer();
            };

            btn.ContextMenu = contextMenu;

            // ═══ DRAG & DROP PARA REORDENAR (Clic Izquierdo + Arrastrar) ═══
            bool isDragging = false;
            System.Windows.Point startPoint;

            btn.PreviewMouseLeftButtonDown += (s, e) =>
            {
                startPoint = e.GetPosition(null);
            };

            btn.PreviewMouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed && !isDragging)
                {
                    System.Windows.Point currentPosition = e.GetPosition(null);
                    var diff = startPoint - currentPosition;

                    // Si se movió más de 5 píxeles, iniciar drag
                    if (Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5)
                    {
                        isDragging = true;
                        DragDrop.DoDragDrop(btn, new DataObject("ShortcutReorder", sc), DragDropEffects.Move);
                        isDragging = false;
                    }
                }
            };

            // Permitir drop sobre el botón
            btn.AllowDrop = true;
            btn.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent("ShortcutReorder"))
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
                e.Handled = true;
            };

            btn.Drop += (s, e) =>
            {
                if (e.Data.GetDataPresent("ShortcutReorder"))
                {
                    var draggedShortcut = e.Data.GetData("ShortcutReorder") as ShortcutModel;
                    if (draggedShortcut != null && draggedShortcut != sc)
                    {
                        // Reordenar: mover draggedShortcut a la posición de sc
                        int oldIndex = Model.Shortcuts.IndexOf(draggedShortcut);
                        int newIndex = Model.Shortcuts.IndexOf(sc);

                        if (oldIndex != -1 && newIndex != -1)
                        {
                            Model.Shortcuts.RemoveAt(oldIndex);
                            Model.Shortcuts.Insert(newIndex, draggedShortcut);
                            BuildItems();
                            ConfigManager.Save();
                        }
                    }
                }
                e.Handled = true;
            };

            return btn;
        }
    }

    private void LaunchShortcut(ShortcutModel sc)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = sc.TargetPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo abrir: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        _popup.IsOpen = false;
    }

    private void MoveShortcut(ShortcutModel sc, int direction)
    {
        int idx = Model.Shortcuts.IndexOf(sc);
        int newIdx = idx + direction;
        if (newIdx < 0 || newIdx >= Model.Shortcuts.Count) return;
        Model.Shortcuts.RemoveAt(idx);
        Model.Shortcuts.Insert(newIdx, sc);
        BuildItems();
    }

    // ─── DRAG & DROP ──────────────────────────────────────────────────────────

    private void ItemsPanel_DragEnter(object sender, DragEventArgs e)
    {
        OpenPopup();
        e.Effects = CanAcceptDrop(e) ? DragDropEffects.Link : DragDropEffects.None;
        e.Handled = true;
    }

    private void ItemsPanel_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = CanAcceptDrop(e) ? DragDropEffects.Link : DragDropEffects.None;
        e.Handled = true;
    }

    private bool CanAcceptDrop(DragEventArgs e)
    {
        return e.Data.GetDataPresent(DataFormats.FileDrop) ||
               e.Data.GetDataPresent("ShortcutReorder");
    }

    private void ItemsPanel_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                AddFileAsShortcut(file);
            }
            BuildItems();
            if (!_editMode) OpenPopup();
        }
        else if (e.Data.GetDataPresent("ShortcutReorder"))
        {
            // Reorder within same list (handled via move buttons in edit mode)
        }
        e.Handled = true;
    }

    private void AddFileAsShortcut(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrWhiteSpace(name)) name = Path.GetFileName(path);

        // Guardar el icono como PNG sin flecha
        string? iconPngPath = IconCacheHelper.SaveIconAsPng(path, ConfigManager.Config.IconSize);

        // Resolve .lnk para obtener el ejecutable real
        if (Path.GetExtension(path).ToLower() == ".lnk")
        {
            string? resolved = IconHelper.ResolveShortcut(path);

            // Si resolved sigue siendo el .lnk original (app UWP sin TargetPath real),
            // copiar el .lnk a AppData para que sea independiente de su ubicación original
            string stablePath = resolved ?? path;
            if (string.Equals(stablePath, path, StringComparison.OrdinalIgnoreCase))
            {
                string shortcutsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "LaunchDock", "shortcuts");
                Directory.CreateDirectory(shortcutsDir);
                string destLnk = Path.Combine(shortcutsDir, Path.GetFileName(path));
                File.Copy(path, destLnk, overwrite: true);
                stablePath = destLnk;
            }

            var sc = new ShortcutModel
            {
                Name = name,
                TargetPath = stablePath,
                IconPath = iconPngPath
            };
            Model.Shortcuts.Add(sc);
        }
        else
        {
            var sc = new ShortcutModel 
            { 
                Name = name, 
                TargetPath = path,
                IconPath = iconPngPath  // Guardar ruta del PNG
            };
            Model.Shortcuts.Add(sc);
        }
    }

    private void AddShortcut_Click(object sender, RoutedEventArgs e)
    {
        // Usar WinForms OpenFileDialog con DereferenceLinks = false para evitar que el
        // diálogo intente resolver shortcuts UWP (p.ej. Clipchamp) y lance "Error catastrófico"
        var dlg = new System.Windows.Forms.OpenFileDialog
        {
            Title = "Seleccionar programa o archivo",
            Filter = "Todos los archivos|*.*|Ejecutables|*.exe|Accesos directos|*.lnk",
            Multiselect = true,
            DereferenceLinks = false,   // devuelve la ruta del .lnk tal cual, sin resolverlo
        };
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            foreach (var f in dlg.FileNames)
                AddFileAsShortcut(f);
            BuildItems();
        }
    }

    // ─── POPUP MANAGEMENT ─────────────────────────────────────────────────────

    private void OpenPopup()
    {
        _popup.IsOpen = true;
    }

    private void StartCloseTimer()
    {
        _closeTimer.Stop();
        _closeTimer.Start();
    }

    // ─── EDIT MODE ────────────────────────────────────────────────────────────

    public void SetEditMode(bool edit)
    {
        _editMode = edit;
        _popup.IsOpen = false;

        // Compactar o restaurar el ancho mínimo del tab según el modo
        _headerBtn.MinWidth = edit ? 60 : 90;
        _headerBtn.Padding = edit ? new Thickness(4, 0, 4, 0) : new Thickness(12, 0, 12, 0);

        UpdateHeaderContent();
        BuildItems();

        if (edit) OpenPopup();
    }

    public void StartRenaming()
    {
        Dispatcher.InvokeAsync(() =>
        {
            _renameBox?.Focus();
            _renameBox?.SelectAll();
        }, DispatcherPriority.Loaded);
    }

    private void CommitRename()
    {
        if (_renameBox != null)
            Model.Name = string.IsNullOrWhiteSpace(_renameBox.Text) ? "SIN NOMBRE" : _renameBox.Text.ToUpper();
        UpdateHeaderContent();
    }

    public void SetOrientation(string orientation)
    {
        _orientation = orientation;

        // Cambiar la posición del popup según la orientación
        if (orientation == "Vertical")
        {
            _popup.Placement = PlacementMode.Right;
            _popup.HorizontalOffset = 0;
            _popup.VerticalOffset = 0;
        }
        else
        {
            _popup.Placement = PlacementMode.Bottom;
            _popup.HorizontalOffset = 0;
            _popup.VerticalOffset = 0;
        }
    }

    public void ApplyCustomization(string accentColor, string textColor, string fontFamily, int fontSize)
    {
        try
        {
            // Aplicar fuente y tamaño al botón de cabecera
            var font = new FontFamily(fontFamily);
            _headerBtn.FontFamily = font;
            _headerBtn.FontSize = fontSize;

            // Aplicar color de texto al botón de cabecera
            var textBrush = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(textColor));
            _headerBtn.Foreground = textBrush;

            // Forzar actualización del contenido del header
            UpdateHeaderContent();

            // Aplicar color de fondo del popup
            var popupColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString(
                ConfigManager.Config.PopupMenuColor?.Length > 0 ? ConfigManager.Config.PopupMenuColor : "#EE1E1E35");
            _popupBorder.Background = new SolidColorBrush(popupColor);
            ClipBorderToCornerRadius(_popupBorder);

            // Reconstruir items para aplicar estilos a los elementos del menú
            BuildItems();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying customization: {ex.Message}");
        }
    }

    /// <summary>
    /// Recorta el Border con un RectangleGeometry que respeta el CornerRadius,
    /// para que las esquinas redondeadas sean realmente transparentes.
    /// </summary>
    private static void ClipBorderToCornerRadius(Border border)
    {
        if (border.ActualWidth <= 0 || border.ActualHeight <= 0) return;
        var cr = border.CornerRadius;
        var geo = new System.Windows.Media.StreamGeometry();
        using (var ctx = geo.Open())
        {
            double w = border.ActualWidth;
            double h = border.ActualHeight;
            double tl = cr.TopLeft, tr = cr.TopRight, br = cr.BottomRight, bl = cr.BottomLeft;
            ctx.BeginFigure(new System.Windows.Point(tl, 0), true, true);
            ctx.LineTo(new System.Windows.Point(w - tr, 0), true, false);
            ctx.ArcTo(new System.Windows.Point(w, tr), new System.Windows.Size(tr, tr), 0, false, System.Windows.Media.SweepDirection.Clockwise, true, false);
            ctx.LineTo(new System.Windows.Point(w, h - br), true, false);
            ctx.ArcTo(new System.Windows.Point(w - br, h), new System.Windows.Size(br, br), 0, false, System.Windows.Media.SweepDirection.Clockwise, true, false);
            ctx.LineTo(new System.Windows.Point(bl, h), true, false);
            ctx.ArcTo(new System.Windows.Point(0, h - bl), new System.Windows.Size(bl, bl), 0, false, System.Windows.Media.SweepDirection.Clockwise, true, false);
            ctx.LineTo(new System.Windows.Point(0, tl), true, false);
            ctx.ArcTo(new System.Windows.Point(tl, 0), new System.Windows.Size(tl, tl), 0, false, System.Windows.Media.SweepDirection.Clockwise, true, false);
        }
        geo.Freeze();
        border.Clip = geo;
    }
}
