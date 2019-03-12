﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace EDPDotNet.EPI {
    /// <summary>
    /// Liest und schreibt EPI-Kommandos über einen Netzwerkstream. Empfangene Nachrichten werden in eine Warteschlange eingereiht.
    /// </summary>
    public class EPIStream : Queue<EPICommand>, IDisposable {
        private Encoding encoding = System.Text.Encoding.ASCII;
        private Char fldSep = '|';
        private ushort maxBufferSize = 4096;
        private char endCommandSign = (char)10;

        private NetworkStream stream;

        private TextWriter statusMessageWriter; 

        private EPICommand resultMessage;

        #region Properties
        public Encoding Encoding {
            get {
                return encoding;
            }

            set {
                encoding = value ?? throw new ArgumentNullException("Encoding must not be null");
            }
        }

        public Char FieldSeparator {
            get {
                return fldSep;
            }

            set {
                fldSep = value;
            }
        }

        public ushort MaxBufferSize {
            get {
                return maxBufferSize;
            }

            set {
                maxBufferSize = value;
            }
        }

        public char EndCommandSign {
            get {
                return endCommandSign;
            }

            set {
                endCommandSign = value;
            }
        }

        public EPICommand ResultMessage {
            get {
                return resultMessage;
            }
        }

        #endregion

        /// <summary>
        /// Instanziert einen neuen EPI-Stream mit Standardeinstellungen.
        /// </summary>
        /// <param name="stream">Netzwerkstream, dieser muss vorher geöffnet werden. Geschlossen wird der Stream automatisch oder über Aufruf der Close-Methode.</param>
        public EPIStream(NetworkStream stream) {
            this.stream = stream ?? throw new ArgumentNullException("stream must not be null");
            resultMessage = CommandBuilder.CreateEmptyCommand();
        }

        #region Decode / Encode
        public string DecodeCommand(EPICommand cmd) {
            if (cmd == null)
                throw new ArgumentNullException("cmd must not be null");

            if (String.IsNullOrEmpty(cmd.CMDWord) || cmd.CMDWord.Length > 3)
                throw new EPIException("epi command word is invalid", cmd);

            StringBuilder line = new StringBuilder();
            line.AppendFormat("{0}{1}{2}", cmd.CMDWord, fldSep, cmd.ActionId);

            foreach (String field in cmd) {
                line.Append(fldSep);
                line.Append(field);
            }

            if (cmd.Completed)
                line.Append(fldSep);

            return line.ToString();
        }

        public EPICommand EncodeCommand(string response) {
            if (String.IsNullOrEmpty(response))
                return null;

            CommandBuilder cmdBuilder = new CommandBuilder();

            string[] fields = response.Split(fldSep);
            cmdBuilder.SetCMDWord(fields[0]);

            // End-Message (wird gesondert behandelt, da es keine TID zurückliefert)
            if (CommandWords.Session.End.Equals(fields[0])) {
                // Nachricht übernehmen
                if (fields.Length > 1)
                    cmdBuilder.AddField(fields[1]);

                // Exit-Code übernehmen
                if (fields.Length > 2)
                    cmdBuilder.AddField(fields[2]);

                cmdBuilder.SetAsCompleted(true);
                return cmdBuilder.Build();
            }

            // TID einlesen
            if (fields.Length > 1) {
                uint actionid;

                if (UInt32.TryParse(fields[1], out actionid)) {
                    cmdBuilder.SetActionId(actionid);
                } else {
                    throw new EPIException("action id (" + fields[1] + ") is invalid from response " + response);
                }
            }

            // Sonstige Felder einlesen
            if (fields.Length > 2) {
                for(int i = 2; i < fields.Length; i++)
                    cmdBuilder.AddField(fields[i]);
            }

            return cmdBuilder.SetAsCompleted(response.EndsWith(fldSep)).Build();
        }
        #endregion

        /// <summary>
        /// Sendet einen EPI-Kommando an den Server. Der Aufruf löscht die aktuelle Warteschlange der zuletzt empfangenen Nachrichten.
        /// </summary>
        /// <param name="cmd">EPI-Kommando</param>
        public void Write(EPICommand cmd) {
            Clear();
            string request = DecodeCommand(cmd);

#if DEBUG
            Console.WriteLine("request: " + request);
#endif

            byte[] data = encoding.GetBytes(request);

            stream.Write(data, 0, data.Length);

            // linefeed senden zum Ausführen des Kommandos
            stream.Write(new byte[] { Convert.ToByte(endCommandSign) }, 0, 1);
        }

        /// <summary>
        /// Liest das nächste EPI-Kommando vom Server. Es wird nur das erste EPI-Kommando ausgewertet, wenn der Server
        /// mehrere Nachrichten sendet, werden diese ignoriert und gehen verloren.
        /// </summary>
        /// <returns>EPI-Kommando</returns>
        public EPICommand ReadNextCommand() {
            byte[] data = new byte[maxBufferSize];
            string prevResponse = String.Empty;

            string response = read();

            int index = response.IndexOf(endCommandSign, 0);
            return readResponse(response, 0, index == -1 ? response.Length : index + 1);
        }

        /// <summary>
        /// Wartet bzw. liest solange EPI-Kommandos vom Server ein, bis eine abschließenden Antwort, 
        /// z.B. ein Acknowledge, eintrifft und liefert das Ergebnis der Abfrage zurück. 
        /// </summary>
        /// <returns>Abschließende Antwortyp des Servers</returns>
        public EPIResponseType Read() {
            EPIResponseType type = EPIResponseType.Undefined;
            resultMessage = CommandBuilder.CreateEmptyCommand();

            do {
                string response = read();

                // im Response können 1 bis n Nachrichten enthalten sein
                int offset = 0;
                int index = -1;

                do {
                    index = response.IndexOf(endCommandSign, offset);

                    if (index >= 0) {
                        EPICommand cmd = readResponse(response, offset, index);
                        handleEPICommand(cmd);

                        if (type == EPIResponseType.Undefined)
                            type = EPIResponseTypeHelper.GetTypeOf(cmd);
                    }

                    offset = index + 1;
                } while (offset > 0 && offset < response.Length);

                // weiterlesen, wenn noch keine abschließende Antowort vom Server gesendet wurde
            } while (type == EPIResponseType.Undefined);

            return type;
        }

        /// <summary>
        /// Liest Daten vom Netzwerkstream, bis das abschließende Endzeichen (Newline) gesendet wird. Damit wird sichergegangen, dass 
        /// die Antwort erst in eine Zeichenkette umgewandelt wird, wenn alle Pakete empfangen wurden.
        /// </summary>
        /// <returns></returns>
        private string read() {
            byte[] data = new byte[maxBufferSize];

            using (MemoryStream ms = new MemoryStream()) {
                int length;
                do {
                    length = stream.Read(data, 0, data.Length);
                    ms.Write(data, 0, length);

                } while (length > 0 && data[length - 1] != Convert.ToByte(endCommandSign));

                try {
                    return encoding.GetString(ms.ToArray());
                } catch (Exception e) {
                    throw new EPIException("error occured at encoding of received data to an epi command", e);
                }
            }
        }

        private EPICommand readResponse(string response, int offset, int endIndex) {


            string trimedResponse = response.Substring(offset, endIndex - offset);

            // Newline am Ende entfernen, dieses wird hier nicht mehr benötigt
            /*if (trimedResponse.EndsWith(endCommandSign))
                trimedResponse = trimedResponse.Substring(0, length - 1);*/

#if DEBUG
            Console.WriteLine("response: " + trimedResponse);
#endif

            return EncodeCommand(trimedResponse);
        }

        private void handleEPICommand(EPICommand cmd) {
            switch (cmd.CMDWord) {
                case CommandWords.Responses.StatusMessage:
                    printStatusMessage(cmd);
                    break;

                case CommandWords.Responses.Acknowledge:
                    resultMessage = cmd;
                    break;

                case CommandWords.Responses.NegativeAcknowledge:
                    resultMessage = cmd;
                    break;

                case CommandWords.Responses.BeginOfData:
                    Enqueue(cmd);
                    break;

                case CommandWords.Responses.ChangeNotification:
                    Enqueue(cmd);
                    break;

                case CommandWords.Responses.Data:
                    Enqueue(cmd);
                    break;

                case CommandWords.Responses.DataContinuation:
                    Enqueue(cmd);
                    break;

                case CommandWords.Responses.EndOfData:
                    Enqueue(cmd);
                    break;

                case CommandWords.Responses.End:
                    resultMessage = cmd;
                    break;

                case CommandWords.Responses.MetaData:
                    Enqueue(cmd);
                    break;

                case CommandWords.Responses.ProgressMessage:
                    Enqueue(cmd);
                    break;

                case CommandWords.Responses.Error:
                    throw new EPIException("server responses an error: " + cmd[CommandFields.Responses.E.MessageText], cmd);
            }
        }

        /// <summary>
        /// Setzt einen TextWriter für Status-Meldungen.
        /// </summary>
        /// <param name="writer">TextWriter oder NULL</param>
        public void SetStatusMessageWriter(TextWriter writer) {
            statusMessageWriter = writer;
        }

        private void printStatusMessage(EPICommand cmd) {
            if (statusMessageWriter != null) {
                statusMessageWriter.WriteLine("Status: {0}", cmd[CommandFields.Responses.S.MessageText]);
            }
        }

        #region Disposable-Implementierung

        public void Close() {
            Dispose();
        }

        public void Dispose() {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EPIStream() {
            dispose(false);
        }

        private void dispose(bool disposing) {
            if (disposing) {
                stream.Close();
            }
        }

        #endregion
    }
}