////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Gadgeteer
{
    using System;
    using Gadgeteer;
    using Gadgeteer.Modules;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Presentation;
    using System.Threading;
    /// <summary>
    /// Represents the main program for a Microsoft .NET Gadgeteer application.
    /// </summary>
    /// <remarks>
    /// The main program of your Microsoft .NET Gadgeteer application must derive from this class, and call the <see cref="Run"/> method, which starts the Gadgeteer dispatcher.  
    /// Before a <see cref="Program"/> is instantiated, the <see cref="Program.Mainboard"/> static variable must be set.
    /// These tasks are normally done using a project template to provide an easy-to-follow structure for end user code.
    /// </remarks>
    public class Program
    {
        private Application _application = new Application();
        private static Mainboard _Mainboard;

        /// <summary>
        /// This property provides access to the Mainboard API.  This is normally not necessary for an end user program.
        /// </summary>
        protected internal static Mainboard Mainboard
        {
            get
            {
                if (_Mainboard == null) throw new Exception("Program instance must be constructed before Mainboard is used (use a template)");
                return _Mainboard;
            }
            protected set
            {
                if (_Mainboard != null) throw new Exception("Mainboard cannot be initialised twice");
                _Mainboard = value;
                Debug.Print("Using mainboard " + Mainboard.MainboardName + " version " + Mainboard.MainboardVersion);
            }
        }
        
        /// <summary>
        /// Represents the dispatcher object used to raise events on your application thread.
        /// </summary>
        /// <remarks>
        /// This static property is used internally by Gadgeteer interfaces and modules to raise events on your application thread.
        /// You should not access this property directly.
        /// </remarks>
        public static Dispatcher Dispatcher { get { if (_dispatcher == null) _dispatcher = Dispatcher.CurrentDispatcher; return _dispatcher; } }
        private static Dispatcher _dispatcher = null;

        private static bool _instanceExists = false;
        private static bool _runCalled = false;
        private static bool _dispatcherStarted = false;

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        protected Program()
        {
            if (Mainboard == null) throw new Exception("Mainboard must be specified before a Program can be started.");
            if (Dispatcher != Dispatcher.CurrentDispatcher) throw new Exception("Program must be instantiated on main (Dispatcher) thread.");
            if (_instanceExists) throw new Exception("Cannot instantiate two Programs");
            _instanceExists = true;
            _dispatcherBlockChecker = new Thread(DispatcherBlockChecker);
            _dispatcherBlockChecker.Start();
        }

        private Thread _dispatcherBlockChecker;

        private void DispatcherBlockChecker()
        {
            int waitincrement = 10; // seconds
            int totalwait = 0;
            while (true)
            {
                totalwait += waitincrement;
                Thread.Sleep(waitincrement*1000);
                if (_dispatcherStarted) return; // dispatcher is running, all is good
                if (_runCalled)
                {
                    Debug.Print("WARN: Total initialization time exceeds " + totalwait + " seconds.\n    : Mainboard.PostInit or LCD initialization is blocking execution.\n    : Events and timers do not yet run because the dispatcher is not started.");
                }
                else
                {
                    Debug.Print("WARN: Total initialization time exceeds " + totalwait + " seconds.\n    : ProgramStarted is blocking execution, which means events and timers will not run properly.\n    : Make sure not to use blocking code such as while(true) - use a GT.Timer instead.");
                }
            }
        }

        /// <summary>
        /// Starts the application, invoking the Dispatcher to handle events and timers. This call does not return.
        /// </summary>
        /// <exception cref="T:System.Exception">An attempt was made to call this method more than once.</exception>
        // Note: double-slash comment below so it doesn't appear in the API docs
        // deliberately a non-static call
        protected void Run()
        {
            if (_runCalled) throw new Exception("Do not call GT.Program.Run twice (templates should call it once already)");
            _runCalled = true;
            Module.DisplayModule.CheckDisplayConfig();
            Mainboard.PostInit();
            _dispatcherStarted = true;
            Dispatcher.Run();
        }

        /// <summary>
        /// Pulses the mainboard's debug LED, if present.  Note that there may be more than one LED on the mainboard. One labelled "PWR" will always be on when the mainboard is powered.  The debug LED is labelled "LED" (if present).
        /// This method also writes "PulseDebugLED called" on the debug print output.
        /// </summary>
        /// <remarks>
        /// The Debug LED can also be turned on/off using the <see cref="Program.Mainboard"/> interface directly.  
        /// </remarks>
        public void PulseDebugLED()
        {
            if (LEDOffTimer == null)
            {
                LEDOffTimer = new Timer(new TimeSpan(0, 0, 0, 0, 10));
                LEDOffTimer.Tick += new Timer.TickEventHandler(LEDOffTimer_Tick);
            }
            Mainboard.SetDebugLED(true);
            Debug.Print("PulseDebugLED called");
            LEDOffTimer.Start();
        }

        private Timer LEDOffTimer;

        void LEDOffTimer_Tick(Timer timer)
        {
            LEDOffTimer.Stop();
            Mainboard.SetDebugLED(false);
        }

        /// <summary>
        /// Executes a specified delegate on the thread on which a particular 
        /// <see cref="T:Microsoft.SPOT.Dispatcher"/> object (dispatcher) was created. 
        /// </summary>
        /// <param name="method">A delegate to a method which is pushed into the dispatcher's event queue.</param>
        /// <param name="args">The object array to be passed as arguments to the delegate.</param>
        /// <remarks>
        /// This static method is used internally by Gadgeteer interfaces and modules to raise events on your application thread.
        /// You should not call this method directly.
        /// </remarks>
        public static void BeginInvoke(Delegate method, params object[] args)
        {
            if (method != null && Program.Dispatcher != null)
            {
                // null params fails for .Net MF version 3 - use empty params list instead
                if (args == null) args = new object[] { };
                Program.Dispatcher.BeginInvoke(Operator, new Operation(method, args));
            }
        }

        /// <summary>
        /// A delegate for the <see cref="Invoke"/> event.
        /// </summary>
        public delegate void InvokeEventHandler();

        /// <summary>
        /// Adding a handler to this event causes the dispatcher to run that handler once.  
        /// </summary>
        /// <remarks>
        /// This provides a simple alternative to using <see cref="BeginInvoke"/> although it does not allow arguments to be passed to the method invoked.  
        /// Simply typing <code>Invoke += [tab][tab]</code> from within an app's Program.cs (or <code>GT.Program.Invoke += [tab][tab]</code> otherwise) causes a method to be created which is immediately invoked.  
        /// </remarks>
        public static event InvokeEventHandler Invoke
        {
            add
            {
                BeginInvoke(value, null);
            }
            remove
            {
                Debug.Print("GT.Program.Invoke is not a regular event - you cannot remove handlers from it.");
            }
        }

        // this helps event callers get onto the Dispatcher thread.  
        // If the event is null then it returns false - avoids extra null checks
        // If not on the thread, it invokes onEvent on the thread (which calls CheckAndInvoke again)
        // If on the thread, it returns true so that the caller can execute the event
        /// <summary>
        /// Validates, and then executes a specified delegate on the dispatcher thread.
        /// </summary>
        /// <param name="eventTriggered">An application-specific event delegate, or <b>null</b> if none have been assigned.</param>
        /// <param name="onEvent">An interface or module delegate that raises an event.</param>
        /// <param name="args">The object array to be passed as arguments to the delegate.</param>
        /// <returns>
        ///  <b>true</b> if the calling thread is the thread associated with dispatcher, otherwise <b>false</b>. 
        /// </returns>
        /// <remarks>
        /// This static method is used by Gadgeteer interfaces and modules to raise events on your application thread.
        /// Apps should not need to call this method directly.  See the module template for an example of the correct pattern for using this method.
        /// </remarks>
        public static bool CheckAndInvoke(Delegate eventTriggered, Delegate onEvent, params object[] args)
        {
            if (eventTriggered == null || onEvent == null) return false;
            if (Dispatcher.CheckAccess()) return true;
            BeginInvoke(onEvent, args);
            return false;
        }

        /// <summary>
        /// Reboots the device immediately.
        /// </summary>
        public void Reboot()
        {
            Microsoft.SPOT.Hardware.PowerState.RebootDevice(false);
        }

        private class Operation
        {
            public Operation(Delegate method, object[] args)
            {
                this.method = method;
                this.args = args;
            }
            public Delegate method;
            public object[] args;
        }

        private static DispatcherOperationCallback Operator = new DispatcherOperationCallback(DoOperation);
        private static object DoOperation(object op)
        {
            var operation = (Operation)op;
            try
            {
                operation.method.Method.Invoke(operation.method.Target, operation.args);
            }
            catch
            {
                Debug.Print("Error invoking method \"" + operation.method.Method.DeclaringType + "\" (check arguments to Program.BeginInvoke are correct)");
            }

            return null;
        }
    }
    
}
