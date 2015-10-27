////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Gadgeteer.Interfaces
{
    using System;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;
    using Gadgeteer.Modules;

    /// <summary>
    /// Represents digital input/output on a single pin.
    /// </summary>
    public class DigitalIO
    {
        /// <summary>
        /// Represents the tristate port for digital input/output.
        /// </summary>
        private TristatePort port;

        /// <summary>
        /// The IOMode 
        /// </summary>
        private IOModes ioModes;


        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="socket">The socket for the digital input/output interface.</param>
        /// <param name="pin">The pin used by the digital input/output interface.</param>
        /// <param name="initialState">
        ///  The initial state to set on the digital input/output interface port.  
        ///  This value becomes effective as soon as the port is enabled as an output port.
        /// </param>
        /// <param name="glitchFilterMode">
        ///  A value from the <see cref="GlitchFilterMode"/> enumeration that specifies 
        ///  whether to enable the glitch filter on this digital input/output interface.
        /// </param>
        /// <param name="resistorMode">
        ///  A value from the <see cref="ResistorMode"/> enumeration that establishes a default state for the digital input/output interface. N.B. .NET Gadgeteer mainboards are only required to support ResistorMode.PullUp on interruptable GPIOs and are never required to support ResistorMode.PullDown; consider putting the resistor on the module itself.
        /// </param>
        /// <param name="module">The module using this interface, which can be null if unspecified.</param>
        public DigitalIO(Socket socket, Socket.Pin pin, bool initialState, GlitchFilterMode glitchFilterMode, ResistorMode resistorMode, Module module)
        {
            this.port = new TristatePort(socket.ReservePin(pin, module), initialState, glitchFilterMode == GlitchFilterMode.On, (Port.ResistorMode)resistorMode);

            if (this.port == null)
            {
                // this is a mainboard error but should not happen since we check for this, but it doesnt hurt to double-check
                throw new Socket.InvalidSocketException("Socket " + socket + " has an error with its Digital IO functionality. Please try a different socket.");
            }
            this.IOMode = IOModes.Input; 
        }

        /// <summary>
        /// Represents the possible modes of a digital input/output interface.
        /// </summary>
        public enum IOModes
        {
            /// <summary>
            /// The interface is set for input operations.
            /// </summary>
            Input,
            /// <summary>
            /// The interface is set for output operations.
            /// </summary>
            Output
        }

        /// <summary>
        /// Gets or sets the mode for the digital input/output interface.
        /// </summary>
        /// <value>
        /// <list type="bullet">
        /// <item><see cref="IOModes">IOModes.Input</see> if the interface is currently set for input operations.</item>
        /// <item><see cref="IOModes">IOModes.Output</see> if the interface is currently set for ouput operations.</item>
        /// </list>
        /// </value>
        public IOModes IOMode
        {
            get
            {
                return this.ioModes;
            }

            set
            {
                this.ioModes = value;

                if (value == IOModes.Input)
                {
                    if (this.port.Active)
                    {
                        this.port.Active = false;
                    }
                }
                else if (value == IOModes.Output)
                {
                    if (!this.port.Active)
                    {
                        this.port.Active = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the interface to <see cref="IOModes">IOModes.Output</see>
        /// and writes the specified value.
        /// </summary>
        /// <param name="state">The value to write.</param>
        public void Write(bool state)
        {
            this.IOMode = IOModes.Output;
            this.port.Write(state);
        }

        /// <summary>
        /// Sets the interface to <see cref="IOModes">IOModes.Intput</see>
        /// and reads a value.
        /// </summary>
        /// <returns>A Boolean value read from the interface.</returns>
        public bool Read()
        {
            this.IOMode = IOModes.Input;
            return this.port.Read();
        }
    }
}