using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightRemoteController
{
    public partial class PromptInstall : UserControl
    {
        public PromptInstall()
        {
            InitializeComponent();
        }

        private void installButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Install();
        }
    }
}
