using System;
using Android.Content;
using Android.Widget;

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
                    data_radio.image = "http://medianau.online/logo.jpg";
                    data_radio.artist = str.Substring(index + 3);
                    data_radio.track = str.Substring(0, index);

                    mDataRadio = (DataRadio)context;
                    mDataRadio.GetDataRadio(data_radio);
                }
                catch(Exception ex)
                {
                    Toast.MakeText(context, ex.Message,ToastLength.Long);
                }
            }
        }
    }
}