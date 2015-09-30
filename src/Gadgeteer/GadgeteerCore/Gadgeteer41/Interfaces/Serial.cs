////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Gadgeteer.Interfaces
{
    using System;
    using System.IO.Ports;
    using System.Threading;
    using Microsoft.SPOT;
    using Gadgeteer.Modules;

    /// <summary>
    /// Represents a serial communcations interface port.
    /// </summary>
    public class Serial
    {

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <remarks>This automatically checks that the socket supports Type U, and reserves the pins.
        /// An exception will be thrown if there is a problem with these checks.</remarks>
        /// <param name="baudRate">The baud rate for the serial port.</param>
        /// <param name="parity">A value from the <see cref="SerialParity"/> enumeration that specifies 
        /// the parity for the port.</param>
        /// <param name="stopBits">A value from the <see cref="SerialStopBits"/> enumeration that specifies 
        /// the stop bits for the port.</param>
        /// <param name="dataBits">The number of data bits.</param>
        /// <param name="socket">The socket for this serial interface.</param>
        /// <param name="hardwareFlowControlRequirement">Specifies whether the module must use hardware flow control, will use hardware flow control if available, or does not use hardware flow control.</param>
        /// <param name="module">The module using this interface (which can be null if unspecified).</param>
        public Serial(Socket socket, int baudRate, SerialParity parity, SerialStopBits stopBits, int dataBits, HardwareFlowControl hardwareFlowControlRequirement, Module module)
        {
            if (!socket.SupportsType('U'))
            {
                if (module != null)
                {
                    throw new Socket.InvalidSocketException("Module " + module + " cannot use socket " + socket + " because it requires a socket supporting type 'K'" + (hardwareFlowControlRequirement == HardwareFlowControl.Required ? "" : " or type 'U'."));
                }
                else
                {
                    throw new Socket.InvalidSocketException("Cannot use socket " + socket + " because it does not support socket type 'K'" + (hardwareFlowControlRequirement == HardwareFlowControl.Required ? "" : " or type 'U'."));
                }
            }

            bool socketSupportsHardwareFlowControl = socket.SupportsType('K');
            if (hardwareFlowControlRequirement == HardwareFlowControl.Required && !socketSupportsHardwareFlowControl)
            {
                if (module != null)
                {
                    throw new Socket.InvalidSocketException("Module " + module + " cannot use socket " + socket + " because it requires a socket supporting type 'K'.");
                }
                else
                {
                    throw new Socket.InvalidSocketException("Cannot use socket " + socket + " because it does not support socket type 'K' and hardware flow control is required. Please relax the requirement for hardware flow control or use a socket supporting type 'K'.");
                }
            }

            string portName = socket.SerialPortName;

            if (portName == null || portName == "")
            {
                // this is a mainboard error that should not happen (we already check for it in SocketInterfaces.RegisterSocket) but just in case...
                throw new Socket.InvalidSocketException("Socket " + socket + " has an error with its Serial functionality. Please try a different socket.");

            }

            socket.ReservePin(Socket.Pin.Four, module);
            socket.ReservePin(Socket.Pin.Five, module);
            if (hardwareFlowControlRequirement != HardwareFlowControl.NotRequired)
            {
                // must reserve hardware flow control pins even if not using them, since they are electrically connected.
                socket.ReservePin(Socket.Pin.Six, module);
                socket.ReservePin(Socket.Pin.Seven, module);
            }

            this.LineReceivedEventDelimiter = "\n";

            this.Encoding = System.Text.Encoding.UTF8;

            this._serialPort = new SerialPort(portName, baudRate, (System.IO.Ports.Parity)parity, dataBits, (System.IO.Ports.StopBits)stopBits);
            if ((hardwareFlowControlRequirement != HardwareFlowControl.NotRequired) && socketSupportsHardwareFlowControl)
            {
                this._serialPort.Handshake = Handshake.RequestToSend;
                this._hardwareFlowControl = true;
            }
            else
            {
                this._hardwareFlowControl = false;
            }

            this._serialPort.DataReceived += new SerialDataReceivedEventHandler(this._serialPort_DataReceived);
            this.ReadTimeout = InfiniteTimeout;
            this.WriteTimeout = InfiniteTimeout;
        }

        private SerialPort _serialPort { get; set; }

        /// <summary>
        /// A value that represents an infinite timeout.
        /// </summary>
        public const int InfiniteTimeout = System.Threading.Timeout.Infinite;

        private string lineReceivedEventDelimiter;

        /// <summary>
        /// Gets or sets the line-received event delimiter.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  The default value of this property is a new-line character, ASCII 0x0A.
        ///  When you set <see cref="AutoReadLineEnabled"/> to <b>true</b>, the value
        ///  of this property is used to determine when a complete line of data has been received
        ///  and, consequently, when to raise the <see cref="LineReceived"/> event.
        /// </para>
        /// <para>
        ///  The value of this property is also appended to the specifed text when you
        ///  call the <see cref="WriteLine"/> method.
        /// </para> 
        /// </remarks>
        public string LineReceivedEventDelimiter
        {
            get
            {
                return this.lineReceivedEventDelimiter;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                if (value.Length == 0)
                {
                    throw new ArgumentException();
                }

                this.lineReceivedEventDelimiter = value;
            }
        }

        /// <summary>
        /// Gets the port name associated with this serial interface.
        /// </summary>
        public string PortName
        {
            get
            {
                return this._serialPort.PortName;
            }
        }

        /// <summary>
        /// Gets or sets the baud rate of this serial interface.
        /// </summary>
        public int BaudRate
        {
            get
            {
                return this._serialPort.BaudRate;
            }

            set
            {
                this._serialPort.BaudRate = value;
            }
        }

        /// <summary>
        /// Gets or sets the parity of this serial interface.
        /// </summary>
        /// <value>
        /// A value from the <see cref="SerialParity"/> enumeration that specifies the parity of 
        /// this serial interface.
        /// </value>
        public SerialParity Parity
        {
            get
            {
                return (SerialParity)this._serialPort.Parity;
            }

            set
            {
                this._serialPort.Parity = (System.IO.Ports.Parity)value;
            }
        }

        /// <summary>
        /// Gets or sets the stop bits of this serial interface.
        /// </summary>
        /// <value>
        ///  A value from the <see cref="SerialStopBits"/> enumeration that specifies the 
        ///  stop bits of this serial interface.
        /// </value>
        public SerialStopBits StopBits
        {
            get
            {
                return (SerialStopBits)this._serialPort.StopBits;
            }

            set
            {
                this._serialPort.StopBits = (System.IO.Ports.StopBits)value;
            }
        }

        /// <summary>
        /// Gets or sets the number of data bits of this serial interface. 
        /// </summary>
        public int DataBits
        {
            get
            {
                return this._serialPort.DataBits;
            }

            set
            {
                this._serialPort.DataBits = value;
            }
        }

        /// <summary>
        /// Returns a Boolean value that indicates whether the Serial interface is using hardware flow control.
        /// </summary>
        public bool UsingHardwareFlowControl
        {
            get
            {
                return _hardwareFlowControl;
            }
        }
        bool _hardwareFlowControl;

        /// <summary>
        /// Gets or sets the encoding used on this serial port for writing and reading strings.
        /// </summary>
        public System.Text.Encoding Encoding { get; set; }

        private Thread readLineThread;

        private bool autoReadLineEnabled;

        /// <summary>
        /// Gets or sets a value that determines whether automatic line reading is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  When you set <see cref="AutoReadLineEnabled"/> to <b>true</b>, automatic reading
        ///  of the serial port is enabled. When enabled, <see cref="Serial"/> will continuously 
        ///  monitor the serial port; if the port is open (that is, <see cref="IsOpen"/> is <b>true</b>),
        ///  <see cref="Serial"/> will collect incoming data. Whenever a complete line of data is received 
        ///  as determined by the value of <see cref="LineReceivedEventDelimiter"/>, 
        ///  <see cref="Serial"/> raises the <see cref="LineReceived"/> event.
        /// </para>
        /// </remarks>
        public bool AutoReadLineEnabled
        {
            get
            {
                return this.autoReadLineEnabled;
            }

            set
            {
                this.autoReadLineEnabled = value;
                if (this.autoReadLineEnabled)
                {
                    if (this.readLineThread == null)
                    {
                        this.readLineThread = new Thread(new ThreadStart(this.ReadLineProcess));
                        this.readLineThread.Start();
                    }
                    else
                    {
                        this.readLineThread.Resume();
                    }
                }
                else
                {
                    if (this.readLineThread != null)
                    {
                        this.readLineThread.Suspend();
                    }
                }
            }
        }

        /// <summary>
        /// Specifies the parity bit for a <see cref="Serial"/> object. 
        /// </summary>
        /// <remarks>
        /// <para>
        ///  Use this enumeration when setting the <see cref="SerialParity"/> property for a serial port connection.
        /// </para>
        /// <para>
        ///  Parity is an error-checking procedure in which the number of 1s must always be the same�either even or odd�for each 
        ///  group of bits that is transmitted without error. In modem-to-modem communications, parity is often one of the parameters 
        ///  that must be agreed upon by sending parties and receiving parties before transmission can take place.
        /// </para>
        /// </remarks>
        public enum SerialParity
        {
            /// <summary>
            /// Sets the parity bit so that the count of bits set is an even number.
            /// </summary>
            Even = System.IO.Ports.Parity.Even,
            /// <summary>
            /// Sets the parity bit so that the count of bits set is an odd number.
            /// </summary>
            Odd = System.IO.Ports.Parity.Odd,
            /// <summary>
            /// Leaves the parity bit set to 1.
            /// </summary>
            Mark = System.IO.Ports.Parity.Mark,
            /// <summary>
            /// Leaves the parity bit set to 0.
            /// </summary>
            Space = System.IO.Ports.Parity.Space,
            /// <summary>
            /// No parity check occurs.
            /// </summary>
            None = System.IO.Ports.Parity.None
        }

        /// <summary>
        /// Specifies the number of stop bits used on the <see cref="Serial"/> object. 
        /// </summary>
        /// <remarks>
        /// <para>
        ///  This enumeration specifies the number of stop bits to use. Stop bits separate each unit of 
        ///  data on an asynchronous serial connection. 
        ///  They are also sent continuously when no data is available for transmission.
        /// </para>
        /// <para>
        /// The <b>None</b> option is not supported. Setting the <see cref="StopBits"/> property 
        /// to <b>None</b> raises an ArgumentOutOfRangeException.
        /// </para>
        /// </remarks>
        public enum SerialStopBits
        {
            /// <summary>
            /// No stop bits are used. This value is not supported. Setting the <see cref="StopBits"/> property 
            /// to <b>None</b> raises an ArgumentOutOfRangeException.
            /// </summary>
            None = System.IO.Ports.StopBits.None,
            /// <summary>
            /// One stop bit is used.
            /// </summary>
            One = System.IO.Ports.StopBits.One,
            /// <summary>
            /// 1.5 stop bits are used.
            /// </summary>
            OnePointFive = System.IO.Ports.StopBits.OnePointFive,
            /// <summary>
            /// Two stop bit are used.
            /// </summary>
            Two = System.IO.Ports.StopBits.Two
        }

        /// <summary>
        /// Specifies whether the <see cref="Serial"/> module requires hardware flow control. 
        /// </summary>
        public enum HardwareFlowControl
        {
            /// <summary>
            /// The module does not require hardware flow control.
            /// </summary>
            NotRequired,

            /// <summary>
            /// The module will use hardware flow control if available.
            /// </summary>
            UseIfAvailable,

            /// <summary>
            /// The module must have hardware flow control and will not function without it.
            /// </summary>
            Required
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish. 
        /// </summary>
        public int ReadTimeout
        {
            get
            {
                return this._serialPort.ReadTimeout;
            }

            set
            {
                this._serialPort.ReadTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a write operation does not finish. 
        /// </summary>
        public int WriteTimeout
        {
            get
            {
                return this._serialPort.WriteTimeout;
            }

            set
            {
                this._serialPort.WriteTimeout = value;
            }
        }

        /// <summary>
        /// Opens a new serial port connection. 
        /// </summary>
        public void Open()
        {
            lock (this._serialPort)
            {
                if (!this._serialPort.IsOpen)
                {
                    this._serialPort.Open();
                }
            }
        }

        /// <summary>
        /// Gets a Boolean value indicating the open or closed status of the <see cref="Serial"/> object. 
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this._serialPort.IsOpen;
            }
        }

        /// <summary>
        /// Closes the port connection, and sets the <see cref="IsOpen"/> property to <b>false</b>.
        /// </summary>
        public void Close()
        {
            lock (this._serialPort)
            {
                if (this._serialPort.IsOpen)
                {
                    this._serialPort.Close();
                }
            }
        }

        /// <summary>
        /// Writes a variable number of bytes to the serial port using data from a buffer. 
        /// </summary>
        /// <param name="data">The data to write as a byte[] array.</param>
        public void Write(params byte[] data)
        {
            if (this._serialPort != null)
            {
                this._serialPort.Write(data, 0, data.Length);
            }
            else
            {
                throw new PortNotOpenException();
            }
        }

        /// <summary>
        /// Writes a specified number of bytes to the serial port using data from a buffer. 
        /// </summary>
        /// <param name="buffer">The byte[] array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset of the <paramref name="buffer"/> parameter 
        /// at which to begin copying bytes to the port.</param>
        /// <param name="count">The number of bytes to write.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            if (this._serialPort != null)
            {
                this._serialPort.Write(buffer, offset, count);
            }
            else
            {
                throw new PortNotOpenException();
            }
        }

        /// <summary>
        /// Writes the specified text to the serial port.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void Write(string text)
        {
            if (this._serialPort != null)
            {
                if (text != null)
                {
                    if (text.Length > 0)
                    {
                        byte[] data = this.Encoding.GetBytes(text);
                        this._serialPort.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }
            else
            {
                throw new PortNotOpenException();
            }
        }

        /// <summary>
        /// Writes the specified text and the value of <see cref="LineReceivedEventDelimiter"/> to the serial port.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void WriteLine(string text)
        {
            this.Write(text + this.LineReceivedEventDelimiter);
        }

        /// <summary>
        /// Reads a number of bytes from the serial port input buffer and writes those bytes 
        /// to a byte array at the specified offset. 
        /// </summary>
        /// <param name="buffer">The byte[] array to write the input to.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> array to begin writing.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            lock (this._serialPort)
            {
                if (this._serialPort != null)
                {
                    return this._serialPort.Read(buffer, offset, count);
                }
                else
                {
                    throw new PortNotOpenException();
                }
            }
        }

        /// <summary>
        /// Reads a byte from the serial port.
        /// </summary>
        /// <returns>The byte read from the port as an integer value.</returns>
        public int ReadByte()
        {
            lock (this._serialPort)
            {
                if (this._serialPort != null)
                {
                    byte[] byteBuffer = new byte[1];
                    int count = this._serialPort.Read(byteBuffer, 0, 1);
                    if (count <= 0)
                    {
                        return -1;
                    }

                    return byteBuffer[0];
                }
                else
                {
                    throw new PortNotOpenException();
                }
            }
        }

        /// <summary>
        /// Sends any data waiting in the 'send' buffer and clears the buffer.
        /// </summary>
        public void Flush()
        {
            lock (this._serialPort)
            {
                this._serialPort.Flush();
            }
        }

        /// <summary>
        /// Discards data from the serial driver's 'send' buffer. 
        /// </summary>
        public void DiscardOutBuffer()
        {
            lock (this._serialPort)
            {
                this._serialPort.DiscardOutBuffer();
            }
        }

        /// <summary>
        /// Discards data from the serial driver's 'receive' buffer. 
        /// </summary>
        public void DiscardInBuffer()
        {
            lock (this._serialPort)
            {
                this._serialPort.DiscardInBuffer();
            }
        }

        /// <summary>
        /// Gets the number of bytes of data in the 'send' buffer. 
        /// </summary>
        public int BytesToWrite
        {
            get { return this._serialPort.BytesToWrite; }
        }

        /// <summary>
        /// Gets the number of bytes of data in the 'receive' buffer. 
        /// </summary>
        public int BytesToRead
        {
            get { return this._serialPort.BytesToRead; }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this.OnDataReceivedEvent(this, e.EventType);
        }

        private void ReadLineProcess()
        {
            string outstandingText = String.Empty;
            byte[] buf = new byte[1];

            this._serialPort.ReadTimeout = 1000;
            while (true)
            {
                try
                {
                    if (!this._serialPort.IsOpen)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    int res = -1;
                    lock (_serialPort)//Lock to avoid a close() call between isOpen and read
                    {
                        if (this._serialPort.IsOpen)//Check it did not get closed
                        {
                            res = this._serialPort.Read(buf, 0, 1);
                        }
                        else
                        {
                            continue; //Closed 
                        }
                    }
                    if (res <= 0)
                        {
                            continue;
                        }

                        char[] charData = this.Encoding.GetChars(buf);
                        string s = new string(charData);
                        outstandingText = outstandingText + s;

                        if (outstandingText.Length >= this.LineReceivedEventDelimiter.Length)
                        {
                            if (
                                outstandingText.Substring(outstandingText.Length -
                                                          this.LineReceivedEventDelimiter.Length).Equals(
                                                              this.LineReceivedEventDelimiter))
                            {
                                // Chop end pattern off.
                                outstandingText = outstandingText.Substring(0,
                                                                            outstandingText.Length -
                                                                            this.LineReceivedEventDelimiter.Length);

                                // Debug.Print(outstandingText);
                                this.OnLineReceivedEvent(this, outstandingText);
                                outstandingText = String.Empty;
                            }
                        }
                    
                }
                catch
                {
                    Debug.Print("Exception parsing serial line");
                }
            }
        }

        /// <summary>
        /// Represents the delegate used for the <see cref="DataReceived"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Serial"/> object that raised the event.</param>
        /// <param name="data">A <see cref="SerialData"/> object that contains the data received.</param>
        public delegate void DataReceivedEventHandler(Serial sender, SerialData data);

        /// <summary>
        /// Delegate that handles the event raised when the serial port signals that data has been received.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        private DataReceivedEventHandler onDataReceived;

        /// <summary>
        /// Event raised when data is received from the <see cref="Serial"/> object.
        /// </summary>
        /// <param name="sender">The <see cref="Serial"/> object that raised the event</param>
        /// <param name="data">A <see cref="SerialData"/> object that contains the data received.</param>
        protected virtual void OnDataReceivedEvent(Serial sender, SerialData data)
        {
            if (this.onDataReceived == null)
            {
                this.onDataReceived = new DataReceivedEventHandler(this.OnDataReceivedEvent);
            }

            if (Program.CheckAndInvoke(this.DataReceived, this.onDataReceived, sender, data))
            {
                this.DataReceived(sender, data);
            }
        }

        /// <summary>
        /// Represents the delegate used for the <see cref="LineReceived"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Serial"/> object that raised the event.</param>
        /// <param name="line">The received line of data as string.</param>
        public delegate void LineReceivedEventHandler(Serial sender, string line);

        /// <summary>
        /// Raised when a complete line of data has been received.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  Handle this event to minimize the overhead required to obtain
        ///  data from the serial port.
        /// </para>
        /// <para>
        ///  When you set <see cref="AutoReadLineEnabled"/> to <b>true</b>, automatic reading
        ///  of the serial port is enabled. When enabled, <see cref="Serial"/> will continuously 
        ///  monitor the serial port; if the port is open (that is, <see cref="IsOpen"/> is <b>true</b>),
        ///  <see cref="Serial"/> will collect incoming data. Whenever a complete line of data is received, 
        ///  as determined by the value of <see cref="LineReceivedEventDelimiter"/>,
        ///  <see cref="Serial"/> raises the <see cref="LineReceived"/> event.
        /// </para>
        /// </remarks>
        public event LineReceivedEventHandler LineReceived;

        private LineReceivedEventHandler onLineReceived;

        /// <summary>
        /// Raises the <see cref="LineReceived"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="Serial"/> object that raised the event.</param>
        /// <param name="line">The received line of data.</param>
        protected virtual void OnLineReceivedEvent(Serial sender, string line)
        {
            if (this.onLineReceived == null)
            {
                this.onLineReceived = new LineReceivedEventHandler(this.OnLineReceivedEvent);
            }

            if (Program.CheckAndInvoke(this.LineReceived, this.onLineReceived, sender, line))
            {
                this.LineReceived(sender, line);
            }
        }

        /// <summary>
        /// Represents the exception that is raised when the serial port has not been
        /// opened prior to a read or write operation.
        /// </summary>
        public class PortNotOpenException : ApplicationException
        {
            // Note: A constructor summary is auto-generated by the doc builder.
            /// <summary></summary>
            public PortNotOpenException()
                : base("The port must be opened before use by calling the Open() method.")
            {
            }

            // Note: A constructor summary is auto-generated by the doc builder.
            /// <summary></summary>
            /// <param name="innerException">The inner exception, or <b>null</b> if none.</param>
            public PortNotOpenException(Exception innerException)
                : base("The port must be opened before use by calling the Open() method.", innerException)
            {
            }
        }

    }
     
}
