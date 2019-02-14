using Android.App;
using System;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using RadioNau.Services;
using System.Threading;

namespace RadioNau
{
    [Service]
    [IntentFilter(new[] { ActionPlay, ActionStop })]
    public class StreamingRadioBackgroundService : Service, AudioManager.IOnAudioFocusChangeListener
    {
        public const string ActionPlay = "action.PLAY";
        public const string ActionStop = "action.STOP";
        public const string ActionStopForeground = "action.STOPFOREGROUND";

        private const string url_radio = @"http://92.249.64.135:8000/live";
        private MediaPlayer player;
        private AudioManager audioManager;
        private Intent intent_notification;
        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;

        private Thread thread_data;
        private bool flag_notification = true;

        public override void OnCreate()
        {
            base.OnCreate();
            audioManager = (AudioManager)GetSystemService(AudioService);
            wifiManager = (WifiManager)GetSystemService(WifiService);
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case ActionPlay: Play(); break;
                case ActionStop: Stop(); break;
                case ActionStopForeground: OnDestroy(); StartService(new Intent(GetRadioInfo.ActionStopGetInfoRadio)); break;
            }
            return StartCommandResult.Sticky;
        }

        private void IntializePlayer()
        {
            player = new MediaPlayer();
            intent_notification = new Intent(this, typeof(RadioNotification));
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

                intent_notification.SetAction(RadioNotification.ActionStartRadioNotification);
                StartService(intent_notification);

                OpenActivity.pause = false;

                if (flag_notification)
                {
                    thread_data = new Thread(new ThreadStart(RadioNotification.delegateUpdateDataNotification));
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
            intent_notification.SetAction(RadioNotification.ActionStopRadioNotification);
            StartService(intent_notification);

            OpenActivity.pause = true;
            ReleaseWifiLock();
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
