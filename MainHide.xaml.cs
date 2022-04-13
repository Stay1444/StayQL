using StayQL.StayWindows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StayQL
{
    /// <summary>
    /// Interaction logic for MainHide.xaml
    /// </summary>
    public partial class MainHide : Window
    {
        public MainHide()
        {
            Program.main();
            this.Close();
        }
    }
}
