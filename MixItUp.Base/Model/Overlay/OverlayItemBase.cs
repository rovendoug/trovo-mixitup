﻿using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public abstract class OverlayItemBase
    {
        public abstract Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);

        public T Copy<T>()
        {
            JObject jobj = JObject.FromObject(this);
            return jobj.ToObject<T>();
        }

        protected async Task<string> ReplaceStringWithSpecialModifiers(string str, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, bool encode = false)
        {
            SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(str, Guid.NewGuid(), encode);
            if (extraSpecialIdentifiers != null)
            {
                foreach (var kvp in extraSpecialIdentifiers)
                {
                    siString.ReplaceSpecialIdentifier(kvp.Key, kvp.Value);
                }
            }
            await siString.ReplaceCommonSpecialModifiers(user, arguments);
            return siString.ToString();
        }
    }
}
