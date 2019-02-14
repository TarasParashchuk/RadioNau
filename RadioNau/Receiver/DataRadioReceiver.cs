using System;
using System.Net;
using Android.Content;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RadioNau.Receiver
{
    [BroadcastReceiver]
    public class DataRadioReceiver : BroadcastReceiver
    {
        private DataRadio mDataRadio;
        private Model_Radio data_radio;
        public static string DataReceiver = "receiver.data.radio";

        public interface DataRadio
        {
            void GetDataRadio(Model_Radio data);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if(intent.Action == DataReceiver)
            {
                try
                {
                    var str = intent.GetStringExtra("DataRadio");
                    var index = str.IndexOf(" - ");

                    data_radio = new Model_Radio();
                    data_radio.artist = str.Substring(index + 3);
                    data_radio.track = str.Substring(0, index);

                    var image = GetImage(String.Format("http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key=539b27f6a7781319b08e5be3719f2764&artist={0}&track={1}&format=json", data_radio.track, data_radio.artist));

                    if (image == string.Empty)
                        data_radio.image = "http://medianau.top/images/logo.png";
                    else data_radio.image = image;

                    mDataRadio = (DataRadio)context;
                    mDataRadio.GetDataRadio(data_radio);
                }
                catch(Exception ex)
                {
                    Toast.MakeText(context, ex.Message + "Taras",ToastLength.Long);
                }
            }
        }

        private string GetImage(string url_json)
        {
            var str_json = new WebClient().DownloadString(url_json);

            var image = string.Empty;
            try
            {
                var json_image = JsonConvert.DeserializeObject<JObject>(str_json);
                image = json_image["track"]["album"]["image"][3]["#text"].ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex);
            }

            return image;
        }
    }
}