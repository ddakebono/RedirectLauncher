﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MetroRadiance.Controls
{
	public class TabView : ListBox
	{
		static TabView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TabView), new FrameworkPropertyMetadata(typeof(TabView)));
		}
	}
}
