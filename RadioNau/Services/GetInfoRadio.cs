using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RadioNau.Services
{
    [Service]
    [IntentFilter(new[] { ActionStartGetInfoRadio, ActionStopGetInfoRadio })]
    public class GetInfoRadio : Service
    {
        public const string ActionStartGetInfoRadio = "action.START";
        public const string ActionStopGetInfoRadio = "action.STOP";
        private Model_JSON json;

        public override void OnCreate()
        {
            base.OnCreate();

            json = new Model_JSON();
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

        /*private async Task<Model_Radio> Get_Data_Radio()
        {
            var client = new WebClient();
            var url = new Uri("http://92.249.82.52:8000/status2.xsl");
            var json = await client.DownloadStringTaskAsync(url);
            
            return JsonConvert.DeserializeObject<Model_Radio>(json);
        }*/

        private async Task<string> Get_Data_Radio()
        {
            var client = new WebClient();
            var url = new Uri("http://92.249.82.52:8000/status2.xsl");
            var str = await client.DownloadStringTaskAsync(url);

            str = str.Remove(0, 20);
            str = str.Remove(str.Length - 3, 3);

            try
            {
                json = JsonConvert.DeserializeObject<Model_JSON>(str);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex);
            }
            //On_Receive(json.song);
            return json.song;
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

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}