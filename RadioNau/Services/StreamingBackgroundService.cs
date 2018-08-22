using Android.App;
using System;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using System.Threading;
using Android.Widget;
using System.Net;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Support.V7.App;
using RadioNau.Services;

namespace RadioNau
{
    [Service]
    [IntentFilter(new[] { ActionPlay, ActionStop })]
    public class StreamingBackgroundService : Service, AudioManager.IOnAudioFocusChangeListener, Receiver.DataRadioReceiver.DataRadio
    {
        public const string ActionPlay = "action.PLAY";
        public const string ActionStop = "action.STOP";
        public const string ActionStopForeground = "action.STOPFOREGROUND";

        private const string url_radio = @"http://92.249.82.52:8000/live";
        public static int Id_Notification = 12;

        private MediaPlayer player;
        private AudioManager audioManager;
        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;

        private RemoteViews contentView;
        private Notification notification;
        private NotificationManager mNotifyManager;
        private NotificationCompat.Builder NotifyBuilder;

        private Thread thread_data;
        private Model_Radio info_radio;

        //private bool paused = true;
        private bool flag_notification = true;
        private string current_name_track = string.Empty;

        public override void OnCreate()
        {
            base.OnCreate();

            RegisterReceiver(new Receiver.DataRadioReceiver(), new IntentFilter(Receiver.DataRadioReceiver.DataReceiver));

            audioManager = (AudioManager)GetSystemService(AudioService);
            wifiManager = (WifiManager)GetSystemService(WifiService);
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
                case ActionPlay: Play(); break;
                case ActionStop: Stop(); break;
                case ActionStopForeground: OnDestroy(); StartService(new Intent(GetInfoRadio.ActionStopGetInfoRadio)); break;
            }
            return StartCommandResult.Sticky;
        }

        private void IntializePlayer()
        {
            player = new MediaPlayer();
            player.SetAudioStreamType(Stream.Music);
            player.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);

            player.Prepared += (sender, args) => player.Start();
            player.Completion += (sender, args) => Stop();
            player.Error += (sender, args) =>
            {
                Console.WriteLine("Error in playback resetting: " + args.What);
                Stop();
            };
        }

        private async void Play()
        {
            if (player == null)
                IntializePlayer();

            if (player.IsPlaying)
                return;

            try
            {
                await player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(url_radio));
                player.PrepareAsync();
                AquireWifiLock();
                StartForeground(true);
                OpenActivity.pause = false;

                if (flag_notification)
                {
                    thread_data = new Thread(new ThreadStart(UpdateDataNotification));
                    thread_data.Start();
                    flag_notification = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to start playback: " + ex);
            }
        }

        private void Stop()
        {
            if (player == null)
                return;

            if (player.IsPlaying)
                player.Stop();

            player.Reset();
            StartForeground(false);

            OpenActivity.pause = true;
            ReleaseWifiLock();
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
                play_pauseAction = PlayActions(0);
                intent_receiver.PutExtra("buttonflag", false);
            }
            else
            {
                notificationAction = Android.Resource.Drawable.IcMediaPlay;
                play_pauseAction = PlayActions(1);
                intent_receiver.PutExtra("buttonflag", true);
            }
            SendBroadcast(intent_receiver);

            contentView = new RemoteViews(ApplicationContext.PackageName, Resource.Layout.notification);

            /*if (data.image == "/logo.png") data.image = "http://medianau.online/logo.png";
            contentView.SetImageViewBitmap(Resource.Id.notification_image, GetImageBitmapFromUrl(data.image));*/

            if (info_radio != null)
            {
                contentView.SetImageViewBitmap(Resource.Id.notification_image, GetImageBitmapFromUrl(info_radio.image));
                contentView.SetTextViewText(Resource.Id.notification_text_title, info_radio.track);
                contentView.SetTextViewText(Resource.Id.notification_text_artist, info_radio.artist);
            }
            else
            {
                contentView.SetImageViewBitmap(Resource.Id.notification_image, GetImageBitmapFromUrl("http://medianau.online/images/logo.png"));
                contentView.SetTextViewText(Resource.Id.notification_text_title, "Подключение...");
                contentView.SetTextViewText(Resource.Id.notification_text_artist, "");
            }

            contentView.SetImageViewResource(Resource.Id.notification_button_play, notificationAction);
            contentView.SetOnClickPendingIntent(Resource.Id.notification_button_play, play_pauseAction);
            contentView.SetOnClickPendingIntent(Resource.Id.notification_button_cancel, PlayActions(2));

            NotifyBuilder = new NotificationCompat.Builder(this);

            notification = NotifyBuilder.SetContentTitle("Radio NAU started!")
                    .SetTicker("Radio NAU started!")
                    .SetContentText("Radio NAU started!")
                    .SetSmallIcon(Resource.Mipmap.logo_min)
                    .SetContentIntent(resultPendingIntent)
                    .SetOngoing(true)
                    .SetAutoCancel(false)
                    .SetOnlyAlertOnce(true)
                    .Build();

            notification.BigContentView = contentView;
            mNotifyManager = (NotificationManager)GetSystemService(Context.NotificationService);
            StartForeground(Id_Notification, notification);
        }


        private PendingIntent PlayActions(int index)
        {
            var PlayIntent = new Intent(this, typeof(StreamingBackgroundService));

            switch (index)
            {
                case 0: PlayIntent.SetAction(ActionStop); return PendingIntent.GetService(Application.Context, index, PlayIntent, PendingIntentFlags.UpdateCurrent);
                case 1: PlayIntent.SetAction(ActionPlay); return PendingIntent.GetService(Application.Context, index, PlayIntent, PendingIntentFlags.UpdateCurrent);
                case 2: PlayIntent.SetAction(ActionStopForeground); return PendingIntent.GetService(Application.Context, index, PlayIntent, PendingIntentFlags.UpdateCurrent);
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

        private void AquireWifiLock()
        {
            if (wifiLock == null)
            {
                wifiLock = wifiManager.CreateWifiLock(WifiMode.Full, "wifi_lock");
            }
            wifiLock.Acquire();
        }

        private void ReleaseWifiLock()
        {
            if (wifiLock == null)
                return;

            wifiLock.Release();
            wifiLock = null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (player != null)
            {
                player.Release();
                StopForeground(true);
                player = null;
            }
        }

        public void OnAudioFocusChange(AudioFocus focusChange)
        {
            switch (focusChange)
            {
                case AudioFocus.Gain:
                    if (player == null)
                        IntializePlayer();

                    if (!player.IsPlaying)
                    {
                        player.Start();
                        OpenActivity.pause = false;
                    }

                    player.SetVolume(1.0f, 1.0f);
                    break;
                case AudioFocus.Loss: Stop(); break;
                case AudioFocus.LossTransientCanDuck: if (player.IsPlaying) player.SetVolume(.1f, .1f); break;
            }
        }
    }
}
