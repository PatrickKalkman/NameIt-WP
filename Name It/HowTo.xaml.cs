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
using Microsoft.Phone.Controls;
using NameIt.ViewModels;

namespace NameIt
{
    public partial class HowTo : PhoneApplicationPage
    {
        public HowTo()
        {
            InitializeComponent();
        }

		private void LeftArrow_Click(object sender, EventArgs e)
        {
            this.slideView.MoveToPreviousItem();
        }

        private void RightArrow_Click(object sender, EventArgs e)
        {
            this.slideView.MoveToNextItem();
        }
    }
}
