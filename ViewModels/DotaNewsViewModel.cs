using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbrellaProject.ViewModels
{
    internal class DotaNewsViewModel : Screen
    {
        public void OpenFaq()
        {
            new Process { StartInfo = new ProcessStartInfo("https://vk.com/misora") }.Start();
        }
    }
}
