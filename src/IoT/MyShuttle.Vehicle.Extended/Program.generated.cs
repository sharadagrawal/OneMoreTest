//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyShuttle.Vehicle {
    using Gadgeteer;
    using GTM = Gadgeteer.Modules;
    
    
    public partial class Program : Gadgeteer.Program {
        
        /// <summary>The Multicolor LED module using socket 18 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.MulticolorLED statusLED;
        
        /// <summary>The Accelerometer module using socket 4 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.Accelerometer Accelerometer;
        
        /// <summary>The RFID Reader module using socket 10 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.RFIDReader rfidReader;
        
        /// <summary>The WiFi RS21 module using socket 11 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.WiFiRS21 wifiRS21;
        
        /// <summary>The UC Battery 4xAA module using socket 8 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.UCBattery4xAA ucBattery4xAA;
        
        /// <summary>The Compass module using socket 2 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.Compass compass;
        
        /// <summary>This property provides access to the Mainboard API. This is normally not necessary for an end user program.</summary>
        protected new static GHIElectronics.Gadgeteer.FEZRaptor Mainboard {
            get {
                return ((GHIElectronics.Gadgeteer.FEZRaptor)(Gadgeteer.Program.Mainboard));
            }
            set {
                Gadgeteer.Program.Mainboard = value;
            }
        }
        
        /// <summary>This method runs automatically when the device is powered, and calls ProgramStarted.</summary>
        public static void Main() {
            // Important to initialize the Mainboard first
            Program.Mainboard = new GHIElectronics.Gadgeteer.FEZRaptor();
            Program p = new Program();
            p.InitializeModules();
            p.ProgramStarted();
            // Starts Dispatcher
            p.Run();
        }
        
        private void InitializeModules() {
            this.statusLED = new GTM.GHIElectronics.MulticolorLED(18);
            this.Accelerometer = new GTM.GHIElectronics.Accelerometer(4);
            this.rfidReader = new GTM.GHIElectronics.RFIDReader(10);
            this.wifiRS21 = new GTM.GHIElectronics.WiFiRS21(11);
            this.ucBattery4xAA = new GTM.GHIElectronics.UCBattery4xAA(8);
            this.compass = new GTM.GHIElectronics.Compass(2);
        }
    }
}
