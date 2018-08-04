using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using Newtonsoft.Json;

namespace RadioNau
{
    class GetDataRadio : Service
    {
        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionStop = "com.xamarin.action.STOP";
        private bool paused;

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            /*switch (intent.Action)
            {
                case ActionPlay: Play(); break;
                case ActionStop: Stop(); break;
            }*/
            return StartCommandResult.Sticky;
        }
    }
}