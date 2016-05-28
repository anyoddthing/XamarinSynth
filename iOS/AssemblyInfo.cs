using System;
using ObjCRuntime;

[assembly: LinkWith ("libStaticLibraryTest.a", LinkTarget.Simulator, ForceLoad = true, IsCxx = true,Frameworks="AVFoundation CoreGraphics CoreMotion CoreVideo Security CoreMedia OpenGLES SystemConfiguration QuartzCore")]
[assembly: LinkWith ("libReferenceLib.a", LinkTarget.Simulator, ForceLoad = true, IsCxx = true, Frameworks="AVFoundation CoreGraphics CoreMotion CoreVideo Security CoreMedia OpenGLES SystemConfiguration QuartzCore")]