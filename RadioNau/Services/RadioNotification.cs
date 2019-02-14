using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Widget;

namespace RadioNau.Services
{
    [Service]
    class RadioNotification : Service, Receiver.DataRadioReceiver.DataRadio
    {
        public const string ActionStartRadioNotification = "action.Start.Notification";
        public const string ActionStopRadioNotification = "action.Stop.Notification";

        public static string Id_Channel = "default";
        public static int Id_Notification = 0;

        public delegate void DelegateUpdateDataNotification();
        public static DelegateUpdateDataNotification delegateUpdateDataNotification;

        private RemoteViews contentView;
        private Notification notification;
        private NotificationManager mNotifyManager;
        private NotificationCompat.Builder NotifyBuilder;

        private Model_Radio info_radio;
        private string current_name_track = string.Empty;

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterReceiver(new Receiver.DataRadioReceiver(), new IntentFilter(Receiver.DataRadioReceiver.DataReceiver));
            delegateUpdateDataNotification = UpdateDataNotification;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public void GetDataRadio(Model_Radio data)
        {
            info_radio = data;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case ActionStartRadioNotification: StartForeground(true); break;
                case ActionStopRadioNotification: StartForeground(false); break;
            }
            return StartCommandResult.Sticky;
        }

        private async void UpdateDataNotification()
        {
            while (true)
            {
                if (info_radio != null && info_radio.track != current_name_track)
                {
                    contentView.SetImageViewBitmap(Resource.Id.notification_image, GetImageBitmapFromUrl(info_radio.image));
                    contentView.SetTextViewText(Resource.Id.notification_text_title, info_radio.track);
                    contentView.SetTextViewText(Resource.Id.notification_text_artist, info_radio.artist);

                    mNotifyManager.Notify(Id_Notification, notification);
                    current_name_track = info_radio.track;
                }
                await Task.Delay(3000);
            }
        }

        private void StartForeground(bool flag)
        {
            Intent resultIntent = new Intent(this, typeof(MainActivity));
            PendingIntent resultPendingIntent = PendingIntent.GetActivity(this, 0, resultIntent, PendingIntentFlags.UpdateCurrent);

            PendingIntent play_pauseAction = null;
            int notificationAction = Android.Resource.Drawable.IcMediaPause;

            var intent_receiver = new Intent(Receiver.ButtonPlayNotificationReceiver.ReceiverPlay);

            if (flag)
            {
                notificationAction = Android.Resource.Drawable.IcMediaPause;
                play_pauseAction = ActionsInNotification(0);
                intent_receiver.PutExtra("buttonflag", false);
            }
            else
            {
                notificationAction = Android.Resource.Drawable.IcMediaPlay;
                play_pauseAction = ActionsInNotification(1);
                intent_receiver.PutExtra("buttonflag", true);
            }
            SendBroadcast(intent_receiver);

            contentView = new RemoteViews(ApplicationContext.PackageName, Resource.Layout.notification);

            if (info_radio != null)
            {
                contentView.SetImageViewBitmap(Resource.Id.notification_image, GetImageBitmapFromUrl(info_radio.image));
                contentView.SetTextViewText(Resource.Id.notification_text_title, info_radio.track);
                contentView.SetTextViewText(Resource.Id.notification_text_artist, info_radio.artist);
            }
            else
            {
                contentView.SetImageViewBitmap(Resource.Id.notification_image, GetImageBitmapFromUrl("http://medianau.top/images/logo.png"));
                contentView.SetTextViewText(Resource.Id.notification_text_title, "Подключение...");
                contentView.SetTextViewText(Resource.Id.notification_text_artist, "");
            }

            contentView.SetImageViewResource(Resource.Id.notification_button_play, notificationAction);
            contentView.SetOnClickPendingIntent(Resource.Id.notification_button_play, play_pauseAction);
            contentView.SetOnClickPendingIntent(Resource.Id.notification_button_cancel, ActionsInNotification(2));

            NotifyBuilder = new NotificationCompat.Builder(this, Id_Channel);

            notification = NotifyBuilder.SetContentTitle("Radio NAU started!")
                    .SetTicker("Radio NAU started!")
                    .SetContentText("Radio NAU started!")
                    .SetSmallIcon(Resource.Mipmap.logo_min)
                    .SetContentIntent(resultPendingIntent)
                    .SetPriority(Android.Support.V4.App.NotificationCompat.PriorityDefault)
                    .SetSound(null)
                    .SetOngoing(true)
                    .SetAutoCancel(false)
                    .SetVisibility(NotificationCompat.VisibilityPublic)
                    .Build();

            notification.BigContentView = contentView;

            mNotifyManager = (NotificationManager)GetSystemService(Context.NotificationService);
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.O)
            {
                NotificationChannel channel = new NotificationChannel(Id_Channel, GetString(Resource.String.noti_channel_default), NotificationImportance.High);
                channel.SetShowBadge(false);
                mNotifyManager.CreateNotificationChannel(channel);
            }
            //mNotifyManager.Notify(Id_Notification, notification);
            StartForeground(Id_Notification, notification);
        }

        private PendingIntent ActionsInNotification(int index)
        {
            var PlayIntent = new Intent(this, typeof(StreamingRadioBackgroundService));

            switch (index)
            {
                case 0: PlayIntent.SetAction(StreamingRadioBackgroundService.ActionStop); return PendingIntent.GetService(Application.Context, index, PlayIntent, PendingIntentFlags.UpdateCurrent);
                case 1: PlayIntent.SetAction(StreamingRadioBackgroundService.ActionPlay); return PendingIntent.GetService(Application.Context, index, PlayIntent, PendingIntentFlags.UpdateCurrent);
                case 2: PlayIntent.SetAction(StreamingRadioBackgroundService.ActionStopForeground); return PendingIntent.GetService(Application.Context, index, PlayIntent, PendingIntentFlags.UpdateCurrent);
                default: break;
            };
            return null;
        }

        public Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            }
            return imageBitmap;
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}