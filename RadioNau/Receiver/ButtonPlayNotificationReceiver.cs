using Android.Content;
using Android.Widget;
using System;

namespace RadioNau.Receiver
{
    [BroadcastReceiver]
    public class ButtonPlayNotificationReceiver : BroadcastReceiver
    {
        private BroadcastDataInterface mInterface;
        public const string ReceiverPlay = "notification.play.receiver";

        public interface BroadcastDataInterface
        {
            void GetDataNotificationRadio(bool buttonflag);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == ReceiverPlay)
            {
                try
                {
                    var buttonflag = intent.GetBooleanExtra("buttonflag", true);

                    mInterface = (BroadcastDataInterface)context;
                    mInterface.GetDataNotificationRadio(buttonflag);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
                }
            }
        }
    }
}