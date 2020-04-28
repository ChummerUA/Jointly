﻿using Jointly.ViewModels;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Jointly.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignInPage : ContentPage
    {
        public SignInPage()
        {
            InitializeComponent();
        }

        private void Entry_Completed(object sender, EventArgs e)
        {
            if(sender is Entry entry)
            {
                if(entry.ReturnCommandParameter is Entry next)
                {
                    next.Focus();
                }
                else if(entry.ReturnCommand != null)
                {
                    entry.ReturnCommand.Execute(null);
                }
            }
        }
    }
}