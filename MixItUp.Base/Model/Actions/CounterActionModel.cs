﻿using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    public enum CounterActionTypeEnum
    {
        Update,
        Reset,
        Set
    }

    [DataContract]
    public class CounterActionModel : ActionModelBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return CounterActionModel.asyncSemaphore; } }

        [DataMember]
        public string CounterName { get; set; }

        [DataMember]
        public CounterActionTypeEnum ActionType { get; set; }

        [DataMember]
        public string Amount { get; set; }

        public CounterActionModel(string counterName, CounterActionTypeEnum actionType, string amount)
            : base(ActionTypeEnum.Counter)
        {
            this.CounterName = counterName;
            this.ActionType = actionType;
            this.Amount = amount;
        }

        internal CounterActionModel(MixItUp.Base.Actions.CounterAction action)
            : base(ActionTypeEnum.Counter)
        {
            this.CounterName = action.CounterName;
            if (action.UpdateAmount) { this.ActionType = CounterActionTypeEnum.Update; }
            else if (action.SetAmount) { this.ActionType = CounterActionTypeEnum.Set; }
            else if (action.ResetAmount) { this.ActionType = CounterActionTypeEnum.Reset; }
            this.Amount = action.Amount;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (ChannelSession.Settings.Counters.ContainsKey(this.CounterName))
            {
                if (this.ActionType == CounterActionTypeEnum.Reset)
                {
                    await ChannelSession.Settings.Counters[this.CounterName].ResetAmount();
                }
                else
                {
                    string amountText = await this.ReplaceStringWithSpecialModifiers(this.Amount, user, platform, arguments, specialIdentifiers);
                    if (double.TryParse(amountText, out double amount))
                    {
                        if (this.ActionType == CounterActionTypeEnum.Update)
                        {
                            await ChannelSession.Settings.Counters[this.CounterName].UpdateAmount(amount);
                        }
                        else if (this.ActionType == CounterActionTypeEnum.Set)
                        {
                            await ChannelSession.Settings.Counters[this.CounterName].SetAmount(amount);
                        }
                    }
                }
            }
        }
    }
}