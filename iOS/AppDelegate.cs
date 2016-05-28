using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace SynthTest.iOS
{
    [Register("AppDelegate")]
    public unsafe partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, AudioIODeviceFactory
    {
        public override bool FinishedLaunching(UIApplication uiapp, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            var app = new App(this, GetAudioSetup());

            LoadApplication(app);
            return base.FinishedLaunching(uiapp, options);
        }

        #region AudioIODeviceFactory implementation

        public AudioIODevice CreateDevice()
        {
            return new IOSAudioIODevice();
        }

        #endregion

        AudioDeviceSetup GetAudioSetup()
        {
            return new AudioDeviceSetup
            {
                BufferSize = 1024,
                NumInputChannels = 0,
                NumOutputChannels = 2,
                SampleRate = 44100.0
            };
        }
    }
}

