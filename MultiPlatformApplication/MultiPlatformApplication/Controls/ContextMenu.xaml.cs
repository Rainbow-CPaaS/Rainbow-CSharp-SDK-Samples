﻿using MultiPlatformApplication.Models;
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

        public String GetSelectedItemId()
        {
            if (StoreSelection)
                return selectedItemId;
            return null;
        }

        public ContextMenu()
        {
            InitializeComponent();

            ListView.ItemSelected += ListView_ItemSelected;
        }

        ~ContextMenu()
        {
            if (originalContextMenuModel != null)
            {
                originalContextMenuModel.ItemVisibilityChanged -= OriginalContextMenuModel_ItemVisibilityChanged;
                if (originalContextMenuModel.Items != null)
                    originalContextMenuModel.Items.CollectionChanged -= OriginalContextMenuModel_Items_CollectionChanged;
            }

            ListView.ItemSelected -= ListView_ItemSelected;

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
        }

        private void OriginalContextMenuModel_Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetHeigthAccordingModel();
        }

        private void SetSelectedItem(ContextMenuModel contextMenuModel, String selectedId)
        {
            if(contextMenuModel?.Items?.Count > 0)
            {
                foreach(var item in contextMenuModel.Items)
                    item.IsSelected = (item.Id == selectedId);
            }
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItemIndex != -1)
            {
                // Reset selection of the UI component
                ListView.SelectedItem = null;

                selectedItemId = null;
                if(contextMenuModelUsed?.Items.Count > e.SelectedItemIndex)
                    selectedItemId = contextMenuModelUsed.Items[e.SelectedItemIndex].Id;

                // Set selection in each models
                if (StoreSelection)
                {
                    SetSelectedItem(contextMenuModelUsed, selectedItemId);
                    SetSelectedItem(originalContextMenuModel, selectedItemId);
                }

                if (Command != null && Command.CanExecute(selectedItemId))
                    Command.Execute(selectedItemId);

                
            }
        }
    }
}