using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EDPDotNet.EPI {
    public class EPIConnection : IDisposable {

        public const string DefaultEDPEncoding = "ISO-8859-1";
        public const char DefaultFieldSeparator = '|';
        public const ushort DefaultMaxBufferSize = 4096;

        private static readonly char[] SupportedFieldSeparators = new char[] { '|', '!', '$', '#', ':', ';' };

        private string host;
        private string mandant;
        private ushort port;
        private string password;

        private TcpClient client;
        EPIStream stream;

        private bool connected = false;

        #region Properties

        public uint ActionId {
            get;
            set;
        }

        public bool Connected {
            get {
                return connected;
            }
        }

        /// <summary>
        /// Ruft die verwendete Textkodierung ab oder legt diese fest.
        /// Diese wird an den Server gesendet und muss vom ERP-System untersützt werden.
        /// </summary>
        public Encoding Encoding {
            get {
                if (!connected)
                    return System.Text.Encoding.GetEncoding(DefaultEDPEncoding);

                return stream.Encoding;
            }

            set {
                if (!connected)
                    throw new InvalidOperationException("encoding can't be changed without an open connection");

                stream.Encoding = value;
                SetOption(EDPOption.Charset.Value, value.WebName);
            }
        }

        /// <summary>
        /// Ruft das verwendete Feldtrennzeichen zurück oder legt dieses fest.
        /// </summary>
        public char FieldSeparator {
            get {
                if (!connected)
                    return DefaultFieldSeparator;

                return stream.FieldSeparator;
            }

            set {
                if (!connected)
                    throw new InvalidOperationException("field separator can't be changed without an open connection");

                if (!Array.Exists(SupportedFieldSeparators, f => f == value))
                    throw new NotSupportedException($"the character {value} is't supported as field separator");

                SetOption(EDPOption.FieldSeparator.Value, Convert.ToString(value));
                stream.FieldSeparator = value;
            }
        }

        #endregion

        /// <summary>
        /// Erzeugt eine neue Instanz der EPIConnection-Klasse und nimmt die Anmeldeinformationen an. 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="mandant"></param>
        /// <param name="port"></param>
        /// <param name="password"></param>
        public EPIConnection(string host, string mandant, ushort port, string password) {
            this.host = host ?? throw new ArgumentNullException("host must not be null");
            this.mandant = mandant ?? throw new ArgumentNullException("mandant must not be null");
            this.port = port;
            this.password = password ?? throw new ArgumentNullException("password must not be null");
        }

        /// <summary>
        /// Öffnet die Verbindung über das Netzwerk und meldet sich am ERP-Server an.
        /// </summary>
        public void Open() {
            if (connected)
                Close();

            try {
                client = new TcpClient(host, port);
            } catch (SocketException e) {
                throw new EPIException("connection has been refused", e);
            }

            stream = new EPIStream(client.GetStream()) {
                Encoding = System.Text.Encoding.GetEncoding(DefaultEDPEncoding),
                FieldSeparator = DefaultFieldSeparator,
                MaxBufferSize = DefaultMaxBufferSize
            };

            // Server sendet als erstes eine Status-Message, welche abgeholt werden kann, wird sie nicht 
            // abgeholt, wird sie mit beim nächsten Befehl empfangen
            EPICommand status = stream.ReadNextCommand();

            stream.Write(createChangeMandantCommand());
            if (stream.Read() != EPIResponseType.Acknowledge)
                throw new EPIException("change mandant failed", stream.ResultMessage);

            stream.Write(createLogOnCommand());
            if (stream.Read() != EPIResponseType.Acknowledge)
                throw new EPIException("logon failed", stream.ResultMessage);

            connected = true;
        }

        /// <summary>
        /// Sendet ein END-Kommando und schließt die Verbindung.
        /// </summary>
        public void Close() {
            if (connected) {
                stream.Write(createEndCommand());
                connected = false;

                if (stream.Read() != EPIResponseType.End)
                    throw new EPIException("server doesn't accept end command", stream.ResultMessage);
            }

            if (stream != null)
                stream.Close();

            if (client != null)
                client.Close();
        }

        public void SetOption(string name, string value) {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name must not be null");

            if (!TrySetOption(name, value))
                throw new EPIException("edp option couldn't be set", stream.ResultMessage);
        }

        public bool TrySetOption(string name, string value) {
            EPICommand set = new CommandBuilder()
                .SetCMDWord(CommandWords.Session.SetOption)
                .SetActionId(ActionId)
                .AddField(name)
                .AddField(value)
                .Build();

            stream.Write(set);
            return stream.Read() == EPIResponseType.Acknowledge;
        }

        public string GetOptionValue(string name) {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name must not be null");

            EPICommand sho = new CommandBuilder()
                .SetCMDWord(CommandWords.Session.ShowOptions)
                .SetActionId(ActionId)
                .AddField(name)
                .AddField("0") // mit Metadaten?
                .Build();

            stream.Write(sho);
            if (stream.Read() != EPIResponseType.Data)
                throw new EPIException("edp option could'nt be received", stream.ResultMessage);

            DataSet ds = DataSet.Fill(stream);

            if (ds.Count == 0)
                throw new EPIException("server response contains no data about the requested option value");

            return ds[0][name];
        }

        public DataSet ExecuteQuery(string select, string fieldList, int limit, int offset, string varLang) {
            EPICommand exq = new CommandBuilder()
               .SetCMDWord(CommandWords.Selecting.ExecuteQuery)
               .SetActionId(ActionId)
               .AddField(select)
               .AddField(fieldList)
               .AddField(limit.ToString())
               .AddField(offset.ToString())
               .AddField(String.Empty) // Edit-TID
               .AddField(String.Empty) // Edit RowNo
               .AddField(String.Empty) // Edit FieldName
               .AddField(String.Empty) // Timeout
               .AddField("0") // Metadaten
               .AddField(varLang)
               .Build();

            stream.Write(exq);

            if (stream.Read() != EPIResponseType.Data)
                throw new EPIException("query execution failed", stream.ResultMessage);

            return DataSet.Fill(stream);
        }

        private EPICommand createChangeMandantCommand() {
            return new CommandBuilder()
                .SetCMDWord(CommandWords.Session.ChangeMandant)
                .SetActionId(ActionId)
                .AddField(mandant)
                .Build();
        }

        private EPICommand createLogOnCommand() {
            return new CommandBuilder()
               .SetCMDWord(CommandWords.Session.Logon)
               .SetActionId(ActionId)
               .AddField(password)
               .AddField(mandant)
               .AddField("3.51")
               .AddField(String.Empty)
               .AddField(String.Empty)
               .AddField(String.Empty)
               .AddField(String.Empty)
               .AddField(String.Empty)
               .AddField("0")
               .AddField("USER")
               .Build();
        }

        private EPICommand createEndCommand() {
            return new CommandBuilder()
               .SetCMDWord(CommandWords.Session.End)
               .Build();
        }

        #region Disposable-Implementierung
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EPIConnection() {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                Close();
            }
        }
        #endregion
    }
}
