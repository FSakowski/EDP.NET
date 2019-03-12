using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI {
    public static class CommandFields {

        public static class Session {
            public static class CHM {
                public const ushort Mandant = 0;
            }

            public static class LGN {
                public const ushort Password = 0;
                public const ushort Mandant = 1;
                public const ushort ClientVersion = 2;
                public const ushort ClientHostname = 3;
                public const ushort ClientApp = 4;
                public const ushort Identifier = 5;
                public const ushort ClientIPAddr = 6;
                public const ushort UserName = 7;
                public const ushort LoginType = 8;
                public const ushort SessionMode = 9;
            }
        }

        public static class Responses {
            /// <summary>
            /// Acknowledge-Message
            /// </summary>
            public static class ACK {
                public const ushort MessageText = 0;
            }

            /// <summary>
            /// Negative Acknowledge
            /// </summary>
            public static class NACK {
                public const ushort MessageText = 0;
                public const ushort ErrorNo = 1;
                public const ushort Context = 2;
            }

            public static class EOD {
                public const ushort OKFlag = 0;
                public const ushort NumRecords = 1;
                public const ushort EOFFlag = 2;
            }

            /// <summary>
            /// Error-Message
            /// </summary>
            public static class E {
                public const ushort MessageText = 0;
                public const ushort MsgType = 1;
                public const ushort MsgNo = 2;
                public const ushort Row = 3;
                public const ushort FieldName = 4;
                public const ushort Context = 5;
            }

            /// <summary>
            /// Status-Message
            /// </summary>
            public static class S {
                public const ushort MessageText = 0;
                public const ushort MsgType = 1;
                public const ushort MsgNo = 2;
                public const ushort Row = 3;
                public const ushort FieldName = 4;
                public const ushort Context = 5;
            }

            /// <summary>
            /// End-Message
            /// </summary>
            public static class END {
                public const ushort MessageText = 0;
                public const ushort ExitStatus = 1;
            }
        }
    }
}
