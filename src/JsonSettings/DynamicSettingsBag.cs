﻿using System;
using System.Dynamic;

namespace Nucs.JsonSettings {
    /// <summary>
    ///     A dynamic wrapper to a <see cref="SettingsBag"/>.
    /// </summary>
    public sealed partial class DynamicSettingsBag : DynamicObject, IDisposable {
        private SettingsBag _bag { get; set; }

        /// <summary>
        ///     Casts back a from <see cref="DynamicSettingsBag"/> to <see cref="SettingsBag"/>.
        /// </summary>
        /// <returns></returns>
        public SettingsBag AsBag() {
            return _bag;
        }

        public DynamicSettingsBag(SettingsBag bag) {
            _bag = bag ?? throw new ArgumentNullException(nameof(bag));
        }


        public void Dispose() {
            _bag = null;
        }

        // Get the property value.
        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = _bag[binder.Name];
            return true;
        }

        // Set the property value.
        public override bool TrySetMember(SetMemberBinder binder, object value) {
            _bag[binder.Name] = value;
            return true;
        }

        // Set the property value by index.
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
            var index = indexes[0] as string;
            if (string.IsNullOrEmpty(index))
                return false;
            _bag[index] = value;
            
            return true;
        }

        // Get the property value by index.
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
            var index = indexes[0] as string;
            if (string.IsNullOrEmpty(index)) {
                result = null;
                return false;
            }
            result = _bag[index];
            return true;
        }
    }
}