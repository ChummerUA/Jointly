﻿using Jointly.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace Jointly.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetEventPage : ContentPage
    {
        public SetEventViewModel ViewModel { get; set; }

        public StackOrientation ContentOrientation { get; set; }

        public double ContainerWidth { get; set; }

        public SetEventPage()
        {
            InitializeComponent();

            ViewModel = new SetEventViewModel();
            BindingContext = ViewModel;

            SetOrientation();

            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        }

        private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            SetOrientation();
        }

        private void SetOrientation()
        {
            var displayInfo = DeviceDisplay.MainDisplayInfo;
            if (displayInfo.Orientation == DisplayOrientation.Portrait)
            {
                ContentOrientation = StackOrientation.Vertical;
                ContainerWidth = displayInfo.Width;
            }
            else if (displayInfo.Orientation == DisplayOrientation.Landscape)
            {
                ContentOrientation = StackOrientation.Horizontal;
                ContainerWidth = displayInfo.Width / 2;
            }
            OnPropertyChanged("ContentOrientation");
            OnPropertyChanged("ContainerWidth");
        }

        protected override void OnAppearing()
        {
            var position = new Position(50.619900, 26.251617);
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMeters(500)));
            base.OnAppearing();
        }
    }
}