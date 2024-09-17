﻿using System;

namespace Nucs.JsonSettings.Modulation {
    public partial class ModularityException : Exception {
        public ModularityException() { }
        public ModularityException(string message) : base(message) { }
        public ModularityException(string message, Exception inner) : base(message, inner) { }
    }
}