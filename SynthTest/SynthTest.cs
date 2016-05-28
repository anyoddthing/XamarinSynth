using System;

using Xamarin.Forms;
using System.Runtime.InteropServices;

namespace SynthTest
{
    public unsafe class App : Application
    {
        AudioIODeviceFactory _deviceFactory;

        AudioDeviceSetup _audioSetup;

        AudioDeviceManager _audioDeviceManager;

        SequencedSynth _synthSource;

        Slider _freqSlider;

        Slider _volSlider;

        AudioSourcePlayer _player;

        public App(AudioIODeviceFactory deviceFactory, AudioDeviceSetup audioSetup)
        {

            _deviceFactory = deviceFactory;
            _audioSetup = audioSetup;

            // The root page of your application
            _freqSlider = new Slider
            {
                Minimum = 0,
                Maximum = 1000,
                Value = 440,
            };

            _volSlider = new Slider
            {
                Minimum = 0,
                Maximum = 1,
                Value = 0.6f,
            };

            _freqSlider.ValueChanged += Slider_ValueChanged;
            _volSlider.ValueChanged += Slider_ValueChanged;

            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        _freqSlider,
                        _volSlider
                    }
                }
            };
        }

        void Slider_ValueChanged (object sender, ValueChangedEventArgs e)
        {
//            if (sender == _freqSlider)
//            {
//                _synthSource.Frequency = (float)e.NewValue;
//            }
//            else 
            if (sender == _volSlider)
            {
                _player.Gain = (float)e.NewValue;
            }
        }

        void StartManaged()
        {
            var audioDeviceManager = new AudioDeviceManager(_deviceFactory, _audioSetup);
            _audioDeviceManager = audioDeviceManager;

            var generator = new SequencedSynth();
            var device = _audioDeviceManager.Device;
            _synthSource = generator;
            _player = new AudioSourcePlayer
            {
                Gain = 0.8f,
                Source = generator
            };

            _audioDeviceManager.AddAudioCallback(_player);
        }
            
        protected override void OnStart()
        {            
            StartManaged();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

