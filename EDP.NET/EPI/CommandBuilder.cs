using System;
using System.Collections.Generic;
using System.Text;

namespace EDPDotNet.EPI {
    public class CommandBuilder {
        private string cmdWord;
        private uint actionId;
        private List<string> fields;
        private bool completed = true;

        public CommandBuilder() {
            fields = new List<string>();
        }

        public CommandBuilder SetCMDWord(string cmd) {
            cmdWord = cmd;
            return this;
        }

        public CommandBuilder SetActionId(uint actionId) {
            this.actionId = actionId;
            return this;
        }

        public CommandBuilder AddField(string fieldValue) {
            fields.Add(fieldValue);
            return this;
        }

        public CommandBuilder SetAsCompleted(bool completed) {
            this.completed = completed;
            return this;
        }

        public EPICommand Build() {
            return new EPICommand(cmdWord, actionId, fields.ToArray(), completed);
        }

        public static EPICommand CreateEmptyCommand() {
            return new EPICommand(String.Empty, 0, new string[] { }, true);
        }
    }
}
