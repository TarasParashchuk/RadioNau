using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RadioNau.Services
{
    [Service]
    [IntentFilter(new[] { ActionStartGetInfoRadio, ActionStopGetInfoRadio })]
    public class GetRadioInfo : Service
    {
        public const string ActionStartGetInfoRadio = "action.START";
        public const string ActionStopGetInfoRadio = "action.STOP";
        private RootObject json;

        public override void OnCreate()
        {
            base.OnCreate();

            json = new RootObject();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case ActionStartGetInfoRadio: TaskDataRadio(); break;
                case ActionStopGetInfoRadio: OnDestroy(); break;
            }
            return StartCommandResult.Sticky;
        }

        private async void TaskDataRadio()
        {
            var intent = new Intent(Receiver.DataRadioReceiver.DataReceiver);
            while (true)
            {
                var data = await Get_Data_Radio();

                intent.PutExtra("DataRadio", data);
                SendBroadcast(intent);

                await Task.Delay(3000);
            }
        }

        private async Task<string> Get_Data_Radio()
        {
            var client = new WebClient();
            var url = new Uri("http://92.249.64.135:8000/status-json.xsl");
            var str = await client.DownloadStringTaskAsync(url);

            try
            {
                json = JsonConvert.DeserializeObject<RootObject>(str);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex);
            }
            return json.icestats.source.title;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}