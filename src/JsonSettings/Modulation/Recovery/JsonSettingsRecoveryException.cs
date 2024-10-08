﻿using System;

namespace Nucs.JsonSettings.Modulation.Recovery
{
    [Serializable]
    public partial class JsonSettingsRecoveryException : JsonSettingsException {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public JsonSettingsRecoveryException() { }
        public JsonSettingsRecoveryException(string message) : base(message) { }
        public JsonSettingsRecoveryException(string message, Exception inner) : base(message, inner) { }
    }
}