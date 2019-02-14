using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Views;
using FFImageLoading.Work;
using FFImageLoading.Transformations;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.App;

namespace RadioNau
{
    [Activity(Label = "RadioNau", MainLauncher = false, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class MainActivity : AppCompatActivity, Receiver.ButtonPlayNotificationReceiver.BroadcastDataInterface, Receiver.DataRadioReceiver.DataRadio, SeekBar.IOnSeekBarChangeListener
    {
        private ImageViewAsync image;
        private TextView name_song;
        private TextView name_singer;
        private ImageView play;
        private ImageView volume_image;
        private SeekBar volumebar;
        private AudioManager audioManager;
        private Model_Radio info_radio;

        private string current_name_track = string.Empty;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "RADIO MEDIANAU";

            image = FindViewById<ImageViewAsync>(Resource.Id.Image);
            play = FindViewById<ImageView>(Resource.Id.ImagePlay);

            volumebar = FindViewById<SeekBar>(Resource.Id.Sound_Volume);
            volume_image = FindViewById<ImageView>(Resource.Id.volume_image);

            audioManager = (AudioManager)GetSystemService(Context.AudioService);
            volumebar.SetOnSeekBarChangeListener(this);

            if (OpenActivity.pause)
            {
                play.SetImageResource(Resource.Mipmap.ic_play_circle_outline_white_48dp);
                OpenActivity.pause = true;
            }
            else
            {
                play.SetImageResource(Resource.Mipmap.ic_pause_circle_outline_white_48dp);
                OpenActivity.pause = false;
            }

            name_song = FindViewById<TextView>(Resource.Id.Name_Song);
            name_singer = FindViewById<TextView>(Resource.Id.Name_Artist);
            play.Click += Play_Click;

            Thread_get_data();
        }

        protected override void OnResume()
        {
            base.OnResume();

            RegisterReceiver(new Receiver.DataRadioReceiver(), new IntentFilter(Receiver.DataRadioReceiver.DataReceiver));
            RegisterReceiver(new Receiver.ButtonPlayNotificationReceiver(), new IntentFilter(Receiver.ButtonPlayNotificationReceiver.ReceiverPlay));
        }

        public void GetDataRadio(Model_Radio data)
        {
            info_radio = data;
        }

        public void OnProgressChanged(SeekBar seekBar, int i, bool b)
        {
            audioManager.SetStreamVolume(Stream.Music, i, 0);

            if (seekBar.Progress == 0)
                volume_image.SetImageResource(Resource.Mipmap.baseline_volume_off_white_48);
            else if (seekBar.Progress > 7)
                volume_image.SetImageResource(Resource.Mipmap.baseline_volume_up_white_48);
            else volume_image.SetImageResource(Resource.Mipmap.baseline_volume_down_white_48);
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
        }

        public void GetDataNotificationRadio(bool buttonflag)
        {
            if (buttonflag)
            {
                play.SetImageResource(Resource.Mipmap.ic_play_circle_outline_white_48dp);
                OpenActivity.pause = true;
            }
            else
            {
                play.SetImageResource(Resource.Mipmap.ic_pause_circle_outline_white_48dp);
                OpenActivity.pause = false;
            }
        }

        private void Play_Click(object sender, EventArgs args)
        {
            if (OpenActivity.pause)
            {
                SendAudioCommand(StreamingRadioBackgroundService.ActionPlay);
                play.SetImageResource(Resource.Mipmap.ic_pause_circle_outline_white_48dp);
                OpenActivity.pause = false;
            }
            else
            {
                SendAudioCommand(StreamingRadioBackgroundService.ActionStop);
                play.SetImageResource(Resource.Mipmap.ic_play_circle_outline_white_48dp);
                OpenActivity.pause = true;
            }
        }

        private void SendAudioCommand(string action)
        {
            var intent = new Intent(this, typeof(StreamingRadioBackgroundService));
            intent.SetAction(action);
            StartService(intent);
        }

        /*private async void Thread_get_glow_effect()
        {
            var Linear_Layout = view.FindViewById<LinearLayout>(Resource.Id.BGLayout);
            while (true)
            {
                var count = 0;
                while (count != 13)
                {
                    Linear_Layout.SetBackgroundResource(2130837590 + count);
                    count++;
                    await Task.Delay(10000);
                }
            }
        }*/

        private async void Thread_get_data()
        {
            var colorHEX = string.Empty;
            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.O) colorHEX = "#2c2b2a";
            else colorHEX = "#5d5956";//"#56514d";

            while (true)
            {
                if (info_radio != null && info_radio.track != current_name_track)
                {
                    ImageService.Instance.LoadUrl(info_radio.image).WithCache(FFImageLoading.Cache.CacheType.Memory).Transform(new List<ITransformation>() { new TintTransformation(colorHEX) }).Into(image);
                    name_song.Text = info_radio.track;
                    name_singer.Text = info_radio.artist;
                    current_name_track = info_radio.track;
                }
                await Task.Delay(3000);
            }
        }
    }
}

