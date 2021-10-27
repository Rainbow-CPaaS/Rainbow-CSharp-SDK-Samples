using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContextMenu : ContentView
    {
        ContextMenuModel originalContextMenuModel = null;
        ContextMenuModel contextMenuModelUsed = new ContextMenuModel();

        Boolean iconUsed = false;
        Boolean descriptionUsed = false;
        String selectedItemId = null;

        ICommand SelectionCommand;

#region CommandProperty

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ContextMenu), null);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

#endregion CommandProperty


#region StoreSelectionProperty

        public static readonly BindableProperty StoreSelectionProperty =
            BindableProperty.Create(nameof(StoreSelection), typeof(Boolean), typeof(ContextMenu), true);

        public Boolean StoreSelection
        {
            get { return (Boolean)GetValue(StoreSelectionProperty); }
            set { SetValue(StoreSelectionProperty, value); }
        }

#endregion StoreSelectionProperty

        public ContextMenuItemModel GetSelectedItem()
        {
            if (StoreSelection)
                return contextMenuModelUsed?.GetSelectedItem();

            return null;
        }

        public String GetSelectedItemId()
        {
            if (StoreSelection)
                return contextMenuModelUsed?.GetSelectedItemId();

            return null;
        }

        public void SetSelectedItemId(String selectedItemId)
        {
            // Set selection in each models
            if (StoreSelection)
            {
                contextMenuModelUsed?.SetSelectedItem(selectedItemId);
                originalContextMenuModel?.SetSelectedItem(selectedItemId);
            }
        }

        public ContextMenu()
        {
            InitializeComponent();

            SelectionCommand = new RelayCommand<object>(SelectionCommandAction);
        }

        ~ContextMenu()
        {
            if (originalContextMenuModel != null)
            {
                originalContextMenuModel.ItemVisibilityChanged -= OriginalContextMenuModel_ItemVisibilityChanged;
                if (originalContextMenuModel.Items != null)
                    originalContextMenuModel.Items.CollectionChanged -= OriginalContextMenuModel_Items_CollectionChanged;
            }

            ListView.BindingContext = null;
            ListView.ItemTemplate = null;

            contextMenuModelUsed = null;
            originalContextMenuModel = null;
        }

        protected override void OnBindingContextChanged()
        {
            if(BindingContext is ContextMenuModel contextMenuModel)
            {
                // If ContextMenuModel has changed we need to clear some stuff
                if(originalContextMenuModel != contextMenuModel)
                {
                    if (originalContextMenuModel != null)
                    {
                        originalContextMenuModel.ItemVisibilityChanged -= OriginalContextMenuModel_ItemVisibilityChanged;
                        if(originalContextMenuModel.Items != null)
                            originalContextMenuModel.Items.CollectionChanged -= OriginalContextMenuModel_Items_CollectionChanged;
                    }
                }

                // Store new ContextMenuModel
                originalContextMenuModel = contextMenuModel;

                // We need to handle collection modification
                originalContextMenuModel.ItemVisibilityChanged += OriginalContextMenuModel_ItemVisibilityChanged;
                originalContextMenuModel.Items.CollectionChanged += OriginalContextMenuModel_Items_CollectionChanged;

                // Create ContextMenuModel used for display
                UpdateContextMenuModelUsedForUI();
            }
        }

        private void UpdateContextMenuModelUsedForUI()
        {
            // Set default values
            iconUsed = false;
            descriptionUsed = false;

            // Clear ContextMenuModel used
            contextMenuModelUsed.Clear();

            // Loop on each item
            foreach (var item in originalContextMenuModel.Items)
            {
                if (!String.IsNullOrEmpty(item.ImageSourceId))
                    iconUsed = true;

                if (!String.IsNullOrEmpty(item.Description))
                    descriptionUsed = true;

                if (item.IsVisible)
                {
                    contextMenuModelUsed.Add(item);
                    if (item.IsSelected)
                        selectedItemId = item.Id;
                }
            }

            // Set correct DataTemplate
            if (descriptionUsed)
            {
                // TODO
                ListView.ItemTemplate = ContextMenuWithDescriptionDataTemplate;
            }
            else
            {
                if (iconUsed)
                    ListView.ItemTemplate = ContextMenuWithIconDataTemplate;
                else
                    ListView.ItemTemplate = ContextMenuWithoutIconDataTemplate;
            }

            // Set selection Command
            contextMenuModelUsed.Command = SelectionCommand;

            // Set the binding context
            ListView.BindingContext = contextMenuModelUsed;

            // Update Heigth
            SetHeigthAccordingModel();
        }

        private void OriginalContextMenuModel_ItemVisibilityChanged(object sender, EventArgs e)
        {
            UpdateContextMenuModelUsedForUI();
        }

        private void SetHeigthAccordingModel()
        {
            if (contextMenuModelUsed?.Items?.Count > 0)
            {
                if (descriptionUsed)
                {
                    ListView.RowHeight = 54;
                    ListView.HeightRequest = (54 * contextMenuModelUsed.Items.Count);
                }
                else
                {
                    ListView.RowHeight = 28;
                    ListView.HeightRequest = (28 * contextMenuModelUsed.Items.Count);
                }
            }
            else
                ListView.HeightRequest = 0;

            // On Android (at least) we need to do this
            if(!Helper.IsDesktopPlatform())
                HeightRequest = ListView.HeightRequest + (2+1+2) * 2; // Add Grid margin + corner radius + listView margin
        }

        private void OriginalContextMenuModel_Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetHeigthAccordingModel();
        }

        public void SelectionCommandAction(object obj)
        {
            if(obj is String id)
            {
                Popup.HideCurrentContextMenu();

                // Reset selection of the UI component
                ListView.SelectedItem = null;

                selectedItemId = id;

                // Set selection 
                SetSelectedItemId(selectedItemId);

                if (Command != null && Command.CanExecute(selectedItemId))
                {
                    Helper.HapticFeedbackLongPress();
                    Command.Execute(selectedItemId);
                }
            }
        }
    }
}