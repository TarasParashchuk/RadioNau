using System;
using FFImageLoading;
using Android.App;
using Android.Widget;
using Android.OS;
using FFImageLoading.Views;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using System.Threading.Tasks;
using Android.Support.V7.App;
using Android.Content;
using Android.Media;

namespace RadioNau
{
    [Activity(Label = "RadioNau", MainLauncher = false, Theme = "@style/Theme.AppCompat.Light.NoActionBar")]
    public class MainActivity : AppCompatActivity, Receiver.ButtonPlayNotificationReceiver.BroadcastDataInterface, Receiver.DataRadioReceiver.DataRadio, SeekBar.IOnSeekBarChangeListener
    {
        private ImageViewAsync image;
        private TextView name_song;
        private TextView name_singer;
        private ImageView play;
        private SeekBar volumebar;
        private AudioManager audioManager;
        private bool flag = true;
        private Model_Radio info_radio;
        private string current_name_track = string.Empty;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "RADIO MEDIANAU";

            RegisterReceiver(new Receiver.DataRadioReceiver(), new IntentFilter(Receiver.DataRadioReceiver.DataReceiver));
            RegisterReceiver(new Receiver.ButtonPlayNotificationReceiver(), new IntentFilter(Receiver.ButtonPlayNotificationReceiver.ReceiverPlay));

            image = FindViewById<ImageViewAsync>(Resource.Id.Image);
            play = FindViewById<ImageView>(Resource.Id.Image_Play);
            volumebar = FindViewById<SeekBar>(Resource.Id.Sound_Volume);

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
            Thread_get_glow_effect();
        }

        public void OnProgressChanged(SeekBar seekBar, int i, bool b)
        {
            audioManager.SetStreamVolume(Stream.Music, i, 0);
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

        public void GetDataRadio(Model_Radio data)
        {
            info_radio = data;
        }

        private void Play_Click(object sender, EventArgs args)
        {
            if (OpenActivity.pause)
            {
                SendAudioCommand(StreamingBackgroundService.ActionPlay);
                play.SetImageResource(Resource.Mipmap.ic_pause_circle_outline_white_48dp);
                OpenActivity.pause = false;
            }
            else
            {
                SendAudioCommand(StreamingBackgroundService.ActionStop);
                play.SetImageResource(Resource.Mipmap.ic_play_circle_outline_white_48dp);
                OpenActivity.pause = true;
            }
        }

        private void SendAudioCommand(string action)
        {
            var intent = new Intent(action);
            StartService(intent);
        }

        private async void Thread_get_glow_effect()
        {
            var Linear_Layout = FindViewById<LinearLayout>(Resource.Id.bg);
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
        }

        private async void Thread_get_data()
        {
            while (true)
            {
                if (info_radio != null && info_radio.track != current_name_track)
                {
                    ImageService.Instance.LoadUrl(info_radio.image).WithCache(FFImageLoading.Cache.CacheType.Memory).Into(image);
                    name_song.Text = info_radio.track;
                    name_singer.Text = info_radio.artist;
                    current_name_track = info_radio.track;
                }
                await Task.Delay(3000);
            }
        }
    }
}

