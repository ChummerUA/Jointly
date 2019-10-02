﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Jointly.Effects
{
    public class RoundCorners : RoutingEffect
    {
        public double CornerRadius { get; set; }

        public RoundCorners() : base($"Jointly.RoundCorners")
        {
        }
    }
}