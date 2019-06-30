using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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

        private uint actionIdCounter;

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
            this.host = host ?? throw new ArgumentNullException("host");
            this.mandant = mandant ?? throw new ArgumentNullException("mandant");
            this.port = port;
            this.password = password ?? throw new ArgumentNullException("password");
        }

        /// <summary>
        /// Öffnet die Verbindung über das Netzwerk und meldet sich am ERP-Server an.
        /// </summary>
        public void Open() {
            if (connected)
                Close();

            ResetActionId();

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
            // abgeholt, wird sie mit dem nächsten Befehl empfangen
            EPICommand status = stream.ReadNextCommand();

            stream.Write(CreateChangeMandantCommand());
            if (stream.Read() != EPIResponseType.Acknowledge)
                throw new EPIException("change mandant failed", stream.ResultMessage);

            stream.Write(CreateLogOnCommand());
            if (stream.Read() != EPIResponseType.Acknowledge)
                throw new EPIException("logon failed", stream.ResultMessage);

            connected = true;
        }

        private void ResetActionId() {
            ActionId = 1;
            actionIdCounter = ActionId;

            if (connected) {
                stream.RemoveAllChannels();
                stream.AddChannel(ActionId);
            }
        }

        /// <summary>
        /// Sendet ein END-Kommando und schließt die Verbindung.
        /// </summary>
        public void Close() {
            if (connected) {
                stream.Write(CreateEndCommand());
                connected = false;

                if (stream.Read() != EPIResponseType.End)
                    throw new EPIException("server doesn't accept end command", stream.ResultMessage);
            }

            if (stream != null)
                stream.Close();

            if (client != null)
                client.Close();
        }

        public uint RegisterNewActionId() {
            uint nextActionId = ++actionIdCounter;
            return nextActionId;
        }

        public void SetOption(string name, string value) {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

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
                throw new ArgumentNullException("name");

            EPICommand sho = new CommandBuilder()
                .SetCMDWord(CommandWords.Session.ShowOptions)
                .SetActionId(ActionId)
                .AddField(name)
                .AddField("0") // mit Metadaten?
                .Build();

            stream.Write(sho);
            if (stream.Read() != EPIResponseType.Data)
                throw new EPIException("edp option could'nt be received", stream.ResultMessage);

            bool valueReaded = false;
            string value = String.Empty;

            while(stream[ActionId].Count > 0) {
                EPICommand cmd = stream[ActionId].Dequeue();
                if (CommandWords.Responses.Data == cmd.CMDWord) {
                    if (valueReaded)
                        throw new EPIException("server responses with an unexpected data command", cmd);

                    if (cmd.Fields.Length < 1)
                        throw new EPIException("server responses with an empty data command", cmd);

                    value = cmd[1];
                    valueReaded = true;
                }

                if (CommandWords.Responses.EndOfData == cmd.CMDWord) {
                    if (!valueReaded)
                        throw new EPIException("server response contains no data about the requested option value");
                    
                    return value;
                }
            }

            throw new EPIException("server response contains no data about the requested option value");
        }

        public Queue<EPICommand> ExecuteQuery(string select, uint actionId, string fieldList, bool withMetaData, int limit, int offset, string varLang) {
            if (fieldList == null)
                throw new ArgumentNullException("fieldList");

            EPICommand exq = new CommandBuilder()
               .SetCMDWord(CommandWords.Selecting.ExecuteQuery)
               .SetActionId(actionId)
               .AddField(select)
               .AddField(fieldList)
               .AddField(limit == 0 ? String.Empty : limit.ToString())
               .AddField(offset == 0 ? String.Empty : offset.ToString())
               .AddField(String.Empty) // Edit-TID
               .AddField(String.Empty) // Edit RowNo
               .AddField(String.Empty) // Edit FieldName
               .AddField(String.Empty) // Timeout
               // Metadaten nur dann aktivieren, wenn keine Feldliste vorhanden ist
               // .AddField("1")
               .AddField(String.IsNullOrEmpty(fieldList) ? "1" : withMetaData ? "1" : "0")
               .AddField(varLang)
               .Build();

            stream.Write(exq);

            if (stream.Read() != EPIResponseType.Data)
                throw new EPIException("query execution failed", stream.ResultMessage);

            return stream[actionId];
        }

        public Queue<EPICommand> GetNextRecord(uint actionId) {
            EPICommand gnr = new CommandBuilder()
                .SetCMDWord(CommandWords.Selecting.GetNextRecord)
                .SetActionId(actionId)
                .Build();

            stream.Write(gnr);

            if (stream.Read() != EPIResponseType.Data)
                throw new EPIException("get next record failed", stream.ResultMessage);

            return stream[actionId];
        }

        public void BreakQueryExecution(uint actionId) {
            EPICommand brq = new CommandBuilder()
                .SetCMDWord(CommandWords.Selecting.BreakQueryExecution)
                .SetActionId(actionId)
                .Build();

            stream.Write(brq);

            // Laut Protokoll sollte ein EOD erfolgen, das passiert aber offenbar nicht
            // if (stream.Read() != EPIResponseType.Data)
            //     throw new EPIException("break query execution failed", stream.ResultMessage);
        }

        private EPICommand CreateChangeMandantCommand() {
            return new CommandBuilder()
                .SetCMDWord(CommandWords.Session.ChangeMandant)
                .SetActionId(ActionId)
                .AddField(mandant)
                .Build();
        }

        private EPICommand CreateLogOnCommand() {
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

        private EPICommand CreateEndCommand() {
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
