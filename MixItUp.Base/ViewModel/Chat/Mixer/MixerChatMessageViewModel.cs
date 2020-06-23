﻿using Mixer.Base.Model.Chat;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Chat.Mixer;
using MixItUp.Base.ViewModel.User;
using System;
using System.Linq;

namespace MixItUp.Base.ViewModel.Chat.Mixer
{
    public class MixerChatMessageViewModel : UserChatMessageViewModel
    {
        private ChatMessageDataModel[] messageData;

        public MixerChatMessageViewModel(ChatMessageEventModel chatMessageEvent, UserViewModel user = null)
            : base(chatMessageEvent.id.ToString(), StreamingPlatformTypeEnum.Mixer, (user != null) ? user : new UserViewModel(chatMessageEvent))
        {
            this.IsInUsersChannel = ChannelSession.MixerChannel.id.Equals(chatMessageEvent.channel);

            this.TargetUsername = chatMessageEvent.target;

            this.ProcessMessageContents(chatMessageEvent.message);
        }

        public MixerChatMessageViewModel(ChatSkillAttributionEventModel chatMessageEvent, UserViewModel user = null)
            : base(chatMessageEvent.id.ToString(), StreamingPlatformTypeEnum.Mixer, (user != null) ? user : new UserViewModel(chatMessageEvent))
        {
            this.ProcessMessageContents(chatMessageEvent.message);
        }

        public override bool ContainsOnlyEmotes()
        {
            if (this.messageData != null && this.messageData.Length > 0)
            {
                return this.messageData.All(m => m.type.Equals("emoticon") || (m.type.Equals("text") && string.IsNullOrWhiteSpace(m.text)));
            }
            return true;
        }

        private void ProcessMessageContents(ChatMessageContentsModel messageContents)
        {
            if (messageContents != null)
            {
                this.messageData = messageContents.message;
                foreach (ChatMessageDataModel message in messageContents.message)
                {
                    message.text = message.text.Trim().Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
                    switch (message.type)
                    {
                        case "emoticon":
                            this.AddStringMessagePart(message.text);
                            MixerChatEmoteModel emote = MixerChatEmoteModel.GetEmoteForMessageData(message);
                            if (emote != null)
                            {
                                this.MessageParts[this.MessageParts.Count - 1] = emote;
                            }
                            break;
                        case "link":
                            this.ContainsLink = true;
                            this.AddStringMessagePart(message.text);
                            break;
                        case "text":
                            this.AddStringMessagePart(message.text);
                            break;
                        case "tag":
                            this.AddStringMessagePart(message.text);
                            break;
                    }
                }
            }
        }
    }
}
